using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using Inject0rHUD.Config;
using Inject0rHUD.Localization;
using Inject0rHUD.Models;
using Inject0rHUD.Profiles;
using Inject0rHUD.Util;
using UnityEngine;

namespace Inject0rHUD.UI
{
    internal sealed partial class HudRenderer
    {
        private void DrawSettingsPanel(ModConfig cfg, Rect rect)
        {
            Color old = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.96f);
            GUI.DrawTexture(rect, _background);
            GUI.color = old;

            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), _editorBorder);

            float x = rect.x + 14f;
            float y = rect.y + 10f;
            float w = rect.width - 28f;

            GUI.Label(new Rect(x, y, w, 24f), ModLocalization.T("settings.title"), _settingsHeader);
            y += 31f;
            DrawTabs(ref y, x, w);

            float footerHeight = 52f;
            Rect body = new Rect(rect.x + 8f, y, rect.width - 16f, Mathf.Max(80f, rect.yMax - footerHeight - y));
            int tabIndex = Mathf.Clamp((int)_tab, 0, _settingsScroll.Length - 1);
            float contentHeight = GetSettingsContentHeight(_tab);
            Rect view = new Rect(0f, 0f, Mathf.Max(100f, body.width - 20f), contentHeight);
            _settingsScroll[tabIndex] = GUI.BeginScrollView(body, _settingsScroll[tabIndex], view);

            float contentX = 8f;
            float contentY = 5f;
            float contentW = Mathf.Max(80f, view.width - 16f);

            switch (_tab)
            {
                case SettingsTab.Durability: DrawDurabilitySettings(ref contentY, contentX, contentW, cfg); break;
                case SettingsTab.Widgets: DrawWidgetSettings(ref contentY, contentX, contentW, cfg); break;
                case SettingsTab.Profiles: DrawProfileSettings(ref contentY, contentX, contentW, cfg); break;
                case SettingsTab.Diagnostics: DrawDiagnostics(ref contentY, contentX, contentW, cfg); break;
                case SettingsTab.Language: DrawLanguageSettings(ref contentY, contentX, contentW, cfg); break;
                default: DrawLayoutSettings(ref contentY, contentX, contentW, cfg); break;
            }

            GUI.EndScrollView();

            string msg = !string.IsNullOrEmpty(_uiMessage) ? _uiMessage : (_profiles != null ? _profiles.LastMessage : string.Empty);
            if (!string.IsNullOrEmpty(msg))
                GUI.Label(new Rect(x, rect.yMax - 46f, w, 18f), FitText(_settingsSmall, msg, w), _settingsSmall);

            GUI.Label(new Rect(x, rect.yMax - 25f, w, 18f), ModLocalization.T("settings.footer"), _settingsSmall);
        }

        private static float GetSettingsContentHeight(SettingsTab tab)
        {
            switch (tab)
            {
                case SettingsTab.Durability: return 650f;
                case SettingsTab.Widgets: return 1320f;
                case SettingsTab.Profiles: return 650f;
                case SettingsTab.Diagnostics: return 560f;
                case SettingsTab.Language: return 430f;
                default: return 620f;
            }
        }

        private void DrawTabs(ref float y, float x, float width)
        {
            string[] names =
            {
                ModLocalization.T("tab.layout"),
                ModLocalization.T("tab.durability"),
                ModLocalization.T("tab.widgets"),
                ModLocalization.T("tab.profiles"),
                ModLocalization.T("tab.diagnostics"),
                ModLocalization.T("tab.language")
            };

            float gap = 3f;
            float bw = (width - gap * (names.Length - 1)) / names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                string label = FitText(GUI.skin.button, names[i], bw - 6f);
                if (GUI.Button(new Rect(x + i * (bw + gap), y, bw, 25f), label)) _tab = (SettingsTab)i;
            }
            y += 32f;
        }

        private void DrawLayoutSettings(ref float y, float x, float w, ModConfig cfg)
        {
            DrawSection(ref y, x, w, ModLocalization.T("section.layout"));
            cfg.BackgroundOpacity.Value = Slider(ref y, x, w, ModLocalization.T("layout.background_opacity"), cfg.BackgroundOpacity.Value, 0f, 1f, Mathf.RoundToInt(cfg.BackgroundOpacity.Value * 100f) + "%");
            cfg.Scale.Value = Slider(ref y, x, w, ModLocalization.T("layout.scale"), cfg.Scale.Value, 0.70f, 1.60f, Mathf.RoundToInt(cfg.Scale.Value * 100f) + "%");
            float fs = Slider(ref y, x, w, ModLocalization.T("layout.text_size"), cfg.FontSize.Value, 10f, 24f, cfg.FontSize.Value.ToString());
            cfg.FontSize.Value = Mathf.Clamp(Mathf.RoundToInt(fs), 10, 24);
            cfg.ShowSectionHeaders.Value = Toggle(ref y, x, w, cfg.ShowSectionHeaders.Value, ModLocalization.T("layout.section_headers"));
            cfg.SeparateBlocks.Value = Toggle(ref y, x, w, cfg.SeparateBlocks.Value, ModLocalization.T("layout.separate_blocks"));
            cfg.SnapEnabled.Value = Toggle(ref y, x, w, cfg.SnapEnabled.Value, ModLocalization.T("layout.snap"));
            GUI.enabled = cfg.SnapEnabled.Value;
            float snap = Slider(ref y, x, w, ModLocalization.T("layout.snap_distance"), cfg.SnapDistance.Value, 4f, 40f, cfg.SnapDistance.Value + " px");
            cfg.SnapDistance.Value = Mathf.RoundToInt(snap);
            GUI.enabled = true;
            cfg.CompactInCombat.Value = Toggle(ref y, x, w, cfg.CompactInCombat.Value, ModLocalization.T("layout.compact_combat"));
            y += 6f;
            if (Button(ref y, x, w, ModLocalization.T("layout.reset"))) ResetLayout(cfg);
        }

        private void DrawDurabilitySettings(ref float y, float x, float w, ModConfig cfg)
        {
            DrawSection(ref y, x, w, ModLocalization.T("section.durability"));
            cfg.ShowDurability.Value = Toggle(ref y, x, w, cfg.ShowDurability.Value, ModLocalization.T("durability.show"));
            GUI.enabled = cfg.ShowDurability.Value;
            cfg.ShowItemIcons.Value = Toggle(ref y, x, w, cfg.ShowItemIcons.Value, ModLocalization.T("durability.icons"));

            string valueMode;
            switch (cfg.DurabilityDisplay.Value)
            {
                case DurabilityValueMode.Percent: valueMode = ModLocalization.T("durability.percent"); break;
                case DurabilityValueMode.Both: valueMode = ModLocalization.T("durability.both"); break;
                default: valueMode = ModLocalization.T("durability.units"); break;
            }
            if (Button(ref y, x, w, ModLocalization.T("durability.format", valueMode))) cfg.DurabilityDisplay.Value = NextDurabilityMode(cfg.DurabilityDisplay.Value);

            string smart;
            switch (cfg.SmartDurability.Value)
            {
                case SmartDurabilityMode.BelowThreshold: smart = ModLocalization.T("durability.smart_below"); break;
                case SmartDurabilityMode.CurrentItemOnly: smart = ModLocalization.T("durability.smart_current"); break;
                default: smart = ModLocalization.T("durability.smart_off"); break;
            }
            if (Button(ref y, x, w, ModLocalization.T("durability.smart", smart))) cfg.SmartDurability.Value = NextSmartMode(cfg.SmartDurability.Value);

            if (cfg.SmartDurability.Value == SmartDurabilityMode.BelowThreshold)
            {
                float t = Slider(ref y, x, w, ModLocalization.T("durability.show_below"), cfg.SmartDurabilityThreshold.Value, 1f, 99f, cfg.SmartDurabilityThreshold.Value + "%");
                cfg.SmartDurabilityThreshold.Value = Mathf.RoundToInt(t);
            }

            float warn = Slider(ref y, x, w, ModLocalization.T("durability.warning"), cfg.WarningPercent.Value, 1f, 99f, cfg.WarningPercent.Value + "%");
            float crit = Slider(ref y, x, w, ModLocalization.T("durability.critical"), cfg.CriticalPercent.Value, 1f, 99f, cfg.CriticalPercent.Value + "%");
            cfg.WarningPercent.Value = Mathf.RoundToInt(warn);
            cfg.CriticalPercent.Value = Mathf.RoundToInt(crit);
            GUI.enabled = true;
            y += 6f;
            if (Button(ref y, x, w, ModLocalization.T("durability.reset"))) ResetDurability(cfg);
        }
    }
}
