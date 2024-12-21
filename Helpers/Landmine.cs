using Comfort.Common;
using EFT;
using EFT.Ballistics;
using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Effects;
using UnityEngine;
using VisibleMines.Patches;

namespace VisibleMines.Components
{
    public struct ExplosionData
    {
        public Vector3 position;
        public float maxDistance;
        public float damage;
        public string effectName;
        public Vector3 effectDir;
        public float damageDropoffMult;
        public List<EBodyPart> targetBodyParts;
    }

    public class PlayerExplosionInfo
    {
        // weird
        private EBodyPart[] _fracturableLimbs =
        [
            EBodyPart.RightArm,
            EBodyPart.LeftArm,
            EBodyPart.RightLeg,
            EBodyPart.LeftLeg
        ];

        public float playerDistance;
        public HashSet<EBodyPart> processedLimbs;
        public Dictionary<EBodyPart, float> limbDistances;

        public PlayerExplosionInfo(float _playerDistance)
        {
            this.playerDistance = _playerDistance;
            processedLimbs = new HashSet<EBodyPart>();
            limbDistances = new Dictionary<EBodyPart, float>();
        }

        public float GetLimbDistance(EBodyPart bodyPart)
        {
            return limbDistances[bodyPart];
        }

        public EBodyPart GetClosestBodyPart()
        {
            return limbDistances.Aggregate((a, b) =>
            {
                return a.Value < b.Value ? a : b;
            }).Key;
        }

        public EBodyPart GetClosestFracturableBodyPart()
        {
            return limbDistances
                .Where(x => _fracturableLimbs.Contains(x.Key)) // is fracturable?
                .Aggregate((a, b) => { return a.Value < b.Value ? a : b; }) // sort by distance
                .Key; // get the bodypart
        }

        public EBodyPart GetClosestLeg() // could have just added a filter but idc!!!!
        {
            return limbDistances
                .Where(x => x.Key == EBodyPart.RightLeg || x.Key == EBodyPart.LeftLeg) // is.. leg?
                .Aggregate((a, b) => { return a.Value < b.Value ? a : b; }) // sort by distance
                .Key; // get the bodypart
        }
    }

    public class Explosion
    {
        public static void CreateLandmineExplosion(ExplosionData explosion)
        {
            // processed limbs for players
            Dictionary<Player, PlayerExplosionInfo> processedPlayers = new Dictionary<Player, PlayerExplosionInfo>();

            // explosion effect
            Singleton<Effects>.Instance.EmitGrenade(explosion.effectName, explosion.position, Vector3.up, 1f);

            // damage
            Collider[] colliders = Physics.OverlapSphere(explosion.position, explosion.maxDistance);
            foreach (Collider collider in colliders)
            {
                Player player = collider.GetComponentInParent<Player>();
                if (player == null) continue;

                BodyPartCollider bodyPartCollider = collider.GetComponent<BodyPartCollider>();
                if (bodyPartCollider == null) continue;

                EBodyPart bodyPart = bodyPartCollider.BodyPartType;
                EBodyPartColliderType colliderType = bodyPartCollider.BodyPartColliderType;

                // collider
                Vector3 colliderPos = collider.transform.position;
                Vector3 colliderDirToExplosion = explosion.position - colliderPos;
                float colliderDistance = colliderDirToExplosion.magnitude;

                // player
                Vector3 playerDirToExplosion = bodyPartCollider.playerBridge.iPlayer.Position - explosion.position;
                float playerDistance = playerDirToExplosion.magnitude;
                float playerDistanceMult = 1 - Mathf.Clamp01(playerDistance / colliderDistance);

                // damage
                float distanceMultiplier = Mathf.Clamp01(1f - (colliderDistance / explosion.maxDistance));
                float finalDamage = explosion.damage * Mathf.Pow(distanceMultiplier, explosion.damageDropoffMult);

                bool playerProcessedExists = processedPlayers.ContainsKey(player);

                // if first damage
                if (!playerProcessedExists)
                {
                    // add player to processedPlayers
                    float playerDistToExplosion = playerDirToExplosion.magnitude;
                    Helpers.Debug.LogInfo($"player distance to explosion: {playerDistToExplosion}");
                    processedPlayers.Add(player, new PlayerExplosionInfo(playerDistToExplosion));

                    // apply screen effects
                    player.ActiveHealthController.DoContusion(20f * playerDistanceMult, playerDistanceMult);
                    player.ActiveHealthController.DoDisorientation(5f * playerDistanceMult);
                    player.ProceduralWeaponAnimation.ForceReact.AddForce(playerDirToExplosion.normalized, playerDistanceMult * Plugin.screenShakeIntensityAmount.Value, Plugin.screenShakeIntensityWeapon.Value, Plugin.screenShakeIntensityCamera.Value);
                }

                // loop through all the limbs
                if (explosion.targetBodyParts.Contains(bodyPart) && !processedPlayers[player].processedLimbs.Contains(bodyPart))
                {
                    Helpers.Debug.LogInfo($"Processing body part {bodyPart}, collider distance {colliderDistance}");

                    DamageInfoStruct dmgInfo = new DamageInfoStruct()
                    {
                        DamageType = EDamageType.Landmine,
                        Damage = finalDamage,
                        ArmorDamage = 0.35f,
                        PenetrationPower = 25,
                        Direction = colliderDirToExplosion,
                        HitNormal = -colliderDirToExplosion,
                        HitPoint = colliderPos,
                        Player = null,
                        Weapon = null,
                        HeavyBleedingDelta = Plugin.landmineHeavyBleedDelta.Value,
                        LightBleedingDelta = Plugin.landmineLightBleedDelta.Value,
                        StaminaBurnRate = Plugin.landmineStaminaBurnRate.Value
                    };

                    // ignore fractures when applying damage
                    DoFracturePatch.SetIgnoreNextFracture(true);
                    player.ApplyDamageInfo(dmgInfo, bodyPart, colliderType, 0.0f);
                    DoFracturePatch.SetIgnoreNextFracture(false);

                    // only add parts that can be fractured (this will be important later!)
                    if (bodyPart != EBodyPart.Chest || bodyPart != EBodyPart.Stomach || bodyPart != EBodyPart.Head)
                    {
                        processedPlayers[player].limbDistances.Add(bodyPart, colliderDistance);
                    }
                }

                processedPlayers[player].processedLimbs.Add(bodyPart);
            }

            foreach (KeyValuePair<Player, PlayerExplosionInfo> kvp in processedPlayers)
            {
                // holy crap i love repeating code!
                Player player = kvp.Key;
                PlayerExplosionInfo explosionInfo = kvp.Value;

                Vector3 playerDirToExplosion = kvp.Key.Position - explosion.position;
                float playerDistance = playerDirToExplosion.magnitude;

                if (playerDistance > 1f) continue; // too far..

                bool isPlayerStanding = player.IsInPronePose;
                if (!isPlayerStanding)
                {
                    EBodyPart closestBodyPart = explosionInfo.GetClosestLeg();
                    player.ActiveHealthController.DoFracture(closestBodyPart);
                }
                else
                {
                    EBodyPart closestBodyPart = explosionInfo.GetClosestFracturableBodyPart();
                    player.ActiveHealthController.DoFracture(closestBodyPart);
                }
            }
        }
    }

