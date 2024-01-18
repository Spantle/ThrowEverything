using BepInEx.Logging;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace ThrowEverything.Models
{
    internal class Throwable
    {
        readonly GrabbableObject item;
        readonly PlayerControllerB thrower;

        internal Throwable(GrabbableObject item, PlayerControllerB thrower)
        {
            this.item = item;
            this.thrower = thrower;
        }

        internal GrabbableObject GetItem()
        {
            return item;
        }

        internal PlayerControllerB GetThrower()
        {
            return thrower;
        }

        private void StartThrowing(CallbackContext ctx)
        {
            State.GetChargingThrow().StartCharging();
        }

        private void Throw(CallbackContext ctx)
        {
            if (item == null || item.playerHeldBy != thrower)
            {
                Plugin.Logger.LogWarning($"tried to throw an invalid item {item == null}");
                State.ClearHeldThrowable();
                return;
            }

            ChargingThrow chargingThrow = State.GetChargingThrow();
            if (!chargingThrow.isCharging)
            {
                // prevents players from switching items while holding down the charge button
                Plugin.Logger.LogInfo($"tried to throw without charging");
                return;
            }

            float chargeDecimal = chargingThrow.GetChargeDecimal();
            float markiplier = Utils.ItemPower(item, chargeDecimal);
            ThrownItem thrownItem = new(item, item.playerHeldBy, chargeDecimal, markiplier);
            State.GetThrownItems().thrownItemsDict.Add(item.GetInstanceID(), thrownItem);


            item.playerHeldBy.DiscardHeldObject(placeObject: true, null, Utils.GetItemThrowDestination(thrownItem));
        }

        internal void HookEvents()
        {
            InputSettings.Instance.ThrowItem.started += StartThrowing;
            InputSettings.Instance.ThrowItem.canceled += Throw;
        }

        internal void UnhookEvents()
        {
            InputSettings.Instance.ThrowItem.started -= StartThrowing;
            InputSettings.Instance.ThrowItem.canceled -= Throw;
        }
    }
}
