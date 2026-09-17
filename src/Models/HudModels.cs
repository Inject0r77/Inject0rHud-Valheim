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

    internal sealed class HudSnapshot
    {
        internal static readonly HudSnapshot Empty =
            new HudSnapshot(
                new List<TimerEntry>(),
                new List<DurabilityEntry>(),
                false,
                0,
                0);

        internal readonly List<TimerEntry> Timers;
        internal readonly List<DurabilityEntry> Durability;
        internal readonly bool InCombat;
        internal readonly int Fps;
        internal readonly int PingMs;

        internal HudSnapshot(
            List<TimerEntry> timers,
            List<DurabilityEntry> durability,
            bool inCombat,
            int fps,
            int pingMs)
        {
            Timers = timers ?? new List<TimerEntry>();
            Durability = durability ?? new List<DurabilityEntry>();
            InCombat = inCombat;
            Fps = fps < 0 ? 0 : fps;
            PingMs = pingMs < 0 ? 0 : pingMs;
        }
    }
}
