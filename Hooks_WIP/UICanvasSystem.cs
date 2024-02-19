using HarmonyLib;
using ProjectM;
using ProjectM.UI;
using RPGAddOnsEx.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGAddOnsEx.Hooks_WIP
{
    [HarmonyPatch(typeof(UICanvasSystem), "OnUpdate")]
    public class UICanvasSystem_Patch
    {
        private static void Prefix(UICanvasSystem __instance)
        {
            Plugin.Logger.LogInfo("UICanvasSystem OnUpdate Prefix called...");
        }
    }
}