using System.Collections.Generic;
using UnityEngine;

namespace Inject0rHUD.Models
{
    internal sealed class TimerEntry
    {
        internal readonly string Name;
        internal readonly float RemainingSeconds;
        internal readonly float TotalSeconds;
        internal readonly string Kind;

        internal TimerEntry(string name, float remainingSeconds, float totalSeconds, string kind)
        {
            Name = name ?? string.Empty;
            RemainingSeconds = remainingSeconds < 0f ? 0f : remainingSeconds;
            TotalSeconds = totalSeconds < 0f ? 0f : totalSeconds;
            Kind = kind ?? string.Empty;
        }

        internal float Fraction
        {
            get
            {
                if (TotalSeconds <= 0f) return -1f;
                return Mathf.Clamp01(RemainingSeconds / TotalSeconds);
            }
        }
    }

    internal sealed class DurabilityEntry
    {
        internal readonly string Name;
        internal readonly float Current;
        internal readonly float Maximum;
        internal readonly string Slot;
        internal readonly Sprite Icon;
        internal readonly bool IsCurrentItem;

        internal DurabilityEntry(
            string name,
            float current,
            float maximum,
            string slot,
            Sprite icon,
            bool isCurrentItem)
        {
            Name = name ?? string.Empty;
            Current = current < 0f ? 0f : current;
            Maximum = maximum < 0f ? 0f : maximum;
            Slot = slot ?? string.Empty;
            Icon = icon;
            IsCurrentItem = isCurrentItem;
        }

        internal float Fraction
        {
            get
            {
                if (Maximum <= 0f) return 1f;
                return Mathf.Clamp01(Current / Maximum);
            }
        }
    }

    internal sealed class ShipContextEntry
    {
        internal static readonly ShipContextEntry Empty = new ShipContextEntry(
            false, string.Empty, 0f, -1f, string.Empty, 0f, 0f, 0f, 0f);

        internal readonly bool Active;
        internal readonly string Name;
        internal readonly float SpeedMetersPerSecond;
        internal readonly float HealthFraction;
        internal readonly string Propulsion;
        internal readonly float WindAngleDegrees;
        internal readonly float WindFactor;
        internal readonly float WindIntensity;
        internal readonly float Rudder;

        internal ShipContextEntry(
            bool active,
            string name,
            float speedMetersPerSecond,
            float healthFraction,
            string propulsion,
            float windAngleDegrees,
            float windFactor,
            float windIntensity,
            float rudder)
        {
            Active = active;
            Name = name ?? string.Empty;
            SpeedMetersPerSecond = speedMetersPerSecond;
            HealthFraction = healthFraction;
            Propulsion = propulsion ?? string.Empty;
            WindAngleDegrees = windAngleDegrees;
            WindFactor = Mathf.Clamp01(windFactor);
            WindIntensity = Mathf.Clamp01(windIntensity);
            Rudder = Mathf.Clamp(rudder, -1f, 1f);
        }
    }

    internal sealed class WorldTimeEntry
    {
        internal static readonly WorldTimeEntry Empty = new WorldTimeEntry(
            false, 0, 0f, 0f, string.Empty, 0f);

        internal readonly bool Available;
        internal readonly int Day;
        internal readonly float DayFraction;
        internal readonly float DayLengthSeconds;
        internal readonly string NextEvent;
        internal readonly float SecondsToEvent;

        internal WorldTimeEntry(
            bool available,
            int day,
            float dayFraction,
            float dayLengthSeconds,
            string nextEvent,
            float secondsToEvent)
        {
            Available = available;
            Day = day < 0 ? 0 : day;
            DayFraction = Mathf.Repeat(dayFraction, 1f);
            DayLengthSeconds = Mathf.Max(1f, dayLengthSeconds);
            NextEvent = nextEvent ?? string.Empty;
            SecondsToEvent = Mathf.Max(0f, secondsToEvent);
        }
    }

    internal sealed class HudSnapshot
    {
        internal static readonly HudSnapshot Empty =
            new HudSnapshot(
                new List<TimerEntry>(),
                new List<DurabilityEntry>(),
                false,
                0,
                0,
                ShipContextEntry.Empty,
                WorldTimeEntry.Empty);

        internal readonly List<TimerEntry> Timers;
        internal readonly List<DurabilityEntry> Durability;
        internal readonly bool InCombat;
        internal readonly int Fps;
        internal readonly int PingMs;
        internal readonly ShipContextEntry Ship;
        internal readonly WorldTimeEntry WorldTime;

        internal HudSnapshot(
            List<TimerEntry> timers,
            List<DurabilityEntry> durability,
            bool inCombat,
            int fps,
            int pingMs,
            ShipContextEntry ship,
            WorldTimeEntry worldTime)
        {
            Timers = timers ?? new List<TimerEntry>();
            Durability = durability ?? new List<DurabilityEntry>();
            InCombat = inCombat;
            Fps = fps < 0 ? 0 : fps;
            PingMs = pingMs < 0 ? 0 : pingMs;
            Ship = ship ?? ShipContextEntry.Empty;
            WorldTime = worldTime ?? WorldTimeEntry.Empty;
        }
    }
}
