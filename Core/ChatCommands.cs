using Bloodstone.API;
using WorldBuild.Data;
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
using Unity.Services.Core.Scheduler.Internal;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AssetBundlePatching;
using UnityEngine.SceneManagement;
using VampireCommandFramework;

namespace WorldBuild.Core
{
    [CommandGroup(name: "worldbuild", shortHand: "wb")]
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

        [Command(name: "togglefreebuild", shortHand: "tfb", adminOnly: true, usage: ".wb tfb", description: "Toggles freebuild debug settings.")]
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

                string enabledColor = WorldBuild.Core.FontColors.Green("enabled");
                ctx.Reply($"freebuild: {enabledColor}");
            }
            else
            {
                ChatCommands.tfbFlag = false;
                ChatCommands.BuildingCostsDebugSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingCostsDebugSetting);

                ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value = ChatCommands.tfbFlag;
                existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingPlacementRestrictionsDisabledSetting);

                string disabledColor = WorldBuild.Core.FontColors.Red("disabled");
                ctx.Reply($"freebuild: {disabledColor}");
            }
        }

        /*
        public class horseFunctions
        {
            internal static Dictionary<ulong, HorseStasisState> playerHorseStasisMap = new Dictionary<ulong, HorseStasisState>();
            internal struct HorseStasisState
            {
                public Entity HorseEntity;
                public bool IsInStasis;

                public HorseStasisState(Entity horseEntity, bool isInStasis)
                {
                    HorseEntity = horseEntity;
                    IsInStasis = isInStasis;
                }
            }
            [Command("spawnhorse", "sh", description: "Spawns a horse with specified stats.", adminOnly: true, usage: ".sh <Speed> <Acceleration> <Rotation> <isSpectral> <#>")]
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
                EntityQuery horseQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new[] {
                    ComponentType.ReadWrite<Immortal>(),
                    ComponentType.ReadWrite<Mountable>(),
                    ComponentType.ReadWrite<BuffBuffer>(),
                    ComponentType.ReadWrite<PrefabGUID>(),
                }
                });

                NativeArray<Entity> horseEntities = horseQuery.ToEntityArray(Allocator.TempJob);
                foreach (var horse in horseEntities)
                {
                    VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(horse, out DynamicBuffer<BuffBuffer> buffBuffer);
                    //horse.LogComponentTypes();
                    for (int i = 0; i < buffBuffer.Length; i++)
                    {
                        var buff = buffBuffer[i];
                        if (buff.PrefabGuid.GuidHash == Data.Prefabs.Buff_General_VampireMount_Dead.GuidHash)
                        {
                            // found dead horse
                            //ctx.Reply("Found dead horse...");
                            if (Utilities.HasComponent<EntityOwner>(horse))
                            {
                                EntityOwner entityOwner = Utilities.GetComponentData<EntityOwner>(horse);
                                // found owner
                                Entity player = entityOwner.Owner;
                                //player.LogComponentTypes();
                                if (Utilities.HasComponent<PlayerCharacter>(player))
                                {
                                    PlayerCharacter playerChar = Utilities.GetComponentData<PlayerCharacter>(player);
                                    Entity userEntity = playerChar.UserEntity;
                                    //found user
                                    User user = Utilities.GetComponentData<User>(userEntity);
                                    ctx.Reply("Found dead horse owner, disabling...");
                                    ulong playerId = user.PlatformId;
                                    playerHorseStasisMap[playerId] = new HorseStasisState(horse, true);
                                    SystemPatchUtil.Disable(horse);
                                }
                            }
                        }
                    }
                }
                horseEntities.Dispose();
                ctx.Reply($"Placed dead player ghost horses in stasis. They can still be resummoned.");
            }
            [Command("enablehorse", "eh", description: "Reactivates the player's horse.", adminOnly: false)]
            public static void ReactivateHorse(ChatCommandContext ctx)
            {
                ulong playerId = ctx.User.PlatformId;
                if (playerHorseStasisMap.TryGetValue(playerId, out var stasisState) && stasisState.IsInStasis)
                {
                    // Assuming SystemPatchUtil.Enable is a method to enable entities
                    SystemPatchUtil.Enable(stasisState.HorseEntity);
                    stasisState.IsInStasis = false; // Update the state
                    playerHorseStasisMap[playerId] = stasisState; // Save the updated state

                    ctx.Reply($"Your horse has been reactivated.");
                }
                else
                {
                    ctx.Reply($"No horse in stasis found to reactivate.");
                }
            }
        }

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

        //[Command(name: "disablenodes", shortHand: "dn", adminOnly: true, usage: ".v dn", description: "Finds and disables all resource nodes in player territories.")]
        public static void DestroyResourcesCommand(ChatCommandContext ctx)
        {
            // maybe if I set their health to 0 instead of destroying them? hmm
            User user = ctx.Event.User;
            Entity killer = ctx.Event.SenderUserEntity;
            EntityManager entityManager = VWorld.Server.EntityManager;
            //ResourceFunctions.SearchAndDestroy();

            ctx.Reply("All found resource nodes in player territories have been disabled.");
        }

        //[Command(name: "resetnodes", shortHand: "rn", adminOnly: true, usage: ".v rn", description: "Atempts to reset resource nodes if they've been disabled.")]
        public static void ResetResourcesCommand(ChatCommandContext ctx)
        {
            // maybe if I set their health to 0 instead of destroying them? hmm
            User user = ctx.Event.User;
            Entity killer = ctx.Event.SenderUserEntity;
            EntityManager entityManager = VWorld.Server.EntityManager;
            //ResourceFunctions.FindAndEnable();

            //ctx.Reply("Resource nodes reset in player territories.");
        }

        public class ResourceFunctions
        {
            // this actually disables but destroy is much catchier
            public static unsafe int SearchAndDestroy()
            {
                EntityManager entityManager = VWorld.Server.EntityManager;
                int counter = 0;
                bool includeDisabled = true;
                var nodeQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                    ComponentType.ReadOnly<YieldResourcesOnDamageTaken>(),
                    ComponentType.ReadOnly<TilePosition>(),
                },
                    Options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
                });

                var resourceNodeEntities = nodeQuery.ToEntityArray(Allocator.Temp);
                foreach (var node in resourceNodeEntities)
                {
                    if (ShouldRemoveNodeBasedOnTerritory(node))
                    {
                        // if node is in a player territory, which is updated for castle heart placement/destruction already, disable it
                        // might need to filter for trees and rocks here
                        counter += 1;
                        SystemPatchUtil.Disable(node);
                        //node.LogComponentTypes();
                    }
                }
                resourceNodeEntities.Dispose();
                return counter;
            }

            private static bool ShouldRemoveNodeBasedOnTerritory(Entity node)
            {
                Entity territoryEntity;
                if (CastleTerritoryCache.TryGetCastleTerritory(node, out territoryEntity))
                {
                    return true;
                }
                return false;
            }

            public static unsafe void FindAndEnable()
            {
                EntityManager entityManager = VWorld.Server.EntityManager;
                int counter = 0;
                bool includeDisabled = true;
                var nodeQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                    ComponentType.ReadOnly<YieldResourcesOnDamageTaken>(),
                    ComponentType.ReadOnly<TilePosition>(),
                },
                    Options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
                });

                var resourceNodeEntities = nodeQuery.ToEntityArray(Allocator.Temp);
                foreach (var node in resourceNodeEntities)
                {
                    if (Utilities.HasComponent<Disabled>(node))
                    {
                        Entity territoryEntity;
                        if (CastleTerritoryCache.TryGetCastleTerritory(node, out territoryEntity))
                        {
                            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
                            EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
                            entityCommandBuffer.RemoveComponent<Disabled>(node);
                            counter += 1;
                        }
                    }
                }
                resourceNodeEntities.Dispose();
                Plugin.Logger.LogInfo($"{counter} resource nodes restored.");
            }
        }
    }
}