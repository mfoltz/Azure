using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using RPGAddOns.Core;
using Unity.Entities;
using Unity.Mathematics;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using Math = System.Math;
using Random = System.Random;

namespace RPGAddOns.PvERank
{
    [HarmonyPatch]
    internal class AbilitiesHook
    {
        [HarmonyPatch(typeof(UIDataSystem), nameof(UIDataSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdate(UIDataSystem __instance)
        {
        }

        // Usage
    }
}