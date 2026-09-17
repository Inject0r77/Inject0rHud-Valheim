using System;
using System.Reflection;
using Inject0rHUD.Localization;
using Inject0rHUD.Util;
using UnityEngine;

namespace Inject0rHUD.Services
{
    internal static class ProductionHoverService
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static string AppendBeehive(Beehive hive, string original)
        {
            if (hive == null)
                return original;

            try
            {
                ZNetView nview = FindNView(hive);
                if (nview == null || !nview.IsValid())
                    return original;

                object zdo = nview.GetZDO();
                if (zdo == null)
                    return original;

                int honey = InvokeInt(hive, "GetHoneyLevel", -1);
                int maxHoney = ReadInt(hive, "m_maxHoney", -1);

                if (honey < 0 || maxHoney <= 0)
                    return original;

                string color = TimerColor(Plugin.BeehiveHoverOpacity);
                string extra = ModLocalization.T("world.honey") + ": " + honey + "/" + maxHoney;

                if (honey < maxHoney)
                {
                    float secPerUnit = ReflectionUtil.ReadFloat(hive, "m_secPerUnit", 0f);
                    float progress = ReadZdoFloat(zdo, "product", 0f);

                    if (secPerUnit > 0f)
                    {
                        float remaining = secPerUnit - progress;
                        if (remaining > 0.5f)
                        {
                            extra += "\n" +
                                ModLocalization.T("world.next_honey") +
                                ": " + FormatTime(remaining);
                        }
                    }
                }

                return original + "\n<color=#" + color + ">" + extra + "</color>";
            }
            catch (Exception ex)
            {
                Plugin.LogHoverErrorOnce("BeehiveHover", ex);
                return original;
            }
        }

        internal static string AppendFermenter(Fermenter fermenter, string original)
        {
            if (fermenter == null)
                return original;

            try
            {
                ZNetView nview = FindNView(fermenter);
                if (nview == null || !nview.IsValid() || nview.GetZDO() == null)
                    return original;

                object status = Invoke(fermenter, "GetStatus");
                string statusName = status != null ? status.ToString() : string.Empty;

                string content = InvokeString(fermenter, "GetContentName");
                if (!string.IsNullOrEmpty(content))
                    content = LocalizationUtil.Localize(content, content);

                if (string.Equals(statusName, "Empty", StringComparison.OrdinalIgnoreCase))
                    return original;

                string color = TimerColor(Plugin.FermenterHoverOpacity);
                string extra = string.Empty;

                if (!string.IsNullOrEmpty(content))
                    extra = ModLocalization.T("world.contents") + ": " + content;

                if (string.Equals(statusName, "Fermenting", StringComparison.OrdinalIgnoreCase))
                {
                    double elapsed = InvokeDouble(fermenter, "GetFermentationTime", -1.0);
                    float duration = ReflectionUtil.ReadFloat(fermenter, "m_fermentationDuration", 0f);

                    if (duration > 0f && elapsed >= 0.0)
                    {
                        double remaining = duration - elapsed;
                        if (remaining > 0.5)
                        {
                            if (!string.IsNullOrEmpty(extra))
                                extra += "\n";

                            extra += ModLocalization.T("world.ready_in") +
                                     ": " + FormatTime(remaining);
                        }
                    }
                }
                else if (string.Equals(statusName, "Ready", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(extra))
                        extra += "\n";
                    extra += ModLocalization.T("world.ready");
                }

                if (string.IsNullOrEmpty(extra))
                    return original;

                return original + "\n<color=#" + color + ">" + extra + "</color>";
            }
            catch (Exception ex)
            {
                Plugin.LogHoverErrorOnce("FermenterHover", ex);
                return original;
            }
        }

