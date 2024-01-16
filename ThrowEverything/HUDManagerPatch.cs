using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ThrowEverything
{
    [HarmonyPatch(typeof(HUDManager))]
    public static class HUDManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDManager), "Update")]
        public static void Update(HUDManager __instance)
        {
            if (GrabbableObject_Patch.throwableItem == null || GrabbableObject_Patch.throwableItem.item == null)
            {
                return;
            }

            GrabbableObject_Patch.SetControlTipsForItem(GrabbableObject_Patch.throwableItem.item);
        }
    }
}
