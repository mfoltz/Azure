using Bloodstone.API;
using VBuild.Core;
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
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.TextCore;
using static ProjectM.CastleBuilding.Placement.GetPlacementResult;
using static ProjectM.Network.SetTimeOfDayEvent;
using static VCF.Core.Basics.RoleCommands;
using Exception = System.Exception;
using Plugin = VBuild.Core.Plugin;
using User = ProjectM.Network.User;
using VBuild.BuildingSystem;
using VBuild.Core.Toolbox;
using VBuild.Data;
using ProjectM.Gameplay.Scripting;
using ProjectM.Tiles;
using ProjectM.Gameplay;
using ProjectM.Shared.Systems;
using VBuild.Core.Services;

//WIP

namespace WorldBuild.Hooks
{

    /*
    [HarmonyPatch(typeof(FollowerSystem), nameof(FollowerSystem.OnUpdate))]
    public static class FollowerSystem_Patch
    {
        public static void Prefix(FollowerSystem __instance)
        {
            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                Plugin.Logger.LogInfo("FollowerSystem Prefix called, processing follower entity00...");
                entity.LogComponentTypes();
                Follower follower = Utilities.GetComponentData<Follower>(entity);
                Entity followerEntity = follower.Followed;
               
                
            }
            entities.Dispose();

            entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                Plugin.Logger.LogInfo("FollowerSystem Prefix called, processing follower entity01...");
                entity.LogComponentTypes();



            }
            entities.Dispose();
        }
    }
    
    
    [HarmonyPatch(typeof(CreateGameplayEventsOnDamageTakenSystem), nameof(CreateGameplayEventsOnDamageTakenSystem.OnUpdate))]
    public static class CreateGameplayEventsOnDamageTakenSystem_Patch
    {
        public static void Prefix(CreateGameplayEventsOnDamageTakenSystem __instance)
        {
            NativeArray<Entity> entities = __instance._DamageTakenEventQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                //Plugin.Logger.LogInfo("CreateGameplayEventsOnDamageTakenSystem Prefix called, processing DamageTakenEvent...");
                //entity.LogComponentTypes();
                DamageTakenEvent damageTakenEvent = Utilities.GetComponentData<DamageTakenEvent>(entity);
                Entity damageTakenEventEntity = damageTakenEvent.Entity;
                Entity source = damageTakenEvent.Source;
                //damageTakenEventEntity.LogComponentTypes();
                //source.LogComponentTypes();
                if (Utilities.HasComponent<EntityOwner>(source))
                {
                    EntityOwner entityOwner = Utilities.GetComponentData<EntityOwner>(source);
                    //entityOwner.Owner.LogComponentTypes();
                    if (!Utilities.HasComponent<ControlledBy>(entityOwner.Owner))
                    {
                        continue;
                    }
                    ControlledBy controlledBy = Utilities.GetComponentData<ControlledBy>(entityOwner.Owner);
                    Entity controller = controlledBy.Controller;
                    //controller.LogComponentTypes();
                    User user = Utilities.GetComponentData<User>(controller);
                    if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
                    {
                        
                        if (settings.DismantleMode)
                        {
                            Plugin.Logger.LogInfo("Player is in dismantle mode, destroying tile...");
                            if (Utilities.HasComponent<TileModel>(damageTakenEventEntity))
                            {
                                string entityString = damageTakenEventEntity.Index.ToString() + ", " + damageTakenEventEntity.Version.ToString(); //funny story, ask me about it if you end up reading this :P
                                //Plugin.Logger.LogInfo(entityString);

                                SystemPatchUtil.Destroy(damageTakenEventEntity);
                            }
                        }
                        
                    }
                }
            }
            entities.Dispose();
        }
    }
    */
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public static class CastleHeartPlacementPatch
    {
        private static readonly PrefabGUID CastleHeartPrefabGUID = new PrefabGUID(-485210554); // castle heart prefab

        public static void Prefix(PlaceTileModelSystem __instance)
        {
            //Plugin.Logger.LogInfo("PlaceTileModelSystem Prefix called...");
            EntityManager entityManager = VWorld.Server.EntityManager;

            var castJobs = __instance._AbilityCastFinishedQuery.ToEntityArray(Allocator.Temp);
            var jobs = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
            foreach (var job in jobs)
            {
                if (IsCastleHeart(job))
                {
                    if (!VBuild.Core.CoreCommands.WorldBuildToggle.wbFlag)
                    {
                        return;
                    }
                    CancelCastleHeartPlacement(entityManager, job);
                }
            }
            jobs.Dispose();

            foreach (var job in castJobs)
            {
                //Plugin.Logger.LogInfo("AbilityCastFinished event...");
                //job.LogComponentTypes();
                if (Utilities.HasComponent<AbilityPreCastFinishedEvent>(job))
                {
                    AbilityPreCastFinishedEvent abilityPreCastFinishedEvent = Utilities.GetComponentData<AbilityPreCastFinishedEvent>(job);

                    Entity abilityGroupData = abilityPreCastFinishedEvent.AbilityGroup;
                    //abilityGroupData.LogComponentTypes();
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(abilityGroupData);
                    Entity character = abilityPreCastFinishedEvent.Character;
                    if (!Utilities.HasComponent<PlayerCharacter>(character))
                    {
                        continue;
                    }
                    PlayerCharacter playerCharacter = Utilities.GetComponentData<PlayerCharacter>(character);
                    Entity userEntity = playerCharacter.UserEntity;
                    User user = Utilities.GetComponentData<User>(userEntity);
                    if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
                    {
                        if (prefabGUID.Equals(VBuild.Data.Prefabs.AB_Consumable_Tech_Ability_Charm_Level02_AbilityGroup))
                        {
                            Plugin.Logger.LogInfo("Charm ability cast detected...");
                            HandleAbilityCast(userEntity);
                        }
                    }
                }
            }
            castJobs.Dispose();
        }
        public static void HandleAbilityCast(Entity userEntity)
        {
            var user = Utilities.GetComponentData<User>(userEntity);

            if (!Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                return; // or handle the case of missing settings
            }

            // Assuming a method that decides the action based on the ability and settings
            var action = DecideAction(settings);

            // Execute the decided action
            action?.Invoke(userEntity, settings);
        }

        private static System.Action<Entity, BuildSettings> DecideAction(BuildSettings settings)
        {
            // Example: Checking for a specific prefabGUID and toggle
            Plugin.Logger.LogInfo("Deciding action based on settings...");
            
            if (settings.GetToggle("InspectToggle"))
            {
                return (userEntity, _) =>
                {
                    //Plugin.Logger.LogInfo("Inspect mode enabled, skipping tile spawn...");
                    TileSets.InspectHoveredEntity(userEntity);
                };
            }
            else if (settings.GetToggle("KillToggle"))
            {
                return (userEntity, _) =>
                {
                    TileSets.KillHoveredEntity(userEntity);
                };
            }
            else if (settings.GetToggle("ControlToggle"))
            {
                return (userEntity, _) =>
                {

                    VBuild.Hooks.EmoteSystemPatch.ControlCommand(userEntity);
                };
            }
            else if (settings.GetToggle("CopyToggle"))
            {
                return (userEntity, _) =>
                {
                    TileSets.SpawnCopy(userEntity);
                };
            }
            else if (settings.GetToggle("DebuffToggle"))
            {
                return (userEntity, _) =>
                {
                    TileSets.DebuffTileModel(userEntity);
                };
            }


            else
            {
                return (userEntity, _) =>
                {
                    Plugin.Logger.LogInfo("No specific action decided, proceeding with default...");
                    TileSets.SpawnTileModel(userEntity);
                };
            }
            

            
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

            StringBuilder message = new StringBuilder("Bad vampire, no merlot! (Castle Heart placement is disabled during worldbuild)");

            ServerChatUtils.SendSystemMessageToClient(entityManager, user, message.ToString());
            SystemPatchUtil.Destroy(job);
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyCanDismantle))]
    public static class VerifyCanDismantlePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            if (!__result) return;

            Plugin.Logger.LogInfo("Intercepting dismantle event...");

            bool canDismantle = TileOperationUtility.CanPerformOperation(entityManager, tileModelEntity);
            __result = canDismantle;

            if (!canDismantle)
            {
                Plugin.Logger.LogInfo("Dismantling disallowed based on permissions and territory ownership.");
            }
            else
            {
                Plugin.Logger.LogInfo("Dismantling allowed in territory or elsewhere if user has permissions.");
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCanMoveOrRotateAfterBuilt))]
    public static class VerifyCanMovePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            if (!__result) return;

            Plugin.Logger.LogInfo("Intercepting move event...");

            bool canMove = TileOperationUtility.CanPerformOperation(entityManager, tileModelEntity);
            __result = canMove;

            if (!canMove)
            {
                Plugin.Logger.LogInfo("Moving disallowed based on permissions and territory ownership.");
            }
            else
            {
                Plugin.Logger.LogInfo("Moving allowed in territory or elsewhere if user has permissions.");
            }
        }
    }
}

