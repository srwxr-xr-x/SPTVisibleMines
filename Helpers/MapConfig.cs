using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VisibleMines.Helpers
{
    public struct MinefieldData
    {
        public string minefieldName { get; set; }
        public int maxMineCount { get; set; }
        public float minDistanceBetweenMines { get; set; }

        public MinefieldData(string _minefieldName, int _maxMineCount, float _minDistanceBetweenMines)
        {
            minefieldName = _minefieldName;
            maxMineCount = _maxMineCount;
            minDistanceBetweenMines = _minDistanceBetweenMines;
        }
    }

    public static class MapConfig
    {
        // mapname, minefielddata
        public static Dictionary<string, List<MinefieldData>> minefieldData = new Dictionary<string, List<MinefieldData>>();

        public static void LoadMapConfig()
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = $"{folder}\\mapconfig.json";

            // does the damn file exist?
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

                    minefieldData[mapName].Add(new MinefieldData(name, maxMineCount, minDistanceBetweenMines));

                    Helpers.Debug.LogInfo($"loaded minefield {name} for {mapName}!");
                }
            }

            Debug.LogInfo($"LOADED MINEFIELDS FOR {minefieldData.Count} MAPS");
        }

        public static List<MinefieldData> GetMapData(string mapName)
        {
            if (minefieldData.TryGetValue(mapName, out List<MinefieldData> mineFields))
            {
                return mineFields;
            }

            return null;
        }
    }
}
