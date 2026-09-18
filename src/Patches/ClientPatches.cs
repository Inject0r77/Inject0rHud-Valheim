using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Inject0rHUD.Services;
using UnityEngine;

namespace Inject0rHUD.Patches
{
    [HarmonyPatch(typeof(Pickable), "GetHoverText")]
    internal static class PickableHoverTimerPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Pickable __instance, ref string __result)
        {
            if (!Plugin.PickableHoverTimersEnabled)
                return;

            __result = WorldHoverTimerService.AppendPickableTimer(__instance, __result);
        }
    }

    [HarmonyPatch(typeof(Plant), "GetHoverText")]
    internal static class PlantHoverTimerPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Plant __instance, ref string __result)
        {
            if (!Plugin.PlantHoverTimersEnabled)
                return;

            __result = WorldHoverTimerService.AppendPlantTimer(__instance, __result);
        }
    }

    [HarmonyPatch(typeof(Beehive), "GetHoverText")]
    internal static class BeehiveHoverInfoPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Beehive __instance, ref string __result)
        {
            if (!Plugin.BeehiveHoverTimersEnabled)
                return;

            __result = ProductionHoverService.AppendBeehive(__instance, __result);
        }
    }

    [HarmonyPatch(typeof(Fermenter), "GetHoverText")]
    internal static class FermenterHoverInfoPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Fermenter __instance, ref string __result)
        {
            if (!Plugin.FermenterHoverTimersEnabled)
                return;

            __result = ProductionHoverService.AppendFermenter(__instance, __result);
        }
    }

    // Valheim 1.0.x does not expose Smelter as a Hoverable. The visible
    // tooltip comes from its Switch objects, which call these private Smelter
    // callbacks. Patch the callbacks directly so production info is appended
    // regardless of which smelter interaction point the player is aiming at.
    [HarmonyPatch(typeof(Smelter), "OnHoverAddOre")]
    internal static class ProductionHoverAddOrePatch
    {
        [HarmonyPostfix]
        private static void Postfix(Smelter __instance, ref string __result)
        {
            if (!Plugin.ProductionHoverTimersEnabled)
                return;

            __result = ProductionHoverService.AppendProduction(__instance, __result);
        }
    }

    [HarmonyPatch(typeof(Smelter), "OnHoverAddFuel")]
    internal static class ProductionHoverAddFuelPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Smelter __instance, ref string __result)
        {
            if (!Plugin.ProductionHoverTimersEnabled)
                return;

            __result = ProductionHoverService.AppendProduction(__instance, __result);
        }
    }

    [HarmonyPatch(typeof(Smelter), "OnHoverEmptyOre")]
    internal static class ProductionHoverEmptyOrePatch
    {
        [HarmonyPostfix]
        private static void Postfix(Smelter __instance, ref string __result)
        {
            if (!Plugin.ProductionHoverTimersEnabled)
                return;

            __result = ProductionHoverService.AppendProduction(__instance, __result);
        }
    }


    // Optional wide-hover mode for Smelter-based stations. Native
    // interaction-point mode is already covered by the three Smelter callbacks
    // above. When the user enables whole-station hover, this HUD-level fallback
    // also resolves plain station colliders (body/output opening/etc.) to their
    // parent Smelter and shows the same read-only production block.
    [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
    internal static class ProductionHoverCentralOutputPatch
    {
        private static readonly FieldInfo HoverNameField =
            AccessTools.Field(typeof(Hud), "m_hoverName");

        [HarmonyPostfix]
        private static void Postfix(Hud __instance, Player player)
        {
            if (!Plugin.ProductionWholeStationHoverEnabled || __instance == null || player == null)
                return;

            GameObject hoverObject = player.GetHoverObject();
            if (hoverObject == null)
                return;

            Smelter smelter = hoverObject.GetComponentInParent<Smelter>();
            if (smelter == null)
                smelter = hoverObject.GetComponent<Smelter>();
            if (smelter == null)
                smelter = hoverObject.GetComponentInChildren<Smelter>();
            if (smelter == null)
                return;

            try
            {
                object hoverName = HoverNameField != null
                    ? HoverNameField.GetValue(__instance)
                    : null;
                if (hoverName == null)
                    return;

                PropertyInfo textProperty = hoverName.GetType().GetProperty(
                    "text", BindingFlags.Instance | BindingFlags.Public);
                if (textProperty == null || !textProperty.CanRead || !textProperty.CanWrite)
                    return;

                string current = textProperty.GetValue(hoverName, null) as string ?? string.Empty;
                string updated = ProductionHoverService.AppendProduction(smelter, current);
                if (!string.Equals(current, updated, System.StringComparison.Ordinal))
                    textProperty.SetValue(hoverName, updated, null);
            }
            catch (System.Exception ex)
            {
                Plugin.LogHoverErrorOnce("ProductionCentralHover", ex);
            }
        }
    }

    [HarmonyPatch(typeof(GameCamera), "UpdateMouseCapture")]
    internal static class EditModeCursorPatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            if (!Plugin.IsEditModeActive)
                return true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Player), "TakeInput")]
    internal static class EditModePlayerInputPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref bool __result)
        {
            if (Plugin.IsEditModeActive)
                __result = false;
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.Last)]
    internal static class EditModePlayerControllerInputPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo method = AccessTools.Method(typeof(PlayerController), "TakeInput");
            if (method != null)
                yield return method;
        }

        private static void Postfix(ref bool __result)
        {
            if (Plugin.IsEditModeActive)
                __result = false;
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.Last)]
    internal static class EditModeTextInputVisiblePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo method = AccessTools.Method(typeof(TextInput), "IsVisible");
            if (method != null)
                yield return method;
        }

        private static void Postfix(ref bool __result)
        {
            if (Plugin.IsEditModeActive)
                __result = true;
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    internal static class EditModeMouseButtonPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            string[] names =
            {
                "GetMouseButton",
                "GetMouseButtonDown",
                "GetMouseButtonUp",
                "GetRadialTap",
                "GetRadialMultiTap"
            };

            for (int i = 0; i < names.Length; i++)
            {
                MethodInfo method = AccessTools.Method(typeof(ZInput), names[i]);
                if (method != null)
                    yield return method;
            }
        }

        private static bool Prefix(ref bool __result)
        {
            if (!Plugin.IsEditModeActive)
                return true;

            __result = false;
            return false;
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.Last)]
    internal static class EditModeFloatInputPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            string[] names =
            {
                "GetMouseScrollWheel",
                "GetJoyLeftStickX",
                "GetJoyLeftStickY",
                "GetJoyRightStickX",
                "GetJoyRightStickY",
                "GetJoyRTrigger",
                "GetJoyLTrigger"
            };

            for (int i = 0; i < names.Length; i++)
            {
                MethodInfo method = AccessTools.Method(typeof(ZInput), names[i]);
                if (method != null)
                    yield return method;
            }
        }

        private static void Postfix(ref float __result)
        {
            if (Plugin.IsEditModeActive)
                __result = 0f;
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.Last)]
    internal static class EditModeMouseDeltaPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo method = AccessTools.Method(typeof(ZInput), "GetMouseDelta");
            if (method != null)
                yield return method;
        }

        private static void Postfix(ref Vector2 __result)
        {
            if (Plugin.IsEditModeActive)
                __result = Vector2.zero;
        }
    }
}
