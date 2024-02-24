using Bloodstone.API;
using FreeBuild.Data;
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

namespace FreeBuild.Core
{
    [CommandGroup(name: "freebuild", shortHand: "fb")]
    public class ChatCommands
    {
        public static bool tfbFlag;

        public static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)5,
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
                
                ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingPlacementRestrictionsDisabledSetting);

                
                string enabledColor = FreeBuild.Core.FontColors.Green("enabled");
                ctx.Reply($"freebuild: {enabledColor}");
            }
            else
            {
                ChatCommands.tfbFlag = false;
                ChatCommands.BuildingCostsDebugSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingCostsDebugSetting);
                
                ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingPlacementRestrictionsDisabledSetting);
                
                string disabledColor = FreeBuild.Core.FontColors.Red("disabled");
                ctx.Reply($"freebuild: {disabledColor}");
            }
        }

        [Command("spawnhorse", "sh", description: "Spawns a horse with specified stats.", adminOnly: true, usage:".sh <Speed> <Acceleration> <Rotation> <isSpectral> <#>")]
        public static void SpawnHorse(ChatCommandContext ctx, float speed, float acceleration, float rotation, bool spectral = false, int num = 1)
        {
            var pos = Utilities.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            var horsePrefab = spectral ? Prefabs.CHAR_Mount_Horse_Spectral : Prefabs.CHAR_Mount_Horse;

            for (int i = 0; i < num; i++)
            {
                UnitSpawnerService.UnitSpawner.SpawnWithCallback(ctx.Event.SenderUserEntity, horsePrefab, pos.xz, -1, (Entity horse) =>
                {
                    var mount = horse.Read<Mountable>();
                    mount.MaxSpeed = speed;
                    mount.Acceleration = acceleration;
                    mount.RotationSpeed = rotation * 10f;
                    horse.Write<Mountable>(mount);
                });
            }

            ctx.Reply($"Spawned {num}{(spectral == false ? "" : " spectral")} horse{(num > 1 ? "s" : "")} (with speed:{speed}, accel:{acceleration}, and rotate:{rotation}) near you.");
        }


        [Command("disablehorses", "dh", description: "Disables dead, dominated ghost horses on the server.", adminOnly: true)]
        public static void DisableGhosts(ChatCommandContext ctx)
        {
            var horses = Helper.GetEntitiesByComponentTypes<Immortal, Mountable>(true).ToArray()
                            .Where(x => x.Read<PrefabGUID>().GuidHash == Prefabs.CHAR_Mount_Horse_Vampire.GuidHash)
                            .Where(x => BuffUtility.HasBuff(VWorld.Server.EntityManager, x, Prefabs.Buff_General_VampireMount_Dead));

            EntityQuery horseQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Immortal>(),
                    ComponentType.ReadOnly<Mountable>(),
                    ComponentType.ReadOnly<PrefabGUID>(),
                    //ComponentType.ReadOnly<BuffComponent>()// Assuming this is how you identify the horse prefab.
                    // If there's a specific component for buffs, include it here. Otherwise, you'll need to filter buffs after querying.
                },
                // Include additional options as necessary, for example, to include disabled entities.
                Options = EntityQueryOptions.Default
            });
            
            NativeArray<Entity> horseEntities = horseQuery.ToEntityArray(Allocator.TempJob);
            foreach (var horse in horseEntities)
            {
                horse.LogComponentTypes();
                PrefabGUID prefabGUID = VWorld.Server.EntityManager.GetComponentData<PrefabGUID>(horse);
                if (prefabGUID.GuidHash == Prefabs.CHAR_Mount_Horse_Vampire.GuidHash)
                {
                    // Assume BuffUtility.HasBuff is a method you can use to check for the buff. You might need to adjust this based on how buffs are implemented.
                    //Plugin.Logger.LogInfo("test");
                }
            }
            horseEntities.Dispose();
            ctx.Reply($"Disabled player ghost horses. They can still be resummoned.");
            
           
        }
        /*
        [Command("spawnhorse", "sh", description: "Spawns a horse", adminOnly: true)]
        public static void SpawnHorse(ChatCommandContext ctx, float speed, float acceleration, float rotation, bool spectral = false, int num = 1)
        {
            var pos = Utilities.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
            var horsePrefab = spectral ? Prefabs.CHAR_Mount_Horse_Spectral : Prefabs.CHAR_Mount_Horse;

            for (int i = 0; i < num; i++)
            {
                UnitSpawner.SpawnWithCallback(ctx.Event.SenderUserEntity, horsePrefab, pos.xz, -1, (Entity horse) =>
                {
                    var mount = horse.Read<Mountable>();
                    mount.MaxSpeed = speed;
                    mount.Acceleration = acceleration;
                    mount.RotationSpeed = rotation * 10f;
                    horse.Write<Mountable>(mount);
                });
            }

            ctx.Reply($"Spawned {num}{(spectral == false ? "" : " spectral")} horse{(num > 1 ? "s" : "")} (with speed:{speed}, accel:{acceleration}, and rotate:{rotation}) near you.");
        }
        */
    }
}