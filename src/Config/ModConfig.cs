using BepInEx.Configuration;
using Inject0rHUD.Localization;
using UnityEngine;

namespace Inject0rHUD.Config
{
    internal enum DurabilityValueMode
    {
        Units,
        Percent,
        Both
    }

    internal enum SmartDurabilityMode
    {
        Off,
        BelowThreshold,
        CurrentItemOnly
    }

    internal sealed class ModConfig
    {
        private readonly ConfigFile _file;

        internal readonly ConfigEntry<bool> Enabled;
        internal readonly ConfigEntry<bool> Visible;
        internal readonly ConfigEntry<KeyCode> ToggleKey;
        internal readonly ConfigEntry<KeyCode> EditKey;
        internal readonly ConfigEntry<float> PollInterval;
        internal readonly ConfigEntry<ModLanguage> InterfaceLanguage;

        internal readonly ConfigEntry<bool> ShowRestedTimer;
        internal readonly ConfigEntry<bool> ShowPowerCooldown;
        internal readonly ConfigEntry<bool> ShowDurability;
        internal readonly ConfigEntry<bool> CompactInCombat;

        internal readonly ConfigEntry<bool> ShowWorldHoverTimers;
        internal readonly ConfigEntry<bool> ShowPickableHoverTimers;
        internal readonly ConfigEntry<bool> ShowPlantHoverTimers;
        internal readonly ConfigEntry<float> PickableHoverOpacity;
        internal readonly ConfigEntry<float> PlantHoverOpacity;
        internal readonly ConfigEntry<bool> ShowBeehiveHoverTimers;
        internal readonly ConfigEntry<bool> ShowFermenterHoverTimers;
        internal readonly ConfigEntry<bool> ShowProductionHoverTimers;
        internal readonly ConfigEntry<float> BeehiveHoverOpacity;
        internal readonly ConfigEntry<float> FermenterHoverOpacity;
        internal readonly ConfigEntry<float> ProductionHoverOpacity;

        internal readonly ConfigEntry<bool> SeparateBlocks;
        internal readonly ConfigEntry<bool> SnapEnabled;
        internal readonly ConfigEntry<int> SnapDistance;

        internal readonly ConfigEntry<int> PosX;
        internal readonly ConfigEntry<int> PosY;
        internal readonly ConfigEntry<float> Scale;
        internal readonly ConfigEntry<int> FontSize;
        internal readonly ConfigEntry<int> PanelWidth;
        internal readonly ConfigEntry<float> BackgroundOpacity;
        internal readonly ConfigEntry<bool> ShowSectionHeaders;

        internal readonly ConfigEntry<int> TimersPosX;
        internal readonly ConfigEntry<int> TimersPosY;
        internal readonly ConfigEntry<int> TimersWidth;
        internal readonly ConfigEntry<int> DurabilityPosX;
        internal readonly ConfigEntry<int> DurabilityPosY;
        internal readonly ConfigEntry<int> DurabilityWidth;

        internal readonly ConfigEntry<DurabilityValueMode> DurabilityDisplay;
        internal readonly ConfigEntry<bool> ShowItemIcons;
        internal readonly ConfigEntry<SmartDurabilityMode> SmartDurability;
        internal readonly ConfigEntry<int> SmartDurabilityThreshold;
        internal readonly ConfigEntry<int> WarningPercent;
        internal readonly ConfigEntry<int> CriticalPercent;

        internal readonly ConfigEntry<bool> ShowFpsWidget;
        internal readonly ConfigEntry<bool> FpsShowValue;
        internal readonly ConfigEntry<bool> FpsShowLabel;
        internal readonly ConfigEntry<float> FpsOpacity;
        internal readonly ConfigEntry<int> FpsPosX;
        internal readonly ConfigEntry<int> FpsPosY;

        internal readonly ConfigEntry<bool> ShowPingWidget;
        internal readonly ConfigEntry<bool> PingShowValue;
        internal readonly ConfigEntry<bool> PingShowLabel;
        internal readonly ConfigEntry<float> PingOpacity;
        internal readonly ConfigEntry<int> PingPosX;
        internal readonly ConfigEntry<int> PingPosY;

        internal readonly ConfigEntry<string> ActiveProfile;

