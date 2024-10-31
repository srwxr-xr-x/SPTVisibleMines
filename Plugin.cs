using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using VisibleHazards.Helpers;
using VisibleHazards.Patches;
using VisibleMines.Patches;

namespace VisibleHazards
{
    [BepInPlugin("com.pein.visiblemines", "PeinVisibleMines", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        // Landmines
        public static ConfigEntry<float> landminePosOffset { get;  set; }
        public static ConfigEntry<float> landmineDamage { get; set; }
        public static ConfigEntry<float> landmineExplosionRange { get; set; }
        public static ConfigEntry<int> landmineDamageDropoffMult { get; set; }
        public static ConfigEntry<float> landmineDamageArmorMult { get; set; }
        public static ConfigEntry<float> landmineLightBleedDelta { get; set; }
        public static ConfigEntry<float> landmineHeavyBleedDelta { get; set; }
        

        // Claymores
        /*
        public static ConfigEntry<float> claymoreDamage { get; set; }
        public static ConfigEntry<float> claymoreRange { get; set; }
        public static ConfigEntry<float> claymoreExplosionRange { get; set; }
        public static ConfigEntry<float> claymoreDamageArmorMult { get; set; }
        public static ConfigEntry<float> claymoreDamageAngle { get; set; }
        */

        public static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            // Landmines
            landminePosOffset = Config.Bind("1. Landmines", "Landmine Pos Offset", 0.01f, new ConfigDescription(
                    "Changes the up/down offset of landmines. Higher values spawn the mines lower into the ground, making them more difficult to spot. Low values may make the mines float slightly off the ground.",
                    new AcceptableValueRange<float>(0.01f, 0.2f),
                    new ConfigurationManagerAttributes() { Order = 1000, IsAdvanced = true }
                ));

            landmineDamage = Config.Bind("1. Landmines", "Landmine Damage", 100f, new ConfigDescription(
                    "Changes the damage of landmines.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 990 }
                ));

            landmineExplosionRange = Config.Bind("1. Landmines", "Landmine Explosion Radius", 5f, new ConfigDescription(
                    "Changes the explosion radius of landmines.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 980 }
                ));

            landmineDamageDropoffMult = Config.Bind("1. Landmines", "Landmine Damage Dropoff Exponent", 5, new ConfigDescription(
                    "Changes the landmine damage dropoff exponent.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 970 }
                ));

            landmineDamageArmorMult = Config.Bind("1. Landmines", "Landmine Armor Damage Multiplier", 0.35f, new ConfigDescription(
                    "Changes the armor damage multiplier of landmines.",
                    new AcceptableValueRange<float>(0.01f, 1f),
                    new ConfigurationManagerAttributes() { Order = 960 }
                ));

            landmineLightBleedDelta = Config.Bind("1. Landmines", "Landmine Light Bleed Delta", 0.5f, new ConfigDescription(
                    "Changes the chance for a bleed to occur after an explosion. Affected by limb distance from explosion.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 950 }
                ));

            landmineHeavyBleedDelta = Config.Bind("1. Landmines", "Landmine Heavy Bleed Delta", 0.2f, new ConfigDescription(
                    "Changes the chance for a bleed to occur after an explosion. Affected by limb distance from explosion.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 940 }
                ));

            // Claymores
            /*
            claymoreDamage = Config.Bind("2. Claymores", "Claymore Damage", 75f, new ConfigDescription(
                    "Changes the damage of claymores.",
                    new AcceptableValueRange<float>(0.01f, 250f)
                ));

            claymoreRange = Config.Bind("2. Claymores", "Claymore Trigger Range", 6f, new ConfigDescription(
                    "Changes the trigger range of claymores.",
                    new AcceptableValueRange<float>(1f, 20f)
                ));

            claymoreExplosionRange = Config.Bind("2. Claymores", "Claymore Explosion Range", 5f, new ConfigDescription(
                    "Changes the explosion radius of claymores.",
                    new AcceptableValueRange<float>(0.01f, 15f)
                ));

            claymoreDamageArmorMult = Config.Bind("2. Claymores", "Claymore Armor Damage Multiplier", 0.5f, new ConfigDescription(
                    "Changes the armor damage multiplier of claymores.",
                    new AcceptableValueRange<float>(0.01f, 1f)
                ));

            claymoreDamageAngle = Config.Bind("2. Claymores", "Claymore Damage Radius", 45f, new ConfigDescription(
                    "Changes the maximum damage angle of claymores.",
                    new AcceptableValueRange<float>(5f, 360f)
                ));*/

            MapConfig.LoadMapConfig();
            BundleHelper.LoadAssets();

            new DoFracturePatch().Enable();
            new OnGameStartedPatch().Enable();
        }
    }
}
