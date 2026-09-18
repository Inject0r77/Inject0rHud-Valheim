using System;
using System.Globalization;
using System.Text;
using Inject0rHUD.Config;

namespace Inject0rHUD.Profiles
{
    internal sealed class HudProfile
    {
        internal string Name;
        internal bool BuiltIn;

        internal bool SeparateBlocks;
        internal bool SnapEnabled;
        internal int SnapDistance;

        internal float Scale;
        internal int FontSize;
        internal int PanelWidth;
        internal float BackgroundOpacity;
        internal bool SectionHeaders;

        internal int PosX;
        internal int PosY;
        internal int TimersX;
        internal int TimersY;
        internal int TimersWidth;
        internal float TimersScale;
        internal int DurabilityX;
        internal int DurabilityY;
        internal int DurabilityWidth;
        internal float DurabilityScale;

        internal bool ShowRested;
        internal bool ShowPower;
        internal bool ShowDurability;
        internal bool CompactInCombat;

        internal DurabilityValueMode DurabilityMode;
        internal bool ShowIcons;
        internal SmartDurabilityMode SmartMode;
        internal int SmartThreshold;

        internal bool WorldTimers;
        internal bool Pickables;
        internal bool Plants;
        internal float PickableOpacity;
        internal float PlantOpacity;
        internal bool Beehives;
        internal bool Fermenters;
        internal bool ProductionStations;
        internal bool ProductionWholeStationHover;
        internal float BeehiveOpacity;
        internal float FermenterOpacity;
        internal float ProductionOpacity;

        internal bool ShowFps;
        internal bool FpsValue;
        internal bool FpsLabel;
        internal float FpsOpacity;
        internal int FpsX;
        internal int FpsY;

        internal bool ShowPing;
        internal bool PingValue;
        internal bool PingLabel;
        internal float PingOpacity;
        internal int PingX;
        internal int PingY;

        internal bool ShowShip;
        internal bool ShipHealth;
        internal bool ShipSpeed;
        internal bool ShipWind;
        internal bool ShipSail;
        internal float ShipOpacity;
        internal int ShipX;
        internal int ShipY;

        internal bool ShowTime;
        internal bool TimeDay;
        internal bool TimeClock;
        internal bool TimeSunEvent;
        internal float TimeOpacity;
        internal int TimeX;
        internal int TimeY;

        internal static HudProfile Capture(string name, bool builtIn, ModConfig c)
        {
            return new HudProfile
            {
                Name = name,
                BuiltIn = builtIn,
                SeparateBlocks = c.SeparateBlocks.Value,
                SnapEnabled = c.SnapEnabled.Value,
                SnapDistance = c.SnapDistance.Value,
                Scale = c.Scale.Value,
                FontSize = c.FontSize.Value,
                PanelWidth = c.PanelWidth.Value,
                BackgroundOpacity = c.BackgroundOpacity.Value,
                SectionHeaders = c.ShowSectionHeaders.Value,
                PosX = c.PosX.Value,
                PosY = c.PosY.Value,
                TimersX = c.TimersPosX.Value,
                TimersY = c.TimersPosY.Value,
                TimersWidth = c.TimersWidth.Value,
                TimersScale = c.TimersScale.Value,
                DurabilityX = c.DurabilityPosX.Value,
                DurabilityY = c.DurabilityPosY.Value,
                DurabilityWidth = c.DurabilityWidth.Value,
                DurabilityScale = c.DurabilityScale.Value,
                ShowRested = c.ShowRestedTimer.Value,
                ShowPower = false,
                ShowDurability = c.ShowDurability.Value,
                CompactInCombat = c.CompactInCombat.Value,
                DurabilityMode = c.DurabilityDisplay.Value,
                ShowIcons = c.ShowItemIcons.Value,
                SmartMode = c.SmartDurability.Value,
                SmartThreshold = c.SmartDurabilityThreshold.Value,
                WorldTimers = c.ShowWorldHoverTimers.Value,
                Pickables = c.ShowPickableHoverTimers.Value,
                Plants = c.ShowPlantHoverTimers.Value,
                PickableOpacity = c.PickableHoverOpacity.Value,
                PlantOpacity = c.PlantHoverOpacity.Value,
                Beehives = c.ShowBeehiveHoverTimers.Value,
                Fermenters = c.ShowFermenterHoverTimers.Value,
                ProductionStations = c.ShowProductionHoverTimers.Value,
                ProductionWholeStationHover = c.ShowProductionOnWholeStation.Value,
                BeehiveOpacity = c.BeehiveHoverOpacity.Value,
                FermenterOpacity = c.FermenterHoverOpacity.Value,
                ProductionOpacity = c.ProductionHoverOpacity.Value,
                ShowFps = c.ShowFpsWidget.Value,
                FpsValue = c.FpsShowValue.Value,
                FpsLabel = c.FpsShowLabel.Value,
                FpsOpacity = c.FpsOpacity.Value,
                FpsX = c.FpsPosX.Value,
                FpsY = c.FpsPosY.Value,
                ShowPing = c.ShowPingWidget.Value,
                PingValue = c.PingShowValue.Value,
                PingLabel = c.PingShowLabel.Value,
                PingOpacity = c.PingOpacity.Value,
                PingX = c.PingPosX.Value,
                PingY = c.PingPosY.Value,
                ShowShip = c.ShowShipWidget.Value,
                ShipHealth = c.ShipShowHealth.Value,
                ShipSpeed = c.ShipShowSpeed.Value,
                ShipWind = c.ShipShowWind.Value,
                ShipSail = c.ShipShowSail.Value,
                ShipOpacity = c.ShipOpacity.Value,
                ShipX = c.ShipPosX.Value,
                ShipY = c.ShipPosY.Value,
                ShowTime = c.ShowTimeWidget.Value,
                TimeDay = c.TimeShowDay.Value,
                TimeClock = c.TimeShowClock.Value,
                TimeSunEvent = c.TimeShowSunEvent.Value,
                TimeOpacity = c.TimeOpacity.Value,
                TimeX = c.TimePosX.Value,
                TimeY = c.TimePosY.Value
            };
        }

