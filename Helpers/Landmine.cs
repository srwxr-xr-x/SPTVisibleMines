using Comfort.Common;
using EFT;
using EFT.Ballistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using Systems.Effects;
using UnityEngine;
using VisibleMines.Patches;

namespace VisibleMines.Components
{
    public struct ExplosionData
    {
        public Collider touchedCollider;
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

        public Player player;
        public float playerDistance;
        public HashSet<EBodyPart> processedLimbs;
        public Dictionary<EBodyPart, Vector3> limbPositions;

        public PlayerExplosionInfo(float _playerDistance)
        {
            this.playerDistance = _playerDistance;
            processedLimbs = new HashSet<EBodyPart>();
            limbPositions = new Dictionary<EBodyPart, Vector3>();
        }

        public float GetLimbDistance(EBodyPart bodyPart, Vector3 position)
        {
            return Vector3.Distance(limbPositions[bodyPart], position);
        }

        public EBodyPart GetClosestFracturableBodyPart(Vector3 position)
        {
            EBodyPart closestBodyPart = default;
            float minDistance = float.MaxValue;

            foreach (EBodyPart bodyPart in _fracturableLimbs)
            {
                float distance = GetLimbDistance(bodyPart, position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestBodyPart = bodyPart;
                }
            }

            return closestBodyPart;
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

            // apply fracture to the limb that touched the mine
            Collider[] nearColliders = Physics.OverlapSphere(explosion.position, 0.2f);
            foreach (Collider collider in nearColliders)
            {
                Player player = collider.GetComponentInParent<Player>();
                if (player == null) continue;

                BodyPartCollider bodyPartCollider = collider.GetComponent<BodyPartCollider>();
                if (bodyPartCollider == null) continue;

                player.ActiveHealthController.DoFracture(bodyPartCollider.BodyPartType);
                break;
            }

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
                    processedPlayers.Add(player, new PlayerExplosionInfo(playerDistToExplosion));

                    // apply screen effects
                    player.ActiveHealthController.DoContusion(20f * playerDistanceMult, playerDistanceMult);
                    player.ActiveHealthController.DoDisorientation(5f * playerDistanceMult);
                    player.ProceduralWeaponAnimation.ForceReact.AddForce(playerDirToExplosion.normalized, playerDistanceMult * Plugin.screenShakeIntensityAmount.Value, Plugin.screenShakeIntensityWeapon.Value, Plugin.screenShakeIntensityCamera.Value);

                    // apply tinnitus
                    AudioClip tinnitusAudio = (AudioClip)typeof(BetterAudio).GetField("_tinnitus", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Singleton<BetterAudio>.Instance);
                    if (tinnitusAudio != null)
                    {
                        Singleton<BetterAudio>.Instance.StartTinnitusEffect(7f, tinnitusAudio);
                    }
                    else
                    {
                        Helpers.Debug.LogWarning("couldn't get tinnitus audio");
                    }
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
                }

                processedPlayers[player].processedLimbs.Add(bodyPart);
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

        public virtual void Explode(Collider otherCollider)
        {
            ExplosionData data = new ExplosionData()
            {
                touchedCollider = otherCollider,
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
            Explode(null);
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
            Explode(other);
        }

        public override void OnTriggerExit(Collider other)
        {
            // umm...
        }
    }
}
