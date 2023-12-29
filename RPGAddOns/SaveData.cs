using BloodyPoints.Command;
using HarmonyLib;
using ProjectM;

namespace BloodyPoints.Patch
{
    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch
    {
        public static void Prefix()
        {
            Commands.SaveWaypoints();
            Commands.SavePlayerPrestige();
            Commands.SavePlayerResets();
        }
    }

    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Prefix()
        {
            Commands.SaveWaypoints();
            Commands.SavePlayerPrestige();
            Commands.SavePlayerResets();
        }
    }
}
