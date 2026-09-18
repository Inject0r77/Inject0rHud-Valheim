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
        private void DiagLine(ref float y, float x, float w, string left, string right)
        {
            GUI.Label(new Rect(x, y, w * 0.62f, 19f), left, _settingsText);
            GUI.Label(new Rect(x + w * 0.62f, y, w * 0.38f, 19f), right, _settingsValue);
            y += 21f;
        }

        private string ReadValheimVersion()
        {
            try
            {
                Type t = typeof(Player).Assembly.GetType("Version", false);
                if (t != null)
                {
                    MethodInfo m = t.GetMethod(
                        "GetVersionString",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null);

                    if (m != null)
                    {
                        object value = m.Invoke(null, null);
                        if (value != null)
                            return value.ToString();
                    }
                }
            }
            catch
            {
            }

            return "Unknown";
        }

        private void SyncAfterProfile(ModConfig cfg)
        {
            _uiMessage = _profiles != null ? _profiles.LastMessage : string.Empty;
            cfg.Save();
        }

        // -----------------------------------------------------------------
        // Reset helpers
        // -----------------------------------------------------------------

        private static void ResetLayout(ModConfig c)
        {
            c.PosX.Value = 20;
            c.PosY.Value = 220;
            c.Scale.Value = 0.90f;
            c.FontSize.Value = 12;
            c.PanelWidth.Value = 280;
            c.BackgroundOpacity.Value = 0.60f;
            c.ShowSectionHeaders.Value = true;
            c.SeparateBlocks.Value = false;
            c.SnapEnabled.Value = true;
            c.SnapDistance.Value = 12;
            c.TimersPosX.Value = 20;
            c.TimersPosY.Value = 220;
            c.TimersWidth.Value = 250;
            c.TimersScale.Value = c.Scale.Value;
            c.DurabilityPosX.Value = 20;
            c.DurabilityPosY.Value = 330;
            c.DurabilityWidth.Value = 280;
            c.DurabilityScale.Value = c.Scale.Value;
        }

        private static void ResetTimers(ModConfig c)
        {
            c.ShowRestedTimer.Value = true;
            c.ShowPowerCooldown.Value = false;
            c.TimersPosX.Value = 20;
            c.TimersPosY.Value = 220;
            c.TimersWidth.Value = 250;
            c.TimersScale.Value = c.Scale.Value;
        }

        private static void ResetDurability(ModConfig c)
        {
            c.ShowDurability.Value = true;
            c.ShowItemIcons.Value = true;
            c.DurabilityDisplay.Value = DurabilityValueMode.Units;
            c.SmartDurability.Value = SmartDurabilityMode.Off;
            c.SmartDurabilityThreshold.Value = 35;
            c.WarningPercent.Value = 25;
            c.CriticalPercent.Value = 10;
            c.DurabilityScale.Value = c.Scale.Value;
        }

        private static void ResetFps(ModConfig c)
        {
            c.ShowFpsWidget.Value = false;
            c.FpsShowValue.Value = true;
            c.FpsShowLabel.Value = true;
            c.FpsOpacity.Value = 0.72f;
            c.FpsPosX.Value = 20;
            c.FpsPosY.Value = 80;
        }

        private static void ResetPing(ModConfig c)
        {
            c.ShowPingWidget.Value = false;
            c.PingShowValue.Value = true;
            c.PingShowLabel.Value = true;
            c.PingOpacity.Value = 0.72f;
            c.PingPosX.Value = 100;
            c.PingPosY.Value = 80;
        }

        private static void ResetShip(ModConfig c)
        {
            c.ShowShipWidget.Value = true;
            c.ShipShowHealth.Value = true;
            c.ShipShowSpeed.Value = true;
            c.ShipShowWind.Value = true;
            c.ShipShowSail.Value = true;
            c.ShipOpacity.Value = 0.72f;
            c.ShipPosX.Value = 20;
            c.ShipPosY.Value = 155;
        }

        private static void ResetTime(ModConfig c)
        {
            c.ShowTimeWidget.Value = true;
            c.TimeShowDay.Value = true;
            c.TimeShowClock.Value = true;
            c.TimeShowSunEvent.Value = true;
            c.TimeOpacity.Value = 0.72f;
            c.TimePosX.Value = 180;
            c.TimePosY.Value = 80;
        }

        private static void ResetWorldTimers(ModConfig c)
        {
            c.ShowWorldHoverTimers.Value = true;
            c.ShowPickableHoverTimers.Value = true;
            c.ShowPlantHoverTimers.Value = true;
            c.ShowBeehiveHoverTimers.Value = true;
            c.ShowFermenterHoverTimers.Value = true;
            c.ShowProductionHoverTimers.Value = true;
            c.ShowProductionOnWholeStation.Value = true;
            c.PickableHoverOpacity.Value = 0.95f;
            c.PlantHoverOpacity.Value = 0.95f;
            c.BeehiveHoverOpacity.Value = 0.95f;
            c.FermenterHoverOpacity.Value = 0.95f;
            c.ProductionHoverOpacity.Value = 0.95f;
        }

        // -----------------------------------------------------------------
        // Small settings helpers
        // -----------------------------------------------------------------

        private void DrawSection(ref float y, float x, float w, string text)
        {
            GUI.Label(new Rect(x, y, w, 18f), text, _settingsHeader);
            y += 22f;
        }

        private float Slider(ref float y, float x, float w, string label, float value, float min, float max, string valueText)
        {
            const float valueW = 70f;
            float labelW = Mathf.Max(40f, w - valueW - 6f);
            float labelH = Mathf.Max(
                18f,
                _settingsText.CalcHeight(new GUIContent(label ?? string.Empty), labelW));

            GUI.Label(
                new Rect(x, y, labelW, labelH),
                label ?? string.Empty,
                _settingsText);

            GUI.Label(
                new Rect(x + w - valueW, y, valueW, 18f),
                valueText ?? string.Empty,
                _settingsValue);

            float sliderY = y + labelH + 4f;

            float result = GUI.HorizontalSlider(
                new Rect(x, sliderY, w, 16f),
                value,
                min,
                max);

            y = sliderY + 20f;
            return Mathf.Clamp(result, min, max);
        }

        private bool Toggle(ref float y, float x, float w, bool value, string label)
        {
            GUIStyle style = _settingsToggle ?? GUI.skin.toggle;
            float rowHeight = Mathf.Max(
                24f,
                style.CalcHeight(new GUIContent(label ?? string.Empty), Mathf.Max(40f, w)));

            bool result = GUI.Toggle(
                new Rect(x, y, w, rowHeight),
                value,
                label ?? string.Empty,
                style);

            y += rowHeight + 4f;
            return result;
        }

        private bool Button(ref float y, float x, float w, string label)
        {
            bool clicked = GUI.Button(new Rect(x, y, w, 26f), label);
            y += 31f;
            return clicked;
        }

        private static DurabilityValueMode NextDurabilityMode(DurabilityValueMode value)
        {
            if (value == DurabilityValueMode.Units) return DurabilityValueMode.Percent;
            if (value == DurabilityValueMode.Percent) return DurabilityValueMode.Both;
            return DurabilityValueMode.Units;
        }

        private static SmartDurabilityMode NextSmartMode(SmartDurabilityMode value)
        {
            if (value == SmartDurabilityMode.Off) return SmartDurabilityMode.BelowThreshold;
            if (value == SmartDurabilityMode.BelowThreshold) return SmartDurabilityMode.CurrentItemOnly;
            return SmartDurabilityMode.Off;
        }
    }
}
