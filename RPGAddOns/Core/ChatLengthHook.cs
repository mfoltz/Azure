using HarmonyLib;
using ProjectM;
using ProjectM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGAddOns.Core
{
    [HarmonyPatch]
    internal class ChatPatch
    {
        [HarmonyPatch(typeof(ClientChatSystem), nameof(ClientChatSystem.LimitString))]
        [HarmonyPrefix]
        public static void LimitString(ref string text, ref int length)
        {
            // Modify the length parameter before the original method is executed
            length = length * 2;
        }
    }
}