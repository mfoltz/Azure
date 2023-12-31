using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace RPGAddOns.Patch
{

    public delegate void OnGameDataInitializedEventHandler(World world);

    internal class ServerEvents
    {
        internal static event OnGameDataInitializedEventHandler OnGameDataInitialized;

        [HarmonyPatch(typeof(LoadPersistenceSystemV2), nameof(LoadPersistenceSystemV2.SetLoadState))]
        [HarmonyPostfix]
        private static void ServerStartupStateChange_Postfix(ServerStartupState.State loadState, LoadPersistenceSystemV2 __instance)
        {
            try
            {
                if (loadState == ServerStartupState.State.SuccessfulStartup)
                {
                    OnGameDataInitialized?.Invoke(__instance.World);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }
    }
}
