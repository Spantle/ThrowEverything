using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using ThrowEverything.Models;
using ThrowEverything.Patches;

namespace ThrowEverything.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    public class HUDManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDManager), "Update")]
        static void Update(HUDManager __instance)
        {
            Throwable heldThrowable = State.GetHeldThrowable();
            if (heldThrowable == null)
            {
                return;
            }

            ControlTips.Set(heldThrowable.GetItem());
        }
    }
}
