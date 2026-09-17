using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Inject0rHUD.Util;

namespace Inject0rHUD.Localization
{
    internal static class ModLocalization
    {
        private static ConfigEntry<ModLanguage> _language;

        internal static void Bind(ConfigEntry<ModLanguage> language)
        {
            _language = language;
        }

        internal static ModLanguage ConfiguredLanguage =>
            _language != null ? _language.Value : ModLanguage.English;

        internal static ModLanguage EffectiveLanguage
        {
            get
            {
                ModLanguage configured = ConfiguredLanguage;
                return configured == ModLanguage.Auto
                    ? DetectValheimLanguage()
                    : configured;
            }
        }

        internal static string T(string key)
        {
            return T(key, Array.Empty<object>());
        }

        internal static string T(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            string value;
            Dictionary<string, string> selected = DictionaryFor(EffectiveLanguage);

            if (!selected.TryGetValue(key, out value) &&
                !EnglishTranslation.Values.TryGetValue(key, out value))
            {
                value = key;
            }

            if (args == null || args.Length == 0)
                return value;

            try
            {
                return string.Format(value, args);
            }
            catch
            {
                return value;
            }
        }

        internal static string DisplayName(ModLanguage language)
        {
            switch (language)
            {
                case ModLanguage.Russian:
                    return TFor(ModLanguage.Russian, "language.russian");
                case ModLanguage.Kazakh:
                    return TFor(ModLanguage.Kazakh, "language.kazakh");
                case ModLanguage.SimplifiedChinese:
                    return TFor(ModLanguage.SimplifiedChinese, "language.chinese");
                case ModLanguage.Auto:
                    return T("language.auto");
                default:
                    return "English";
            }
        }

        internal static ModLanguage Next(ModLanguage language)
        {
            switch (language)
            {
                case ModLanguage.English: return ModLanguage.Russian;
                case ModLanguage.Russian: return ModLanguage.Kazakh;
                case ModLanguage.Kazakh: return ModLanguage.SimplifiedChinese;
                case ModLanguage.SimplifiedChinese: return ModLanguage.Auto;
                default: return ModLanguage.English;
            }
        }

        internal static ModLanguage Previous(ModLanguage language)
        {
            switch (language)
            {
                case ModLanguage.English: return ModLanguage.Auto;
                case ModLanguage.Russian: return ModLanguage.English;
                case ModLanguage.Kazakh: return ModLanguage.Russian;
                case ModLanguage.SimplifiedChinese: return ModLanguage.Kazakh;
                default: return ModLanguage.SimplifiedChinese;
            }
        }

        private static string TFor(ModLanguage language, string key)
        {
            string value;
            Dictionary<string, string> selected = DictionaryFor(language);

            if (selected.TryGetValue(key, out value))
                return value;

            if (EnglishTranslation.Values.TryGetValue(key, out value))
                return value;

            return key;
        }

        private static Dictionary<string, string> DictionaryFor(ModLanguage language)
        {
            switch (language)
            {
                case ModLanguage.Russian:
                    return RussianTranslation.Values;
                case ModLanguage.Kazakh:
                    return KazakhTranslation.Values;
                case ModLanguage.SimplifiedChinese:
                    return SimplifiedChineseTranslation.Values;
                default:
                    return EnglishTranslation.Values;
            }
        }

        private static ModLanguage DetectValheimLanguage()
        {
            string language = LocalizationUtil.GetSelectedLanguage() ?? string.Empty;
            string lower = language.ToLowerInvariant();

            if (lower.Contains("russian") || lower.Contains("рус"))
                return ModLanguage.Russian;

            if (lower.Contains("chinese") ||
                lower.Contains("simplified") ||
                lower.Contains("简体") ||
                lower.Contains("中文"))
            {
                return ModLanguage.SimplifiedChinese;
            }

            // Valheim does not currently expose a Kazakh translation in the
            // standard language list, so Auto intentionally falls back to EN.
            return ModLanguage.English;
        }
    }
}
