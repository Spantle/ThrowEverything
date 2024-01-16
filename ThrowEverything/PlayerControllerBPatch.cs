using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ThrowEverything
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public static class PlayerControllerB_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        public static void Update(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.localPlayerController != __instance)
            {
                return;
            }

            StartThrowing startThrowing = GrabbableObject_Patch.startThrowing;
            if (startThrowing.isHeld && !startThrowing.isComplete)
            {
                __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - (0.01f / 4), 0f, 1f);
            }

            if (__instance.sprintMeter < 0.3f || __instance.isExhausted)
            {
                __instance.isExhausted = true;
                GrabbableObject_Patch.startThrowing.End();
            }
        }
    }
}
