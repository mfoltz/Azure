using AdminCommands;
using Bloodstone.API;
using DismantleDenied.Core;
using FMOD.Studio;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.CastleBuilding;
using ProjectM.CastleBuilding.Placement;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.TextCore;
using VRising.GameData;
using VRising.GameData.Models;
using static ProjectM.Network.SetTimeOfDayEvent;
using static VCF.Core.Basics.RoleCommands;
using Exception = System.Exception;
using Plugin = DismantleDenied.Core.Plugin;
using User = ProjectM.Network.User;

//WIP

namespace DismantleDenied.Hooks
{
    [HarmonyPatch(typeof(SpawnCastleHeartSystem))]
    public static class SpawnCastleHeartSystemPatch
    {
        [HarmonyPatch("OnUpdate"), HarmonyPostfix]
        public static void OnUpdatePostfix(SpawnCastleHeartSystem __instance)
        {
            if (ChatCommands.CastleLimitsDisabledSetting.Value)
            {
                // don't update cache when freebuild turned on, idk why exactly but that feels like a bad idea
                return;
            }
            try
            {
                // retrieve territoryEntity and entityManager
                Entity territoryEntity;
                EntityManager entityManager = VWorld.Server.EntityManager;
                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entityArray)
                {
                    // Only proceed if the entity has a EntityOwner component
                    if (Utilities.HasComponent<EntityOwner>(entity))
                    {
                        Entity owner = Utilities.GetComponentData<EntityOwner>(entity);
                        if (CastleTerritoryCache.TryGetCastleTerritory(owner, out territoryEntity))
                        {
                            // update territory cache
                            Plugin.Logger.LogInfo("Adding to territory cache...");
                            CastleTerritoryCache.AddTerritory(territoryEntity, entityManager);
                        }
                        else
                        {
                            // reinitialize if needed
                            Plugin.Logger.LogInfo("Reinitializing territory cache...");
                            CastleTerritoryCache.Initialize();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogInfo(e);
            }
        }
    }

    [HarmonyPatch(typeof(DestroyCastleHeartSystem))]
    public static class DestroyCastleHeartSystemPatch
    {
        [HarmonyPatch("OnUpdate"), HarmonyPostfix]
        public static void OnUpdatePostfix(DestroyCastleHeartSystem __instance)
        {
            if (ChatCommands.CastleLimitsDisabledSetting.Value)
            {
                // don't update cache when freebuild turned on, idk why exactly but that feels like a bad idea
                return;
            }
            try
            {
                // retrieve territoryEntity and entityManager
                Entity territoryEntity;
                EntityManager entityManager = VWorld.Server.EntityManager;
                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entityArray)
                {
                    // Only proceed if the entity has a EntityOwner component
                    if (Utilities.HasComponent<EntityOwner>(entity))
                    {
                        Entity owner = Utilities.GetComponentData<EntityOwner>(entity);
                        if (CastleTerritoryCache.TryGetCastleTerritory(owner, out territoryEntity))
                        {
                            // update territory cache
                            Plugin.Logger.LogInfo("Removing from territory cache...");
                            CastleTerritoryCache.RemoveTerritory(territoryEntity, entityManager);
                        }
                        else
                        {
                            // reinitialize if needed
                            Plugin.Logger.LogInfo("Reinitializing territory cache...");
                            CastleTerritoryCache.Initialize();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo(ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.HandleDismantleTileModelEvents))]
    public static class PlaceTileModelSystem_DismantlePatch
    {
        private static HashSet<Entity> processedEntities = new HashSet<Entity>();

        public static bool Prefix(PlaceTileModelSystem __instance, NativeHashMap<NetworkId, Entity> networkIdToEntityMap)
        {
            // Assume dismantling is disallowed by default.
            bool allowDismantling = false;

            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> dismantleArray = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);
                // Process dismantling events
                allowDismantling = ProcessDismantlingEvents(entityManager, dismantleArray, networkIdToEntityMap);

                dismantleArray.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
                // On exception, we maintain the default stance of not allowing dismantling.
            }

            // Return the result of processing. If true, dismantling is allowed; if false, it's disallowed.
            return allowDismantling;
        }

        private static bool ProcessDismantlingEvents(EntityManager entityManager, NativeArray<Entity> dismantleArray, NativeHashMap<NetworkId, Entity> networkIdToEntityMap)
        {
            try
            {
                foreach (Entity entity in dismantleArray)
                {
                    if (processedEntities.Contains(entity))
                    {
                        continue; // Skip already processed entities
                    }

                    processedEntities.Add(entity);
                    //entity.LogComponentTypes();
                    DismantleTileModelEvent dismantleTileModelEvent = Utilities.GetComponentData<DismantleTileModelEvent>(entity);
                    NetworkId targetNetworkId = dismantleTileModelEvent.Target;
                    // Only proceed if the entity has a FromCharacter component
                    if (networkIdToEntityMap.TryGetValue(targetNetworkId, out Entity targetEntity))
                    {
                        Plugin.Logger.LogInfo("Intercepting dismantle event...");
                        Entity userEntity = entityManager.GetComponentData<FromCharacter>(entity).User;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        if (user.IsAdmin)
                        {
                            Plugin.Logger.LogInfo("Admin dismantling allowed anywhere.");
                            return true;
                        }
                        string name = user.CharacterName.ToString();
                        UserModel userModel = GameData.Users.GetUserByCharacterName(name);
                        Entity territoryEntity;
                        // Attempt to retrieve the castle territory based on block tile coordinates
                        if (CastleTerritoryCache.TryGetCastleTerritory(targetEntity, out territoryEntity))
                        {
                            territoryEntity.LogComponentTypes();
                            // Further checks to ensure the territory entity matches the player's castle territory
                            Plugin.Logger.LogInfo("Dismantling allowed if object is in player's territory.");
                            return true;
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Dismantling attempt outside player's territory.");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo(ex.Message);
                return false;
            }
            return false;
            // Default to disallowing dismantling if no entities specifically allow it.
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.HandleBuildTileModelEvents))]
    public static class PlaceTileModelSystem_BuildPatch
    {
        private static HashSet<Entity> processedEntities = new HashSet<Entity>();
        private static readonly PrefabGUID CastleHeartPrefabGUID = new PrefabGUID(-485210554); // Castle Heart prefab

        public static bool Prefix(PlaceTileModelSystem __instance, NativeHashMap<NetworkId, Entity> networkIdToEntityMap)
        {
            bool allowBuilding = true;

            // Assume building is disallowed by default and only made true once a castle heart is verified to not be involved for freebuild mode only
            if (ChatCommands.CastleLimitsDisabledSetting.Value)
            {
                try
                {
                    EntityManager entityManager = __instance.EntityManager;
                    NativeArray<Entity> buildArray = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
                    // Process building events for castle hearts
                    allowBuilding = ProcessBuildingEvents(__instance, entityManager, buildArray, networkIdToEntityMap);
                    if (!allowBuilding)
                    {
                        __instance.DestroyInstance();
                    }
                    buildArray.Dispose();
                    return allowBuilding;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError(ex.Message);
                    return false;
                    // On exception, we maintain the default stance of not allowing dismantling.
                }
            }
            else
            {
                return allowBuilding;
            }
        }

        private static bool ProcessBuildingEvents(PlaceTileModelSystem __instance, EntityManager entityManager, NativeArray<Entity> buildArray, NativeHashMap<NetworkId, Entity> networkIdToEntityMap)
        {
            try
            {
                foreach (Entity entity in buildArray)
                {
                    if (processedEntities.Contains(entity))
                    {
                        continue; // Skip already processed entities
                    }

                    processedEntities.Add(entity);
                    //entity.LogComponentTypes();
                    BuildTileModelEvent buildTileModelEvent = Utilities.GetComponentData<BuildTileModelEvent>(entity);
                    PrefabGUID prefabGUID = buildTileModelEvent.PrefabGuid;
                    FromCharacter fromCharacter = Utilities.GetComponentData<FromCharacter>(entity);
                    Entity userEntity = fromCharacter.User;

                    if (prefabGUID == CastleHeartPrefabGUID)
                    {
                        User user = Utilities.GetComponentData<User>(userEntity);
                        // Additional logic to inform the player, as before
                        string bonk = "Bad Vampire, no merlot!";
                        ServerChatUtils.SendSystemMessageToClient(entityManager, user, bonk);
                        Plugin.Logger.LogInfo("CastleHeart placement detected, attempting to yeet...");
                        // Call your method to destroy the Castle Heart entity
                        //PlaceTileModelSystemExtensions.TryDestroyCastleHeart(entity); // Adjust this call as necessary for your context
                        PlaceTileModelSystemExtensions.ScheduleCastleHeartDestruction(__instance, entity);
                        return false; // Prevent further building event processing
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo(ex.Message);
                return true;
            }
            return true;
            // Default to allowing building if no castle hearts detected
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem))]
    public static class PlaceTileModelSystem_AbilityCastFinishedPatch
    {
        private static HashSet<Entity> processedEntities = new HashSet<Entity>();

        [HarmonyPatch(nameof(PlaceTileModelSystem.HandlePlaceTileModelAbilityCastFinishedEvents))]
        [HarmonyPrefix]
        public static bool Prefix(PlaceTileModelSystem __instance)
        {
            bool allowEventProcessing = true; // Assume processing is allowed by default

            try
            {
                EntityManager entityManager = __instance.EntityManager;
                // Assume you have a query or method to obtain relevant entities for ability cast finished events

                if (__instance._AbilityCastFinishedQuery.IsEmpty || __instance._AbilityCastFinishedQuery.IsEmptyIgnoreFilter)
                {
                    // Proceed with logic using the query
                    return false;
                }
                NativeArray<Entity> abilityCastFinishedArray = __instance._AbilityCastFinishedQuery.ToEntityArray(Allocator.Temp);
                // Process each entity related to the ability cast finished event
                foreach (Entity entity in abilityCastFinishedArray)
                {
                    if (processedEntities.Contains(entity))
                    {
                        continue; // Skip already processed entities
                    }
                    processedEntities.Add(entity);

                    // Example: Log event or apply custom logic based on entity data
                    Plugin.Logger.LogInfo("Processing ability cast finished event for entity.");

                    // Custom logic to determine if event processing should continue
                    // This might involve checking the entity's components, game state, or other conditions
                }

                abilityCastFinishedArray.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
                allowEventProcessing = false; // Disallow processing on exception
            }

            return allowEventProcessing; // Return the result of processing
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem))]
    public static class PlaceTileModelSystem_StartEditPatch
    {
        [HarmonyPatch(nameof(PlaceTileModelSystem.HandleStartEditTileModelEvents))]
        [HarmonyPrefix]
        public static bool Prefix(PlaceTileModelSystem __instance, NativeHashMap<NetworkId, Entity> networkIdToEntityMap)
        {
            try
            {
                //Plugin.Logger.LogInfo("Processing start edit tile model events...");
                if (__instance._StartEditQuery.IsEmptyIgnoreFilter)
                {
                    //Plugin.Logger.LogInfo("No start edit tile model events to process.");
                    return false; // Skip the original method execution
                }
                return true;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem))]
    public static class PlaceTileModelSystem_CancelEditPatch
    {
        [HarmonyPatch(nameof(PlaceTileModelSystem.HandleCancelEditTileModelEvents))]
        [HarmonyPrefix]
        public static bool Prefix(PlaceTileModelSystem __instance, NativeHashMap<NetworkId, Entity> networkIdToEntityMap)
        {
            try
            {
                //Plugin.Logger.LogInfo("Processing start edit tile model events...");
                if (__instance._CancelEditQuery.IsEmptyIgnoreFilter)
                {
                    //Plugin.Logger.LogInfo("No start edit tile model events to process.");
                    return false; // Skip the original method execution
                }
                return true;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e.Message);
                return true;
            }
        }
    }

    public class PlaceTileModelSystemExtensions
    {
        public static void TryDestroyCastleHeart(Entity castleHeartEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            if (!entityManager.Exists(castleHeartEntity))
            {
                Plugin.Logger.LogInfo("Castle Heart entity does not exist or has already been destroyed.");
                return;
            }

            try
            {
                Plugin.Logger.LogInfo($"Attempting to destroy Castle Heart entity: {castleHeartEntity.Index}");
                entityManager.DestroyEntity(castleHeartEntity);
                // Adjusted to pass the entity if needed
                // Verify destruction if possible
                if (!entityManager.Exists(castleHeartEntity))
                {
                    Plugin.Logger.LogInfo("Castle Heart entity successfully destroyed.");
                }
                else
                {
                    Plugin.Logger.LogWarning("Castle Heart entity destruction verification failed.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error destroying Castle Heart entity: {ex.Message}");
                // Handle error or fallback
            }
        }

        public static void ScheduleCastleHeartDestruction(PlaceTileModelSystem __instance, Entity castleHeartEntity)
        {
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
                Plugin.Logger.LogInfo("Preparing for Castle Heart entity destruction...");
                // Obtain an EntityCommandBuffer from a system or create one via a system's EntityCommandBufferSystem
                EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
                // Schedule the destruction of the Castle Heart entity
                if (entityManager.Exists(castleHeartEntity))
                {
                    // Schedule the destruction of the Castle Heart entity
                    Plugin.Logger.LogInfo("Scheduling Castle Heart entity destruction...");
                    entityCommandBuffer.DestroyEntity(castleHeartEntity);
                }
                else
                {
                    Plugin.Logger.LogInfo("Castle Heart entity already destroyed or invalid.");
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e.Message);
            }

            // The actual destruction will be performed at the end of the simulation frame,
            // avoiding issues with immediate state changes during entity iteration or system updates
        }
    }
}