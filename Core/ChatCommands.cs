using Bloodstone.API;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
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

namespace DismantleDenier.Core
{
    [CommandGroup(name: "ddcommands", shortHand: "dd")]
    internal class ChatCommands
    {
        public static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)5,
            Value = false
        };

        public static SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)16,
            Value = Plugin.buildingPlacementRestrictions
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
            ChatCommands.BuildingCostsDebugSetting.Value = !ChatCommands.BuildingCostsDebugSetting.Value;
            existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingCostsDebugSetting);
            ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value = !ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value;
            existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingPlacementRestrictionsDisabledSetting);
            ChatCommands.CastleLimitsDisabledSetting.Value = !ChatCommands.CastleLimitsDisabledSetting.Value;
            existingSystem.SetDebugSetting(user.Index, ref ChatCommands.CastleLimitsDisabledSetting);
            if (ChatCommands.BuildingCostsDebugSetting.Value)
            {
                string enabledColor = DismantleDenier.Core.FontColors.Green("enabled");
                ctx.Reply($"freebuild: {enabledColor}");
            }
            else
            {
                string disabledColor = DismantleDenier.Core.FontColors.Red("enabled");
                ctx.Reply($"freebuild: {disabledColor}");
            }
        }

        [Command(name: "destroynodes", shortHand: "dn", adminOnly: true, usage: ".dd dn", description: "Finds and destroys all resource nodes in castle territories.")]
        public static void DestroyResourcesCommand(ChatCommandContext ctx)
        {
            User user = ctx.Event.User;
            EntityManager entityManager = VWorld.Server.EntityManager;
            DismantleDenier.Core.ResourceFunctions.SearchAndDestroyCastleResourceNodes();

            ctx.Reply("All resource nodes in buildable castle territories have been destroyed.");
        }
    }

    public class ResourceFunctions
    {
        public static void SearchAndDestroyCastleResourceNodes()
        {
            bool includeDisabled = true;
            var nodeQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {
                        ComponentType.ReadOnly<EntityCategory>(),
                        ComponentType.ReadOnly<LocalToWorld>(),
                        ComponentType.ReadOnly<CastleTerritoryTiles>(),
                    },
                Options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
            });

            var resourceNodeEntities = nodeQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in resourceNodeEntities)
            {
                var entityCategory = VWorld.Server.EntityManager.GetComponentData<EntityCategory>(entity);
                var castleTerritoryTiles = VWorld.Server.EntityManager.GetComponentData<CastleTerritoryTiles>(entity);
                Plugin.Logger.LogInfo($"InsideBuildableTerritory: {castleTerritoryTiles.InsideBuildableTerritory} | ResourceLevel: {entityCategory.MainCategory}");
                if (entityCategory.MainCategory == MainEntityCategory.Resource && castleTerritoryTiles.InsideBuildableTerritory)
                {
                    Plugin.Logger.LogInfo($"Resource node found: {entity}");
                    VWorld.Server.EntityManager.DestroyEntity(entity);
                }
            }
            resourceNodeEntities.Dispose();
        }
    }
}