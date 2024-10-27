using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VisibleHazards.Helpers
{
    public class BundleHelper
    {
        public static AssetBundle LandmineBundle;
        public static GameObject Landmine_PMN2_Prefab;
        //public static GameObject Claymore_MON50_Prefab;

        public static void LoadAssets()
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = $"{folder}\\landmineassets";

            try
            {
                LandmineBundle = AssetBundle.LoadFromFile(path);
                Landmine_PMN2_Prefab = (GameObject)LandmineBundle.LoadAsset("Landmine_PMN2_Prefab");
                //Claymore_MON50_Prefab = (GameObject)LandmineBundle.LoadAsset("Claymore_MON50_Prefab");
                Plugin.Logger.LogInfo("Successfully loaded assets!");
            }
            catch (Exception ex) 
            {
                Plugin.Logger.LogError("Failed to load landmineassets.bundle due to.. whatever reason.");
            }
        }
    }
}
