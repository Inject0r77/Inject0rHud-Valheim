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

    // Valheim's camera look is gated separately from Player.TakeInput().
    // Blocking PlayerController.TakeInput plus the local ZInput mouse/gamepad
    // channels makes F10 behave like a real modal editor instead of requiring
    // the pause menu to be opened with Escape first.
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
