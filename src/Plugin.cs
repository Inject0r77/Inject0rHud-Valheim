using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Inject0rHUD.Config;
using Inject0rHUD.Localization;
using Inject0rHUD.Models;
using Inject0rHUD.Profiles;
using Inject0rHUD.Services;
using Inject0rHUD.UI;
using UnityEngine;

namespace Inject0rHUD
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "inject0r.Inject0rHUD";
        public const string PluginName = "Inject0r HUD";
        public const string PluginVersion = "0.5.1";

        internal static ManualLogSource Log;
        internal static bool IsEditModeActive { get; private set; }

        private static Plugin Instance;

        internal static bool WorldHoverTimersEnabled =>
            Instance != null &&
            Instance._config != null &&
            Instance._config.Enabled.Value &&
            Instance._config.ShowWorldHoverTimers.Value;

        internal static bool PickableHoverTimersEnabled =>
            WorldHoverTimersEnabled &&
            Instance._config.ShowPickableHoverTimers.Value;

        internal static bool PlantHoverTimersEnabled =>
            WorldHoverTimersEnabled &&
            Instance._config.ShowPlantHoverTimers.Value;

        internal static bool BeehiveHoverTimersEnabled =>
            WorldHoverTimersEnabled &&
            Instance._config.ShowBeehiveHoverTimers.Value;

        internal static bool FermenterHoverTimersEnabled =>
            WorldHoverTimersEnabled &&
            Instance._config.ShowFermenterHoverTimers.Value;

        internal static bool ProductionHoverTimersEnabled =>
            WorldHoverTimersEnabled &&
            Instance._config.ShowProductionHoverTimers.Value;

        internal static float BeehiveHoverOpacity =>
            Instance != null && Instance._config != null
                ? Mathf.Clamp01(Instance._config.BeehiveHoverOpacity.Value)
                : 1f;

        internal static float FermenterHoverOpacity =>
            Instance != null && Instance._config != null
                ? Mathf.Clamp01(Instance._config.FermenterHoverOpacity.Value)
                : 1f;

        internal static float ProductionHoverOpacity =>
            Instance != null && Instance._config != null
                ? Mathf.Clamp01(Instance._config.ProductionHoverOpacity.Value)
                : 1f;

        internal static float PickableHoverOpacity =>
            Instance != null && Instance._config != null
                ? Mathf.Clamp01(Instance._config.PickableHoverOpacity.Value)
                : 1f;

        internal static float PlantHoverOpacity =>
            Instance != null && Instance._config != null
                ? Mathf.Clamp01(Instance._config.PlantHoverOpacity.Value)
                : 1f;

        private ModConfig _config;
        private ProfileService _profiles;
        private HudRenderer _renderer;
        private HudSnapshot _snapshot = HudSnapshot.Empty;
        private Harmony _harmony;

        private float _nextPoll;
        private float _fpsSmoothed;
        private readonly Dictionary<string, float> _nextErrorLog = new Dictionary<string, float>();
        private static readonly HashSet<string> HoverErrorsLogged = new HashSet<string>();

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _config = new ModConfig(Config);
            ModLocalization.Bind(_config.InterfaceLanguage);
            _profiles = new ProfileService(_config);
            _renderer = new HudRenderer(_profiles);

            try
            {
                _harmony = new Harmony(PluginGuid);
                _harmony.PatchAll();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to install local client patches: " + ex);
            }

            Logger.LogInfo(
                $"{PluginName} v{PluginVersion} loaded. Client-side only; no custom RPC, ServerSync, world writes, or character-save writes.");
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0.0001f && dt < 1f)
            {
                float instant = 1f / dt;
                _fpsSmoothed = _fpsSmoothed <= 0f
                    ? instant
                    : Mathf.Lerp(_fpsSmoothed, instant, 0.08f);
            }

            if (_config == null || !_config.Enabled.Value)
            {
                LeaveEditMode();
                _snapshot = HudSnapshot.Empty;
                return;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            {
                LeaveEditMode();
                _snapshot = HudSnapshot.Empty;
                return;
            }

            if (_config.EditKey.Value != KeyCode.None && Input.GetKeyDown(_config.EditKey.Value))
            {
                if (IsEditModeActive)
                    LeaveEditMode();
                else
                    EnterEditMode();
            }

            if (!IsEditModeActive &&
                _config.ToggleKey.Value != KeyCode.None &&
                Input.GetKeyDown(_config.ToggleKey.Value))
            {
                _config.Visible.Value = !_config.Visible.Value;
            }

            if (IsEditModeActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            float now = Time.realtimeSinceStartup;
            if (now < _nextPoll)
                return;

            _nextPoll = now + Mathf.Clamp(_config.PollInterval.Value, 0.10f, 2.0f);

            var timers = new List<TimerEntry>(4);
            var durability = new List<DurabilityEntry>(12);

            bool inCombat = false;
            TryModule("CombatState", () => inCombat = CombatStateService.IsInCombat(player));

            bool compactCombat = _config.CompactInCombat.Value && inCombat;

            if (_config.ShowRestedTimer.Value)
            {
                TryModule("PositiveEffects", () =>
                {
                    timers.AddRange(PositiveEffectTimerService.Read(player));
                });
            }

            if (_config.ShowDurability.Value)
            {
                TryModule("Durability", () =>
                {
                    foreach (DurabilityEntry entry in DurabilityService.Read(player))
                    {
                        if (!ShouldIncludeDurability(entry, compactCombat))
                            continue;

                        durability.Add(entry);
                    }
                });
            }

            int ping = 0;
            if (_config.ShowPingWidget.Value)
                TryModule("Ping", () => ping = NetworkStatsService.GetPingMs());

            _snapshot = new HudSnapshot(
                timers,
                durability,
                inCombat,
                Mathf.Max(0, Mathf.RoundToInt(_fpsSmoothed)),
                ping);
        }

        private bool ShouldIncludeDurability(DurabilityEntry entry, bool compactCombat)
        {
            if (entry == null)
                return false;

            if (compactCombat &&
                entry.Slot.StartsWith("1 ", StringComparison.Ordinal))
            {
                return false;
            }

            switch (_config.SmartDurability.Value)
            {
                case SmartDurabilityMode.BelowThreshold:
                    return entry.Fraction * 100f <= _config.SmartDurabilityThreshold.Value;

                case SmartDurabilityMode.CurrentItemOnly:
                    return entry.IsCurrentItem;

                default:
                    return true;
            }
        }

        private void LateUpdate()
        {
            if (IsEditModeActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnGUI()
        {
            if (_config == null || _renderer == null)
                return;

            if (!_config.Enabled.Value)
                return;

            if (!_config.Visible.Value && !IsEditModeActive)
                return;

            if (Player.m_localPlayer == null)
                return;

            try
            {
                _renderer.Draw(_snapshot, _config, IsEditModeActive);
            }
            catch (Exception ex)
            {
                ThrottledError("Renderer", ex);
            }
        }

        private void EnterEditMode()
        {
            if (Player.m_localPlayer == null || _renderer == null)
                return;

            IsEditModeActive = true;
            _config.Visible.Value = true;
            _renderer.BeginEdit(_config);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LeaveEditMode()
        {
            if (!IsEditModeActive)
                return;

            try
            {
                _renderer?.EndEdit(_config);
                _config?.Save();
            }
            catch (Exception ex)
            {
                ThrottledError("EditMode", ex);
            }

            IsEditModeActive = false;
        }

        private void OnDestroy()
        {
            LeaveEditMode();

            try { _renderer?.Dispose(); } catch { }
            try { _harmony?.UnpatchSelf(); } catch { }

            _renderer = null;
            _snapshot = HudSnapshot.Empty;
            _harmony = null;

            if (ReferenceEquals(Instance, this))
                Instance = null;
        }

        private void TryModule(string name, Action action)
        {
            try { action(); }
            catch (Exception ex) { ThrottledError(name, ex); }
        }

        private void ThrottledError(string module, Exception ex)
        {
            float now = Time.realtimeSinceStartup;
            float next;

            if (_nextErrorLog.TryGetValue(module, out next) && now < next)
                return;

            _nextErrorLog[module] = now + 15f;
            Logger.LogWarning($"[{module}] non-fatal error: {ex.GetType().Name}: {ex.Message}");
        }

        internal static void LogHoverErrorOnce(string module, Exception ex)
        {
            if (Log == null)
                return;

            lock (HoverErrorsLogged)
            {
                if (!HoverErrorsLogged.Add(module + ":" + ex.GetType().FullName))
                    return;
            }

            Log.LogWarning($"[{module}] hover timer failed safely: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
