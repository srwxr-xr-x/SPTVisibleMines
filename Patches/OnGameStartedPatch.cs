using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using VisibleHazards.Helpers;
using UnityEngine;
using EFT.Interactive;
using System.Collections.Generic;
using VisibleHazards.Components;

namespace VisibleHazards.Patches
{
    public class OnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostFix(GameWorld __instance)
        {
            List<MinefieldData> minefieldList = MapConfig.GetMapData(__instance.MainPlayer.Location);
            MineDirectional[] mineDirectionalList = GameObject.FindObjectsOfType<MineDirectional>();

            // MINEFIELDS
            foreach (MinefieldData field in minefieldList)
            {
                GameObject mineField = GameObject.Find(field.minefieldName);
                if (mineField == null)
                {
                    Plugin.Logger.LogError($"Couldn't find {field.minefieldName}.");
                    continue;
                }

                Minefield minefieldComponent = mineField.GetComponent<Minefield>();
                if (minefieldComponent != null)
                {
                    // disable this shit
                    minefieldComponent.enabled = false;
                }

                BoxCollider boxCollider = mineField.GetComponent<BoxCollider>();
                List<Vector3> spawnPoints = PoissonDiskSampling.GeneratePointsCollider(boxCollider, field.maxMineCount, field.minDistanceBetweenMines);

                foreach (Vector3 spawnPoint in spawnPoints)
                {
                    Vector3 newSpawnPoint = new Vector3(spawnPoint.x, boxCollider.bounds.center.y + boxCollider.bounds.max.y, spawnPoint.z);
                    RaycastHit hit;
                    if (Physics.Raycast(spawnPoint, Vector3.down, out hit, 250f, LayerMaskClass.TerrainLowPoly))
                    {
                        // create mine
                        Vector3 placePos = hit.point - hit.normal * Plugin.landminePosOffset.Value;
                        GameObject landMine = GameObject.Instantiate(BundleHelper.Landmine_PMN2_Prefab, placePos, Quaternion.FromToRotation(Vector3.up, hit.normal));

                        // add component
                        Landmine landmineComponent = landMine.AddComponent<Landmine>();
                    }
                }
            }

            //CLAYMORES
            /*
            foreach (MineDirectional mineDirectional in mineDirectionalList)
            {
                MineDirectionalColliders[] colliders = mineDirectional.gameObject.GetComponentsInChildren<MineDirectionalColliders>();
                Transform oldClaymore = mineDirectional.gameObject.transform.Find("mine_mon50");

                if (oldClaymore != null)
                {
                    // create mine
                    GameObject claymore = GameObject.Instantiate(BundleHelper.Claymore_MON50_Prefab, oldClaymore.position, Quaternion.Euler(0, mineDirectional.transform.eulerAngles.y, 0));

                    // add component
                    Claymore claymoreComponent = claymore.AddComponent<Claymore>();
                    claymoreComponent.explosionDamage = Plugin.claymoreDamage.Value;
                    claymoreComponent.explosionRadius = Plugin.claymoreExplosionRange.Value;
                    claymoreComponent.explosionArmorMult = Plugin.claymoreDamageArmorMult.Value;
                    claymoreComponent.explosionRadius = 45;
                    claymoreComponent.MineData = (MineDirectional.MineSettings)typeof(MineDirectional).GetField("_mineData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mineDirectional);
                }

                // GET OUT !!!
                GameObject.Destroy(mineDirectional.gameObject);
            }*/
        }
    }
}
