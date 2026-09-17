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
        private Rect BuildUnifiedRect(HudSnapshot snapshot, ModConfig cfg)
        {
            float s = cfg.Scale.Value;
            float w = Mathf.Max(120f, cfg.PanelWidth.Value * s);
            float h = ComputePanelHeight(snapshot.Timers.Count, snapshot.Durability.Count, cfg.ShowSectionHeaders.Value, false, s);
            return KeepOnScreen(new Rect(cfg.PosX.Value, cfg.PosY.Value, w, h));
        }

        private Rect BuildTimersRect(HudSnapshot snapshot, ModConfig cfg)
        {
            float s = cfg.Scale.Value;
            float w = Mathf.Max(120f, cfg.TimersWidth.Value * s);
            float h = ComputePanelHeight(snapshot.Timers.Count, 0, cfg.ShowSectionHeaders.Value, true, s);
            return KeepOnScreen(new Rect(cfg.TimersPosX.Value, cfg.TimersPosY.Value, w, h));
        }

        private Rect BuildDurabilityRect(HudSnapshot snapshot, ModConfig cfg)
        {
            float s = cfg.Scale.Value;
            float w = Mathf.Max(120f, cfg.DurabilityWidth.Value * s);
            float h = ComputePanelHeight(0, snapshot.Durability.Count, cfg.ShowSectionHeaders.Value, true, s);
            return KeepOnScreen(new Rect(cfg.DurabilityPosX.Value, cfg.DurabilityPosY.Value, w, h));
        }

        private Rect BuildWidgetRect(int x, int y, float scale)
        {
            return KeepOnScreen(new Rect(x, y, 72f * scale, 64f * scale));
        }

        private static float ComputePanelHeight(int timers, int durability, bool headers, bool separate, float scale)
        {
            float pad = 8f * scale;
            float headerH = 20f * scale;
            float rowH = 23f * scale;
            float gap = 5f * scale;
            int rows = timers + durability;
            int headerCount = 0;

            if (headers)
            {
                if (timers > 0 || separate) headerCount++;
                if (durability > 0 && !separate) headerCount++;
            }

            float h = pad * 2f + rows * rowH + headerCount * headerH;
            if (!separate && timers > 0 && durability > 0) h += gap;
            if (rows == 0) h = Mathf.Max(h, 58f * scale);
            return h;
        }

        private Rect KeepOnScreen(Rect r)
        {
            r.x = Mathf.Clamp(r.x, 0f, Mathf.Max(0f, Screen.width - r.width));
            r.y = Mathf.Clamp(r.y, 0f, Mathf.Max(0f, Screen.height - r.height));
            return r;
        }

        private Rect BuildSettingsRect(Rect anchor)
        {
            const float width = 430f;
            float height = Mathf.Min(640f, Screen.height - 24f);
            const float gap = 14f;
            const float edge = 12f;
            float x = anchor.xMax + gap;
            if (x + width > Screen.width - edge) x = anchor.xMin - gap - width;
            if (x < edge) x = Mathf.Max(edge, Screen.width - width - edge);
            float y = Mathf.Clamp(anchor.y, edge, Mathf.Max(edge, Screen.height - height - edge));
            return new Rect(x, y, width, height);
        }

        private void HandleEditorInput(HudSnapshot snapshot, ModConfig cfg, Rect settings, ref Rect unified, ref Rect timers, ref Rect durability, ref Rect fps, ref Rect ping)
        {
            Event e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (settings.Contains(e.mousePosition)) return;
                EditTarget target = HitTarget(cfg, e.mousePosition, unified, timers, durability, fps, ping);
                if (target == EditTarget.None) return;
                _target = target;
                Rect rect = GetRect(target, unified, timers, durability, fps, ping);
                _resizeMode = IsMetric(target) ? ResizeMode.Move : HitResizeMode(rect, e.mousePosition, 8f);
                _dragStart = e.mousePosition;
                _dragRectStart = rect;
                _dragScaleStart = cfg.Scale.Value;
                e.Use();
                return;
            }

            if (e.type == EventType.MouseDrag && e.button == 0 && _target != EditTarget.None)
            {
                Vector2 d = e.mousePosition - _dragStart;
                Rect changed = _dragRectStart;
                if (_resizeMode == ResizeMode.Move || IsMetric(_target))
                {
                    changed.x += d.x;
                    changed.y += d.y;
                    SnapMove(ref changed, cfg);
                }
                else
                {
                    ApplyResize(ref changed, d, cfg);
                }
                ApplyRectToConfig(_target, changed, cfg);
                e.Use();
                return;
            }

            if (e.type == EventType.MouseUp && e.button == 0 && _target != EditTarget.None)
            {
                _target = EditTarget.None;
                _resizeMode = ResizeMode.None;
                cfg.Save();
                e.Use();
            }
        }

        private static bool IsMetric(EditTarget target)
        {
            return target == EditTarget.Fps || target == EditTarget.Ping;
        }

        private EditTarget HitTarget(ModConfig cfg, Vector2 mouse, Rect unified, Rect timers, Rect durability, Rect fps, Rect ping)
        {
            if (Expanded(ping, 7f).Contains(mouse)) return EditTarget.Ping;
            if (Expanded(fps, 7f).Contains(mouse)) return EditTarget.Fps;
            if (cfg.SeparateBlocks.Value)
            {
                if (Expanded(durability, 7f).Contains(mouse)) return EditTarget.Durability;
                if (Expanded(timers, 7f).Contains(mouse)) return EditTarget.Timers;
            }
            else if (Expanded(unified, 7f).Contains(mouse)) return EditTarget.Unified;
            return EditTarget.None;
        }

        private static Rect GetRect(EditTarget target, Rect unified, Rect timers, Rect durability, Rect fps, Rect ping)
        {
            switch (target)
            {
                case EditTarget.Timers: return timers;
                case EditTarget.Durability: return durability;
                case EditTarget.Fps: return fps;
                case EditTarget.Ping: return ping;
                default: return unified;
            }
        }

        private void ApplyResize(ref Rect rect, Vector2 delta, ModConfig cfg)
        {
            bool left = _resizeMode == ResizeMode.Left || _resizeMode == ResizeMode.TopLeft || _resizeMode == ResizeMode.BottomLeft;
            bool right = _resizeMode == ResizeMode.Right || _resizeMode == ResizeMode.TopRight || _resizeMode == ResizeMode.BottomRight;
            bool top = _resizeMode == ResizeMode.Top || _resizeMode == ResizeMode.TopLeft || _resizeMode == ResizeMode.TopRight;
            bool bottom = _resizeMode == ResizeMode.Bottom || _resizeMode == ResizeMode.BottomLeft || _resizeMode == ResizeMode.BottomRight;

            if (left) { rect.x += delta.x; rect.width -= delta.x; }
            if (right) rect.width += delta.x;
            if (top || bottom)
            {
                float desiredHeight = _dragRectStart.height + (bottom ? delta.y : -delta.y);
                float ratio = desiredHeight / Mathf.Max(1f, _dragRectStart.height);
                cfg.Scale.Value = Mathf.Clamp(_dragScaleStart * ratio, 0.70f, 1.60f);
                if (top) rect.y = _dragRectStart.yMax - (_dragRectStart.height * (cfg.Scale.Value / _dragScaleStart));
            }

            rect.width = Mathf.Clamp(rect.width, 120f, Mathf.Min(900f, Screen.width));
            if (left) rect.x = Mathf.Clamp(rect.x, 0f, Mathf.Max(0f, Screen.width - rect.width));
            rect.y = Mathf.Clamp(rect.y, 0f, Mathf.Max(0f, Screen.height - rect.height));
        }

        private void ApplyRectToConfig(EditTarget target, Rect rect, ModConfig cfg)
        {
            switch (target)
            {
                case EditTarget.Timers:
                    cfg.TimersPosX.Value = Mathf.RoundToInt(rect.x); cfg.TimersPosY.Value = Mathf.RoundToInt(rect.y);
                    cfg.TimersWidth.Value = Mathf.Clamp(Mathf.RoundToInt(rect.width / Mathf.Max(0.01f, cfg.Scale.Value)), 120, 720); break;
                case EditTarget.Durability:
                    cfg.DurabilityPosX.Value = Mathf.RoundToInt(rect.x); cfg.DurabilityPosY.Value = Mathf.RoundToInt(rect.y);
                    cfg.DurabilityWidth.Value = Mathf.Clamp(Mathf.RoundToInt(rect.width / Mathf.Max(0.01f, cfg.Scale.Value)), 120, 720); break;
                case EditTarget.Fps:
                    cfg.FpsPosX.Value = Mathf.RoundToInt(rect.x); cfg.FpsPosY.Value = Mathf.RoundToInt(rect.y); break;
                case EditTarget.Ping:
                    cfg.PingPosX.Value = Mathf.RoundToInt(rect.x); cfg.PingPosY.Value = Mathf.RoundToInt(rect.y); break;
                default:
                    cfg.PosX.Value = Mathf.RoundToInt(rect.x); cfg.PosY.Value = Mathf.RoundToInt(rect.y);
                    cfg.PanelWidth.Value = Mathf.Clamp(Mathf.RoundToInt(rect.width / Mathf.Max(0.01f, cfg.Scale.Value)), 120, 720); break;
            }
        }

        private void SnapMove(ref Rect r, ModConfig cfg)
        {
            if (!cfg.SnapEnabled.Value) { r = KeepOnScreen(r); return; }
            float d = cfg.SnapDistance.Value;
            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;
            if (Mathf.Abs(r.xMin) <= d) r.x = 0f;
            if (Mathf.Abs(Screen.width - r.xMax) <= d) r.x = Screen.width - r.width;
            if (Mathf.Abs(r.center.x - centerX) <= d) r.x = centerX - r.width * 0.5f;
            if (Mathf.Abs(r.yMin) <= d) r.y = 0f;
            if (Mathf.Abs(Screen.height - r.yMax) <= d) r.y = Screen.height - r.height;
            if (Mathf.Abs(r.center.y - centerY) <= d) r.y = centerY - r.height * 0.5f;
            r = KeepOnScreen(r);
        }

        private static ResizeMode HitResizeMode(Rect r, Vector2 mouse, float edge)
        {
            bool left = Mathf.Abs(mouse.x - r.xMin) <= edge, right = Mathf.Abs(mouse.x - r.xMax) <= edge;
            bool top = Mathf.Abs(mouse.y - r.yMin) <= edge, bottom = Mathf.Abs(mouse.y - r.yMax) <= edge;
            if (left && top) return ResizeMode.TopLeft;
            if (right && top) return ResizeMode.TopRight;
            if (left && bottom) return ResizeMode.BottomLeft;
            if (right && bottom) return ResizeMode.BottomRight;
            if (left) return ResizeMode.Left;
            if (right) return ResizeMode.Right;
            if (top) return ResizeMode.Top;
            if (bottom) return ResizeMode.Bottom;
            return ResizeMode.Move;
        }

        private void DrawEditorChrome(Rect r, string title)
        {
            float line = 2f;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, line), _editorBorder);
            GUI.DrawTexture(new Rect(r.x, r.yMax - line, r.width, line), _editorBorder);
            GUI.DrawTexture(new Rect(r.x, r.y, line, r.height), _editorBorder);
            GUI.DrawTexture(new Rect(r.xMax - line, r.y, line, r.height), _editorBorder);
            float h = 7f;
            DrawHandle(r.xMin, r.yMin, h); DrawHandle(r.xMax, r.yMin, h); DrawHandle(r.xMin, r.yMax, h); DrawHandle(r.xMax, r.yMax, h);
            string text = title + " • F10";
            Vector2 size = _editorLabel.CalcSize(new GUIContent(text));
            float bw = size.x + 12f, bh = size.y + 4f;
            float bx = Mathf.Clamp(r.xMax - bw, 0f, Mathf.Max(0f, Screen.width - bw));
            float by = r.y >= bh + 4f ? r.y - bh - 4f : r.y + 4f;
            GUI.DrawTexture(new Rect(bx, by, bw, bh), _background);
            GUI.Label(new Rect(bx + 6f, by + 2f, bw - 12f, bh - 4f), text, _editorLabel);
        }

        private void DrawHandle(float x, float y, float size)
        {
            GUI.DrawTexture(new Rect(x - size * 0.5f, y - size * 0.5f, size, size), _editorHandle);
        }
    }
}
