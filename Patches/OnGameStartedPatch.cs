using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using VisibleMines.Helpers;
using UnityEngine;
using VisibleMines.Components;
using System.Collections.Generic;

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
        }
    }
}
