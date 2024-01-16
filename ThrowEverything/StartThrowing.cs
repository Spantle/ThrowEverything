using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ThrowEverything
{
    public class StartThrowing
    {
        public bool isHeld = false;
        public DateTime startTime;

        public bool hasEnded = false; // run out of stamina
        public DateTime endTime;

        public bool isComplete = false;

        public void Start()
        {
            startTime = DateTime.Now;
            isHeld = true;
            hasEnded = false;
            isComplete = false;
        }

        public void Stop()
        {
            isHeld = false;
            hasEnded = false;
            isComplete = false;
        }

        public void End()
        {
            if (!hasEnded)
            {
                hasEnded = true;
                endTime = DateTime.Now;
            }
        }

        private float GetTime()
        {
            if (hasEnded) return (float)(endTime - startTime).TotalMilliseconds;
            DateTime now = DateTime.Now;
            return (float)(now - startTime).TotalMilliseconds;
        }

        public float GetPercentage(GrabbableObject __instance)
        {
            float itemWeight = GrabbableObject_Patch.ItemWeight(__instance);
            float p = Math.Clamp(GetTime() / Math.Clamp((int)Math.Round(itemWeight * 2000), 500, 2000), 0, 1);

            if (isHeld && p == 1)
            {
                isComplete = true;
            }

            return p;
        }
    }
}
