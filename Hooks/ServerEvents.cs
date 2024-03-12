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
                // return all players to their original bodies
                ReturnSoulsOnQuit();
            }
        }

        [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
        public class TriggerPersistenceSaveSystem_Patch
        {
            public static void Prefix()
            {
                DataStructures.Save();
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

        public static void ReturnSoulsOnQuit()
        {
            var souls = DataStructures.PlayerSettings;
            foreach (var soul in souls)
            {
                if (souls.TryGetValue(soul.Key, out Omnitool data) && !data.OriginalBody.Equals(null))
                {
                    ReturnSoul(data);
                }
            }
        }

        public static void ReturnSoul(Omnitool soul)
        {
            string[] parts = soul.OriginalBody.Split(", ");
            if (parts.Length == 2 && int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int version))
            {
                Entity originalBody = new Entity { Index = index, Version = version };
                if (VWorld.Server.EntityManager.Exists(originalBody))
                {
                    ControlDebugEvent controlDebugEvent = new ControlDebugEvent()
                    {
                        EntityTarget = originalBody,
                        Target = originalBody.Read<NetworkId>()
                    };
                    DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                    existingSystem.ControlUnit(new FromCharacter() { User = originalBody, Character = originalBody.Read<User>().LocalCharacter._Entity }, controlDebugEvent);
                    soul.OriginalBody = "";
                    DataStructures.Save();
                    //ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, soul.Value.User.Read<User>(), "Returned to original body.");
                }
                else
                {
                    // find player's original body if entity reference no longer valid, need to implement
                }
            }
        }
    }
}