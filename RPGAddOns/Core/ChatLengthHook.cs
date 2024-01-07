using HarmonyLib;
using ProjectM.UI;

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
            Plugin.Logger.LogInfo($"ClientChatSystem OnUpdate()");
        }
    }
}