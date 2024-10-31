using EFT.HealthSystem;
using SPT.Reflection.Patching;
using System.Reflection;

namespace VisibleMines.Patches
{
    public class DoFracturePatch : ModulePatch
    {
        private static bool _ignoreNextFracture = false;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(ActiveHealthController).GetMethod(nameof(ActiveHealthController.DoFracture));
        }

        public static void SetIgnoreNextFracture(bool state)
        {
            _ignoreNextFracture = state;
        }

        [PatchPrefix]
        private static bool PatchPrefix(ActiveHealthController __instance, EBodyPart bodyPart)
        {
            if (_ignoreNextFracture)
            {
                _ignoreNextFracture = false;
                return false;
            }

            return true;
        }
    }
}
