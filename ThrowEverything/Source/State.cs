using System;
using System.Collections.Generic;
using System.Text;
using ThrowEverything.Models;

namespace ThrowEverything
{
    internal class State
    {
        static Throwable heldThrowable = null;
        static readonly ChargingThrow chargingThrow = new();
        static readonly ThrownItems thrownItems = new();

        internal static void SetHeldThrowable(GrabbableObject item)
        {
            heldThrowable = new(item, item.playerHeldBy); // just in case it changes somehow
            heldThrowable.HookEvents();
        }

        internal static void ClearHeldThrowable()
        {
            chargingThrow.Stop();

            if (heldThrowable != null)
            {
                heldThrowable.UnhookEvents();
                heldThrowable = null;
            }
        }

        internal static Throwable GetHeldThrowable()
        {
            return heldThrowable;
        }

        internal static ChargingThrow GetChargingThrow()
        {
            return chargingThrow;
        }

        internal static ThrownItems GetThrownItems()
        {
            return thrownItems;
        }
    }
}
