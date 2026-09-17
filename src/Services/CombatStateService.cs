using System;
using System.Reflection;

namespace Inject0rHUD.Services
{
    internal static class CombatStateService
    {
        private static bool _resolved;
        private static MethodInfo _combatMethod;

        internal static bool IsInCombat(Player player)
        {
            if (player == null)
                return false;

            try
            {
                Resolve(player.GetType());
                if (_combatMethod == null)
                    return false;

                object value = _combatMethod.Invoke(player, null);
                return value is bool && (bool)value;
            }
            catch
            {
                return false;
            }
        }

        private static void Resolve(Type type)
        {
            if (_resolved)
                return;

            _resolved = true;

            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            string[] names = { "InCombat", "IsInCombat", "GetInCombat" };
            for (int i = 0; i < names.Length && _combatMethod == null; i++)
            {
                MethodInfo method = type.GetMethod(
                    names[i], flags, null, Type.EmptyTypes, null);

                if (method != null && method.ReturnType == typeof(bool))
                    _combatMethod = method;
            }
        }
    }
}
