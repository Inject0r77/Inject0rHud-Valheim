using System;
using System.Reflection;
using Inject0rHUD.Localization;
using Inject0rHUD.Util;
using UnityEngine;

namespace Inject0rHUD.Services
{
    internal static class WorldHoverTimerService
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static MethodInfo _zdoGetLongString;
        private static MethodInfo _zdoGetLongHash;
        private static bool _zdoMethodsResolved;
        private static object _pickedTimeHash;
        private static bool _pickedHashResolved;

        private static MethodInfo _plantGetGrowTime;
        private static MethodInfo _plantTimeSincePlanted;
        private static bool _plantMethodsResolved;

        internal static string AppendPickableTimer(Pickable pickable, string original)
        {
            if (pickable == null)
                return original;

            try
            {
                ZNetView nview = FindNView(pickable);
                if (nview == null || !nview.IsValid())
                    return original;

                if (pickable.CanBePicked() || pickable.m_respawnTimeMinutes <= 0f)
                    return original;

                object zdo = nview.GetZDO();
                if (zdo == null || ZNet.instance == null)
                    return original;

                long ticks = ReadLong(zdo, "picked_time", ref _pickedTimeHash, "s_pickedTime");
                if (ticks <= DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                    return original;

                DateTime pickedAt = new DateTime(ticks);
                TimeSpan elapsed = ZNet.instance.GetTime() - pickedAt;
                TimeSpan remaining =
                    TimeSpan.FromMinutes(pickable.m_respawnTimeMinutes) - elapsed;

                if (remaining.TotalSeconds <= 0.5)
                    return original;

                string label = ModLocalization.T("world.ready_in");
                string color = TimerColor(Plugin.PickableHoverOpacity);
                return original + "\n<color=#" + color + ">" +
                       label + ": " + FormatTime(remaining.TotalSeconds) +
                       "</color>";
            }
            catch (Exception ex)
            {
                Plugin.LogHoverErrorOnce("PickableHover", ex);
                return original;
            }
        }

        internal static string AppendPlantTimer(Plant plant, string original)
        {
            if (plant == null)
                return original;

            try
            {
                ZNetView nview = FindNView(plant);
                if (nview == null || !nview.IsValid())
                    return original;

                // Do not show a countdown for blocked/unhealthy plants.
                FieldInfo statusField = ReflectionUtil.FindInstanceField(
                    typeof(Plant), "m_status");

                if (statusField != null)
                {
                    object value = statusField.GetValue(plant);
                    if (value != null && Convert.ToInt32(value) != 0)
                        return original;
                }

                double growTime;
                double elapsed;

                if (!TryReadPlantTimes(plant, out growTime, out elapsed))
                    return original;

                double remaining = growTime - elapsed;
                if (growTime <= 0.0 || remaining <= 0.5)
                    return original;

                string label = ModLocalization.T("world.grows_in");

                string color = TimerColor(Plugin.PlantHoverOpacity);
                return original + "\n<color=#" + color + ">" +
                       label + ": " + FormatTime(remaining) +
                       "</color>";
            }
            catch (Exception ex)
            {
                Plugin.LogHoverErrorOnce("PlantHover", ex);
                return original;
            }
        }

        private static ZNetView FindNView(Component component)
        {
            if (component == null)
                return null;

            ZNetView view = component.GetComponent<ZNetView>();
            if (view != null)
                return view;

            view = component.GetComponentInParent<ZNetView>();
            if (view != null)
                return view;

            return component.GetComponentInChildren<ZNetView>();
        }

        private static bool TryReadPlantTimes(
            Plant plant,
            out double growTime,
            out double elapsed)
        {
            growTime = 0.0;
            elapsed = 0.0;

            ResolvePlantMethods();

            // Preferred path: call Valheim's own calculation by reflection.
            // This remains exact even when the methods are non-public in
            // the raw game assembly used by local builds.
            if (_plantGetGrowTime != null && _plantTimeSincePlanted != null)
            {
                object totalValue = _plantGetGrowTime.Invoke(plant, null);
                object elapsedValue = _plantTimeSincePlanted.Invoke(plant, null);

                if (TryConvertDouble(totalValue, out growTime) &&
                    TryConvertDouble(elapsedValue, out elapsed))
                {
                    return growTime > 0.0;
                }
            }

            // Fallback for a future game build: use plantTime from its ZDO
            // and the configured grow-time fields. If a randomized max exists,
            // use the midpoint rather than inventing an exact value.
            ZNetView nview = FindNView(plant);
            if (nview == null || !nview.IsValid() || ZNet.instance == null)
                return false;

            object zdo = nview.GetZDO();
            if (zdo == null)
                return false;

            object plantTimeHash = null;
            long ticks = ReadLong(zdo, "plantTime", ref plantTimeHash, "s_plantTime");
            if (ticks <= DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return false;

            DateTime plantedAt = new DateTime(ticks);
            elapsed = (ZNet.instance.GetTime() - plantedAt).TotalSeconds;

            FieldInfo growMinField =
                ReflectionUtil.FindInstanceField(typeof(Plant), "m_growTime");
            FieldInfo growMaxField =
                ReflectionUtil.FindInstanceField(typeof(Plant), "m_growTimeMax");

            if (growMinField == null)
                return false;

            double min;
            if (!TryConvertDouble(growMinField.GetValue(plant), out min))
                return false;

            double max = min;
            if (growMaxField != null)
            {
                double parsedMax;
                if (TryConvertDouble(growMaxField.GetValue(plant), out parsedMax) &&
                    parsedMax > 0.0)
                {
                    max = parsedMax;
                }
            }

            growTime = max > min ? (min + max) * 0.5 : min;
            return growTime > 0.0;
        }

        private static void ResolvePlantMethods()
        {
            if (_plantMethodsResolved)
                return;

            _plantMethodsResolved = true;

            _plantGetGrowTime = typeof(Plant).GetMethod(
                "GetGrowTime",
                InstanceFlags,
                null,
                Type.EmptyTypes,
                null);

            _plantTimeSincePlanted = typeof(Plant).GetMethod(
                "TimeSincePlanted",
                InstanceFlags,
                null,
                Type.EmptyTypes,
                null);
        }

        private static bool TryConvertDouble(object value, out double result)
        {
            result = 0.0;

            if (value == null)
                return false;

            try
            {
                result = Convert.ToDouble(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static long ReadLong(
            object zdo,
            string stringKey,
            ref object cachedHash,
            string zdoVarsField)
        {
            if (zdo == null)
                return 0L;

            ResolveZdoMethods(zdo.GetType());

            if (_zdoGetLongString != null)
            {
                object value = _zdoGetLongString.Invoke(
                    zdo,
                    new object[] { stringKey, 0L });

                if (value is long)
                    return (long)value;
            }

            if (cachedHash == null)
                cachedHash = ResolveZdoVarHash(zdoVarsField);

            if (_zdoGetLongHash != null && cachedHash != null)
            {
                object value = _zdoGetLongHash.Invoke(
                    zdo,
                    new object[] { cachedHash, 0L });

                if (value is long)
                    return (long)value;
            }

            return 0L;
        }

        private static void ResolveZdoMethods(Type zdoType)
        {
            if (_zdoMethodsResolved)
                return;

            _zdoMethodsResolved = true;

            _zdoGetLongString = zdoType.GetMethod(
                "GetLong",
                InstanceFlags,
                null,
                new[] { typeof(string), typeof(long) },
                null);

            MethodInfo[] methods = zdoType.GetMethods(InstanceFlags);
            for (int i = 0; i < methods.Length && _zdoGetLongHash == null; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != "GetLong" ||
                    method.ReturnType != typeof(long))
                {
                    continue;
                }

                ParameterInfo[] p = method.GetParameters();
                if (p.Length == 2 &&
                    p[1].ParameterType == typeof(long) &&
                    p[0].ParameterType != typeof(string))
                {
                    _zdoGetLongHash = method;
                }
            }
        }

        private static object ResolveZdoVarHash(string fieldName)
        {
            try
            {
                Type zdoVars = null;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                for (int i = 0; i < assemblies.Length && zdoVars == null; i++)
                {
                    try
                    {
                        zdoVars = assemblies[i].GetType("ZDOVars", false);
                    }
                    catch
                    {
                    }
                }

                if (zdoVars == null)
                    return null;

                FieldInfo field = zdoVars.GetField(
                    fieldName,
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                return field != null ? field.GetValue(null) : null;
            }
            catch
            {
                return null;
            }
        }

        private static string TimerColor(float opacity)
        {
            int alpha = Mathf.RoundToInt(Mathf.Clamp01(opacity) * 255f);
            return "8EE89B" + alpha.ToString("X2");
        }

        private static string FormatTime(double seconds)
        {
            if (seconds < 0.0)
                seconds = 0.0;

            long total = (long)Math.Ceiling(seconds);
            long days = total / 86400;
            long hours = (total % 86400) / 3600;
            long minutes = (total % 3600) / 60;
            long secs = total % 60;

            if (days > 0)
            {
                return days + "d " +
                       hours.ToString("00") + ":" +
                       minutes.ToString("00") + ":" +
                       secs.ToString("00");
            }

            if (hours > 0)
            {
                return hours + ":" +
                       minutes.ToString("00") + ":" +
                       secs.ToString("00");
            }

            return minutes + ":" + secs.ToString("00");
        }
    }
}
