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
    internal sealed partial class HudRenderer : IDisposable
    {
        private enum EditTarget
        {
            None,
            Unified,
            Timers,
            Durability,
            Fps,
            Ping,
            Ship,
            Time
        }

        private enum ResizeMode
        {
            None,
            Move,
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        private enum SettingsTab
        {
            Layout,
            Durability,
            Widgets,
            Profiles,
            Diagnostics,
            Language
        }

        private readonly ProfileService _profiles;

        private Texture2D _background;
        private Texture2D _barBack;
        private Texture2D _barGood;
        private Texture2D _barWarning;
        private Texture2D _barCritical;
        private Texture2D _accent;
        private Texture2D _editorBorder;
        private Texture2D _editorHandle;

        private GUIStyle _text;
        private GUIStyle _value;
        private GUIStyle _header;
        private GUIStyle _editorLabel;
        private GUIStyle _settingsText;
        private GUIStyle _settingsHeader;
        private GUIStyle _settingsSmall;
        private GUIStyle _settingsValue;
        private GUIStyle _widgetValue;
        private GUIStyle _widgetLabel;
        private GUIStyle _settingsToggle;

        private readonly Vector2[] _settingsScroll = new Vector2[6];

        private bool _editSession;
        private SettingsTab _tab = SettingsTab.Layout;
        private string _profileName = "My Profile";
        private string _profileCode = string.Empty;
        private string _uiMessage = string.Empty;

        private EditTarget _target;
        private ResizeMode _resizeMode;
        private Vector2 _dragStart;
        private Rect _dragRectStart;
        private float _dragScaleStart;

        internal HudRenderer(ProfileService profiles)
        {
            _profiles = profiles;
        }

        internal void BeginEdit(ModConfig cfg)
        {
            _editSession = true;
            _target = EditTarget.None;
            _resizeMode = ResizeMode.None;
        }

        internal void EndEdit(ModConfig cfg)
        {
            _editSession = false;
            _target = EditTarget.None;
            _resizeMode = ResizeMode.None;
            cfg.Save();
        }

        internal void Draw(HudSnapshot snapshot, ModConfig cfg, bool editMode)
        {
            if (snapshot == null || cfg == null || Event.current == null)
                return;

            if (editMode && !_editSession)
                BeginEdit(cfg);

            EnsureResources(cfg);

            Rect unified = BuildUnifiedRect(snapshot, cfg);
            Rect timers = BuildTimersRect(snapshot, cfg);
            Rect durability = BuildDurabilityRect(snapshot, cfg);
            Rect fps = BuildWidgetRect(cfg.FpsPosX.Value, cfg.FpsPosY.Value, cfg.Scale.Value);
            Rect ping = BuildWidgetRect(cfg.PingPosX.Value, cfg.PingPosY.Value, cfg.Scale.Value);
            Rect ship = BuildShipWidgetRect(cfg);
            Rect time = BuildTimeWidgetRect(cfg);
            Rect settings = editMode ? BuildSettingsRect(cfg.SeparateBlocks.Value ? durability : unified) : Rect.zero;

            if (editMode)
            {
                HandleEditorInput(
                    snapshot, cfg, settings,
                    ref unified, ref timers, ref durability, ref fps, ref ping, ref ship, ref time);

                // Rebuild after input changed config.
                unified = BuildUnifiedRect(snapshot, cfg);
                timers = BuildTimersRect(snapshot, cfg);
                durability = BuildDurabilityRect(snapshot, cfg);
                fps = BuildWidgetRect(cfg.FpsPosX.Value, cfg.FpsPosY.Value, cfg.Scale.Value);
                ping = BuildWidgetRect(cfg.PingPosX.Value, cfg.PingPosY.Value, cfg.Scale.Value);
                ship = BuildShipWidgetRect(cfg);
                time = BuildTimeWidgetRect(cfg);
                settings = BuildSettingsRect(cfg.SeparateBlocks.Value ? durability : unified);

                DrawSettingsPanel(cfg, settings);
            }

            if (Event.current.type != EventType.Repaint)
                return;

            if (cfg.SeparateBlocks.Value)
            {
                if (snapshot.Timers.Count > 0 || editMode)
                    DrawTimersPanel(snapshot, cfg, timers, editMode);

                if (snapshot.Durability.Count > 0 || editMode)
                    DrawDurabilityPanel(snapshot, cfg, durability, editMode);
            }
            else
            {
                if (snapshot.Timers.Count > 0 || snapshot.Durability.Count > 0 || editMode)
                    DrawUnifiedPanel(snapshot, cfg, unified, editMode);
            }

            EnsureHudStyles(cfg, cfg.Scale.Value);

            if (cfg.ShowFpsWidget.Value || editMode)
                DrawMetricWidget(fps, snapshot.Fps, "FPS", cfg.FpsShowValue.Value, cfg.FpsShowLabel.Value, cfg.FpsOpacity.Value, editMode);

            if (cfg.ShowPingWidget.Value || editMode)
                DrawMetricWidget(ping, snapshot.PingMs, "PING", cfg.PingShowValue.Value, cfg.PingShowLabel.Value, cfg.PingOpacity.Value, editMode);

            if ((cfg.ShowShipWidget.Value && snapshot.Ship.Active) || editMode)
                DrawShipWidget(ship, snapshot.Ship, cfg, editMode);

            if ((cfg.ShowTimeWidget.Value && snapshot.WorldTime.Available) || editMode)
                DrawTimeWidget(time, snapshot.WorldTime, cfg, editMode);

            if (editMode)
            {
                if (cfg.SeparateBlocks.Value)
                {
                    DrawEditorChrome(timers, ModLocalization.T("hud.timers"));
                    DrawEditorChrome(durability, ModLocalization.T("hud.durability"));
                }
                else
                {
                    DrawEditorChrome(unified, "HUD");
                }

                DrawEditorChrome(fps, "FPS");
                DrawEditorChrome(ping, "PING");
                DrawEditorChrome(ship, ModLocalization.T("ship.title"));
                DrawEditorChrome(time, ModLocalization.T("time.title"));
            }
        }

        // -----------------------------------------------------------------
        // Rendering
        // -----------------------------------------------------------------

        private void DrawUnifiedPanel(HudSnapshot snapshot, ModConfig cfg, Rect panel, bool editMode)
        {
            EnsureHudStyles(cfg, cfg.Scale.Value);
            DrawPanelBackground(panel, cfg.BackgroundOpacity.Value);

            float scale = cfg.Scale.Value;
            float pad = 8f * scale;
            float headerH = 20f * scale;
            float rowH = 23f * scale;
            float barH = Mathf.Max(2f, 3f * scale);
            float gap = 5f * scale;
            float y = panel.y + pad;

            if (snapshot.Timers.Count == 0 && snapshot.Durability.Count == 0 && editMode)
            {
                GUI.Label(new Rect(panel.x + pad, y, panel.width - pad * 2f, 24f * scale), "Inject0r HUD", _header);
                GUI.Label(new Rect(panel.x + pad, y + 24f * scale, panel.width - pad * 2f, 24f * scale),
                    ModLocalization.T("hud.drag_hint"), _text);
                return;
            }

            if (snapshot.Timers.Count > 0)
            {
                if (cfg.ShowSectionHeaders.Value)
                {
                    DrawHeader(new Rect(panel.x + pad, y, panel.width - pad * 2f, headerH), ModLocalization.T("hud.timers"), scale);
                    y += headerH;
                }

                for (int i = 0; i < snapshot.Timers.Count; i++)
                {
                    DrawTimerRow(panel.x + pad, y, panel.width - pad * 2f, rowH, barH, snapshot.Timers[i], scale);
                    y += rowH;
                }
            }

            if (snapshot.Timers.Count > 0 && snapshot.Durability.Count > 0)
                y += gap;

            if (snapshot.Durability.Count > 0)
            {
                if (cfg.ShowSectionHeaders.Value)
                {
                    DrawHeader(new Rect(panel.x + pad, y, panel.width - pad * 2f, headerH), ModLocalization.T("hud.durability"), scale);
                    y += headerH;
                }

                for (int i = 0; i < snapshot.Durability.Count; i++)
                {
                    DrawDurabilityRow(panel.x + pad, y, panel.width - pad * 2f, rowH, barH, snapshot.Durability[i], scale, cfg);
                    y += rowH;
                }
            }
        }

        private void DrawTimersPanel(HudSnapshot snapshot, ModConfig cfg, Rect panel, bool editMode)
        {
            EnsureHudStyles(cfg, cfg.TimersScale.Value);
            DrawPanelBackground(panel, cfg.BackgroundOpacity.Value);

            float scale = cfg.TimersScale.Value;
            float pad = 8f * scale;
            float headerH = 20f * scale;
            float rowH = 23f * scale;
            float barH = Mathf.Max(2f, 3f * scale);
            float y = panel.y + pad;

            if (cfg.ShowSectionHeaders.Value)
            {
                DrawHeader(new Rect(panel.x + pad, y, panel.width - pad * 2f, headerH), ModLocalization.T("hud.timers"), scale);
                y += headerH;
            }

            if (snapshot.Timers.Count == 0 && editMode)
            {
                GUI.Label(new Rect(panel.x + pad, y, panel.width - pad * 2f, rowH), ModLocalization.T("hud.no_timers"), _text);
                return;
            }

            for (int i = 0; i < snapshot.Timers.Count; i++)
            {
                DrawTimerRow(panel.x + pad, y, panel.width - pad * 2f, rowH, barH, snapshot.Timers[i], scale);
                y += rowH;
            }
        }

        private void DrawDurabilityPanel(HudSnapshot snapshot, ModConfig cfg, Rect panel, bool editMode)
        {
            EnsureHudStyles(cfg, cfg.DurabilityScale.Value);
            DrawPanelBackground(panel, cfg.BackgroundOpacity.Value);

            float scale = cfg.DurabilityScale.Value;
            float pad = 8f * scale;
            float headerH = 20f * scale;
            float rowH = 23f * scale;
            float barH = Mathf.Max(2f, 3f * scale);
            float y = panel.y + pad;

            if (cfg.ShowSectionHeaders.Value)
            {
                DrawHeader(new Rect(panel.x + pad, y, panel.width - pad * 2f, headerH), ModLocalization.T("hud.durability"), scale);
                y += headerH;
            }

            if (snapshot.Durability.Count == 0 && editMode)
            {
                GUI.Label(new Rect(panel.x + pad, y, panel.width - pad * 2f, rowH), ModLocalization.T("hud.no_durability"), _text);
                return;
            }

            for (int i = 0; i < snapshot.Durability.Count; i++)
            {
                DrawDurabilityRow(panel.x + pad, y, panel.width - pad * 2f, rowH, barH, snapshot.Durability[i], scale, cfg);
                y += rowH;
            }
        }

        private void DrawMetricWidget(
            Rect rect,
            int value,
            string label,
            bool showValue,
            bool showLabel,
            float opacity,
            bool editMode)
        {
            if (!showValue && !showLabel && !editMode)
                return;

            DrawPanelBackground(rect, opacity);

            string valueText = label == "PING"
                ? (value > 0 ? value + " ms" : "—")
                : (value > 0 ? value.ToString() : "—");

            if (showValue || editMode)
                GUI.Label(new Rect(rect.x, rect.y + 7f, rect.width, 31f), valueText, _widgetValue);

            if (showLabel || editMode)
                GUI.Label(new Rect(rect.x, rect.y + rect.height - 25f, rect.width, 20f), label, _widgetLabel);
        }

        private void DrawShipWidget(Rect rect, ShipContextEntry ship, ModConfig cfg, bool editMode)
        {
            EnsureHudStyles(cfg, cfg.Scale.Value);
            DrawPanelBackground(rect, cfg.ShipOpacity.Value);

            float s = cfg.Scale.Value;
            float pad = 8f * s;
            float line = 20f * s;
            float y = rect.y + pad;
            float width = rect.width - pad * 2f;

            string title = ship != null && ship.Active && !string.IsNullOrEmpty(ship.Name)
                ? ship.Name
                : ModLocalization.T("ship.title");

            GUI.Label(new Rect(rect.x + pad, y, width, line), FitText(_header, title, width), _header);
            y += line;

            if (ship == null || !ship.Active)
            {
                if (editMode)
                    GUI.Label(new Rect(rect.x + pad, y, width, line), ModLocalization.T("ship.no_ship"), _text);
                return;
            }

            if (cfg.ShipShowHealth.Value)
            {
                string health = ship.HealthFraction >= 0f
                    ? Mathf.RoundToInt(ship.HealthFraction * 100f) + "%"
                    : "—";
                DrawContextLine(rect.x + pad, ref y, width, line, ModLocalization.T("ship.health"), health);
            }

            if (cfg.ShipShowSpeed.Value)
            {
                float knots = ship.SpeedMetersPerSecond * 1.9438445f;
                string speed = knots.ToString("0.0") + " kn  (" + ship.SpeedMetersPerSecond.ToString("0.0") + " m/s)";
                DrawContextLine(rect.x + pad, ref y, width, line, ModLocalization.T("ship.speed"), speed);
            }

            if (cfg.ShipShowWind.Value)
            {
                string wind = Mathf.RoundToInt(ship.WindIntensity * 100f) + "% • " + FormatWindBearing(ship.WindAngleDegrees);
                DrawContextLine(rect.x + pad, ref y, width, line, ModLocalization.T("ship.wind"), wind);
            }

            if (cfg.ShipShowSail.Value)
            {
                string state = FormatShipState(ship.Propulsion);
                if (ship.Propulsion == "Half" || ship.Propulsion == "Full")
                    state += " • " + Mathf.RoundToInt(ship.WindFactor * 100f) + "%";

                if (Mathf.Abs(ship.Rudder) >= 0.02f)
                    state += " • " + ModLocalization.T(ship.Rudder > 0f ? "ship.rudder_right" : "ship.rudder_left", Mathf.RoundToInt(Mathf.Abs(ship.Rudder) * 100f));

                DrawContextLine(rect.x + pad, ref y, width, line, ModLocalization.T("ship.state"), state);
            }
        }

        private void DrawTimeWidget(Rect rect, WorldTimeEntry worldTime, ModConfig cfg, bool editMode)
        {
            EnsureHudStyles(cfg, cfg.Scale.Value);
            DrawPanelBackground(rect, cfg.TimeOpacity.Value);

            float s = cfg.Scale.Value;
            float pad = 8f * s;
            float line = 20f * s;
            float y = rect.y + pad;
            float width = rect.width - pad * 2f;

            GUI.Label(new Rect(rect.x + pad, y, width, line), ModLocalization.T("time.title"), _header);
            y += line;

            if (worldTime == null || !worldTime.Available)
            {
                if (editMode)
                    GUI.Label(new Rect(rect.x + pad, y, width, line), ModLocalization.T("time.unavailable"), _text);
                return;
            }

            string dayClock = string.Empty;
            if (cfg.TimeShowDay.Value)
                dayClock = ModLocalization.T("time.day", worldTime.Day);
            if (cfg.TimeShowClock.Value)
            {
                if (dayClock.Length > 0) dayClock += " • ";
                dayClock += FormatWorldClock(worldTime.DayFraction);
            }
            if (dayClock.Length > 0)
            {
                GUI.Label(new Rect(rect.x + pad, y, width, line), FitText(_text, dayClock, width), _text);
                y += line;
            }

            if (cfg.TimeShowSunEvent.Value)
            {
                bool sunrise = string.Equals(worldTime.NextEvent, "sunrise", StringComparison.OrdinalIgnoreCase);
                string eventName = ModLocalization.T(sunrise ? "time.sunrise" : "time.sunset");
                string eventClock = sunrise ? "06:00" : "18:00";
                string text = eventName + " " + eventClock + " • " + FormatTime(worldTime.SecondsToEvent);
                GUI.Label(new Rect(rect.x + pad, y, width, line), FitText(_text, text, width), _text);
            }
        }

        private void DrawContextLine(float x, ref float y, float width, float lineHeight, string label, string value)
        {
            float labelWidth = Mathf.Min(width * 0.38f, Mathf.Max(58f, _text.CalcSize(new GUIContent(label)).x + 6f));
            GUI.Label(new Rect(x, y, labelWidth, lineHeight), FitText(_text, label, labelWidth), _text);
            GUI.Label(new Rect(x + labelWidth, y, width - labelWidth, lineHeight), FitText(_value, value, width - labelWidth), _value);
            y += lineHeight;
        }

        private static string FormatWorldClock(float dayFraction)
        {
            int totalMinutes = Mathf.FloorToInt(Mathf.Repeat(dayFraction, 1f) * 1440f + 0.5f) % 1440;
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            return hours.ToString("00") + ":" + minutes.ToString("00");
        }

        private static string FormatShipState(string propulsion)
        {
            switch (propulsion)
            {
                case "Back": return ModLocalization.T("ship.state_reverse");
                case "Slow": return ModLocalization.T("ship.state_oars");
                case "Half": return ModLocalization.T("ship.state_half");
                case "Full": return ModLocalization.T("ship.state_full");
                default: return ModLocalization.T("ship.state_idle");
            }
        }

        private static string FormatWindBearing(float angle)
        {
            float off = Mathf.Abs(angle);
            if (off < 7.5f) return ModLocalization.T("ship.wind_astern");
            if (off > 172.5f) return ModLocalization.T("ship.wind_ahead");

            string side = ModLocalization.T(angle > 0f ? "ship.wind_starboard" : "ship.wind_port");
            string part = ModLocalization.T(off < 67.5f ? "ship.wind_quarter" : off <= 112.5f ? "ship.wind_beam" : "ship.wind_bow");
            return side + " " + part;
        }

        private void DrawPanelBackground(Rect rect, float opacity)
        {
            Color old = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(opacity));
            GUI.DrawTexture(rect, _background);
            GUI.color = old;
        }

        private void DrawHeader(Rect rect, string text, float scale)
        {
            GUI.DrawTexture(
                new Rect(rect.x, rect.y + rect.height - 2f * scale, rect.width, Mathf.Max(1f, scale)),
                _accent);

            GUI.Label(rect, FitText(_header, text, rect.width), _header);
        }

        private void DrawTimerRow(float x, float y, float width, float rowHeight, float barHeight, TimerEntry entry, float scale)
        {
            string time = FormatTime(entry.RemainingSeconds);
            float valueWidth = Mathf.Max(60f * scale, _value.CalcSize(new GUIContent(time)).x + 5f * scale);
            float gap = 6f * scale;
            float leftWidth = Mathf.Max(1f, width - valueWidth - gap);

            GUI.Label(
                new Rect(x, y, leftWidth, rowHeight - barHeight - 2f * scale),
                FitText(_text, entry.Name, leftWidth),
                _text);

            GUI.Label(
                new Rect(x + width - valueWidth, y, valueWidth, rowHeight - barHeight - 2f * scale),
                time,
                _value);

            float fraction = entry.Fraction;
            if (fraction >= 0f)
            {
                Rect back = new Rect(x, y + rowHeight - barHeight, width, barHeight);
                GUI.DrawTexture(back, _barBack);
                GUI.DrawTexture(new Rect(back.x, back.y, back.width * fraction, back.height), _accent);
            }
        }

        private void DrawDurabilityRow(
            float x,
            float y,
            float width,
            float rowHeight,
            float barHeight,
            DurabilityEntry entry,
            float scale,
            ModConfig cfg)
        {
            int pct = Mathf.RoundToInt(entry.Fraction * 100f);
            string value;

            switch (cfg.DurabilityDisplay.Value)
            {
                case DurabilityValueMode.Percent:
                    value = pct + "%";
                    break;
                case DurabilityValueMode.Both:
                    value = pct + "% (" + entry.Current.ToString("0") + "/" + entry.Maximum.ToString("0") + ")";
                    break;
                default:
                    value = entry.Current.ToString("0") + "/" + entry.Maximum.ToString("0");
                    break;
            }

            float iconSize = cfg.ShowItemIcons.Value && entry.Icon != null
                ? Mathf.Min(18f * scale, rowHeight - barHeight - 3f * scale)
                : 0f;

            float iconGap = iconSize > 0f ? 5f * scale : 0f;
            float valueWidth = Mathf.Max(
                cfg.DurabilityDisplay.Value == DurabilityValueMode.Both ? 110f * scale : 62f * scale,
                _value.CalcSize(new GUIContent(value)).x + 5f * scale);

            float gap = 6f * scale;
            float leftX = x + iconSize + iconGap;
            float leftWidth = Mathf.Max(1f, width - iconSize - iconGap - valueWidth - gap);

            if (iconSize > 0f)
                DrawSprite(entry.Icon, new Rect(x, y + 1f * scale, iconSize, iconSize));

            GUI.Label(
                new Rect(leftX, y, leftWidth, rowHeight - barHeight - 2f * scale),
                FitText(_text, entry.Name, leftWidth),
                _text);

            GUI.Label(
                new Rect(x + width - valueWidth, y, valueWidth, rowHeight - barHeight - 2f * scale),
                value,
                _value);

            Rect back = new Rect(x, y + rowHeight - barHeight, width, barHeight);
            GUI.DrawTexture(back, _barBack);

            Texture2D fill = _barGood;
            if (pct <= cfg.CriticalPercent.Value) fill = _barCritical;
            else if (pct <= cfg.WarningPercent.Value) fill = _barWarning;

            GUI.DrawTexture(new Rect(back.x, back.y, back.width * entry.Fraction, back.height), fill);
        }

        private static void DrawSprite(Sprite sprite, Rect rect)
        {
            if (sprite == null || sprite.texture == null)
                return;

            try
            {
                Rect tr = sprite.textureRect;
                Texture tex = sprite.texture;

                Rect uv = new Rect(
                    tr.x / tex.width,
                    tr.y / tex.height,
                    tr.width / tex.width,
                    tr.height / tex.height);

                GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
            }
            catch
            {
                // Icon failure is cosmetic only.
            }
        }
    }
}