    public abstract class BaseLandmine : MonoBehaviour, IPhysicsTrigger
    {
        private BallisticCollider _ballisticCollider;
        public string Description { get; }

        public abstract void OnTriggerEnter(Collider other);

        public abstract void OnTriggerExit(Collider other);

        private void CreateBallisticCollider()
        {
            MeshCollider collider = GetComponentInChildren<MeshCollider>();
            if (collider != null)
            {
                _ballisticCollider = collider.gameObject.AddComponent<BallisticCollider>();
                _ballisticCollider.OnHitAction += OnHit;
                _ballisticCollider.TypeOfMaterial = MaterialType.MetalThin;
                _ballisticCollider.PenetrationLevel = 1;
                _ballisticCollider.PenetrationChance = 100;
                _ballisticCollider.SurfaceSound = BaseBallistic.ESurfaceSound.MetalThin;
            }
        }

        public virtual void Explode()
        {
            ExplosionData data = new ExplosionData()
            {
                position = gameObject.transform.position,
                effectDir = Vector3.up,
                effectName = "Grenade_new",
                damage = Plugin.landmineDamage.Value,
                damageDropoffMult = Plugin.landmineDamageDropoffMult.Value,
                maxDistance = Plugin.landmineExplosionRange.Value,
                targetBodyParts = new List<EBodyPart>
                {
                    EBodyPart.RightLeg,
                    EBodyPart.LeftLeg,
                    EBodyPart.Stomach,
                    EBodyPart.RightArm,
                    EBodyPart.LeftArm,
                    EBodyPart.Chest,
                }
            };

            Explosion.CreateLandmineExplosion(data);
            gameObject.SetActive(false);
        }

        public virtual void OnHit(DamageInfoStruct damageInfo)
        {
            Explode();
        }

        public virtual void Awake()
        {
            Helpers.Debug.LogInfo($"Created {this.name} at {this.gameObject.transform.position}");
            CreateBallisticCollider();
        }

        public virtual void FixedUpdate()
        {

        }
    }

    public class Landmine : BaseLandmine
    {
        public override void OnTriggerEnter(Collider other)
        {
            Explode();
        }

        public override void OnTriggerExit(Collider other)
        {
            // umm...
        }
    }
}