        internal static string AppendProduction(Smelter smelter, string original)
        {
            if (smelter == null)
                return original;

            try
            {
                ZNetView nview = FindNView(smelter);
                if (nview == null || !nview.IsValid())
                    return original;

                object zdo = nview.GetZDO();
                if (zdo == null)
                    return original;

                int queue = InvokeInt(smelter, "GetQueueSize", -1);
                if (queue < 0)
                    queue = ReadZdoInt(zdo, "queued", 0);

                int maxOre = ReadInt(smelter, "m_maxOre", 0);
                int maxFuel = ReadInt(smelter, "m_maxFuel", 0);
                float fuel = ReadZdoFloat(zdo, "fuel", 0f);

                string extra = string.Empty;

                if (maxOre > 0)
                    extra = ModLocalization.T("world.queue") + ": " + queue + "/" + maxOre;
                else if (queue > 0)
                    extra = ModLocalization.T("world.queue") + ": " + queue;

                if (maxFuel > 0)
                {
                    if (!string.IsNullOrEmpty(extra))
                        extra += "\n";

                    extra += ModLocalization.T("world.fuel") + ": " +
                             FormatAmount(fuel) + "/" + maxFuel;
                }

                bool paused = IsPaused(smelter, maxFuel, fuel);
                if (queue > 0)
                {
                    if (paused)
                    {
                        if (!string.IsNullOrEmpty(extra))
                            extra += "\n";
                        extra += ModLocalization.T("world.paused");
                    }
                    else if (CanShowWallClockTimer(smelter))
                    {
                        float secPerProduct = ReflectionUtil.ReadFloat(
                            smelter, "m_secPerProduct", 0f);

                        float bakeTimer = ReadZdoFloat(zdo, "bakeTimer", -1f);

                        if (secPerProduct > 0f && bakeTimer >= 0f)
                        {
                            float remaining = secPerProduct - bakeTimer;
                            if (remaining > 0.5f && remaining <= secPerProduct + 2f)
                            {
                                if (!string.IsNullOrEmpty(extra))
                                    extra += "\n";

                                extra += ModLocalization.T("world.next_output") +
                                         ": " + FormatTime(remaining);
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(extra))
                    return original;

                string color = TimerColor(Plugin.ProductionHoverOpacity);
                return original + "\n<color=#" + color + ">" + extra + "</color>";
            }
            catch (Exception ex)
            {
                Plugin.LogHoverErrorOnce("ProductionHover", ex);
                return original;
            }
        }

        private static bool CanShowWallClockTimer(Smelter smelter)
        {
            // Windmill throughput changes with live wind strength. bakeTimer is
            // trustworthy progress, but m_secPerProduct - bakeTimer is not a
            // trustworthy wall-clock ETA while wind changes, so do not invent it.
            string prefab = GetPrefabName(smelter).ToLowerInvariant();
            return !prefab.Contains("windmill");
        }

        private static bool IsPaused(Smelter smelter, int maxFuel, float fuel)
        {
            if (maxFuel > 0 && fuel <= 0.0001f)
                return true;

            bool requiresRoof = ReadBool(smelter, "m_requiresRoof", false);
            bool haveRoof = ReadBool(smelter, "m_haveRoof", true);
            bool blockedSmoke = ReadBool(smelter, "m_blockedSmoke", false);

            if (requiresRoof && !haveRoof)
                return true;

            return blockedSmoke;
        }

        private static string GetPrefabName(Component component)
        {
            if (component == null || component.gameObject == null)
                return string.Empty;

            try
            {
                MethodInfo method = typeof(Utils).GetMethod(
                    "GetPrefabName",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(GameObject) },
                    null);

                if (method != null)
                {
                    object value = method.Invoke(null, new object[] { component.gameObject });
                    if (value != null)
                        return value.ToString();
                }
            }
            catch
            {
            }

            string name = component.gameObject.name ?? string.Empty;
            int clone = name.IndexOf("(Clone)", StringComparison.Ordinal);
            return clone >= 0 ? name.Substring(0, clone) : name;
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

        private static object Invoke(object instance, string name)
        {
            if (instance == null)
                return null;

            MethodInfo method = ReflectionUtil.FindMethod(
                instance.GetType(), name, Type.EmptyTypes);

            return method != null ? method.Invoke(instance, null) : null;
        }

        private static int InvokeInt(object instance, string name, int fallback)
        {
            object value = Invoke(instance, name);
            if (value == null)
                return fallback;

            try { return Convert.ToInt32(value); }
            catch { return fallback; }
        }

        private static double InvokeDouble(object instance, string name, double fallback)
        {
            object value = Invoke(instance, name);
            if (value == null)
                return fallback;

            try { return Convert.ToDouble(value); }
            catch { return fallback; }
        }

        private static string InvokeString(object instance, string name)
        {
            object value = Invoke(instance, name);
            return value != null ? value.ToString() : string.Empty;
        }

        private static int ReadInt(object instance, string fieldName, int fallback)
        {
            if (instance == null)
                return fallback;

            FieldInfo field = ReflectionUtil.FindInstanceField(
                instance.GetType(), fieldName);

            if (field == null)
                return fallback;

            try { return Convert.ToInt32(field.GetValue(instance)); }
            catch { return fallback; }
        }

        private static bool ReadBool(object instance, string fieldName, bool fallback)
        {
            if (instance == null)
                return fallback;

            FieldInfo field = ReflectionUtil.FindInstanceField(
                instance.GetType(), fieldName);

            if (field == null)
                return fallback;

            try { return Convert.ToBoolean(field.GetValue(instance)); }
            catch { return fallback; }
        }

        private static float ReadZdoFloat(object zdo, string key, float fallback)
        {
            if (zdo == null)
                return fallback;

            MethodInfo method = zdo.GetType().GetMethod(
                "GetFloat",
                InstanceFlags,
                null,
                new[] { typeof(string), typeof(float) },
                null);

            if (method == null)
                return fallback;

            try
            {
                object value = method.Invoke(zdo, new object[] { key, fallback });
                return Convert.ToSingle(value);
            }
            catch
            {
                return fallback;
            }
        }

        private static int ReadZdoInt(object zdo, string key, int fallback)
        {
            if (zdo == null)
                return fallback;

            MethodInfo method = zdo.GetType().GetMethod(
                "GetInt",
                InstanceFlags,
                null,
                new[] { typeof(string), typeof(int) },
                null);

            if (method == null)
                return fallback;

            try
            {
                object value = method.Invoke(zdo, new object[] { key, fallback });
                return Convert.ToInt32(value);
            }
            catch
            {
                return fallback;
            }
        }

        private static string FormatAmount(float value)
        {
            float rounded = Mathf.Round(value * 10f) / 10f;
            if (Mathf.Abs(rounded - Mathf.Round(rounded)) < 0.01f)
                return Mathf.RoundToInt(rounded).ToString();

            return rounded.ToString("0.0");
        }

        private static string FormatTime(double seconds)
        {
            int total = Math.Max(0, (int)Math.Ceiling(seconds));
            int hours = total / 3600;
            int minutes = (total % 3600) / 60;
            int secs = total % 60;

            if (hours > 0)
                return hours + ":" + minutes.ToString("00") + ":" + secs.ToString("00");

            return minutes + ":" + secs.ToString("00");
        }

        private static string TimerColor(float opacity)
        {
            int alpha = Mathf.Clamp(Mathf.RoundToInt(opacity * 255f), 0, 255);
            return "FFE27A" + alpha.ToString("X2");
        }
    }
}
