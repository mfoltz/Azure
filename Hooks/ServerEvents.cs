using Bloodstone.API;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Systems;

namespace VCreate.Hooks
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
                Plugin.Log.LogError(ex);
            }
        }

        [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
        public static class GameBootstrapQuit_Patch
        {
            public static void Prefix()
            {
                DataStructures.Save();
                // reset all horses to enabled state
                EnableHorsesOnQuit();
            }
        }

        [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
        public class TriggerPersistenceSaveSystem_Patch
        {
            public static void Prefix()
            {
                //DataStructures.Save();
            }
        }
        public static void EnableHorsesOnQuit()
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            NativeArray<Entity> entityArray = entityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = (Il2CppStructArray<ComponentType>)new ComponentType[4]
              {
            ComponentType.ReadWrite<Immortal>(),
            ComponentType.ReadWrite<Mountable>(),
            ComponentType.ReadWrite<BuffBuffer>(),
            ComponentType.ReadWrite<PrefabGUID>()
              },
                Options = EntityQueryOptions.IncludeDisabled
            }).ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entityArray)
            {
                if (Utilities.HasComponent<Disabled>(entity))
                    SystemPatchUtil.Enable(entity);
            }
            entityArray.Dispose();
        }
    }
}