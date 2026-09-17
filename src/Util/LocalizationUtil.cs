using System;
using System.Reflection;

namespace Inject0rHUD.Util
{
    internal static class LocalizationUtil
    {
        private static bool _resolved;
        private static Type _localizationType;
        private static FieldInfo _instanceField;
        private static PropertyInfo _instanceProperty;
        private static MethodInfo _localizeMethod;
        private static MethodInfo _getSelectedLanguageMethod;

        internal static string Localize(string token, string fallback = null)
        {
            if (string.IsNullOrEmpty(token))
                return fallback ?? string.Empty;

            try
            {
                object instance = GetInstance();
                if (instance != null && _localizeMethod != null)
                {
                    object value = _localizeMethod.Invoke(instance, new object[] { token });
                    string localized = value as string;
                    if (!string.IsNullOrEmpty(localized))
                        return localized;
                }
            }
            catch
            {
                // Cosmetic only.
            }

            string cleaned = token.TrimStart('$');
            return string.IsNullOrEmpty(cleaned) ? (fallback ?? string.Empty) : cleaned;
        }

        internal static bool TryLocalize(string token, out string localized)
        {
            localized = string.Empty;

            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                object instance = GetInstance();
                if (instance == null || _localizeMethod == null)
                    return false;

                object value = _localizeMethod.Invoke(instance, new object[] { token });
                string result = value as string;

                if (string.IsNullOrEmpty(result))
                    return false;

                string cleanedToken = token.TrimStart('$');

                if (string.Equals(result, token, StringComparison.Ordinal) ||
                    string.Equals(result, cleanedToken, StringComparison.Ordinal))
                {
                    return false;
                }

                localized = result;
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static string GetSelectedLanguage()
        {
            try
            {
                object instance = GetInstance();
                if (instance != null && _getSelectedLanguageMethod != null)
                {
                    string language = _getSelectedLanguageMethod.Invoke(instance, null) as string;
                    if (!string.IsNullOrEmpty(language))
                        return language;
                }
            }
            catch
            {
                // Fall through.
            }

            return "English";
        }

        internal static bool IsRussian()
        {
            return string.Equals(GetSelectedLanguage(), "Russian", StringComparison.OrdinalIgnoreCase);
        }

        private static object GetInstance()
        {
            Resolve();

            if (_localizationType == null)
                return null;

            if (_instanceField != null)
                return _instanceField.GetValue(null);

            if (_instanceProperty != null)
                return _instanceProperty.GetValue(null, null);

            return null;
        }

        private static void Resolve()
        {
            if (_resolved)
                return;

            _resolved = true;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length && _localizationType == null; i++)
            {
                try
                {
                    _localizationType = assemblies[i].GetType("Localization", false);
                }
                catch
                {
                    // Ignore assemblies that cannot be inspected.
                }
            }

            if (_localizationType == null)
                return;

            const BindingFlags staticFlags =
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            const BindingFlags instanceFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            _instanceField =
                _localizationType.GetField("instance", staticFlags) ??
                _localizationType.GetField("m_instance", staticFlags);

            _instanceProperty =
                _localizationType.GetProperty("instance", staticFlags) ??
                _localizationType.GetProperty("Instance", staticFlags);

            _localizeMethod = _localizationType.GetMethod(
                "Localize",
                instanceFlags,
                null,
                new[] { typeof(string) },
                null);

            _getSelectedLanguageMethod = _localizationType.GetMethod(
                "GetSelectedLanguage",
                instanceFlags,
                null,
                Type.EmptyTypes,
                null);
        }
    }
}
