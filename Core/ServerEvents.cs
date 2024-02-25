using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace WorldBuild.Core
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
        [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
        public static class GameBootstrapQuit_Patch
        {
            public static void Prefix()
            {
                // Existing logic

                // New logic to re-enable all horses
                var entityManager = VWorld.Server.EntityManager;
                EntityQuery horseQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new[] {
                    ComponentType.ReadWrite<Immortal>(),
                    ComponentType.ReadWrite<Mountable>(),
                    ComponentType.ReadWrite<BuffBuffer>(),
                    ComponentType.ReadWrite<PrefabGUID>(),
                    },
                    Options = EntityQueryOptions.IncludeDisabled
                });
                NativeArray<Entity> horseEntities = horseQuery.ToEntityArray(Allocator.TempJob);
                foreach (var horse in horseEntities)
                {
                    if (Utilities.HasComponent<Disabled>(horse))
                    {
                        SystemPatchUtil.Enable(horse);
                    }
                    
                }
                horseEntities.Dispose();

                // Additional cleanup or saving logic as needed
            }
        }
    }
}