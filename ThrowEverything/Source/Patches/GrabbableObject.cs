using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using ThrowEverything.Models;
using UnityEngine;

namespace ThrowEverything.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    public class GrabbableObject_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "EquipItem")]
        static void EquipItem(GrabbableObject __instance)
        {
            if (__instance.IsOwner)
            {
                Plugin.Logger.LogInfo($"equipped {Utils.Name(__instance)}");

                State.ClearHeldThrowable();
                State.SetHeldThrowable(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "PocketItem")]
        static void PocketItem(GrabbableObject __instance)
        {
            if (__instance.IsOwner)
            {
                Plugin.Logger.LogInfo($"pocketed {Utils.Name(__instance)}");

                State.ClearHeldThrowable();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "DiscardItem")]
        static void DiscardItem(GrabbableObject __instance)
        {
            if (__instance.IsOwner)
            {
                Plugin.Logger.LogInfo($"discarded {Utils.Name(__instance)}");

                State.ClearHeldThrowable();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "SetControlTipsForItem")]
        static void SetControlTipsForItem(GrabbableObject __instance)
        {
            ControlTips.Set(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "Update")]
        static void Update(GrabbableObject __instance)
        {
            ThrownItems thrownItems = State.GetThrownItems();
            if (!thrownItems.thrownItemsDict.TryGetValue(__instance.GetInstanceID(), out ThrownItem thrownItem))
            {
                return;
            }

            thrownItems.Update(thrownItem);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GrabbableObject), "FallWithCurve")]
        static bool FallWithCurve(GrabbableObject __instance)
        {
            // borrowed (and slightly modified) from lethal company
            float magnitude = (__instance.startFallingPosition - __instance.targetFloorPosition).magnitude;
            __instance.transform.rotation = Quaternion.Lerp(__instance.transform.rotation, Quaternion.Euler(__instance.itemProperties.restingRotation.x, __instance.transform.eulerAngles.y, __instance.itemProperties.restingRotation.z), 14f * Time.deltaTime / magnitude);
            __instance.transform.localPosition = Vector3.Lerp(__instance.startFallingPosition, __instance.targetFloorPosition, FallCurve.fallCurve.Evaluate(__instance.fallTime));
            if (magnitude > 5f)
            {
                __instance.transform.localPosition = Vector3.Lerp(new Vector3(__instance.transform.localPosition.x, __instance.startFallingPosition.y, __instance.transform.localPosition.z), new Vector3(__instance.transform.localPosition.x, __instance.targetFloorPosition.y, __instance.transform.localPosition.z), FallCurve.verticalFallCurveNoBounce.Evaluate(__instance.fallTime));
            }
            else
            {
                __instance.transform.localPosition = Vector3.Lerp(new Vector3(__instance.transform.localPosition.x, __instance.startFallingPosition.y, __instance.transform.localPosition.z), new Vector3(__instance.transform.localPosition.x, __instance.targetFloorPosition.y, __instance.transform.localPosition.z), FallCurve.verticalFallCurve.Evaluate(__instance.fallTime));
            }
            __instance.fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);

            return false;
        }
    }
}