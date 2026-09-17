using System;
using System.Collections.Generic;
using System.Reflection;
using Inject0rHUD.Models;
using Inject0rHUD.Util;

namespace Inject0rHUD.Services
{
    internal static class DurabilityService
    {
        private static bool _currentWeaponResolved;
        private static MethodInfo _getCurrentWeapon;

        internal static IEnumerable<DurabilityEntry> Read(Player player)
        {
            var result = new List<DurabilityEntry>(10);
            if (player == null) return result;

            Inventory inventory = player.GetInventory();
            if (inventory == null) return result;

            List<ItemDrop.ItemData> items = inventory.GetEquippedItems();
            if (items == null) return result;

            ItemDrop.ItemData current = GetCurrentItem(player);
            var seen = new HashSet<ItemDrop.ItemData>();

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];
                if (item == null || item.m_shared == null) continue;
                if (!seen.Add(item)) continue;
                if (!item.m_shared.m_useDurability) continue;

                float max = item.GetMaxDurability();
                if (max <= 0f) continue;

                float currentDurability = item.m_durability;
                if (currentDurability < 0f) currentDurability = 0f;
                if (currentDurability > max) currentDurability = max;

                result.Add(new DurabilityEntry(
                    LocalizationUtil.Localize(item.m_shared.m_name, "Item"),
                    currentDurability,
                    max,
                    SlotName(item.m_shared.m_itemType),
                    ItemIconService.GetIcon(item),
                    ReferenceEquals(item, current)));
            }

            result.Sort((a, b) =>
            {
                int slotCompare = string.Compare(a.Slot, b.Slot, StringComparison.Ordinal);
                return slotCompare != 0
                    ? slotCompare
                    : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            return result;
        }

        private static ItemDrop.ItemData GetCurrentItem(Player player)
        {
            try
            {
                if (!_currentWeaponResolved)
                {
                    _currentWeaponResolved = true;
                    _getCurrentWeapon = player.GetType().GetMethod(
                        "GetCurrentWeapon",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null);
                }

                return _getCurrentWeapon != null
                    ? _getCurrentWeapon.Invoke(player, null) as ItemDrop.ItemData
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static string SlotName(ItemDrop.ItemData.ItemType type)
        {
            switch (type)
            {
                case ItemDrop.ItemData.ItemType.Helmet: return "1 Armor";
                case ItemDrop.ItemData.ItemType.Chest: return "1 Armor";
                case ItemDrop.ItemData.ItemType.Legs: return "1 Armor";
                case ItemDrop.ItemData.ItemType.Shoulder: return "1 Armor";
                case ItemDrop.ItemData.ItemType.Shield: return "2 Shield";
                case ItemDrop.ItemData.ItemType.Tool: return "3 Tool";
                default: return "4 Weapon";
            }
        }
    }
}
