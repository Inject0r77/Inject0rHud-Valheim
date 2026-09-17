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
        private void EnsureResources(ModConfig cfg)
        {
            if (_background == null)
            {
                _background = MakeTexture(new Color(0.035f, 0.04f, 0.045f, 1f));
                _barBack = MakeTexture(new Color(0.12f, 0.13f, 0.14f, 1f));
                _barGood = MakeTexture(new Color(0.35f, 0.72f, 0.42f, 1f));
                _barWarning = MakeTexture(new Color(0.95f, 0.68f, 0.18f, 1f));
                _barCritical = MakeTexture(new Color(0.88f, 0.25f, 0.22f, 1f));
                _accent = MakeTexture(new Color(0.48f, 0.82f, 0.55f, 1f));
                _editorBorder = MakeTexture(new Color(0.65f, 0.95f, 0.70f, 1f));
                _editorHandle = MakeTexture(new Color(0.88f, 1.0f, 0.90f, 1f));
            }

            int font = Mathf.Max(10, Mathf.RoundToInt(cfg.FontSize.Value * cfg.Scale.Value));

            if (_text == null || _text.fontSize != font)
            {
                _text = new GUIStyle(GUI.skin.label)
                {
                    fontSize = font,
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    wordWrap = false,
                    normal = { textColor = new Color(0.93f, 0.94f, 0.94f, 1f) }
                };

                _value = new GUIStyle(_text)
                {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(0.79f, 0.86f, 0.81f, 1f) }
                };

                _header = new GUIStyle(_text)
                {
                    fontSize = Mathf.Max(font - 1, 9),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = new Color(0.55f, 0.88f, 0.61f, 1f) }
                };

                _editorLabel = new GUIStyle(_text)
                {
                    fontSize = Mathf.Max(font - 2, 9),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.72f, 1.0f, 0.78f, 1f) }
                };

                _widgetValue = new GUIStyle(_text)
                {
                    fontSize = Mathf.Max(font + 4, 14),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };

                _widgetLabel = new GUIStyle(_text)
                {
                    fontSize = Mathf.Max(font - 1, 9),
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.55f, 0.88f, 0.61f, 1f) }
                };
            }

            if (_settingsText == null)
            {
                _settingsText = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    wordWrap = true,
                    normal = { textColor = new Color(0.93f, 0.94f, 0.94f, 1f) }
                };

                _settingsHeader = new GUIStyle(_settingsText)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.55f, 0.88f, 0.61f, 1f) }
                };

                _settingsSmall = new GUIStyle(_settingsText)
                {
                    fontSize = 11,
                    wordWrap = true,
                    normal = { textColor = new Color(0.78f, 0.82f, 0.80f, 1f) }
                };

                _settingsValue = new GUIStyle(_settingsText)
                {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(0.72f, 1.0f, 0.78f, 1f) }
                };

                _settingsToggle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 13,
                    wordWrap = true,
                    alignment = TextAnchor.MiddleLeft
                };

                _settingsToggle.normal.textColor = new Color(0.93f, 0.94f, 0.94f, 1f);
                _settingsToggle.hover.textColor = Color.white;
                _settingsToggle.active.textColor = Color.white;
                _settingsToggle.focused.textColor = Color.white;
            }
        }

        private static string FitText(GUIStyle style, string text, float maxWidth)
        {
            if (style == null || string.IsNullOrEmpty(text) || maxWidth <= 2f)
                return string.Empty;

            if (style.CalcSize(new GUIContent(text)).x <= maxWidth)
                return text;

            const string ellipsis = "…";
            float ew = style.CalcSize(new GUIContent(ellipsis)).x;
            if (ew > maxWidth)
                return string.Empty;

            int low = 0;
            int high = text.Length;

            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                string candidate = text.Substring(0, mid) + ellipsis;

                if (style.CalcSize(new GUIContent(candidate)).x <= maxWidth)
                    low = mid;
                else
                    high = mid - 1;
            }

            return text.Substring(0, low) + ellipsis;
        }

        private static string FormatTime(float seconds)
        {
            if (seconds < 0f) seconds = 0f;

            int total = Mathf.CeilToInt(seconds);
            int h = total / 3600;
            int m = (total % 3600) / 60;
            int s = total % 60;

            return h > 0 ? h + ":" + m.ToString("00") + ":" + s.ToString("00") : m + ":" + s.ToString("00");
        }

        private static Rect Expanded(Rect r, float amount)
        {
            return new Rect(r.x - amount, r.y - amount, r.width + amount * 2f, r.height + amount * 2f);
        }

        private static Texture2D MakeTexture(Color color)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.name = "Inject0rHUD_RuntimeTexture";
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.SetPixel(0, 0, color);
            tex.Apply(false, true);
            return tex;
        }

        public void Dispose()
        {
            DestroyTexture(_background);
            DestroyTexture(_barBack);
            DestroyTexture(_barGood);
            DestroyTexture(_barWarning);
            DestroyTexture(_barCritical);
            DestroyTexture(_accent);
            DestroyTexture(_editorBorder);
            DestroyTexture(_editorHandle);
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (texture != null)
                UnityEngine.Object.Destroy(texture);
        }
    }
}
