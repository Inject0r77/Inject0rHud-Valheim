using System;
using System.Reflection;

namespace Inject0rHUD.Services
{
    internal static class NetworkStatsService
    {
        private static bool _resolved;
        private static MethodInfo _getNetStats;
        private static MethodInfo _getServerPing;

        internal static int GetPingMs()
        {
            object znet = ZNet.instance;
            if (znet == null)
                return 0;

            try
            {
                Resolve(znet.GetType());

                if (_getNetStats != null)
                {
                    object[] args = { 0f, 0f, 0, 0f, 0f };
                    _getNetStats.Invoke(znet, args);

                    if (args[2] is int)
                    {
                        int ping = (int)args[2];
                        if (ping >= 0 && ping < 10000)
                            return ping;
                    }
                }

                if (_getServerPing != null)
                {
                    object value = _getServerPing.Invoke(znet, null);
                    if (value != null)
                    {
                        double seconds = Convert.ToDouble(value);
                        if (seconds >= 0.0 && seconds < 60.0)
                            return (int)Math.Round(seconds * 1000.0);
                    }
                }
            }
            catch
            {
            }

            return 0;
        }

        private static void Resolve(Type type)
        {
            if (_resolved)
                return;

            _resolved = true;

            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            _getNetStats = type.GetMethod(
                "GetNetStats",
                flags,
                null,
                new[]
                {
                    typeof(float).MakeByRefType(),
                    typeof(float).MakeByRefType(),
                    typeof(int).MakeByRefType(),
                    typeof(float).MakeByRefType(),
                    typeof(float).MakeByRefType()
                },
                null);

            _getServerPing = type.GetMethod(
                "GetServerPing",
                flags,
                null,
                Type.EmptyTypes,
                null);
        }
    }
}
