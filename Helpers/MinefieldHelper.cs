using System.Collections.Generic;
using UnityEngine;
using VisibleMines.Components;
using EFT.Interactive;
using UnityEngine.AI;

namespace VisibleMines.Helpers
{
    public static class MinefieldHelper
    {
        public static void ConvertMinefields(string locationName)
        {
            List<MinefieldData> minefieldList = MapConfig.GetMapData(locationName);
            if (minefieldList == null)
            {
                Helpers.Debug.LogWarning($"Couldn't find {locationName}.");
                return;
            }

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
                if (Plugin.minefieldsAiIgnore.Value)
                {
                    NavMeshObstacle obstacle = mineField.AddComponent<NavMeshObstacle>();
                    obstacle.enabled = true;
                    obstacle.carving = true;
                    obstacle.center = boxCollider.center;
                    obstacle.size = boxCollider.size;
                }

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
        }

        public static void CreateCustomMinefields(string locationName)
        {
            List<MinefieldDataCustom> minefieldList = MapConfig.GetMapDataCustom(locationName);
            if (minefieldList == null)
            {
                Helpers.Debug.LogWarning($"Couldn't find {locationName}.");
                return;
            }

            // MINEFIELDS
            foreach (MinefieldDataCustom field in minefieldList)
            {
                GameObject mineField = new GameObject(field.minefieldName);
                mineField.transform.position = field.position;
                mineField.transform.rotation = Quaternion.Euler(field.rotationEuler);
                mineField.layer = LayerMask.NameToLayer("Triggers");

                Debug.LogInfo(field.ToString());

                BoxCollider boxCollider = mineField.AddComponent<BoxCollider>();
                boxCollider.size = field.size;
                boxCollider.isTrigger = true;

                if (Plugin.minefieldsAiIgnore.Value)
                {
                    NavMeshObstacle obstacle = mineField.AddComponent<NavMeshObstacle>();
                    obstacle.enabled = true;
                    obstacle.carving = true;
                    obstacle.center = boxCollider.center;
                    obstacle.size = boxCollider.size;
                }

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

                foreach (WarningSignData warningSignData in field.warningSignData)
                {
                    GameObject warningSign = GameObject.Instantiate(BundleHelper.Warning_Plywood_Prefab, warningSignData.position, Quaternion.Euler(warningSignData.rotationEuler));
                }
            }
        }
    }
}
