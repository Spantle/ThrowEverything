using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThrowEverything
{
    public class ThrownItem
    {
        public readonly GrabbableObject item;
        public readonly int id;
        public readonly PlayerControllerB thrower;
        public readonly List<IHittable> hits = new();
        public readonly Vector3 forward;
        public readonly float percentage;

        public readonly DateTime createdAt = DateTime.Now;

        public ThrownItem(GrabbableObject item, float percentage)
        {
            this.item = item;
            this.percentage = percentage;
            id = item.GetInstanceID();
            thrower = item.playerHeldBy;
            forward = item.playerHeldBy.gameplayCamera.transform.forward;
        }

        public bool Panic()
        {
            return (DateTime.Now - createdAt).TotalMilliseconds >= 3000;
        }
    }
}
