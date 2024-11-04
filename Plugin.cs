using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using VisibleMines.Helpers;
using VisibleMines.Patches;

namespace VisibleMines
{
    [BepInPlugin("com.pein.visiblemines", "Visible Minefields", "1.1.1")]
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
        public static ConfigEntry<float> landmineFractureDelta { get; set; }
        public static ConfigEntry<float> landmineStaminaBurnRate { get; set; }

        // Screen shake
        public static ConfigEntry<float> screenShakeIntensityAmount { get; set; }
        public static ConfigEntry<float> screenShakeIntensityWeapon { get; set; }
        public static ConfigEntry<float> screenShakeIntensityCamera { get; set; }

        // Debug
        public static ConfigEntry<bool> debugEnabled { get; set; }


        // Claymores
        /*
        public static ConfigEntry<float> claymoreDamage { get; set; }
        public static ConfigEntry<float> claymoreRange { get; set; }
        public static ConfigEntry<float> claymoreExplosionRange { get; set; }
        public static ConfigEntry<float> claymoreDamageArmorMult { get; set; }
        public static ConfigEntry<float> claymoreDamageAngle { get; set; }
        */

        private void Awake()
        {
            Debug.SetLogger(Logger);

            string mineCategory = "1. Landmines";
            string visualCategory = "2. Screen Shake";
            string debugCategory = "3. Debug";

            // Landmine settings
            landminePosOffset = Config.Bind(mineCategory, "Landmine Pos Offset", 0.01f, new ConfigDescription(
                    "Changes the up/down offset of landmines. Higher values spawn the mines lower into the ground, making them more difficult to spot. Low values may make the mines float slightly off the ground.",
                    new AcceptableValueRange<float>(0.01f, 0.2f),
                    new ConfigurationManagerAttributes() { Order = 1000 }
                ));

            landmineDamage = Config.Bind(mineCategory, "Landmine Damage", 100f, new ConfigDescription(
                    "Changes the damage of landmines.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 990 }
                ));

            landmineExplosionRange = Config.Bind(mineCategory, "Landmine Explosion Radius", 5f, new ConfigDescription(
                    "Changes the explosion radius of landmines.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 980 }
                ));

            landmineDamageDropoffMult = Config.Bind(mineCategory, "Landmine Damage Dropoff Exponent", 5, new ConfigDescription(
                    "Changes the landmine damage dropoff exponent.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 970 }
                ));

            landmineDamageArmorMult = Config.Bind(mineCategory, "Landmine Armor Damage Multiplier", 0.35f, new ConfigDescription(
                    "Changes the armor damage multiplier of landmines.",
                    new AcceptableValueRange<float>(0.01f, 1f),
                    new ConfigurationManagerAttributes() { Order = 960 }
                ));

            landmineLightBleedDelta = Config.Bind(mineCategory, "Landmine Light Chance", 0.4f, new ConfigDescription(
                    "Changes the chance for a bleed to occur after an explosion. Chance is calculated per limb. Affected by limb distance from explosion.",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes() { Order = 950 }
                ));

            landmineHeavyBleedDelta = Config.Bind(mineCategory, "Landmine Heavy Chance", 0.2f, new ConfigDescription(
                    "Changes the chance for a bleed to occur after an explosion. Chance is calculated per limb. Affected by limb distance from explosion.",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes() { Order = 940 }
                ));

            landmineFractureDelta = Config.Bind(mineCategory, "Landmine Fracture Chance", 1.0f, new ConfigDescription(
                    "Changes the chance for a fracture to occur after an explosion. Fractures appear on the limb closest to the explosion. Affected by limb distance from the explosion.",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes() { Order = 930 }
                ));

            landmineStaminaBurnRate = Config.Bind(mineCategory, "Landmine Stamina Burn Rate", 1.0f, new ConfigDescription(
                    "Changes the chance for a fracture to occur after an explosion. Fractures appear on the limb closest to the explosion. Affected by limb distance from the explosion.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 929 }
                ));

            // Explosion screen shake settings
            screenShakeIntensityAmount = Config.Bind(visualCategory, "Overall Shake Intensity", 1.0f, new ConfigDescription(
                    "Changes the overall shake intensity.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 920 }
                ));

            screenShakeIntensityWeapon = Config.Bind(visualCategory, "Weapon Shake Intensity", 0.25f, new ConfigDescription(
                    "Changes the weapon shake intensity.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 910 }
                ));

            screenShakeIntensityCamera = Config.Bind(visualCategory, "Camera Shake Intensity", 1.0f, new ConfigDescription(
                    "Changes the camera shake intensity.",
                    null,
                    new ConfigurationManagerAttributes() { Order = 900 }
                ));

            debugEnabled = Config.Bind(debugCategory, "Enable Debug", false, new ConfigDescription(
                    "Enables debug mode",
                    null,
                    new ConfigurationManagerAttributes() { Order = 890, IsAdvanced = true }
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