public static class TileOperationUtility
{
    public static bool CanPerformOperation(EntityManager entityManager, Entity tileModelEntity)
    {
        if (!Utilities.HasComponent<UserOwner>(tileModelEntity))
        {
            Plugin.Logger.LogInfo("Unable to verify user entity, disallowing by default.");
            return false;
        }

        var userOwner = Utilities.GetComponentData<UserOwner>(tileModelEntity);
        var user = Utilities.GetComponentData<User>(userOwner.Owner._Entity);

        return CanEditTiles(user) || IsTileInUserTerritory(user, tileModelEntity);
    }

    private static bool CanEditTiles(User user)
    {
        if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings data))
        {
            return data.GetToggle("CanEditTiles");
        }
        return false;
    }

    private static bool IsTileInUserTerritory(User user, Entity tileModelEntity)
    {
        if (CastleTerritoryCache.TryGetCastleTerritory(tileModelEntity, out Entity territoryEntity))
        {
            CastleTerritory castleTerritory = Utilities.GetComponentData<CastleTerritory>(territoryEntity);
            Entity castleHeart = castleTerritory.CastleHeart;
            NetworkedEntity territoryOwner = Utilities.GetComponentData<UserOwner>(castleHeart).Owner;
            User territoryUser = Utilities.GetComponentData<User>(territoryOwner._Entity);
            return user.PlatformId.Equals(territoryUser.PlatformId);
        }
        return false;
    }
}