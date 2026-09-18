using Inject0rHUD.Models;
using UnityEngine;

namespace Inject0rHUD.Services
{
    internal static class WorldTimeService
    {
        private const float SunriseFraction = 0.25f;
        private const float SunsetFraction = 0.75f;

        internal static WorldTimeEntry Read()
        {
            try
            {
                EnvMan env = EnvMan.instance;
                if (env == null || ZNet.instance == null)
                    return WorldTimeEntry.Empty;

                float fraction = Mathf.Repeat(env.GetDayFraction(), 1f);
                float dayLength = Mathf.Max(1f, env.m_dayLengthSec);
                int day = Mathf.Max(1, env.GetDay());

                string nextEvent;
                float delta;

                if (fraction < SunriseFraction)
                {
                    nextEvent = "sunrise";
                    delta = SunriseFraction - fraction;
                }
                else if (fraction < SunsetFraction)
                {
                    nextEvent = "sunset";
                    delta = SunsetFraction - fraction;
                }
                else
                {
                    nextEvent = "sunrise";
                    delta = 1f - fraction + SunriseFraction;
                }

                return new WorldTimeEntry(
                    true,
                    day,
                    fraction,
                    dayLength,
                    nextEvent,
                    delta * dayLength);
            }
            catch
            {
                return WorldTimeEntry.Empty;
            }
        }
    }
}
