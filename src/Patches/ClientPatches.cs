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

    [HarmonyPatch(typeof(Smelter), "GetHoverText")]
    internal static class ProductionHoverInfoPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Smelter __instance, ref string __result)
        {
            if (!Plugin.ProductionHoverTimersEnabled)
                return;

            __result = ProductionHoverService.AppendProduction(__instance, __result);
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
}