        internal void Apply(ModConfig c)
        {
            c.SeparateBlocks.Value = SeparateBlocks;
            c.SnapEnabled.Value = SnapEnabled;
            c.SnapDistance.Value = SnapDistance;
            c.Scale.Value = Scale;
            c.FontSize.Value = FontSize;
            c.PanelWidth.Value = PanelWidth;
            c.BackgroundOpacity.Value = BackgroundOpacity;
            c.ShowSectionHeaders.Value = SectionHeaders;
            c.PosX.Value = PosX;
            c.PosY.Value = PosY;
            c.TimersPosX.Value = TimersX;
            c.TimersPosY.Value = TimersY;
            c.TimersWidth.Value = TimersWidth;
            c.TimersScale.Value = TimersScale > 0f ? TimersScale : Scale;
            c.DurabilityPosX.Value = DurabilityX;
            c.DurabilityPosY.Value = DurabilityY;
            c.DurabilityWidth.Value = DurabilityWidth;
            c.DurabilityScale.Value = DurabilityScale > 0f ? DurabilityScale : Scale;
            c.ShowRestedTimer.Value = ShowRested;
            c.ShowPowerCooldown.Value = false;
            c.ShowDurability.Value = ShowDurability;
            c.CompactInCombat.Value = CompactInCombat;
            c.DurabilityDisplay.Value = DurabilityMode;
            c.ShowItemIcons.Value = ShowIcons;
            c.SmartDurability.Value = SmartMode;
            c.SmartDurabilityThreshold.Value = SmartThreshold;
            c.ShowWorldHoverTimers.Value = WorldTimers;
            c.ShowPickableHoverTimers.Value = Pickables;
            c.ShowPlantHoverTimers.Value = Plants;
            c.PickableHoverOpacity.Value = PickableOpacity;
            c.PlantHoverOpacity.Value = PlantOpacity;
            c.ShowBeehiveHoverTimers.Value = Beehives;
            c.ShowFermenterHoverTimers.Value = Fermenters;
            c.ShowProductionHoverTimers.Value = ProductionStations;
            c.ShowProductionOnWholeStation.Value = ProductionWholeStationHover;
            c.BeehiveHoverOpacity.Value = BeehiveOpacity;
            c.FermenterHoverOpacity.Value = FermenterOpacity;
            c.ProductionHoverOpacity.Value = ProductionOpacity;
            c.ShowFpsWidget.Value = ShowFps;
            c.FpsShowValue.Value = FpsValue;
            c.FpsShowLabel.Value = FpsLabel;
            c.FpsOpacity.Value = FpsOpacity;
            c.FpsPosX.Value = FpsX;
            c.FpsPosY.Value = FpsY;
            c.ShowPingWidget.Value = ShowPing;
            c.PingShowValue.Value = PingValue;
            c.PingShowLabel.Value = PingLabel;
            c.PingOpacity.Value = PingOpacity;
            c.PingPosX.Value = PingX;
            c.PingPosY.Value = PingY;
            c.ShowShipWidget.Value = ShowShip;
            c.ShipShowHealth.Value = ShipHealth;
            c.ShipShowSpeed.Value = ShipSpeed;
            c.ShipShowWind.Value = ShipWind;
            c.ShipShowSail.Value = ShipSail;
            c.ShipOpacity.Value = ShipOpacity > 0f ? ShipOpacity : 0.72f;
            c.ShipPosX.Value = ShipX;
            c.ShipPosY.Value = ShipY;
            c.ShowTimeWidget.Value = ShowTime;
            c.TimeShowDay.Value = TimeDay;
            c.TimeShowClock.Value = TimeClock;
            c.TimeShowSunEvent.Value = TimeSunEvent;
            c.TimeOpacity.Value = TimeOpacity > 0f ? TimeOpacity : 0.72f;
            c.TimePosX.Value = TimeX;
            c.TimePosY.Value = TimeY;
            c.ActiveProfile.Value = Name;
            c.Save();
        }

