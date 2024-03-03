using HarmonyLib;
using ProjectM.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBuild.Core;

namespace VBuild.Hooks
{
    [HarmonyPatch(typeof(HybridModelSystem), nameof(HybridModelSystem.OnUpdate))]
    public static class HybridModelSystem_Patch
    {
        public static void Prefix(HybridModelSystem __instance)
        {
            Plugin.Logger.LogInfo("HybridModelSystem Prefix called...");
        }
    }
}

