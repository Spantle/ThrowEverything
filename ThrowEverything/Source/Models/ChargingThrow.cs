using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThrowEverything.Patches;
using UnityEngine;

namespace ThrowEverything.Models
{
    internal class ChargingThrow
    {
        const int MIN_CHARGING_TIME = 500;
        const int MAX_CHARGING_TIME = 2000;

        internal bool isCharging = false;
        internal DateTime startChargingTime;

        internal bool hasRunOutOfStamina = false;
        internal DateTime runOutOfStaminaTime;

        internal bool hasFullyCharged = false;

        GameObject preview;

        internal void StartCharging()
        {
            isCharging = true;
            startChargingTime = DateTime.Now;

            hasRunOutOfStamina = false;
            hasFullyCharged = false;
        }

        internal void Stop()
        {
            isCharging = false;
            hasRunOutOfStamina = false;
            hasFullyCharged = false;

            UnityEngine.Object.Destroy(preview);
        }

        internal void Exhausted()
        {
            if (!hasRunOutOfStamina)
            {
                hasRunOutOfStamina = true;
                runOutOfStaminaTime = DateTime.Now;
            }
        }

        private float GetTime()
        {
            if (hasRunOutOfStamina)
            {
                return (float)(runOutOfStaminaTime - startChargingTime).TotalMilliseconds;
            }
            else
            {
                DateTime now = DateTime.Now;
                return (float)(now - startChargingTime).TotalMilliseconds;
            }
        }

        internal float GetChargeDecimal()
        {
            float itemWeight = Utils.ItemWeight(State.GetHeldThrowable().GetItem());
            float percentage = Math.Clamp(GetTime() / Math.Clamp((int)Math.Round(itemWeight * MAX_CHARGING_TIME), MIN_CHARGING_TIME, MAX_CHARGING_TIME), 0, 1);

            if (isCharging && percentage == 1)
            {
                hasFullyCharged = true;
            }

            return percentage;
        }

        internal int GetChargedPercentage()
        {
            return (int)Math.Floor(GetChargeDecimal() * 100);
        }

        internal void DrawLandingCircle()
        {
            if (preview == null)
            {
                preview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                preview.layer = 6;
                preview.GetComponent<Renderer>().material = ShipBuildModeManager.Instance.ghostObjectGreen;
            }

            Throwable throwable = State.GetHeldThrowable();
            ChargingThrow chargingThrow = State.GetChargingThrow();
            GrabbableObject item = throwable.GetItem();

            float m = Utils.ItemScale(item);
            preview.transform.localScale = new Vector3(m, m, m);
            preview.transform.position = Utils.GetItemThrowDestination(item, throwable.GetThrower(), chargingThrow.GetChargeDecimal());
        }
    }
}
