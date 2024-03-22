using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core.Toolbox;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Systems;
using Plugin = VCreate.Core.Plugin;
using User = ProjectM.Network.User;
using VRising.GameData.Models;

namespace WorldBuild.Hooks
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public static class CastleHeartPlacementPatch
    {
        private static readonly PrefabGUID CastleHeartPrefabGUID = new PrefabGUID(-485210554); // castle heart prefab

        public static void Prefix(PlaceTileModelSystem __instance)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;

            var jobs = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
            foreach (var job in jobs)
            {
                if (IsCastleHeart(job))
                {
                    if (!WorldBuildToggle.WbFlag) continue;
                    CancelCastleHeartPlacement(entityManager, job);
                }
                else
                {
                    if (WorldBuildToggle.WbFlag)
                    {
                        //method for query
                        Plugin.Log.LogInfo("querying for walls...");
                        bool includeDisabled = true;
                        EntityQuery wallQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                        {
                            All = new ComponentType[]
                            {
                                ComponentType.ReadOnly<PrefabGUID>(),
                                ComponentType.ReadOnly<CastleDecayAndRegen>(),
                            },
                            Options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
                        });
                        NativeArray<Entity> wallEntities = wallQuery.ToEntityArray(Allocator.Temp);
                        try
                        {
                            foreach (var entity in wallEntities)
                            {
                                if (!entity.Read<PrefabGUID>().LookupName().ToLower().Contains("wall") || !entity.Read<PrefabGUID>().LookupName().ToLower().Contains("castle")) continue;
                                if (entity.Read<PrefabGUID>().LookupName().ToLower().Contains("ruins")) continue;
                                else if (CastleTerritoryCache.TryGetCastleTerritory(entity, out var _))
                                {
                                    
                                    //skip things in territories
                                    continue;
                                }
                                else
                                {
                                    Plugin.Log.LogInfo(entity.Read<PrefabGUID>().LookupName());
                                    //entity.LogComponentTypes();
                                    UserOwner userOwner = entity.Read<UserOwner>();
                                    if (userOwner.Owner._Entity.Has<User>())
                                    {
                                        User user = userOwner.Owner._Entity.Read<User>();
                                        UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(user.PlatformId);
                                        if (userModel.Internals.CastleHeartConnection.HasValue)
                                        {
                                            CastleHeartConnection castleHeartConnection = userModel.Internals.CastleHeartConnection.Value;
                                            Entity newHeart = castleHeartConnection.CastleHeartEntity._Entity;
                                            CastleHeartConnection castleHeartConnectionEntity = entity.Read<CastleHeartConnection>();
                                            castleHeartConnectionEntity.CastleHeartEntity._Entity = newHeart;
                                            entity.Write(castleHeartConnectionEntity);
                                            Plugin.Log.LogInfo("Replaced null heart. Maybe.");
                                        }
                                        else
                                        {
                                            Plugin.Log.LogInfo("No valid heart connection found.");
                                        }
                                        
                                    }
                                    
                                    // if this doesnt work replace castleheart entity with the valid one from whoever placed the tile
                                }
                            }
                        }
                        finally
                        {
                            wallEntities.Dispose();
                        }
                    }
                }
            }
            jobs.Dispose();

            jobs = __instance._AbilityCastFinishedQuery.ToEntityArray(Allocator.Temp);
            foreach (var job in jobs)
            {
                if (!Utilities.HasComponent<AbilityPreCastFinishedEvent>(job)) continue;

                AbilityPreCastFinishedEvent abilityPreCastFinishedEvent = Utilities.GetComponentData<AbilityPreCastFinishedEvent>(job);
                Entity abilityGroupData = abilityPreCastFinishedEvent.AbilityGroup;
                PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(abilityGroupData);
                Entity character = abilityPreCastFinishedEvent.Character;

                if (!Utilities.HasComponent<PlayerCharacter>(character)) continue;

                PlayerCharacter playerCharacter = Utilities.GetComponentData<PlayerCharacter>(character);
                Entity userEntity = playerCharacter.UserEntity;
                User user = Utilities.GetComponentData<User>(userEntity);
                if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out _) && prefabGUID.Equals(VCreate.Data.Prefabs.AB_Interact_Siege_Structure_T02_AbilityGroup))
                {
                    Plugin.Log.LogInfo("SiegeT02 ability cast detected...");
                    HandleAbilityCast(userEntity);
                }
            }
            jobs.Dispose();
        }

        public static void HandleAbilityCast(Entity userEntity)
        {
            var user = Utilities.GetComponentData<User>(userEntity);

            if (!DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool settings))
            {
                return; // or handle the case of missing settings
            }
            if (!settings.Emotes)
            {
                return;
            }
            // Assuming a method that decides the action based on the ability and settings
            var action = DecideAction(settings);

            // Execute the decided action
            action?.Invoke(userEntity, settings);
        }

        private static System.Action<Entity, Omnitool> DecideAction(Omnitool settings)
        {
            // Example: Checking for a specific prefabGUID and toggle
            Plugin.Log.LogInfo("Deciding action based on mode...");

            if (settings.GetMode("InspectToggle"))
            {
                return (userEntity, _) =>
                {
                    //Plugin.Logger.LogInfo("Inspect mode enabled, skipping tile spawn...");
                    OnHover.InspectHoveredEntity(userEntity);
                };
            }
            else if (settings.GetMode("DestroyToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.DestroyAtHover(userEntity);
                };
            }
            else if (settings.GetMode("CopyToggle"))
            {
                return (userEntity, _) =>
                {
                    // change this to add specified component to hovered entity
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Deprecated for now.");
                };
            }
            else if (settings.GetMode("DebuffToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.DebuffAtHover(userEntity);
                };
            }
            else if (settings.GetMode("ConvertToggle"))
            {
                return (userEntity, _) =>
                {
                    // change this to remove specified component from hovered entity
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Deprecated for now.");
                };
            }
            else if (settings.GetMode("BuffToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.BuffAtHover(userEntity);
                };
            }
            else if (settings.GetMode("TileToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.SpawnTileModel(userEntity);
                };
            }
            else
            {
                return null;
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
            //if (!__result) return;

            Plugin.Log.LogInfo("Verifying dismantle event...");

            bool canDismantle = TileOperationUtility.CanPerformOperation(entityManager, tileModelEntity);

            //__result = canDismantle;

            if (!canDismantle)
            {
                Plugin.Log.LogInfo("Disallowed based on permissions and ownership.");
                __result = false;
            }
            else
            {
                Plugin.Log.LogInfo("Allowing normal game handling for dismantle if owned or user has permissions.");
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCanMoveOrRotateAfterBuilt))]
    public static class VerifyCanMovePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            //if (!__result) return;

            Plugin.Log.LogInfo("Verifying move event...");

            bool canMove = TileOperationUtility.CanPerformOperation(entityManager, tileModelEntity);
            //__result = canMove;

            if (!canMove)
            {
                Plugin.Log.LogInfo("Disallowed based on permissions and ownership.");
                __result = false;
            }
            else
            {
                Plugin.Log.LogInfo("Allowed if owned or user has permission.");
                return;
            }
        }
    }

    /*
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifySharedCanStartEditOrDismantle))]
    public static class VerifySharedCanStartEditOrDismantlePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
        }
    }
    */

    public static class TileOperationUtility
    {
        public static bool CanPerformOperation(EntityManager entityManager, Entity tileModelEntity)
        {
            var userOwner = Utilities.GetComponentData<UserOwner>(tileModelEntity);
            var user = Utilities.GetComponentData<User>(userOwner.Owner._Entity);
            Plugin.Log.LogInfo($"User: {user.CharacterName}");
            return CanEditTiles(user) || HasValidCastleHeartConnection(user, tileModelEntity);
        }

        private static bool CanEditTiles(User user)
        {
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                return data.Permissions;
            }
            return false;
        }

        private static bool HasValidCastleHeartConnection(User user, Entity tileModelEntity)
        {
            if (!tileModelEntity.Has<CastleHeartConnection>()) return false;
            else
            {
                Plugin.Log.LogInfo("Checking for valid castle heart connection...");
                CastleHeartConnection castleHeartConnection = tileModelEntity.Read<CastleHeartConnection>();
                Entity castleHeart = castleHeartConnection.CastleHeartEntity._Entity;
                if (castleHeart == Entity.Null) return false;
                else
                {
                    Plugin.Log.LogInfo("Castle heart entity not null.");
                    return true;
                }
            }
        }
    }
}