
using HarmonyLib;
using ProjectM;

namespace RPGAddOns
{
    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch
    {
        public static void Prefix()
        {
            Commands.SavePlayerPrestige();
            Commands.SavePlayerResets();
        }
    }

    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Prefix()
        {
            Commands.SavePlayerPrestige();
            Commands.SavePlayerResets();
        }
    }
}
