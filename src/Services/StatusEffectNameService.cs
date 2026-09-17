using System;
using System.Collections;
using System.Reflection;
using Inject0rHUD.Localization;
using Inject0rHUD.Util;
using UnityEngine;

namespace Inject0rHUD.Services
{
    internal static class StatusEffectNameService
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags StaticFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        internal static string FromStatusEffect(object statusEffect, string fallback)
        {
            if (statusEffect == null)
                return fallback ?? string.Empty;

            try
            {
                FieldInfo nameField =
                    ReflectionUtil.FindInstanceField(statusEffect.GetType(), "m_name");

                if (nameField != null)
                {
                    string token = nameField.GetValue(statusEffect) as string;
                    string localized;

                    if (!string.IsNullOrEmpty(token) &&
                        LocalizationUtil.TryLocalize(token, out localized))
                    {
                        return localized;
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        string cleaned = token.TrimStart('$');
                        if (!string.IsNullOrEmpty(cleaned))
                            return cleaned;
                    }
                }
            }
            catch
            {
            }

            return fallback ?? string.Empty;
        }

        internal static string GuardianPower(string internalName)
        {
            if (string.IsNullOrEmpty(internalName))
                return ModLocalization.T("effects.forsaken");

            if (internalName.StartsWith("$", StringComparison.Ordinal))
            {
                string direct;
                if (LocalizationUtil.TryLocalize(internalName, out direct))
                    return direct;
            }

            object statusEffect = FindStatusEffect(internalName);
            if (statusEffect != null)
            {
                string resolved = FromStatusEffect(statusEffect, string.Empty);
                if (!string.IsNullOrEmpty(resolved))
                    return resolved;
            }

            string normalized = internalName;
            if (normalized.StartsWith("GP_", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(3);

            // Vanilla/mod localization packs commonly expose status-effect names
            // using one of these token shapes. They are only accepted if the
            // current Valheim Localization instance actually resolves them.
            string lower = normalized.ToLowerInvariant();
            string[] candidates =
            {
                "$se_" + lower + "_name",
                "$se_" + lower,
                "$" + lower + "_name",
                "$" + lower
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                string localized;
                if (LocalizationUtil.TryLocalize(candidates[i], out localized))
                    return localized;
            }

            string readable = normalized.Replace("_", " ");

            return ModLocalization.T("effects.power_prefix", readable);
        }

        private static object FindStatusEffect(string internalName)
        {
            try
            {
                Type objectDbType = typeof(Player).Assembly.GetType("ObjectDB", false);
                if (objectDbType == null)
                    return null;

                object instance = GetStaticMember(objectDbType, "instance") ??
                                  GetStaticMember(objectDbType, "m_instance");

                if (instance == null)
                    return null;

                FieldInfo effectsField =
                    ReflectionUtil.FindInstanceField(objectDbType, "m_StatusEffects") ??
                    ReflectionUtil.FindInstanceField(objectDbType, "m_statusEffects");

                if (effectsField == null)
                    return null;

                IEnumerable effects = effectsField.GetValue(instance) as IEnumerable;
                if (effects == null)
                    return null;

                foreach (object effect in effects)
                {
                    if (effect == null)
                        continue;

                    string objectName = string.Empty;

                    UnityEngine.Object unityObject = effect as UnityEngine.Object;
                    if (unityObject != null)
                        objectName = unityObject.name ?? string.Empty;

                    if (string.IsNullOrEmpty(objectName))
                    {
                        PropertyInfo property = effect.GetType().GetProperty(
                            "name",
                            InstanceFlags);

                        if (property != null)
                            objectName = property.GetValue(effect, null) as string ?? string.Empty;
                    }

                    if (string.Equals(objectName, internalName, StringComparison.OrdinalIgnoreCase))
                        return effect;
                }
            }
            catch
            {
            }

            return null;
        }

        private static object GetStaticMember(Type type, string name)
        {
            try
            {
                FieldInfo field = type.GetField(name, StaticFlags);
                if (field != null)
                    return field.GetValue(null);

                PropertyInfo property = type.GetProperty(name, StaticFlags);
                if (property != null)
                    return property.GetValue(null, null);
            }
            catch
            {
            }

            return null;
        }
    }
}
