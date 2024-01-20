using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using ThrowEverything.Models;
using ThrowEverything.Patches;
using UnityEngine;

namespace ThrowEverything.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerControllerB_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        static void Update(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.localPlayerController != __instance)
            {
                return;
            }

            ChargingThrow chargingThrow = State.GetChargingThrow();
            if (chargingThrow.isCharging)
            {
                chargingThrow.DrawLandingCircle();

                if (!chargingThrow.hasFullyCharged)
                {
                    __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - (0.01f / 4), 0f, 1f);
                }
                else
                {
                    __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - (0.01f / 8), 0f, 1f);
                }
            }

            if (__instance.sprintMeter < 0.3f || __instance.isExhausted)
            {
                __instance.isExhausted = true;
                chargingThrow.Exhausted();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "ScrollMouse_performed")]
        static void ScrollMouse_performed(PlayerControllerB __instance)
        {
            if (State.GetChargingThrow().isCharging)
            {
                __instance.isGrabbingObjectAnimation = false; // put it back (hopefully to the way it was)
            }

            if (Utils.CanUseItem(__instance))
            {
                // even though switching items will probably run this, we do it just in case of a two handed item where you cannot switch items
                Throwable throwable = State.GetHeldThrowable();
                if (throwable != null)
                {
                    GrabbableObject item = throwable.GetItem();
                    State.ClearHeldThrowable();
                    State.SetHeldThrowable(item);
                }
            }
        }
    }
}
