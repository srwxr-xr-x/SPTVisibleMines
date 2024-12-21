using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using VisibleMines.Helpers;

namespace VisibleMines.Patches
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
            MinefieldHelper.ConvertMinefields(__instance.MainPlayer.Location);

            if (Plugin.minefieldsAddCustom.Value)
            {
                MinefieldHelper.CreateCustomMinefields(__instance.MainPlayer.Location);
            }
<<<<<<< HEAD
=======

            MineDirectional[] mineDirectionalList = GameObject.FindObjectsOfType<MineDirectional>();

            // MINEFIELDS
            foreach (MinefieldData field in minefieldList)
            {
                GameObject mineField = GameObject.Find(field.minefieldName);
                if (mineField == null)
                {
                    Helpers.Debug.LogError($"Couldn't find {field.minefieldName}.");
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
                        Vector3 placePos = hit.point;
                        GameObject landMine = GameObject.Instantiate(BundleHelper.Landmine_PMN2_Prefab, placePos, Quaternion.FromToRotation(Vector3.up, hit.normal));

                        // add component
                        Landmine landmineComponent = landMine.AddComponent<Landmine>();

                        // rotate the mine and move it into the ground
                        Transform modelGameObject = landMine.transform.Find("Model"); // weird!!!
                        modelGameObject.localPosition -= new Vector3(0, Plugin.landminePosOffset.Value, 0);
                        modelGameObject.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    }
                }
            }
>>>>>>> 5f5055859b57bf44150c4bb5d8658d5e0df4579f
        }
    }
}
