using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThrowEverything.Models
{
    internal class ThrownItem
    {
        readonly GrabbableObject item;
        readonly PlayerControllerB thrower; // this was funny as fuck at like 1am
        readonly float chargeDecimal;
        readonly float markiplier; // same here (markiplier = multiplier = power * charge)
        
        readonly DateTime thrownAt;
        readonly List<IHittable> hits = new();

        internal ThrownItem(GrabbableObject item, PlayerControllerB thrower, float chargeDecimal, float markiplier)
        {
            this.item = item;
            this.thrower = thrower;
            this.chargeDecimal = chargeDecimal;
            this.markiplier = markiplier;

            thrownAt = DateTime.Now;
        }

        internal GrabbableObject GetItem()
        {
            return item;
        }

        internal PlayerControllerB GetThrower()
        {
            return thrower;
        }

        internal float GetMarkiplier()
        {
            return markiplier;
        }

        internal float GetChargeDecimal()
        {
            return chargeDecimal;
        }

        internal bool CheckIfHitOrAdd(IHittable hittable)
        {
            bool hasHit = hits.Contains(hittable);
            if (hasHit)
            {
                return true;
            }
            else
            {
                hits.Add(hittable);
                return false;
            }
        }

        // we've been airborne for too long - something's probably wrong
        internal bool IsPanicking()
        {
            return (DateTime.Now - thrownAt).TotalMilliseconds >= 5000;
        }

        internal void LandAndRemove()
        {
            float loudness = 1 - (1 - markiplier) * (1 - markiplier);
            Plugin.Logger.LogInfo($"playing sound for {item.name} at markiplier {loudness}");
            RoundManager.Instance.PlayAudibleNoise(item.transform.position, Math.Clamp(loudness * 50, 8f, 50f), Math.Clamp(loudness, 0.5f, 1f), 0, item.isInElevator && StartOfRound.Instance.hangarDoorsClosed, 941);

            State.GetThrownItems().thrownItemsDict.Remove(item.GetInstanceID());
        }
    }
}
