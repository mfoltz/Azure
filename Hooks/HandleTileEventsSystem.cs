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

//WIP

namespace WorldBuild.Hooks
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public static class CastleHeartPlacementPatch
    {
        private static readonly PrefabGUID CastleHeartPrefabGUID = new PrefabGUID(-485210554); // castle heart prefab

        public static void Prefix(PlaceTileModelSystem __instance)
        {
            //Plugin.Logger.LogInfo("PlaceTileModelSystem Prefix called...");
            EntityManager entityManager = VWorld.Server.EntityManager;

            var jobs = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
            foreach (var job in jobs)
            {
                if (IsCastleHeart(job))
                {
                    if (!CoreCommands.WorldBuildToggle.WbFlag) return;
                    CancelCastleHeartPlacement(entityManager, job);
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
            else if (settings.GetMode("KillToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.DestroyAtHover(userEntity);
                };
            }
            else if (settings.GetMode("ControlToggle"))
            {
                return (userEntity, _) =>
                {
                    VCreate.Hooks.EmoteSystemPatch.ControlCommand(userEntity);
                };
            }
            else if (settings.GetMode("CopyToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.SpawnCopy(userEntity);
                };
            }
            else if (settings.GetMode("DebuffToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.DebuffTileModel(userEntity);
                };
            }
            else if (settings.GetMode("ConvertToggle"))
            {
                return (userEntity, _) =>
                {
                    OnHover.ConvertCharacter(userEntity);
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
            else if (settings.GetMode("LinkToggle"))
            {
                return (userEntity, _) =>
                {
                    //OnHover.LinkHelper(userEntity);
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "WIP, currently not implemented.");
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
            if (!__result) return;

            Plugin.Log.LogInfo("Verifying dismantle event...");

            bool canDismantle = TileOperationUtility.CanPerformOperation(entityManager, tileModelEntity);
            __result = canDismantle;

            if (!canDismantle)
            {
                Plugin.Log.LogInfo("Disallowed based on permissions and ownership.");
            }
            else
            {
                Plugin.Log.LogInfo("Allowed if owned or user has permission.");
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCanMoveOrRotateAfterBuilt))]
    public static class VerifyCanMovePatch
    {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity)
        {
            if (!__result) return;

            Plugin.Log.LogInfo("Verifying move event...");

            bool canMove = TileOperationUtility.CanPerformOperation(entityManager, tileModelEntity);
            __result = canMove;

            if (!canMove)
            {
                Plugin.Log.LogInfo("Disallowed based on permissions and ownership.");
            }
            else
            {
                Plugin.Log.LogInfo("Allowed if owned or user has permission.");
            }
        }
    }


    public static class TileOperationUtility
    {
        public static bool CanPerformOperation(EntityManager entityManager, Entity tileModelEntity)
        {
            if (!Utilities.HasComponent<UserOwner>(tileModelEntity))
            {
                Plugin.Log.LogInfo("Unable to verify user entity for tile operation, disallowing.");
                return false;
            }

            var userOwner = Utilities.GetComponentData<UserOwner>(tileModelEntity);
            var user = Utilities.GetComponentData<User>(userOwner.Owner._Entity);

            return CanEditTiles(user) || IsTileOwnedByUser(user, tileModelEntity);
        }

        private static bool CanEditTiles(User user)
        {
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                return data.Permissions;
            }
            return false;
        }

        private static bool IsTileOwnedByUser(User user, Entity tileModelEntity)
        {
            
            if (tileModelEntity.Read<UserOwner>().Owner._Entity.Read<User>().PlatformId.Equals(user.PlatformId))
            {
                Plugin.Log.LogInfo("Object owned by user, allowing.");
                return true;
            }
            else
            {
                Plugin.Log.LogInfo("Object not owned by user, disallowing.");
                return false;
            }
            
        }
    }
}