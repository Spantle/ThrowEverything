using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace ThrowEverything.Models
{
    internal class ThrownItems
    {
        internal Dictionary<int, ThrownItem> thrownItemsDict = new();

        internal void Update(ThrownItem thrownItem)
        {
            GrabbableObject item = thrownItem.GetItem();

            if (item.reachedFloorTarget || thrownItem.IsPanicking())
            {
                thrownItem.LandAndRemove();
                return;
            }

            float size = item.transform.localScale.magnitude;
            RaycastHit[] colliders = Physics.SphereCastAll(item.transform.position, size, item.transform.forward, 0f, 11012424, QueryTriggerInteraction.Collide);
            foreach (RaycastHit hit in colliders)
            {
                string name = hit.collider.name;

                if (item == hit.collider)
                {
                    Plugin.Logger.LogDebug($"skipping {name} (is itself)");
                    continue;
                }

                if (hit.transform.gameObject.layer != 3 && hit.transform.gameObject.layer != 19)
                {
                    Plugin.Logger.LogDebug($"skipping {name} (failed layer check)");
                    continue;
                }

                hit.transform.TryGetComponent(out PlayerControllerB hitPlayer);
                PlayerControllerB thrower = thrownItem.GetThrower();
                if (thrower == hitPlayer)
                {
                    Plugin.Logger.LogDebug($"skipping {name} (is the thrower)");
                    continue;
                }

                if (!hit.transform.TryGetComponent(out IHittable hittable))
                {
                    Plugin.Logger.LogInfo($"skipping {name} (not hittable)"); // loginfo is intentional
                    continue;
                }

                // don't forget! this already counts as a hit
                if (thrownItem.CheckIfHitOrAdd(hittable))
                {
                    Plugin.Logger.LogDebug($"skipping {name} (already been hit)");
                    continue;
                }

                float markiplier = thrownItem.GetMarkiplier();
                if (hitPlayer != null)
                {
                    int damage = (int)Math.Round(markiplier * 100);
                    Plugin.Logger.LogInfo($"damaging a player {damage} ({hitPlayer.health}");
                    Utils.DamagePlayer(hitPlayer, damage, item.transform.forward, thrownItem.GetThrower());

                    Plugin.Logger.LogInfo($"they now have {hitPlayer.health} ({hitPlayer.isPlayerDead})");
                    if (hitPlayer.isPlayerDead || hitPlayer.health == 0 || hitPlayer.health - damage <= 0)
                    {
                        Plugin.Logger.LogInfo("it killed them (lol)");
                        continue; // don't drop to ground if the item killed
                    }
                }
                else
                {
                    int damage = (int)Math.Round(markiplier * 10);
                    Plugin.Logger.LogInfo($"hitting something else {damage}");
                    hittable.Hit(damage, item.transform.forward, thrownItem.GetThrower(), true);
                }

                if (hit.collider.TryGetComponent(out EnemyAI enemyAI))
                {
                    float stunTime = markiplier * 5;
                    Plugin.Logger.LogInfo($"stunning an enemy {stunTime}");
                    enemyAI.SetEnemyStunned(true, markiplier * 5f, thrownItem.GetThrower());

                    if (enemyAI.isEnemyDead || enemyAI.enemyHP == 0)
                    {
                        Plugin.Logger.LogInfo("it killed it");
                        continue; // don't drop to ground if the item killed
                    }
                }

                Plugin.Logger.LogInfo("dropping to floor");
                item.startFallingPosition = item.transform.localPosition;
                item.FallToGround();
                thrownItem.LandAndRemove();
                break;
            }
        }
    }
}
