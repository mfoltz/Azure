using HarmonyLib;
using ProjectM;

namespace RPGAddOns
{
    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch
    {
        public static void Prefix()
        {
            Commands.SavePlayerRanks();
            Commands.SavePlayerPrestiges();
        }
    }

    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    public static class TriggerPersistenceSaveSystem_Patch
    {
        public static void Prefix()
        {
            Commands.SavePlayerRanks();
            Commands.SavePlayerPrestiges();
        }
    }
}