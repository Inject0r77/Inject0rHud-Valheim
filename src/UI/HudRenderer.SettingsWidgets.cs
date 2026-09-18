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
        private void DrawWidgetSettings(ref float y, float x, float w, ModConfig cfg)
        {
            DrawSection(ref y, x, w, ModLocalization.T("section.hud_timers"));

            cfg.ShowRestedTimer.Value = Toggle(
                ref y, x, w,
                cfg.ShowRestedTimer.Value,
                ModLocalization.T("timers.positive"));

            if (Button(ref y, x, w, ModLocalization.T("timers.reset")))
                ResetTimers(cfg);

            y += 4f;
            DrawSection(ref y, x, w, "FPS");

            cfg.ShowFpsWidget.Value = Toggle(
                ref y, x, w,
                cfg.ShowFpsWidget.Value,
                ModLocalization.T("fps.show"));

            GUI.enabled = cfg.ShowFpsWidget.Value;
            cfg.FpsShowValue.Value = Toggle(ref y, x, w, cfg.FpsShowValue.Value, ModLocalization.T("fps.value"));
            cfg.FpsShowLabel.Value = Toggle(ref y, x, w, cfg.FpsShowLabel.Value, ModLocalization.T("fps.label"));
            cfg.FpsOpacity.Value = Slider(
                ref y, x, w,
                ModLocalization.T("fps.opacity"),
                cfg.FpsOpacity.Value, 0.10f, 1f,
                Mathf.RoundToInt(cfg.FpsOpacity.Value * 100f) + "%");
            GUI.enabled = true;

            if (Button(ref y, x, w, ModLocalization.T("fps.reset")))
                ResetFps(cfg);

            y += 5f;
            DrawSection(ref y, x, w, "PING");

            cfg.ShowPingWidget.Value = Toggle(
                ref y, x, w,
                cfg.ShowPingWidget.Value,
                ModLocalization.T("ping.show"));

            GUI.enabled = cfg.ShowPingWidget.Value;
            cfg.PingShowValue.Value = Toggle(ref y, x, w, cfg.PingShowValue.Value, ModLocalization.T("ping.value"));
            cfg.PingShowLabel.Value = Toggle(ref y, x, w, cfg.PingShowLabel.Value, ModLocalization.T("ping.label"));
            cfg.PingOpacity.Value = Slider(
                ref y, x, w,
                ModLocalization.T("ping.opacity"),
                cfg.PingOpacity.Value, 0.10f, 1f,
                Mathf.RoundToInt(cfg.PingOpacity.Value * 100f) + "%");
            GUI.enabled = true;

            if (Button(ref y, x, w, ModLocalization.T("ping.reset")))
                ResetPing(cfg);

            y += 5f;
            DrawSection(ref y, x, w, ModLocalization.T("section.ship_widget"));
            cfg.ShowShipWidget.Value = Toggle(ref y, x, w, cfg.ShowShipWidget.Value, ModLocalization.T("ship.show"));
            GUI.enabled = cfg.ShowShipWidget.Value;
            cfg.ShipShowHealth.Value = Toggle(ref y, x, w, cfg.ShipShowHealth.Value, ModLocalization.T("ship.show_health"));
            cfg.ShipShowSpeed.Value = Toggle(ref y, x, w, cfg.ShipShowSpeed.Value, ModLocalization.T("ship.show_speed"));
            cfg.ShipShowWind.Value = Toggle(ref y, x, w, cfg.ShipShowWind.Value, ModLocalization.T("ship.show_wind"));
            cfg.ShipShowSail.Value = Toggle(ref y, x, w, cfg.ShipShowSail.Value, ModLocalization.T("ship.show_sail"));
            cfg.ShipOpacity.Value = Slider(ref y, x, w, ModLocalization.T("ship.opacity"), cfg.ShipOpacity.Value, 0.10f, 1f, Mathf.RoundToInt(cfg.ShipOpacity.Value * 100f) + "%");
            GUI.enabled = true;
            if (Button(ref y, x, w, ModLocalization.T("ship.reset"))) ResetShip(cfg);

            y += 5f;
            DrawSection(ref y, x, w, ModLocalization.T("section.time_widget"));
            cfg.ShowTimeWidget.Value = Toggle(ref y, x, w, cfg.ShowTimeWidget.Value, ModLocalization.T("time.show"));
            GUI.enabled = cfg.ShowTimeWidget.Value;
            cfg.TimeShowDay.Value = Toggle(ref y, x, w, cfg.TimeShowDay.Value, ModLocalization.T("time.show_day"));
            cfg.TimeShowClock.Value = Toggle(ref y, x, w, cfg.TimeShowClock.Value, ModLocalization.T("time.show_clock"));
            cfg.TimeShowSunEvent.Value = Toggle(ref y, x, w, cfg.TimeShowSunEvent.Value, ModLocalization.T("time.show_sun"));
            cfg.TimeOpacity.Value = Slider(ref y, x, w, ModLocalization.T("time.opacity"), cfg.TimeOpacity.Value, 0.10f, 1f, Mathf.RoundToInt(cfg.TimeOpacity.Value * 100f) + "%");
            GUI.enabled = true;
            if (Button(ref y, x, w, ModLocalization.T("time.reset"))) ResetTime(cfg);

            y += 5f;
            DrawSection(ref y, x, w, ModLocalization.T("section.world_timers"));

            cfg.ShowWorldHoverTimers.Value = Toggle(
                ref y, x, w,
                cfg.ShowWorldHoverTimers.Value,
                ModLocalization.T("world.master"));

            GUI.enabled = cfg.ShowWorldHoverTimers.Value;

            cfg.ShowPickableHoverTimers.Value = Toggle(
                ref y, x, w,
                cfg.ShowPickableHoverTimers.Value,
                ModLocalization.T("world.pickables"));

            cfg.PickableHoverOpacity.Value = Slider(
                ref y, x, w,
                ModLocalization.T("world.pickable_opacity"),
                cfg.PickableHoverOpacity.Value, 0.20f, 1.0f,
                Mathf.RoundToInt(cfg.PickableHoverOpacity.Value * 100f) + "%");

            cfg.ShowPlantHoverTimers.Value = Toggle(
                ref y, x, w,
                cfg.ShowPlantHoverTimers.Value,
                ModLocalization.T("world.plants"));

            cfg.PlantHoverOpacity.Value = Slider(
                ref y, x, w,
                ModLocalization.T("world.plant_opacity"),
                cfg.PlantHoverOpacity.Value, 0.20f, 1.0f,
                Mathf.RoundToInt(cfg.PlantHoverOpacity.Value * 100f) + "%");

            cfg.ShowBeehiveHoverTimers.Value = Toggle(
                ref y, x, w,
                cfg.ShowBeehiveHoverTimers.Value,
                ModLocalization.T("world.beehives"));

            cfg.BeehiveHoverOpacity.Value = Slider(
                ref y, x, w,
                ModLocalization.T("world.beehive_opacity"),
                cfg.BeehiveHoverOpacity.Value, 0.20f, 1.0f,
                Mathf.RoundToInt(cfg.BeehiveHoverOpacity.Value * 100f) + "%");

            cfg.ShowFermenterHoverTimers.Value = Toggle(
                ref y, x, w,
                cfg.ShowFermenterHoverTimers.Value,
                ModLocalization.T("world.fermenters"));

            cfg.FermenterHoverOpacity.Value = Slider(
                ref y, x, w,
                ModLocalization.T("world.fermenter_opacity"),
                cfg.FermenterHoverOpacity.Value, 0.20f, 1.0f,
                Mathf.RoundToInt(cfg.FermenterHoverOpacity.Value * 100f) + "%");

            cfg.ShowProductionHoverTimers.Value = Toggle(
                ref y, x, w,
                cfg.ShowProductionHoverTimers.Value,
                ModLocalization.T("world.production"));

            GUI.enabled = cfg.ShowWorldHoverTimers.Value && cfg.ShowProductionHoverTimers.Value;
            cfg.ShowProductionOnWholeStation.Value = Toggle(
                ref y, x, w,
                cfg.ShowProductionOnWholeStation.Value,
                ModLocalization.T("world.production_whole_station"));

            cfg.ProductionHoverOpacity.Value = Slider(
                ref y, x, w,
                ModLocalization.T("world.production_opacity"),
                cfg.ProductionHoverOpacity.Value, 0.20f, 1.0f,
                Mathf.RoundToInt(cfg.ProductionHoverOpacity.Value * 100f) + "%");

            GUI.enabled = true;

            if (Button(ref y, x, w, ModLocalization.T("world.reset")))
                ResetWorldTimers(cfg);
        }

        private void DrawProfileSettings(ref float y, float x, float w, ModConfig cfg)
        {
            DrawSection(ref y, x, w, ModLocalization.T("section.profiles"));

            HudProfile current = _profiles != null ? _profiles.Current : null;
            string currentName = current != null ? current.Name : "—";

            GUI.Label(
                new Rect(x, y, w, 20f),
                ModLocalization.T("profiles.current", currentName),
                _settingsText);
            y += 25f;

            float gap = 5f;
            float bw = (w - gap * 2f) / 3f;

            if (GUI.Button(new Rect(x, y, bw, 25f), "◀"))
            {
                _profiles.Previous();
                SyncAfterProfile(cfg);
            }

            if (GUI.Button(new Rect(x + bw + gap, y, bw, 25f), ModLocalization.T("profiles.apply")))
            {
                _profiles.ApplyCurrent();
                SyncAfterProfile(cfg);
            }

            if (GUI.Button(new Rect(x + (bw + gap) * 2f, y, bw, 25f), "▶"))
            {
                _profiles.Next();
                SyncAfterProfile(cfg);
            }
            y += 32f;

            GUI.Label(new Rect(x, y, 95f, 20f), ModLocalization.T("profiles.name"), _settingsText);
            _profileName = GUI.TextField(new Rect(x + 95f, y, w - 95f, 22f), _profileName ?? string.Empty);
            y += 29f;

            bw = (w - gap) / 2f;

            if (GUI.Button(new Rect(x, y, bw, 25f), ModLocalization.T("profiles.new")))
            {
                _profiles.CreateCustom(_profileName);
                _profileName = _profiles.Current != null ? _profiles.Current.Name : _profileName;
                SyncAfterProfile(cfg);
            }

            if (GUI.Button(new Rect(x + bw + gap, y, bw, 25f), ModLocalization.T("profiles.save")))
            {
                _profiles.SaveCurrent();
            }
            y += 31f;

            if (GUI.Button(new Rect(x, y, bw, 25f), ModLocalization.T("profiles.rename")))
            {
                _profiles.RenameCurrent(_profileName);
            }

            GUI.enabled = current != null && !current.BuiltIn;
            if (GUI.Button(new Rect(x + bw + gap, y, bw, 25f), ModLocalization.T("profiles.delete")))
            {
                _profiles.DeleteCurrent();
                SyncAfterProfile(cfg);
            }
            GUI.enabled = true;
            y += 37f;

            DrawSection(ref y, x, w, ModLocalization.T("profiles.import_export"));

            if (GUI.Button(new Rect(x, y, bw, 25f), ModLocalization.T("profiles.export_clipboard")))
            {
                try
                {
                    string code = _profiles.ExportCurrent();
                    GUIUtility.systemCopyBuffer = code;
                    _profileCode = code;
                    _uiMessage = ModLocalization.T("profiles.copied");
                }
                catch (Exception ex)
                {
                    _uiMessage = ex.Message;
                }
            }

            if (GUI.Button(new Rect(x + bw + gap, y, bw, 25f), ModLocalization.T("profiles.paste_clipboard")))
            {
                _profileCode = GUIUtility.systemCopyBuffer ?? string.Empty;
            }
            y += 32f;

            GUI.Label(new Rect(x, y, w, 18f), ModLocalization.T("profiles.code"), _settingsText);
            y += 20f;

            _profileCode = GUI.TextField(
                new Rect(x, y, w, 42f),
                _profileCode ?? string.Empty);
            y += 48f;

            if (Button(ref y, x, w, ModLocalization.T("profiles.import")))
            {
                try
                {
                    _profiles.ImportCode(_profileCode);
                    _profileName = _profiles.Current != null ? _profiles.Current.Name : "Imported";
                    SyncAfterProfile(cfg);
                    _uiMessage = ModLocalization.T("profiles.imported");
                }
                catch (Exception ex)
                {
                    _uiMessage = ModLocalization.T("profiles.import_error", ex.Message);
                }
            }
        }

        private void DrawDiagnostics(ref float y, float x, float w, ModConfig cfg)
        {
            DrawSection(ref y, x, w, ModLocalization.T("section.compatibility"));

            DiagLine(ref y, x, w, "Inject0r HUD", Plugin.PluginVersion);
            DiagLine(ref y, x, w, "Unity", Application.unityVersion);
            DiagLine(ref y, x, w, "BepInEx", typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString());
            DiagLine(ref y, x, w, "Valheim", ReadValheimVersion());

            y += 8f;
            DrawSection(ref y, x, w, ModLocalization.T("section.modules"));

            DiagLine(ref y, x, w, "HUD", ModLocalization.T("diag.ok"));
            DiagLine(ref y, x, w, ModLocalization.T("diag.durability"),
                cfg.ShowDurability.Value ? ModLocalization.T("diag.on") : ModLocalization.T("diag.off"));
            DiagLine(ref y, x, w, ModLocalization.T("diag.world_hover"),
                cfg.ShowWorldHoverTimers.Value ? ModLocalization.T("diag.on") : ModLocalization.T("diag.off"));
            DiagLine(ref y, x, w, ModLocalization.T("diag.production"),
                cfg.ShowProductionHoverTimers.Value ? ModLocalization.T("diag.on") : ModLocalization.T("diag.off"));
            DiagLine(ref y, x, w, ModLocalization.T("diag.fps"),
                cfg.ShowFpsWidget.Value ? ModLocalization.T("diag.on") : ModLocalization.T("diag.off"));
            DiagLine(ref y, x, w, ModLocalization.T("diag.ping"),
                cfg.ShowPingWidget.Value ? ModLocalization.T("diag.on") : ModLocalization.T("diag.off"));
            DiagLine(ref y, x, w, ModLocalization.T("diag.client_only"), ModLocalization.T("diag.yes"));
            DiagLine(ref y, x, w, ModLocalization.T("diag.server_sync"), ModLocalization.T("diag.none"));

            y += 8f;
            GUI.Label(
                new Rect(x, y, w, 90f),
                ModLocalization.T("diag.note"),
                _settingsSmall);
        }

        private void DrawLanguageSettings(ref float y, float x, float w, ModConfig cfg)
        {
            DrawSection(ref y, x, w, ModLocalization.T("section.language"));

            GUI.Label(
                new Rect(x, y, w, 20f),
                ModLocalization.T("language.current"),
                _settingsText);
            y += 24f;

            const float arrowW = 42f;
            float centerW = Mathf.Max(120f, w - arrowW * 2f - 10f);

            if (GUI.Button(new Rect(x, y, arrowW, 28f), "◀"))
                cfg.InterfaceLanguage.Value = ModLocalization.Previous(cfg.InterfaceLanguage.Value);

            if (GUI.Button(
                new Rect(x + arrowW + 5f, y, centerW, 28f),
                ModLocalization.DisplayName(cfg.InterfaceLanguage.Value)))
            {
                cfg.InterfaceLanguage.Value = ModLocalization.Next(cfg.InterfaceLanguage.Value);
            }

            if (GUI.Button(new Rect(x + arrowW + 10f + centerW, y, arrowW, 28f), "▶"))
                cfg.InterfaceLanguage.Value = ModLocalization.Next(cfg.InterfaceLanguage.Value);

            y += 38f;

            float gap = 5f;
            float bw = (w - gap) / 2f;

            if (GUI.Button(new Rect(x, y, bw, 27f), "English"))
                cfg.InterfaceLanguage.Value = ModLanguage.English;

            if (GUI.Button(new Rect(x + bw + gap, y, bw, 27f), "Русский"))
                cfg.InterfaceLanguage.Value = ModLanguage.Russian;
            y += 33f;

            if (GUI.Button(new Rect(x, y, bw, 27f), "Қазақша"))
                cfg.InterfaceLanguage.Value = ModLanguage.Kazakh;

            if (GUI.Button(new Rect(x + bw + gap, y, bw, 27f), "简体中文"))
                cfg.InterfaceLanguage.Value = ModLanguage.SimplifiedChinese;
            y += 33f;

            if (Button(ref y, x, w, ModLocalization.T("language.auto")))
                cfg.InterfaceLanguage.Value = ModLanguage.Auto;

            y += 8f;

            GUI.Label(
                new Rect(x, y, w, 64f),
                ModLocalization.T("language.default_note"),
                _settingsSmall);
            y += 70f;

            GUI.Label(
                new Rect(x, y, w, 22f),
                ModLocalization.T("language.valheim", LocalizationUtil.GetSelectedLanguage()),
                _settingsSmall);
        }
    }
}
