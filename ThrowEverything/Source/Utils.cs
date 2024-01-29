using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using ThrowEverything.Models;
using UnityEngine;

namespace ThrowEverything
{
    internal class Utils
    {
        internal static string Name(GrabbableObject __instance)
        {
            if (__instance == null) return "NULL";
            if (__instance.itemProperties == null) return $"{__instance.name} (props was NULL)";
            return __instance.itemProperties.name;
        }

        internal static float ItemWeight(GrabbableObject __instance)
        {
            float ow = __instance.itemProperties.weight;
            float t = __instance.itemProperties.twoHanded ? 2 : 1;
            float w = Math.Clamp((ow - 1) * t, 0, 1);
            return w;
        }

        internal static float ItemPower(GrabbableObject __instance, float powerDecimal, bool inverse = false)
        {
            float w = ItemWeight(__instance);
            float v;
            if (inverse) v = (1 - w) * (1 - w);
            else v = w * w;
            v = Math.Clamp(v, 0, 1);

            return v * powerDecimal;
        }

        internal static float ItemScale(GrabbableObject __instance)
        {
            return __instance.transform.localScale.magnitude;
        }

        internal static void DamagePlayer(PlayerControllerB player, int damage, Vector3 hitDirection, PlayerControllerB damager)
        {
            if (!player.AllowPlayerDeath() || player.inAnimationWithEnemy)
            {
                return;
            }

            player.DamagePlayerFromOtherClientServerRpc(damage, hitDirection, (int)damager.playerClientId);
        }

        internal static Vector3 FindLandingRay(Vector3 location, bool logging = false)
        {
            Ray landingRay = new(location, Vector3.down); // the ray of where the item will land (basically the location pointing down)
            if (Physics.Raycast(landingRay, out RaycastHit hitInfo, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                // if we collide with the floor then we return the collision spot elevated a bit
                if (logging) Plugin.Logger.LogInfo("we hit the floor");
                return hitInfo.point + Vector3.up * 0.05f;
            }

            // otherwise we return the destination straight down
            if (logging) Plugin.Logger.LogInfo("we did not hit the floor");
            return landingRay.GetPoint(100f);
        }

        internal static Vector3 GetItemThrowDestination(GrabbableObject item, PlayerControllerB thrower, float chargeDecimal)
        {
            Ray throwRay = new(thrower.gameplayCamera.transform.position, thrower.gameplayCamera.transform.forward); // a ray from in front of the player
            RaycastHit hitInfo; // where the ray collides
            float itemDistance = ItemPower(item, chargeDecimal, true) * 20;
            float distance;
            if (Physics.Raycast(throwRay, out hitInfo, itemDistance, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                // if we collide with a surface then we make the destination the collision
                Plugin.Logger.LogInfo("we hit a surface");
                distance = hitInfo.distance;
            }
            else
            {
                // if we don't then we go the full length
                Plugin.Logger.LogInfo("we did not hit a surface");
                distance = itemDistance;
                
            }

            distance = Math.Max(0, distance - ItemScale(item) / 2); // we reduce the distance by the item's scale to avoid clipping into (or even through) surfaces
            Plugin.Logger.LogInfo($"throwing {Name(item)} ({item.itemProperties.weight}): {distance} units ({itemDistance})");

            Vector3 destination = throwRay.GetPoint(distance);
            return FindLandingRay(destination);
        }

        internal static Vector3 GetItemThrowDestination(ThrownItem thrownItem)
        {
            return GetItemThrowDestination(thrownItem.GetItem(), thrownItem.GetThrower(), thrownItem.GetChargeDecimal());
        }

        // borrowed from a private function in lethal company
        internal static bool CanUseItem(PlayerControllerB player)
        {
            if ((!player.IsOwner || !player.isPlayerControlled || (player.IsServer && !player.isHostPlayerObject)) && !player.isTestingPlayer)
            {
                return false;
            }

            if (!player.isHoldingObject || player.currentlyHeldObjectServer == null)
            {
                return false;
            }

            if (player.quickMenuManager.isMenuOpen)
            {
                return false;
            }

            if (player.isPlayerDead)
            {
                return false;
            }

            if (!player.currentlyHeldObjectServer.itemProperties.usableInSpecialAnimations && (player.isGrabbingObjectAnimation || player.inTerminalMenu || player.isTypingChat || (player.inSpecialInteractAnimation && !player.inShockingMinigame)))
            {
                return false;
            }

            return true;
        }
    }
}
