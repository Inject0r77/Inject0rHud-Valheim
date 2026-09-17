using System;
using System.Reflection;
using UnityEngine;

namespace Inject0rHUD.Services
{
    internal static class ItemIconService
    {
        private static MethodInfo _getIconMethod;
        private static bool _resolved;

        internal static Sprite GetIcon(ItemDrop.ItemData item)
        {
            if (item == null)
                return null;

            try
            {
                Resolve(item.GetType());

                if (_getIconMethod != null)
                {
                    object value = _getIconMethod.Invoke(item, null);
                    Sprite sprite = value as Sprite;
                    if (sprite != null)
                        return sprite;
                }

                if (item.m_shared != null &&
                    item.m_shared.m_icons != null &&
                    item.m_shared.m_icons.Length > 0)
                {
                    int variant = item.m_variant;
                    if (variant < 0 || variant >= item.m_shared.m_icons.Length)
                        variant = 0;

                    return item.m_shared.m_icons[variant];
                }
            }
            catch
            {
            }

            return null;
        }

        private static void Resolve(Type type)
        {
            if (_resolved)
                return;

            _resolved = true;
            _getIconMethod = type.GetMethod(
                "GetIcon",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);
        }
    }
}
