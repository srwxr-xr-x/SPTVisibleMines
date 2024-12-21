using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VisibleMines.Helpers
{
    public class BundleHelper
    {
        public static AssetBundle LandmineBundle;
        public static AssetBundle WarningsBundle;

        public static GameObject Landmine_PMN2_Prefab;
<<<<<<< HEAD
        public static GameObject Warning_Plywood_Prefab;
=======
>>>>>>> 5f5055859b57bf44150c4bb5d8658d5e0df4579f

        public static void LoadAssets()
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pathMineAssets = $"{folder}\\landmineassets";
            string pathWarningAssets = $"{folder}\\warningassets";

            try
            {
                LandmineBundle = AssetBundle.LoadFromFile(pathMineAssets);
                Landmine_PMN2_Prefab = (GameObject)LandmineBundle.LoadAsset("Landmine_PMN2_Prefab");
<<<<<<< HEAD

                WarningsBundle = AssetBundle.LoadFromFile(pathWarningAssets);
                Warning_Plywood_Prefab = (GameObject)WarningsBundle.LoadAsset("warning_plywood");

=======
>>>>>>> 5f5055859b57bf44150c4bb5d8658d5e0df4579f
                VisibleMines.Helpers.Debug.LogInfo("Successfully loaded assets!");
            }
            catch (Exception ex) 
            {
                VisibleMines.Helpers.Debug.LogInfo("Failed to load landmineassets.bundle due to.. whatever reason.");
            }
        }
    }
}
