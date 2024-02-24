using Bloodstone.API;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Terrain;
using ProjectM.Tiles;
using ProjectM.UI;
using Stunlock.Core;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AssetBundlePatching;
using UnityEngine.SceneManagement;
using VampireCommandFramework;

namespace DismantleDenied.Core
{
    [CommandGroup(name: "ddcommands", shortHand: "dd")]
    public class ChatCommands
    {
        public static bool tfbFlag;

        public static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)5,
            Value = false
        };

        public static SetDebugSettingEvent GlobalCastleTerritoryEnabled = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)10,
            Value = false
        };

        public static SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)16,
            Value = false
        };

        public static SetDebugSettingEvent CastleHeartConnectionRequirementDisabled = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)27,
            Value = false
        };

        public static SetDebugSettingEvent CastleLimitsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)31,
            Value = false
        };

        [Command(name: "togglefreebuild", shortHand: "tfb", adminOnly: true, usage: ".dd tfb", description: "Toggles freebuild debug settings.")]
        public static void ToggleBuildDebugCommand(ChatCommandContext ctx)
        {
            User user = ctx.Event.User;
            DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            if (!ChatCommands.tfbFlag)
            {
                ChatCommands.tfbFlag = true;
                ChatCommands.BuildingCostsDebugSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingCostsDebugSetting);
                ChatCommands.CastleLimitsDisabledSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.CastleLimitsDisabledSetting);

                if (Plugin.castleHeartConnectionRequirement)
                {
                    ChatCommands.CastleHeartConnectionRequirementDisabled.Value = ChatCommands.tfbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref ChatCommands.CastleHeartConnectionRequirementDisabled);
                }
                if (Plugin.buildingPlacementRestrictions)
                {
                    ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value = ChatCommands.tfbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingPlacementRestrictionsDisabledSetting);
                }
                if (Plugin.globalCastleTerritory)
                {
                    ChatCommands.GlobalCastleTerritoryEnabled.Value = ChatCommands.tfbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref ChatCommands.GlobalCastleTerritoryEnabled);
                }
                string enabledColor = DismantleDenied.Core.FontColors.Green("enabled");
                ctx.Reply($"freebuild: {enabledColor}");
            }
            else
            {
                ChatCommands.tfbFlag = false;
                ChatCommands.BuildingCostsDebugSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingCostsDebugSetting);
                ChatCommands.CastleLimitsDisabledSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.CastleLimitsDisabledSetting);

                if (Plugin.castleHeartConnectionRequirement)
                {
                    ChatCommands.CastleHeartConnectionRequirementDisabled.Value = ChatCommands.tfbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref ChatCommands.CastleHeartConnectionRequirementDisabled);
                }

                if (Plugin.buildingPlacementRestrictions)
                {
                    ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value = ChatCommands.tfbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingPlacementRestrictionsDisabledSetting);
                }
                if (Plugin.globalCastleTerritory)
                {
                    ChatCommands.GlobalCastleTerritoryEnabled.Value = ChatCommands.tfbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref ChatCommands.GlobalCastleTerritoryEnabled);
                }
                string disabledColor = DismantleDenied.Core.FontColors.Red("disabled");
                ctx.Reply($"freebuild: {disabledColor}");
            }
        }

        //[Command(name: "destroynodes", shortHand: "dn", adminOnly: true, usage: ".dd dn", description: "Finds and destroys all resource nodes.")]
        public static void DestroyResourcesCommand(ChatCommandContext ctx)
        {
            // maybe if I set their health to 0 instead of destroying them? hmm
            User user = ctx.Event.User;
            Entity killer = ctx.Event.SenderUserEntity;
            EntityManager entityManager = VWorld.Server.EntityManager;
            //ResourceFunctions.SearchAndDestroy(killer, ctx);

            ctx.Reply("All found resource nodes have been destroyed.");
        }

        public class ResourceFunctions
        {
            /*
            public static unsafe void SearchAndDestroy(Entity killer, ChatCommandContext ctx)
            {
                EntityManager entityManager = VWorld.Server.EntityManager;

                MapZoneCollectionSystem mapZoneCollectionSystem = VWorld.Server.GetExistingSystem<MapZoneCollectionSystem>();
                MapZoneCollection mapZoneCollection = mapZoneCollectionSystem.GetMapZoneCollection();

                PlayCommandsSystem_Server playCommandsSystem = VWorld.Server.GetExistingSystem<PlayCommandsSystem_Server>();
                EntityQuery serverTimeQuery = playCommandsSystem._ServerTime._SingletonQuery;
                int counter = 0;

                if (serverTimeQuery.CalculateEntityCount() == 1)
                {
                    var serverTimeEntity = serverTimeQuery.GetSingletonEntity();
                    var serverTime = Utilities.GetComponentData<ServerTime>(serverTimeEntity);
                     double currentTime = serverTime.TimeOnServer; // Or whichever field is appropriate
                }

                bool includeDisabled = true;
                var nodeQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                        ComponentType.ReadOnly<YieldResourcesOnDamageTaken>(),
                        ComponentType.ReadOnly<Health>(),
                        ComponentType.ReadOnly<TilePosition>(),
                    },
                    Options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
                });

                var resourceNodeEntities = nodeQuery.ToEntityArray(Allocator.Temp);

                foreach (var entity in resourceNodeEntities)
                {
                    TilePosition tilePosition = entityManager.GetComponentData<TilePosition>(entity);
                    float2 worldFloat = SpaceConversion.TileToWorld(tilePosition.Tile.x, tilePosition.Tile.y);
                    int2 worldTile = SpaceConversion.WorldToTile(worldFloat.x, worldFloat.y);

                    if (CastleTerritoryExtensions.TryGetCastleTerritory(mapZoneCollection, entityManager, worldTile, out CastleTerritory castleTerritory))
                    {
                        // This resource is within a castle territory, proceed with destruction
                        StatChangeUtility.KillEntity(entityManager, entity, killer, currentTime, false);
                        counter++;
                    }
                }
                Plugin.Logger.LogInfo($"Resource nodes destroyed: {counter}");
                resourceNodeEntities.Dispose();
            }
            */
        }
    }
}