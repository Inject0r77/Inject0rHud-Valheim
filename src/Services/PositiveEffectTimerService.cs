using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Inject0rHUD.Localization;
using Inject0rHUD.Models;
using Inject0rHUD.Util;
using UnityEngine;

namespace Inject0rHUD.Services
{
    internal static class PositiveEffectTimerService
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static FieldInfo _semanField;
        private static FieldInfo _statusEffectsField;

        private static readonly Dictionary<Type, MethodInfo> IsDebuffMethods =
            new Dictionary<Type, MethodInfo>();

        private static readonly Dictionary<Type, FieldInfo> IsDebuffFields =
            new Dictionary<Type, FieldInfo>();

        private static readonly Dictionary<Type, PropertyInfo> IsDebuffProperties =
            new Dictionary<Type, PropertyInfo>();

        internal static IEnumerable<TimerEntry> Read(Player player)
        {
            var result = new List<TimerEntry>(8);

            if (player == null)
                return result;

            object seman = GetSEMan(player);
            if (seman == null)
                return result;

            IEnumerable effects = GetStatusEffects(seman);
            if (effects == null)
                return result;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (object effect in effects)
            {
                if (effect == null)
                    continue;

                float ttl = ReflectionUtil.ReadFloat(effect, "m_ttl", 0f);
                float elapsed = ReflectionUtil.ReadFloat(effect, "m_time", 0f);

                // Only effects with a real active countdown belong in TIMERS.
                // Permanent/equipment/environment effects with ttl <= 0 are ignored.
                if (ttl <= 0.05f)
                    continue;

                float remaining = ttl - elapsed;
                if (remaining <= 0.05f)
                    continue;

                if (!IsPositive(effect))
                    continue;

                string fallback = GetReadableFallback(effect);
                string label = StatusEffectNameService.FromStatusEffect(effect, fallback);

                if (string.IsNullOrWhiteSpace(label))
                    continue;

                string key = GetStableEffectKey(effect, label);
                if (!seen.Add(key))
                    continue;

                result.Add(new TimerEntry(
                    label,
                    remaining,
                    ttl,
                    "positive"));
            }

            result.Sort((a, b) =>
            {
                int timeCompare = a.RemainingSeconds.CompareTo(b.RemainingSeconds);
                return timeCompare != 0
                    ? timeCompare
                    : string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
            });

            return result;
        }

        private static object GetSEMan(Player player)
        {
            if (_semanField == null)
                _semanField = ReflectionUtil.FindInstanceField(typeof(Character), "m_seman");

            return _semanField != null ? _semanField.GetValue(player) : null;
        }

        private static IEnumerable GetStatusEffects(object seman)
        {
            if (seman == null)
                return null;

            if (_statusEffectsField == null)
            {
                _statusEffectsField =
                    ReflectionUtil.FindInstanceField(seman.GetType(), "m_statusEffects") ??
                    ReflectionUtil.FindInstanceField(seman.GetType(), "m_statusEffect");
            }

            if (_statusEffectsField == null)
                return null;

            return _statusEffectsField.GetValue(seman) as IEnumerable;
        }

        private static bool IsPositive(object effect)
        {
            Type type = effect.GetType();

            // Prefer Valheim's own debuff classification if the current build
            // exposes it as a method/property/field.
            MethodInfo method;
            if (!IsDebuffMethods.TryGetValue(type, out method))
            {
                method =
                    type.GetMethod("IsDebuff", InstanceFlags, null, Type.EmptyTypes, null) ??
                    type.GetMethod("GetIsDebuff", InstanceFlags, null, Type.EmptyTypes, null);

                IsDebuffMethods[type] = method;
            }

            if (method != null && method.ReturnType == typeof(bool))
            {
                try
                {
                    return !(bool)method.Invoke(effect, null);
                }
                catch
                {
                }
            }

            PropertyInfo property;
            if (!IsDebuffProperties.TryGetValue(type, out property))
            {
                property =
                    type.GetProperty("IsDebuff", InstanceFlags) ??
                    type.GetProperty("isDebuff", InstanceFlags);

                IsDebuffProperties[type] = property;
            }

            if (property != null && property.PropertyType == typeof(bool))
            {
                try
                {
                    return !(bool)property.GetValue(effect, null);
                }
                catch
                {
                }
            }

            FieldInfo field;
            if (!IsDebuffFields.TryGetValue(type, out field))
            {
                field =
                    ReflectionUtil.FindInstanceField(type, "m_isDebuff") ??
                    ReflectionUtil.FindInstanceField(type, "isDebuff");

                IsDebuffFields[type] = field;
            }

            if (field != null && field.FieldType == typeof(bool))
            {
                try
                {
                    return !(bool)field.GetValue(effect);
                }
                catch
                {
                }
            }

            // Conservative fallback: when this Valheim build does not expose
            // a debuff flag, only allow effect names that are clearly buffs.
            // Unknown effects are skipped rather than risking Wet/Poison/etc.
            string internalName = GetInternalName(effect);
            string lower = internalName.ToLowerInvariant();

            if (lower.Contains("rested") ||
                lower.Contains("corpsrun") ||
                lower.Contains("corpserun") ||
                lower.StartsWith("gp_") ||
                lower.Contains("eikthyr") ||
                lower.Contains("elder") ||
                lower.Contains("bonemass") ||
                lower.Contains("moder") ||
                lower.Contains("yagluth") ||
                lower.Contains("queen") ||
                lower.Contains("fader") ||
                lower.Contains("power") ||
                lower.Contains("potion") ||
                lower.Contains("tasty") ||
                lower.Contains("barleywine") ||
                lower.Contains("mead"))
            {
                return true;
            }

            return false;
        }

        private static string GetInternalName(object effect)
        {
            try
            {
                UnityEngine.Object unityObject = effect as UnityEngine.Object;
                if (unityObject != null && !string.IsNullOrEmpty(unityObject.name))
                    return unityObject.name;

                FieldInfo nameField =
                    ReflectionUtil.FindInstanceField(effect.GetType(), "m_name");

                if (nameField != null)
                {
                    string value = nameField.GetValue(effect) as string;
                    if (!string.IsNullOrEmpty(value))
                        return value.TrimStart('$');
                }
            }
            catch
            {
            }

            return effect.GetType().Name;
        }

        private static string GetReadableFallback(object effect)
        {
            string name = GetInternalName(effect);

            if (string.IsNullOrEmpty(name))
                return ModLocalization.T("effects.positive");

            if (name.StartsWith("SE_", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(3);

            if (name.StartsWith("GP_", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(3);

            return name.Replace("_", " ");
        }

        private static string GetStableEffectKey(object effect, string label)
        {
            try
            {
                UnityEngine.Object unityObject = effect as UnityEngine.Object;
                if (unityObject != null && !string.IsNullOrEmpty(unityObject.name))
                    return unityObject.name;
            }
            catch
            {
            }

            return effect.GetType().FullName + "|" + label;
        }
    }
}