        internal string ExportCode()
        {
            string raw = string.Join("\t", new[]
            {
                "IHUD6",
                Enc(Name),
                B(SeparateBlocks), B(SnapEnabled), SnapDistance.ToString(),
                F(Scale), FontSize.ToString(), PanelWidth.ToString(), F(BackgroundOpacity), B(SectionHeaders),
                PosX.ToString(), PosY.ToString(),
                TimersX.ToString(), TimersY.ToString(), TimersWidth.ToString(),
                DurabilityX.ToString(), DurabilityY.ToString(), DurabilityWidth.ToString(),
                B(ShowRested), B(ShowPower), B(ShowDurability), B(CompactInCombat),
                ((int)DurabilityMode).ToString(), B(ShowIcons), ((int)SmartMode).ToString(), SmartThreshold.ToString(),
                B(WorldTimers), B(Pickables), B(Plants), F(PickableOpacity), F(PlantOpacity),
                B(Beehives), B(Fermenters), B(ProductionStations),
                F(BeehiveOpacity), F(FermenterOpacity), F(ProductionOpacity),
                B(ShowFps), B(FpsValue), B(FpsLabel), F(FpsOpacity), FpsX.ToString(), FpsY.ToString(),
                B(ShowPing), B(PingValue), B(PingLabel), F(PingOpacity), PingX.ToString(), PingY.ToString(),
                F(TimersScale), F(DurabilityScale),
                B(ShowShip), B(ShipHealth), B(ShipSpeed), B(ShipWind), B(ShipSail), F(ShipOpacity), ShipX.ToString(), ShipY.ToString(),
                B(ShowTime), B(TimeDay), B(TimeClock), B(TimeSunEvent), F(TimeOpacity), TimeX.ToString(), TimeY.ToString(),
                B(ProductionWholeStationHover)
            });

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        }

