using Bloodstone.API;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using static VCreate.Core.Commands.PetCommands;

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
                DataStructures.SavePlayerSettings();
                // reset all horses to enabled state
                EnableHorsesOnQuit();
                EnableFamiliarsOnQuit();
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

        public static void EnableFamiliarsOnQuit()
        {
            var keys = DataStructures.PlayerPetsMap.Keys;
            foreach (var key in keys)
            {
                var pet = DataStructures.PlayerPetsMap[key];
                var otherkeys = pet.Keys;
                foreach (var otherkey in otherkeys)
                {
                    if (pet.TryGetValue(otherkey, out var value))
                    {
                        if (!value.Combat)
                        {
                            value.Combat = true;
                            pet[otherkey] = value;
                            DataStructures.PlayerPetsMap[key] = pet;
                            DataStructures.SavePetExperience();
                        }
                    }
                }
            }
            foreach (var key in PetCommands.PlayerFamiliarStasisMap.Keys)
            {
                if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(key, out FamiliarStasisState data))
                {
                    if (data.IsInStasis)
                    {
                        SystemPatchUtil.Enable(data.FamiliarEntity);
                        data.IsInStasis = false;
                        PetCommands.PlayerFamiliarStasisMap[key] = data;
                    }
                }
            }
        }
    }
}