using Comfort.Common;
using EFT;
using EFT.Ballistics;
using System.Collections.Generic;
using Systems.Effects;
using UnityEngine;

namespace VisibleHazards.Components
{
    public struct ExplosionData
    {
        public Vector3 position;
        public float distance;
        public float damage;
        public string effectName;
        public Vector3 effectDir;
        public float damageDropoffMult;
        public List<EBodyPart> targetBodyParts;
    }

    public static class Explosion
    {
        public static void CreateLandmineExplosion(ExplosionData explosion)
        {
            Dictionary<Player, HashSet<EBodyPart>> processedPlayers = new Dictionary<Player, HashSet<EBodyPart>>();

            // effect
            Singleton<Effects>.Instance.EmitGrenade(explosion.effectName, explosion.position, Vector3.up, 1f);

            // damage
            Collider[] colliders = Physics.OverlapSphere(explosion.position, explosion.distance);
            foreach (Collider collider in colliders)
            {
                Player player = collider.GetComponentInParent<Player>();
                if (player == null) continue;

                BodyPartCollider bodyPartCollider = collider.GetComponent<BodyPartCollider>();
                if (bodyPartCollider == null) continue;

                EBodyPart bodyPart = bodyPartCollider.BodyPartType;
                EBodyPartColliderType colliderType = bodyPartCollider.BodyPartColliderType;

                // GO AWAY DONT LOOK AT THIS!!!!
                // todo: optimize all this.
                Vector3 colliderPos = collider.transform.position;
                Vector3 dir = colliderPos - explosion.position;
                Vector3 dirNormalized = dir.normalized;
                float colliderDistToExplosion = dir.magnitude;
                float playerDistToExplosion = (player.Position - explosion.position).magnitude;
                float distanceMult = Mathf.Clamp01(1f - (playerDistToExplosion / explosion.distance));
                float colliderDistMult = Mathf.Pow(Mathf.Clamp01(1f - (colliderDistToExplosion / explosion.distance)), explosion.damageDropoffMult);

                bool playerProcessedExists = processedPlayers.ContainsKey(player);
                // if first damage
                if (!playerProcessedExists)
                {
                    processedPlayers.Add(player, new HashSet<EBodyPart>());

                    player.ActiveHealthController.DoContusion(25f * distanceMult, distanceMult);
                    player.ActiveHealthController.DoDisorientation(5f * distanceMult);
                    player.ProceduralWeaponAnimation.ForceReact.AddForce(dirNormalized, distanceMult * 2, 1f, 2f);
                }

                if (explosion.targetBodyParts.Contains(bodyPart) && !processedPlayers[player].Contains(bodyPart))
                {
                    DamageInfo dmgInfo = new DamageInfo()
                    {
                        DamageType = EDamageType.Landmine,
                        Damage = explosion.damage * colliderDistMult,
                        ArmorDamage = 0.35f,
                        PenetrationPower = 25,
                        Direction = dirNormalized,
                        HitNormal = -dirNormalized,
                        HitPoint = colliderPos,
                        Player = null,
                        Weapon = null,
                        HeavyBleedingDelta = Plugin.landmineHeavyBleedDelta.Value,
                        LightBleedingDelta = Plugin.landmineLightBleedDelta.Value
                    };

                    player.ApplyDamageInfo(dmgInfo, bodyPart, colliderType, 0.0f);

                    //Plugin.Logger.LogInfo($"{bodyPart} {explosion.damage * distanceMult}");
                }

                processedPlayers[player].Add(bodyPart);
            }
        } 
    }

    /*
    public class LaserTrigger : MonoBehaviour
    {
        public Transform origin;
        public float maxDistance = 25f;
        public LayerMask layerMask;

        private LineRenderer _lineRenderer;

        public event Action OnTriggered;

        public void FixedUpdate()
        {
            RaycastHit hit;
            Vector3 startPos = origin.position;
            Vector3 forward = origin.TransformDirection(Vector3.forward);
            Vector3 endPos = origin.position + forward * 250;

            if (Physics.Raycast(startPos, forward, Plugin.claymoreRange.Value, layerMask))
            {
                OnTriggered?.Invoke();
            }
            else
            {
                _lineRenderer.SetPosition(0, origin.position);
                _lineRenderer.SetPosition(1, startPos + forward * maxDistance);
            }
        }

        public void Awake()
        {
            origin = gameObject.transform;

            _lineRenderer = gameObject.GetOrAddComponent<LineRenderer>(); // also set material!
            _lineRenderer.startWidth = 0.01f;
            _lineRenderer.endWidth = 0.01f;
        }
    }*/

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
                distance = Plugin.landmineExplosionRange.Value,
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

        public virtual void OnHit(DamageInfo damageInfo)
        {
            Explode();
        }

        public virtual void Awake()
        {
            //Plugin.Logger.LogInfo($"Created {this.name} at {this.gameObject.transform.position}");
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

    /*
    public class Claymore : BaseLandmine
    {
        private Transform _wirePos;
        private LaserTrigger _trigger;
        private Lazy<ISharedBallisticsCalculator> _ballisticCalculator;
        private DamageInfo _damageInfo;
        public MineDirectional.MineSettings MineData;

        public void SetMineDataValue(string name, float value)
        {
            // it sucks
            MineData.GetType().GetField(name)?.SetValueDirect(__makeref(MineData), value);
        }

        private DamageInfo getDamageInfo()
        {
            return new DamageInfo()
            {
                DamageType = EDamageType.Landmine,
                Damage = 0f,
                ArmorDamage = 0.2f,
                StaminaBurnRate = 5f,
                PenetrationPower = 20,
                Direction = Vector3.zero,
                Player = null,
                IsForwardHit = true
            };
        }

        public override void OnTriggerEnter(Collider other)
        {
            // nothing
        }

        public override void OnTriggerExit(Collider other)
        {
            // nothing
        }

        public override void Explode()
        {
            Singleton<Effects>.Instance.EmitGrenade("Grenade_new", gameObject.transform.position, Vector3.up, 1f);
            gameObject.SetActive(false);
            MineData.Explosion(transform.position, null, _ballisticCalculator.Value, null, new Func<DamageInfo>(this.getDamageInfo), 0f, 75f, transform.forward);
        }

        public override void Awake()
        {
            // probably create a single global ballistic calculator?
            _ballisticCalculator = new Lazy<ISharedBallisticsCalculator>(new Func<ISharedBallisticsCalculator>(MineDirectional.Class321.class321_0.method_0));
            _wirePos = gameObject.transform.Find("WirePos");
            if (_wirePos != null )
            {
                _trigger = _wirePos.gameObject.AddComponent<LaserTrigger>();
                _trigger.maxDistance = Plugin.claymoreRange.Value;
                _trigger.layerMask = LayerMaskClass.PlayerMask;
                _trigger.OnTriggered += Explode;
            }
            base.Awake();
        }
    }*/
}
