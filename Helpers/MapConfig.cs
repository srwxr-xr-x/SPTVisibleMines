using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VisibleMines.Helpers
{
    public class MinefieldData
    {
        public string minefieldName { get; set; }
        public int maxMineCount { get; set; }
        public float minDistanceBetweenMines { get; set; }
    }

    public class WarningSignData
    {
        public string prefabName { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotationEuler { get; set; }
    }

    public class MinefieldDataCustom
    {
        public float spawnChance { get; set; }
        public string minefieldName { get; set; }
        public int maxMineCount { get; set; }
        public float minDistanceBetweenMines { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotationEuler { get; set; }
        public Vector3 size { get; set; }
        public List<WarningSignData> warningSignData { get; set; }

        public MinefieldDataCustom()
        {
            warningSignData = new List<WarningSignData>();
        }
    }

    // this is weird and lua-like. fix it.. at some.... point....... yeah.
    public static class MapConfig
    {
        // mapname, minefielddata
        public static Dictionary<string, List<MinefieldData>> minefieldData = new Dictionary<string, List<MinefieldData>>();
        public static Dictionary<string, List<MinefieldDataCustom>> minefieldDataCustom = new Dictionary<string, List<MinefieldDataCustom>>();

        public static void LoadMapConfig()
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = $"{folder}\\mapconfig.json";

            if (!File.Exists(path)) { Debug.LogError("Failed to load mapconfig.json"); return; }

            JObject json;
            try
            {
                json = JObject.Parse(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse mapconfig.json!");
                return;
            }

            foreach (var map in json)
            {
                var mapName = map.Key;
                var mapMinefields = map.Value;

                if (!LocationSettingsClass.Location.AvailableMaps.Contains(mapName))
                {
                    Helpers.Debug.LogError($"failed to load map {mapName}");
                    continue;
                }

                minefieldData[mapName] = new List<MinefieldData>();
                foreach (var minefield in mapMinefields)
                {

                    string name = (string)minefield["name"];
                    int maxMineCount = (int)minefield["maxMineCount"];
                    float minDistanceBetweenMines = (float)minefield["minDistanceBetweenMines"];

                    MinefieldData data = new MinefieldData()
                    {
                        minefieldName = name,
                        maxMineCount = maxMineCount,
                        minDistanceBetweenMines = minDistanceBetweenMines,
                    };

                    minefieldData[mapName].Add(data);

                    Helpers.Debug.LogInfo($"loaded minefield {name} for {mapName}!");
                }
            }

            Debug.LogInfo($"LOADED MINEFIELDS FOR {minefieldData.Count} MAPS");
        }

        public static void LoadMapConfigCustom()
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = $"{folder}\\mapconfig_custom.json";

            if (!File.Exists(path)) { Debug.LogError("Failed to load mapconfig_custom.json"); return; }

            JObject json;
            try
            {
                json = JObject.Parse(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse mapconfig_custom.json!");
                return;
            }

            // load custom minefield data
            foreach (var map in json)
            {
                string mapName = map.Key;
                JObject minefieldGroups = (JObject)map.Value;

                if (!minefieldDataCustom.ContainsKey(mapName))
                {
                    minefieldDataCustom[mapName] = new List<MinefieldDataCustom>();
                }

                foreach (var group in minefieldGroups)
                {
                    string groupName = group.Key;
                    JObject groupData = (JObject)group.Value;

                    float spawnChance = (float)groupData["chance"];
                    JArray minefields = (JArray)groupData["minefields"];
                    JArray warningSigns = (JArray)groupData["warningSigns"];

                    MinefieldDataCustom newData = new MinefieldDataCustom();

                    foreach (var sign in warningSigns)
                    {
                        newData.warningSignData.Add(new WarningSignData()
                        {
                            prefabName = (string)sign["prefab"],
                            position = new Vector3(
                                    (float)sign["position"]["x"],
                                    (float)sign["position"]["y"],
                                    (float)sign["position"]["z"]
                                ),
                            rotationEuler = new Vector3(
                                    (float)sign["rotation"]["x"],
                                    (float)sign["rotation"]["y"],
                                    (float)sign["rotation"]["z"]
                                )
                        });
                    }

                    foreach (var field in minefields)
                    {
                        newData.minefieldName = groupName;
                        newData.maxMineCount = (int)field["maxMineCount"];
                        newData.minDistanceBetweenMines = (float)field["minDistanceBetweenMines"];
                        newData.position = new Vector3(
                                (float)field["position"]["x"],
                                (float)field["position"]["y"],
                                (float)field["position"]["z"]
                            );
                        newData.rotationEuler = new Vector3(
                                (float)field["rotation"]["x"],
                                (float)field["rotation"]["y"],
                                (float)field["rotation"]["z"]
                            );
                        newData.size = new Vector3(
                                (float)field["size"]["x"],
                                (float)field["size"]["y"],
                                (float)field["size"]["z"]
                            );
                    }

                    minefieldDataCustom[mapName].Add(newData);
                }
            }

            Debug.LogInfo($"LOADED CUSTOM MINEFIELDS FOR {minefieldDataCustom.Count} MAPS");
        }

        public static List<MinefieldData> GetMapData(string mapName)
        {
            if (minefieldData.TryGetValue(mapName, out List<MinefieldData> mineFields))
            {
                return mineFields;
            }

            return null;
        }

        public static List<MinefieldDataCustom> GetMapDataCustom(string mapName)
        {
            if (minefieldDataCustom.TryGetValue(mapName, out List<MinefieldDataCustom> mineFields))
            {
                return mineFields;
            }

            return null;
        }
    }
}
