using Bloodstone.API;
using DismantleDenied.Core;
using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.TextCore;
using VRising.GameData;
using VRising.GameData.Models;

//WIP

namespace DismantleDenied.Hooks
{
    [HarmonyPatch(typeof(SpawnCastleHeartSystem))]
    public static class SpawnCastleHeartSystemPatch
    {
        [HarmonyPatch("OnUpdate"), HarmonyPostfix]
        public static void OnUpdatePostfix(SpawnCastleHeartSystem __instance)
        {
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
    public static class PlaceTileModelSystem_Patch
    {
        private static HashSet<Entity> processedEntities = new HashSet<Entity>();

        public static bool Prefix(PlaceTileModelSystem __instance)
        {
            // Assume dismantling is disallowed by default.
            bool allowDismantling = false;

            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> dismantleArray = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);

                // Process dismantling events
                allowDismantling = ProcessDismantlingEvents(entityManager, dismantleArray);

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

        private static bool ProcessDismantlingEvents(EntityManager entityManager, NativeArray<Entity> dismantleArray)
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

                    // Only proceed if the entity has a FromCharacter component
                    if (Utilities.HasComponent<FromCharacter>(entity) && Utilities.HasComponent<TilePosition>(entity))
                    {
                        Plugin.Logger.LogInfo("Intercepting dismantle event...");
                        Entity userEntity = entityManager.GetComponentData<FromCharacter>(entity).User;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        string name = user.CharacterName.ToString();
                        UserModel userModel = GameData.Users.GetUserByCharacterName(name);

                        // Convert entity's tile position to block tile coordinates
                        TilePosition tilePosition = entityManager.GetComponentData<TilePosition>(entity);
                        float2 blockTileCoordinates = tilePosition.Tile / CastleTerritoryCache.TileToBlockDivisor;

                        Entity territoryEntity;
                        // Attempt to retrieve the castle territory based on block tile coordinates
                        if (CastleTerritoryCache.TryGetCastleTerritory(blockTileCoordinates, out territoryEntity))
                        {
                            // Further checks to ensure the territory entity matches the player's castle territory
                            Plugin.Logger.LogInfo("Dismantling allowed within player's territory.");
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
}