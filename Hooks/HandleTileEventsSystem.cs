using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using V.Core.Commands;
using V.Core.Tools;
using Exception = System.Exception;
using Plugin = V.Core.Plugin;
using User = ProjectM.Network.User;

//WIP

namespace V.Hooks
{
    [HarmonyPatch(typeof(SpawnCastleHeartSystem))]
    public static class SpawnCastleHeartSystem_Patch
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
    public static class DestroyCastleHeartSystem_Patch
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

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public static class CastleHeartPlacementPatch
    {
        private static readonly PrefabGUID CastleHeartPrefabGUID = new PrefabGUID(-485210554); // Adjust this GUID to match your Castle Heart's

        public static void Prefix(PlaceTileModelSystem __instance)
        {
            EntityManager entityManager = __instance.EntityManager;
            if (!ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value)
            {
                return;
            }

            var jobs = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
            foreach (var job in jobs)
            {
                if (IsCastleHeart(job))
                {
                    CancelCastleHeartPlacement(entityManager, job);
                }
            }
            jobs.Dispose();
        }

        private static bool IsCastleHeart(Entity job)
        {
            var entityManager = VWorld.Server.EntityManager;
            var buildTileModelData = entityManager.GetComponentData<BuildTileModelEvent>(job);
            return buildTileModelData.PrefabGuid.Equals(CastleHeartPrefabGUID);
        }

        private static void CancelCastleHeartPlacement(EntityManager entityManager, Entity job)
        {
            var userEntity = entityManager.GetComponentData<FromCharacter>(job).User;
            var user = entityManager.GetComponentData<User>(userEntity);

            StringBuilder message = new StringBuilder("Bad vampire, no merlot! (Castle Heart placement is disabled during freebuild)");

            ServerChatUtils.SendSystemMessageToClient(entityManager, user, message.ToString());
            SystemPatchUtil.Destroy(job);
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyCanDismantle))]
    public static class VerifyCanDismantlePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            // Early exit if dismantling is already deemed not possible
            if (!__result) return;

            Plugin.Logger.LogInfo("Intercepting dismantle event...");
            //tileModelEntity.LogComponentTypes();
            if (Utilities.HasComponent<UserOwner>(tileModelEntity))
            {
                NetworkedEntity networkedEntity = Utilities.GetComponentData<UserOwner>(tileModelEntity).Owner;
                Entity entity = networkedEntity._Entity;
                //entity.LogComponentTypes();
                User user = Utilities.GetComponentData<User>(entity);
                //User user = Utilities.GetComponentData<User>(userEntity);
                if (user.IsAdmin)
                {
                    Plugin.Logger.LogInfo("Admin dismantling allowed.");
                    return; // Allow admins to dismantle anything without further checks
                }

                string name = user.CharacterName.ToString();
                Entity territoryEntity;
                if (CastleTerritoryCache.TryGetCastleTerritory(tileModelEntity, out territoryEntity))
                {
                    Plugin.Logger.LogInfo("Dismantling allowed within player's territory.");
                    return; // Allow dismantling
                }
                else
                {
                    Plugin.Logger.LogInfo("Dismantling attempt outside player's territory.");
                    __result = false;
                }
            }
            else
            {
                // Log or handle cases where the user component is missing or userEntity isn't valid
                Plugin.Logger.LogInfo("Unable to verify user entity for dismantle operation.");
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCanMoveOrRotateAfterBuilt))]
    public static class VerifyCanMovePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            // Early exit if moving is already deemed not possible
            if (!__result) return;

            Plugin.Logger.LogInfo("Intercepting move event...");
            //tileModelEntity.LogComponentTypes();
            if (Utilities.HasComponent<UserOwner>(tileModelEntity))
            {
                NetworkedEntity networkedEntity = Utilities.GetComponentData<UserOwner>(tileModelEntity).Owner;
                Entity entity = networkedEntity._Entity;
                //entity.LogComponentTypes();
                User user = Utilities.GetComponentData<User>(entity);
                if (user.IsAdmin)
                {
                    Plugin.Logger.LogInfo("Admin moving allowed.");
                    return; // Allow admins to dismantle anything without further checks
                }

                string name = user.CharacterName.ToString();
                Entity territoryEntity;
                if (CastleTerritoryCache.TryGetCastleTerritory(tileModelEntity, out territoryEntity))
                {
                    Plugin.Logger.LogInfo("Moving allowed within player's territory.");
                    return; // Allow dismantling
                }
                else
                {
                    Plugin.Logger.LogInfo("Moving attempt outside player's territory.");
                    __result = false;
                }
            }
            else
            {
                // Log or handle cases where the user component is missing or userEntity isn't valid
                Plugin.Logger.LogInfo("Unable to verify user entity for move operation.");
            }
        }
    }
}