        internal static HudProfile ImportCode(string code, bool builtIn)
        {
            string raw = Encoding.UTF8.GetString(Convert.FromBase64String(code.Trim()));
            string[] p = raw.Split('\t');

            if (p.Length < 43 || (p[0] != "IHUD4" && p[0] != "IHUD5" && p[0] != "IHUD6"))
                throw new FormatException("Unsupported profile code.");

            bool v5Plus = p[0] == "IHUD5" || p[0] == "IHUD6";
            bool v6 = p[0] == "IHUD6";

            int i = 1;
            var v = new HudProfile();
            v.Name = Dec(p[i++]);
            v.BuiltIn = builtIn;
            v.SeparateBlocks = PB(p[i++]);
            v.SnapEnabled = PB(p[i++]);
            v.SnapDistance = PI(p[i++], 12);
            v.Scale = PF(p[i++], 0.9f);
            v.FontSize = PI(p[i++], 12);
            v.PanelWidth = PI(p[i++], 280);
            v.BackgroundOpacity = PF(p[i++], 0.6f);
            v.SectionHeaders = PB(p[i++]);
            v.PosX = PI(p[i++], 20);
            v.PosY = PI(p[i++], 220);
            v.TimersX = PI(p[i++], 20);
            v.TimersY = PI(p[i++], 220);
            v.TimersWidth = PI(p[i++], 250);
            v.DurabilityX = PI(p[i++], 20);
            v.DurabilityY = PI(p[i++], 330);
            v.DurabilityWidth = PI(p[i++], 280);
            v.ShowRested = PB(p[i++]);
            v.ShowPower = PB(p[i++]);
            v.ShowDurability = PB(p[i++]);
            v.CompactInCombat = PB(p[i++]);
            v.DurabilityMode = (DurabilityValueMode)PI(p[i++], 0);
            v.ShowIcons = PB(p[i++]);
            v.SmartMode = (SmartDurabilityMode)PI(p[i++], 0);
            v.SmartThreshold = PI(p[i++], 35);
            v.WorldTimers = PB(p[i++]);
            v.Pickables = PB(p[i++]);
            v.Plants = PB(p[i++]);
            v.PickableOpacity = PF(p[i++], 0.95f);
            v.PlantOpacity = PF(p[i++], 0.95f);

            if (v5Plus)
            {
                if (p.Length < 49)
                    throw new FormatException("Incomplete IHUD5 profile code.");

                v.Beehives = PB(p[i++]);
                v.Fermenters = PB(p[i++]);
                v.ProductionStations = PB(p[i++]);
                v.BeehiveOpacity = PF(p[i++], 0.95f);
                v.FermenterOpacity = PF(p[i++], 0.95f);
                v.ProductionOpacity = PF(p[i++], 0.95f);
                v.ProductionWholeStationHover = true;
            }
            else
            {
                v.Beehives = true;
                v.Fermenters = true;
                v.ProductionStations = true;
                v.BeehiveOpacity = 0.95f;
                v.FermenterOpacity = 0.95f;
                v.ProductionOpacity = 0.95f;
                v.ProductionWholeStationHover = true;
            }

            v.ShowFps = PB(p[i++]);
            v.FpsValue = PB(p[i++]);
            v.FpsLabel = PB(p[i++]);
            v.FpsOpacity = PF(p[i++], 0.72f);
            v.FpsX = PI(p[i++], 20);
            v.FpsY = PI(p[i++], 80);
            v.ShowPing = PB(p[i++]);
            v.PingValue = PB(p[i++]);
            v.PingLabel = PB(p[i++]);
            v.PingOpacity = PF(p[i++], 0.72f);
            v.PingX = PI(p[i++], 100);
            v.PingY = PI(p[i++], 80);

            if (v6)
            {
                if (p.Length < 66)
                    throw new FormatException("Incomplete IHUD6 profile code.");

                v.TimersScale = PF(p[i++], v.Scale);
                v.DurabilityScale = PF(p[i++], v.Scale);
                v.ShowShip = PB(p[i++]);
                v.ShipHealth = PB(p[i++]);
                v.ShipSpeed = PB(p[i++]);
                v.ShipWind = PB(p[i++]);
                v.ShipSail = PB(p[i++]);
                v.ShipOpacity = PF(p[i++], 0.72f);
                v.ShipX = PI(p[i++], 20);
                v.ShipY = PI(p[i++], 155);
                v.ShowTime = PB(p[i++]);
                v.TimeDay = PB(p[i++]);
                v.TimeClock = PB(p[i++]);
                v.TimeSunEvent = PB(p[i++]);
                v.TimeOpacity = PF(p[i++], 0.72f);
                v.TimeX = PI(p[i++], 180);
                v.TimeY = PI(p[i++], 80);

                // IHUD6 may include the optional whole-station production-hover flag.
                // Older IHUD6 codes remain valid and keep the current default.
                if (i < p.Length)
                    v.ProductionWholeStationHover = PB(p[i++]);
            }
            else
            {
                v.TimersScale = v.Scale;
                v.DurabilityScale = v.Scale;
                v.ShowShip = false;
                v.ShipHealth = true;
                v.ShipSpeed = true;
                v.ShipWind = true;
                v.ShipSail = true;
                v.ShipOpacity = 0.72f;
                v.ShipX = 20;
                v.ShipY = 155;
                v.ShowTime = false;
                v.TimeDay = true;
                v.TimeClock = true;
                v.TimeSunEvent = true;
                v.TimeOpacity = 0.72f;
                v.TimeX = 180;
                v.TimeY = 80;
            }

            return v;
        }

        private static string B(bool v) { return v ? "1" : "0"; }
        private static bool PB(string v) { return v == "1" || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase); }
        private static string F(float v) { return v.ToString("0.###", CultureInfo.InvariantCulture); }

        private static float PF(string v, float fallback)
        {
            float result;
            return float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                ? result
                : fallback;
        }

        private static int PI(string v, int fallback)
        {
            int result;
            return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                ? result
                : fallback;
        }

        private static string Enc(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static string Dec(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
    }
}
