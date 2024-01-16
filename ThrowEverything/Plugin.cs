using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Collections;
using static UnityEngine.SendMouseEvents;

namespace ThrowEverything
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ThrowEverythingPlugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        public static BepInEx.Logging.ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;

            harmony.PatchAll(typeof(GrabbableObject_Patch));
            harmony.PatchAll(typeof(HUDManager_Patch));
            harmony.PatchAll(typeof(PlayerControllerB_Patch));

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }

    [HarmonyPatch(typeof(GrabbableObject))]
    public static class GrabbableObject_Patch
    {
        private static readonly BepInEx.Logging.ManualLogSource Logger = ThrowEverythingPlugin.logger;
        private static readonly Keyframe[] fallCurveKeyframes = { new(0, 0, 2, 2), new(1, 1, 0, 0) };
        private static readonly Keyframe[] verticalFallCurveKeyframes = { new(0, 0, 0.116908506f, 0.116908506f, 0, 0.27230743f), new(0.49081117f, 1, 4.1146584f, -1.81379f, 0.07234045f, 0.28319725f), new(0.7587703f, 1, 1.4123471f, -1.3678839f, 0.31997186f, 0.56917864f), new(0.9393898f, 1, 0.82654804f, -0.029021755f, 0.53747445f, 1), new(1, 1) };
        private static readonly Keyframe[] verticalFallCurveNoBounceKeyFrames = { new(0, 0, 0.116908506f, 0.116908506f, 0, 0.27230743f), new(0.69081117f, 1, 0.1146584f, 0.06098772f, 0.07234045f, 0.20768756f), new(0.9393898f, 1, 0.06394797f, -0.029021755f, 0.1980713f, 1), new(1, 1) };
        private static readonly AnimationCurve fallCurve = new(fallCurveKeyframes);
        private static readonly AnimationCurve verticalFallCurve = new(verticalFallCurveKeyframes);
        private static readonly AnimationCurve verticalFallCurveNoBounce = new(verticalFallCurveNoBounceKeyFrames);

        private static readonly Dictionary<int, ThrownItem> thrownItems = new();
        public static ThrowableItem throwableItem;
        public static StartThrowing startThrowing = new();

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(GrabbableObject), "Start")]
        //static void Start(GrabbableObject __instance)
        //{
        //Type derivedType = __instance.GetType();
        //if (AccessTools.Method(derivedType, "EquipItem").DeclaringType != typeof(GrabbableObject))
        //{
        //    Logger.LogInfo("fucking " + __instance.name);
        //    AccessTools.Method(derivedType, "EquipItem").CreateDelegate(typeof (GrabbableObject));
        //}
        //}

        static string Name(GrabbableObject __instance)
        {
            if (__instance == null) return "NULL";
            if (__instance.itemProperties == null) return $"{__instance.name} (properties was null)";
            return __instance.itemProperties.name;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "EquipItem")]
        static void EquipItem(GrabbableObject __instance)
        {
            if (__instance.IsOwner)
            {
                Logger.LogInfo($"equipped {Name(__instance)}");
                Dispose();
                throwableItem = new((e) => ThrowItem(e, __instance), __instance);
                ThrowEverythingInputSettings.Instance.ThrowItem.canceled += throwableItem.throwEvent;
                ThrowEverythingInputSettings.Instance.ThrowItem.started += StartThrowing;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "PocketItem")]
        static void PocketItem(GrabbableObject __instance)
        {
            if (__instance.IsOwner)
            {
                Logger.LogInfo($"pocketed {Name(__instance)}");
                Dispose();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "DiscardItem")]
        static void DiscardItem(GrabbableObject __instance)
        {
            if (__instance.IsOwner)
            {
                Logger.LogInfo($"discarded {Name(__instance)}");
                Dispose();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "SetControlTipsForItem")]
        public static void SetControlTipsForItem(GrabbableObject __instance)
        {
            try
            {
                //int num = __instance.itemProperties.toolTips.Length + 1;
                string name = __instance.itemProperties.itemName;
                string key = ThrowEverythingInputSettings.Instance.ThrowItem.bindings[0].path.Split('/').Last().ToUpper();
                //HUDManager.Instance.ChangeControlTip(num, $"Throw {name} : [{key}]");
                string[] tt = new string[__instance.itemProperties.toolTips.Length + 1];

                if (startThrowing.isHeld)
                {
                    int percentage = (int)Math.Floor(startThrowing.GetPercentage(__instance) * 100);
                    tt[0] = $"Throw {name} : [{percentage}%]";
                }
                else
                {
                    tt[0] = $"Throw {name} : [{key}]";
                }
                for (int i = 1; i < tt.Length; i++)
                {
                    tt[i] = __instance.itemProperties.toolTips[i - 1];
                }
                HUDManager.Instance.ChangeControlTipMultiple(tt, holdingItem: true, __instance.itemProperties);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"failed to inject tooltip for {Name(__instance)}");
                Logger.LogWarning(e);
            }
        }

        static void Dispose()
        {
            startThrowing.Stop();

            if (throwableItem != null)
            {
                ThrowEverythingInputSettings.Instance.ThrowItem.canceled -= throwableItem.throwEvent;
                ThrowEverythingInputSettings.Instance.ThrowItem.started -= StartThrowing;
                throwableItem = null;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrabbableObject), "Update")]
        static void Update(GrabbableObject __instance)
        {
            if (thrownItems.Count > 0 && thrownItems.TryGetValue(__instance.GetInstanceID(), out ThrownItem thrownItem))
            {
                //Logger.LogInfo($"{__instance.name} has hit ground {__instance.reachedFloorTarget} {__instance.hasHitGround}");
                if (__instance.reachedFloorTarget || thrownItem.Panic())
                {
                    ThrownItemLands(thrownItem);
                    return;
                }

                float size = __instance.transform.localScale.magnitude;
                RaycastHit[] colliders = Physics.SphereCastAll(__instance.transform.position, size, __instance.transform.forward, 0f, 11012424, QueryTriggerInteraction.Collide);
                for (int i = 0; i < colliders.Length; i++)
                {
                    RaycastHit c = colliders[i];
                    string name = c.collider.name;

                    if (c.transform.gameObject.layer != 3 && c.transform.gameObject.layer != 19)
                    {
                        Logger.LogInfo($"skipping {name} (failed check)");
                        continue;
                    }

                    c.transform.TryGetComponent(out PlayerControllerB player);
                    if (thrownItem.thrower != null && thrownItem.thrower == player)
                    {
                        Logger.LogInfo($"skipping {name} (the thrower)");
                        continue;
                    }

                    if (__instance == c.collider)
                    {
                        Logger.LogInfo($"skipping {name} (itself)");
                        continue;
                    }

                    try
                    {
                        if (!c.transform.TryGetComponent(out IHittable component))
                        {
                            Logger.LogInfo($"skipping {name} (not hittable)");
                            continue;
                        }

                        if (thrownItem.hits.Contains(component))
                        {
                            Logger.LogInfo($"skipping {name} (already been hit)");
                            continue;
                        }

                        // we hit
                        thrownItem.hits.Add(component);
                        float markiplier = ItemPower(__instance, thrownItem.percentage);
                        if (player != null)
                        {
                            int damage = (int)Math.Round(markiplier * 100);
                            Logger.LogInfo($"damaging a player {damage}");
                            DamagePlayer(player, damage, thrownItem.forward, thrownItem.thrower);

                            Logger.LogInfo($"{player.isPlayerDead} {player.health}");
                            if (player.isPlayerDead || player.health == 0 || player.health - damage <= 0)
                            {
                                Logger.LogInfo("it killed them (lol)");
                                // don't drop to ground if it killed
                                continue;
                            }
                        }
                        else
                        {
                            Logger.LogInfo($"hitting something else");
                            component.Hit((int)Math.Round(markiplier * 10), thrownItem.forward, thrownItem.thrower, true);
                        }

                        if (c.collider.TryGetComponent(out EnemyAI enemyAI))
                        {
                            Logger.LogInfo($"stunning an enemy {markiplier}");
                            enemyAI.SetEnemyStunned(true, markiplier * 5f, thrownItem.thrower);

                            if (enemyAI.isEnemyDead || enemyAI.enemyHP == 0)
                            {
                                Logger.LogInfo("it killed it");
                                // don't drop to ground if it killed
                                continue;
                            }
                        }

                        // we drop
                        //__instance.targetFloorPosition = findLandingRay(__instance.transform.position);
                        Logger.LogInfo($"dropping to floor");
                        __instance.startFallingPosition = __instance.transform.localPosition;
                        __instance.FallToGround();
                        ThrownItemLands(thrownItem);
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning($"failed to hit {name}");
                        Logger.LogWarning(e);
                    }
                }
            }
        }

        static void ThrownItemLands(ThrownItem thrownItem)
        {
            float power = ItemPower(thrownItem.item, thrownItem.percentage);
            float markiplier = 1 - (1 - power) * (1 - power);
            Logger.LogInfo($"playing sound for {thrownItem.item.name} at markiplier {markiplier}");
            RoundManager.Instance.PlayAudibleNoise(thrownItem.item.transform.position, Math.Clamp(markiplier * 50, 8f, 50f), Math.Clamp(markiplier, 0.5f, 1f), 0, thrownItem.item.isInElevator && StartOfRound.Instance.hangarDoorsClosed, 941);

            thrownItems.Remove(thrownItem.item.GetInstanceID());
        }

        static void DamagePlayer(PlayerControllerB player, int damage, Vector3 hitDirection, PlayerControllerB damager)
        {
            if (!player.AllowPlayerDeath())
            {
                return;
            }
            if (player.inAnimationWithEnemy)
            {
                return;
            }
            player.DamagePlayerFromOtherClientServerRpc(damage, hitDirection, (int)damager.playerClientId);
        }

        static void StartThrowing(InputAction.CallbackContext ctx)
        {
            startThrowing.Start();
        }

        static void ThrowItem(InputAction.CallbackContext ctx, GrabbableObject __instance)
        {
            ThrownItem thrownItem = new ThrownItem(__instance, startThrowing.GetPercentage(__instance));
            thrownItems.Add(__instance.GetInstanceID(), thrownItem);

            if (__instance == null)
            {
                Logger.LogWarning("tried to throw a null item");
            }
            else if (__instance.playerHeldBy == null)
            {
                Logger.LogWarning("tried to throw an unheld item");
            }
            else
            {
                __instance.playerHeldBy.DiscardHeldObject(placeObject: true, null, GetItemThrowDestination(thrownItem));
            }

            Dispose();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GrabbableObject), "FallWithCurve")]
        static bool FallWithCurve(GrabbableObject __instance)
        {
            if (__instance != null && thrownItems.ContainsKey(__instance.GetInstanceID()))
            {
                float magnitude = (__instance.startFallingPosition - __instance.targetFloorPosition).magnitude;
                __instance.transform.rotation = Quaternion.Lerp(__instance.transform.rotation, Quaternion.Euler(__instance.itemProperties.restingRotation.x, __instance.transform.eulerAngles.y, __instance.itemProperties.restingRotation.z), 14f * Time.deltaTime / magnitude);
                __instance.transform.localPosition = Vector3.Lerp(__instance.startFallingPosition, __instance.targetFloorPosition, fallCurve.Evaluate(__instance.fallTime));
                if (magnitude > 5f)
                {
                    __instance.transform.localPosition = Vector3.Lerp(new Vector3(__instance.transform.localPosition.x, __instance.startFallingPosition.y, __instance.transform.localPosition.z), new Vector3(__instance.transform.localPosition.x, __instance.targetFloorPosition.y, __instance.transform.localPosition.z), verticalFallCurveNoBounce.Evaluate(__instance.fallTime));
                }
                else
                {
                    __instance.transform.localPosition = Vector3.Lerp(new Vector3(__instance.transform.localPosition.x, __instance.startFallingPosition.y, __instance.transform.localPosition.z), new Vector3(__instance.transform.localPosition.x, __instance.targetFloorPosition.y, __instance.transform.localPosition.z), verticalFallCurve.Evaluate(__instance.fallTime));
                }
                __instance.fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);

                return false;
            }

            return true;
        }

        public static float ItemWeight(GrabbableObject __instance)
        {
            float ow = __instance.itemProperties.weight;
            float t = __instance.itemProperties.twoHanded ? 2 : 1;
            float w = Math.Clamp((ow - 1) * t, 0, 1);
            return w;
        }

        public static float ItemPower(GrabbableObject __instance, float percentage, bool inverse = false)
        {
            float w = ItemWeight(__instance);
            float v;
            if (inverse) v = (1 - w) * (1 - w);
            else v = w * w;
            v = Math.Clamp(v, 0, 1);

            return v * percentage;
        }

        static Vector3 FindLandingRay(Vector3 location, bool logging = false)
        {
            Ray landingRay = new(location, Vector3.down); // the ray of where the item will land (basically the location pointing down)
            if (Physics.Raycast(landingRay, out RaycastHit hitInfo, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                // if we collide with the floor then we return the collision spot elevated a bit
                if (logging) Logger.LogInfo("we hit the floor");
                return hitInfo.point + Vector3.up * 0.05f;
            }

            // otherwise we return the destination straight down
            if (logging) Logger.LogInfo("we did not hit the floor");
            return landingRay.GetPoint(100f);
        }

        static Vector3 GetItemThrowDestination(ThrownItem thrownItem)
        {
            GrabbableObject __instance = thrownItem.item;
            Ray throwRay = new(__instance.playerHeldBy.gameplayCamera.transform.position, __instance.playerHeldBy.gameplayCamera.transform.forward); // a ray from in front of the player
            RaycastHit hitInfo; // where the ray collides
            Vector3 destination;
            float distance = ItemPower(__instance, thrownItem.percentage, true) * 20;
            Logger.LogInfo($"throwing {Name(__instance)} ({__instance.itemProperties.weight}): {distance} units");
            if (Physics.Raycast(throwRay, out hitInfo, distance, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
                // if we collide with a wall then we make the destination the collision
                Logger.LogInfo("we hit a wall");
                destination = throwRay.GetPoint(hitInfo.distance - __instance.originalScale.normalized.x / 2);
            }
            else
            {
                // if we don't then we go the full length
                Logger.LogInfo("we did not hit a wall");
                destination = throwRay.GetPoint(distance);
            }

            return FindLandingRay(destination);
        }
    }
}