        internal ModConfig(ConfigFile config)
        {
            _file = config;

            Enabled = config.Bind("General", "Enabled", true,
                "Master switch for Inject0r HUD.");

            Visible = config.Bind("General", "Visible", true,
                "Current HUD visibility. Toggle in-game with ToggleKey.");

            ToggleKey = config.Bind("General", "ToggleKey", KeyCode.F8,
                "Hotkey to show/hide the HUD.");

            EditKey = config.Bind("General", "EditKey", KeyCode.F10,
                "Hotkey to enter/leave HUD edit mode and open the live settings panel.");

            PollInterval = config.Bind("General", "PollInterval", 0.25f,
                new ConfigDescription(
                    "Seconds between game-state refreshes.",
                    new AcceptableValueRange<float>(0.10f, 2.0f)));

            InterfaceLanguage = config.Bind("Language", "InterfaceLanguage", ModLanguage.English,
                "Inject0r HUD interface language. English is the default; Auto follows supported Valheim languages.");

            ShowRestedTimer = config.Bind("Modules", "RestedTimer", true,
                "Legacy key kept for profile compatibility. Controls active timed positive-effect timers.");

            ShowPowerCooldown = config.Bind("Modules", "ForsakenPowerCooldown", false,
                "Deprecated legacy setting. Guardian Power cooldown is intentionally not shown because Valheim already displays it.");

            ShowDurability = config.Bind("Modules", "DurabilityHUD", true,
                "Show equipment durability.");

            CompactInCombat = config.Bind("Modules", "CompactInCombat", false,
                "Optional: while in combat, hide normal timers/armor rows and keep only combat-relevant durability.");

            ShowWorldHoverTimers = config.Bind("World Hover Timers", "Enabled", true,
                "Master switch for native hover-text respawn/growth timers.");

            ShowPickableHoverTimers = config.Bind("World Hover Timers", "Pickables", true,
                "Show respawn timers for picked berry bushes and other respawning Pickable objects.");

            ShowPlantHoverTimers = config.Bind("World Hover Timers", "Plants", true,
                "Show growth timers for healthy planted crops.");

            PickableHoverOpacity = config.Bind("World Hover Timers", "PickableOpacity", 0.95f,
                new ConfigDescription("Opacity of bush/resource respawn timer text.",
                    new AcceptableValueRange<float>(0.20f, 1.0f)));

            PlantHoverOpacity = config.Bind("World Hover Timers", "PlantOpacity", 0.95f,
                new ConfigDescription("Opacity of planted-crop growth timer text.",
                    new AcceptableValueRange<float>(0.20f, 1.0f)));

            ShowBeehiveHoverTimers = config.Bind("World Hover Timers", "Beehives", true,
                "Show honey amount and exact next-honey countdown when the client has the data.");

            ShowFermenterHoverTimers = config.Bind("World Hover Timers", "Fermenters", true,
                "Show fermenter contents and exact remaining fermentation time.");

            ShowProductionHoverTimers = config.Bind("World Hover Timers", "ProductionStations", true,
                "Show queue/fuel and next-output timing for Smelter-based production stations.");

            BeehiveHoverOpacity = config.Bind("World Hover Timers", "BeehiveOpacity", 0.95f,
                new ConfigDescription("Opacity of beehive hover information.",
                    new AcceptableValueRange<float>(0.20f, 1.0f)));

            FermenterHoverOpacity = config.Bind("World Hover Timers", "FermenterOpacity", 0.95f,
                new ConfigDescription("Opacity of fermenter hover information.",
                    new AcceptableValueRange<float>(0.20f, 1.0f)));

            ProductionHoverOpacity = config.Bind("World Hover Timers", "ProductionOpacity", 0.95f,
                new ConfigDescription("Opacity of Smelter/production hover information.",
                    new AcceptableValueRange<float>(0.20f, 1.0f)));

            SeparateBlocks = config.Bind("Layout", "SeparateBlocks", false,
                "Allow Timers and Durability to use separate draggable panels.");

            SnapEnabled = config.Bind("Layout", "SnapEnabled", true,
                "Snap movable blocks to screen edges and screen center.");

            SnapDistance = config.Bind("Layout", "SnapDistance", 12,
                new ConfigDescription("Snap distance in pixels.",
                    new AcceptableValueRange<int>(4, 40)));

            PosX = config.Bind("UI", "PositionX", 20, "Unified HUD X position.");
            PosY = config.Bind("UI", "PositionY", 220, "Unified HUD Y position.");

            Scale = config.Bind("UI", "Scale", 0.90f,
                new ConfigDescription("HUD content scale.",
                    new AcceptableValueRange<float>(0.70f, 1.60f)));

            FontSize = config.Bind("UI", "FontSize", 12,
                new ConfigDescription("Base font size.",
                    new AcceptableValueRange<int>(10, 24)));

            PanelWidth = config.Bind("UI", "PanelWidth", 280,
                new ConfigDescription("Unified panel width.",
                    new AcceptableValueRange<int>(120, 720)));

            BackgroundOpacity = config.Bind("UI", "BackgroundOpacity", 0.60f,
                new ConfigDescription("HUD background opacity.",
                    new AcceptableValueRange<float>(0.0f, 1.0f)));

            ShowSectionHeaders = config.Bind("UI", "SectionHeaders", true,
                "Show TIMERS and DURABILITY headers.");

            TimersPosX = config.Bind("Separate Timers", "PositionX", 20, "Timers panel X position.");
            TimersPosY = config.Bind("Separate Timers", "PositionY", 220, "Timers panel Y position.");
            TimersWidth = config.Bind("Separate Timers", "Width", 250,
                new ConfigDescription("Timers panel width.",
                    new AcceptableValueRange<int>(120, 720)));

            DurabilityPosX = config.Bind("Separate Durability", "PositionX", 20, "Durability panel X position.");
            DurabilityPosY = config.Bind("Separate Durability", "PositionY", 330, "Durability panel Y position.");
            DurabilityWidth = config.Bind("Separate Durability", "Width", 280,
                new ConfigDescription("Durability panel width.",
                    new AcceptableValueRange<int>(120, 720)));

            DurabilityDisplay = config.Bind("Durability", "ValueMode", DurabilityValueMode.Units,
                "Units = 200/200, Percent = 100%, Both = 100% (200/200).");

            ShowItemIcons = config.Bind("Durability", "ShowItemIcons", true,
                "Show the item's native Valheim icon next to durability rows.");

            SmartDurability = config.Bind("Durability", "SmartMode", SmartDurabilityMode.Off,
                "Optional smart filter. Off shows all, BelowThreshold shows only low durability, CurrentItemOnly shows only the held/current item.");

            SmartDurabilityThreshold = config.Bind("Durability", "SmartThreshold", 35,
                new ConfigDescription("Threshold used by BelowThreshold smart mode.",
                    new AcceptableValueRange<int>(1, 99)));

            WarningPercent = config.Bind("Durability", "WarningPercent", 25,
                new ConfigDescription("Warning threshold.",
                    new AcceptableValueRange<int>(1, 99)));

            CriticalPercent = config.Bind("Durability", "CriticalPercent", 10,
                new ConfigDescription("Critical threshold.",
                    new AcceptableValueRange<int>(1, 99)));

            ShowFpsWidget = config.Bind("FPS Widget", "Enabled", false,
                "Show the independent FPS cube.");

            FpsShowValue = config.Bind("FPS Widget", "ShowValue", true,
                "Show the numeric FPS value.");

            FpsShowLabel = config.Bind("FPS Widget", "ShowLabel", true,
                "Show the FPS label.");

            FpsOpacity = config.Bind("FPS Widget", "Opacity", 0.72f,
                new ConfigDescription("FPS cube opacity.",
                    new AcceptableValueRange<float>(0.10f, 1.0f)));

            FpsPosX = config.Bind("FPS Widget", "PositionX", 20, "FPS cube X position.");
            FpsPosY = config.Bind("FPS Widget", "PositionY", 80, "FPS cube Y position.");

            ShowPingWidget = config.Bind("Ping Widget", "Enabled", false,
                "Show the independent ping cube.");

            PingShowValue = config.Bind("Ping Widget", "ShowValue", true,
                "Show the numeric ping value.");

            PingShowLabel = config.Bind("Ping Widget", "ShowLabel", true,
                "Show the PING label.");

            PingOpacity = config.Bind("Ping Widget", "Opacity", 0.72f,
                new ConfigDescription("Ping cube opacity.",
                    new AcceptableValueRange<float>(0.10f, 1.0f)));

            PingPosX = config.Bind("Ping Widget", "PositionX", 100, "Ping cube X position.");
            PingPosY = config.Bind("Ping Widget", "PositionY", 80, "Ping cube Y position.");

            ActiveProfile = config.Bind("Profiles", "ActiveProfile", "Compact",
                "Selected HUD profile name.");
        }

        internal void Save()
        {
            try { _file?.Save(); }
            catch { }
        }
    }
}
