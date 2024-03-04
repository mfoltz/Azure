using Bloodstone.API;
using Gee.External.Capstone.X86;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using LibCpp2IL.BinaryStructures;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Hybrid;
using ProjectM.Network;
using ProjectM.Shared.Mathematics;
using ProjectM.Shared.Systems;
using ProjectM.Terrain;
using ProjectM.Tiles;
using ProjectM.UI;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;
using VBuild.Core;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using VBuild.Data;
using VRising.GameData.Utils;
using static ProjectM.SLSEntityRemapping;
using static VBuild.BuildingSystem.TileSets.HorseFunctions;
using static VCF.Core.Basics.RoleCommands;
using StringComparer = System.StringComparer;
using User = ProjectM.Network.User;

namespace VBuild.BuildingSystem
{
    internal class TileSets
    {
        // can activate this by monitoring for ability player gets to use with shift key to place a tile at mouse location
        // use charm/siege interact T02 or something, monitor for abilitycast finishes that match the prefab and run this method


        public unsafe static void InspectHoveredEntity(Entity character)
        {
            Plugin.Logger.LogInfo("InspectHoveredEntity Triggered");
            if (Utilities.HasComponent<PlayerCharacter>(character))
            {
                PlayerCharacter player = Utilities.GetComponentData<PlayerCharacter>(character);
                string playerName = player.Name.ToString();
                if (PlayerService.TryGetUserFromName(playerName, out Entity userEntity))
                {
                    User user = Utilities.GetComponentData<User>(userEntity);

                    // Obtain the hovered entity from the player's input
                    Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;

                    // Check if the hovered entity is valid
                    if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
                    {
                        // Log the component types of the hovered entity
                        hoveredEntity.LogComponentTypes();
                        string entityString = hoveredEntity.Index.ToString() + ", " + hoveredEntity.Version.ToString();
                        ulong steamId = user.PlatformId;
                        if (Databases.playerBuildSettings.TryGetValue(steamId, out BuildSettings settings))
                        {
                            // Create a unique string reference for the entity or prefab or whatever
                            PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
                            settings.TileModel = prefabGUID.GuidHash;
                            // Add this reference to the LastTilesPlaced stack
                            Databases.SaveBuildSettings();
                            string copySuccess = $"Copied hovered entity for pasting: {entityString}, {prefabGUID.LookupName()}";
                            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, copySuccess);
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Couldn't find player build settings for eye-dropper.");
                        }
                        // Send a confirmation message to the player
                        string message = "Inspected hovered entity. Check the log for details.";
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                    }
                    else
                    {
                        // Send an error message if no valid entity is hovered
                        string message = "No valid entity is being hovered. Please hover over an entity to inspect.";
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                    }
                }
                else
                {
                    Plugin.Logger.LogInfo("User entity not found.");
                }
            }
        }

        public unsafe static void SpawnTileModel(Entity character)
        {
            Plugin.Logger.LogInfo("SpawnTileModel Triggered");
            if (Utilities.HasComponent<PlayerCharacter>(character))
            {
                PlayerCharacter player = Utilities.GetComponentData<PlayerCharacter>(character);
                string playerName = player.Name.ToString();
                PlayerService.TryGetUserFromName(playerName, out Entity userEntity);
                User user = Utilities.GetComponentData<User>(userEntity);
                ulong SteamId = user.PlatformId;
                Nullable_Unboxed<float3> aimPosition = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().AimPosition);
                if (Databases.playerBuildSettings.TryGetValue(SteamId, out BuildSettings data) && !data.KillToggle)
                {
                    PrefabGUID prefabGUID = new(data.TileModel);
                    Entity prefabEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[prefabGUID];

                    string prefabName = prefabGUID.LookupName();

                    // Check if 'char' is present in the prefab name
                    if (prefabName.ToLower().Contains("char"))
                    {
                        // Use SpawnWithCallback when 'char' is detected in the prefab name
                        // give lifetime component
                        if (!Utilities.HasComponent<LifeTime>(prefabEntity))
                        {
                            Utilities.AddComponentData(prefabEntity, new LifeTime { Duration = 300f });
                        }
                        else
                        {
                            Utilities.SetComponentData(prefabEntity, new LifeTime { Duration = 300f });
                        }
                        if(!Utilities.HasComponent<Immortal>(prefabEntity))
                        {
                            Utilities.AddComponentData(prefabEntity, new Immortal { IsImmortal = true });
                        }
                        else
                        {
                            Utilities.SetComponentData(prefabEntity, new Immortal { IsImmortal = true });
                        }
                        Utilities.SetComponentData(prefabEntity, new Translation { Value = aimPosition.Value });

                        int rotation = data.TileRotation;
                        float radians = math.radians(rotation);
                        quaternion rotationQuaternion = quaternion.EulerXYZ(new float3(0, radians, 0));
                        Utilities.SetComponentData(prefabEntity, new Rotation { Value = rotationQuaternion });
                        if (!Utilities.HasComponent<Health>(prefabEntity))
                        {
                            Utilities.SetComponentData(prefabEntity, new Health { Value = 1000 });
                        }
                        else
                        {
                            Utilities.AddComponentData(prefabEntity, new Health { Value = 1000 });
                        }
                    }
                    else
                    {
                        // Fallback to using EntityManager.Instantiate if 'char' is not in the name
                        

                        Entity tileEntity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
                        Utilities.SetComponentData(tileEntity, new Translation { Value = aimPosition.Value });

                        int rotation = data.TileRotation;
                        float radians = math.radians(rotation);
                        quaternion rotationQuaternion = quaternion.EulerXYZ(new float3(0, radians, 0));
                        Utilities.SetComponentData(tileEntity, new Rotation { Value = rotationQuaternion });

                        //instantiate entity to steal its collider component, sure why not
                        //prefabEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[WorldBuild.Data.Prefabs.TM_Fortressoflight_Brazier01];
                        //Entity colliderEntity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
                        //PhysicsCollider physicsCollider = Utilities.GetComponentData<PhysicsCollider>(colliderEntity);
                        //Health health = Utilities.GetComponentData<Health>(colliderEntity);
                        //Utilities.SetComponentData(tileEntity, physicsCollider);
                        //Utilities.SetComponentData(tileEntity, health);
                        //SystemPatchUtil.Destroy(colliderEntity);
                        //that mostly worked but the chest became unlootable unless destroyed with nukeall, moving on for now since that sounds like quite the rabbit hole
                        if (data.ImmortalTiles)
                        {
                            Utilities.AddComponentData(tileEntity, new Immortal { IsImmortal = true });
                            // this doesnt work for the altars and a few other things, not sure hwy yet
                        }
                        if (data.MapIconToggle)
                        {
                            PrefabGUID prefab = new(data.MapIcon);
                            if (data.MapIcon == 0)
                            {
                                string noIcon = "No map icon selected.";
                                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, noIcon);
                                return;
                            }
                            if (!Utilities.HasComponent<AttachMapIconsToEntity>(tileEntity))
                            {
                                VWorld.Server.EntityManager.AddBuffer<AttachMapIconsToEntity>(tileEntity);

                                VWorld.Server.EntityManager.GetBuffer<AttachMapIconsToEntity>(tileEntity).Add(new AttachMapIconsToEntity { Prefab = prefab });

                                // if you really need to just borrow one from the weird waygate

                            }
                            else
                            {
                                VWorld.Server.EntityManager.GetBuffer<AttachMapIconsToEntity>(tileEntity).Add(new AttachMapIconsToEntity { Prefab = prefab });

                            }
                        }

                        if (data.SnappingToggle)
                        {

                            float3 position = tileEntity.Read<Translation>().Value;
                            float gridSize = 5f; // Define your desired grid size
                            position = new float3(
                                math.round(position.x / gridSize) * gridSize,
                                position.y,
                                math.round(position.z / gridSize) * gridSize);
                            Utilities.SetComponentData(tileEntity, new Translation { Value = position });



                        }


                        string message = $"Tile spawned at {aimPosition.value.xy} with rotation {data.TileRotation} degrees clockwise.";
                        string entityString = tileEntity.Index.ToString() + ", " + tileEntity.Version.ToString();
                        //data.LastTilesPlaced = entityString;
                        data.AddTilePlaced(entityString);
                        Plugin.Logger.LogInfo($"Tile placed: {entityString}");
                        //tileEntity.LogComponentTypes();

                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                    }



                    
                }
                else if (data.KillToggle)
                {
                    User killer = Utilities.GetComponentData<User>(userEntity);

                    // Obtain the hovered entity from the player's input
                    Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
                    
                    if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
                    {
                        if (!Utilities.HasComponent<Dead>(hoveredEntity))
                        {
                            Utilities.AddComponentData(hoveredEntity, new Dead { DoNotDestroy = false });
                            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, killer, "Target destroyed");
                        }
                        else
                        {
                            Utilities.SetComponentData(hoveredEntity, new Dead { DoNotDestroy = false });
                            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, killer, "Target destroyed");
                        }
                        
                    }
                    else
                    {
                        // Send an error message if no valid entity is hovered
                        string message = "No valid entity is being hovered. Please hover over an entity to kill.";
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                    }

                }
                else
                {
                    string message = "Couldn't find your build preferences, try again after setting them.";
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                }
              
              
            }
        }
        public static void MoveClosestToMouseToggle(Entity character)
        {
            DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            
        }

        /*
        public static void DestroyTileModel(Entity character)
        {
            //Plugin.Logger.LogInfo("DestroyTileNearHover Triggered");
            if (Utilities.HasComponent<PlayerCharacter>(character))
            {
                PlayerCharacter player = Utilities.GetComponentData<PlayerCharacter>(character);
                if (PlayerService.TryGetUserFromName(player.Name.ToString(), out Entity userEntity))
                {
                    User user = Utilities.GetComponentData<User>(userEntity);
                    EntityInput entityInput = Utilities.GetComponentData<EntityInput>(character);
                    if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings data))
                    {

                        //Entity hoveredEntity = entityInput.HoveredEntity;
                        // apparently hovered entity is only for hovered non-tiles or something
                        
                        Nullable_Unboxed<float3> aimPosition = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().ProjectileAimPosition);
                        Nullable_Unboxed<float3> aimDirection = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().AimDirection);
                        Ray aimRay = new Ray(aimPosition.Value, aimDirection.Value);
                        //Plugin.Logger.LogInfo("Ray casted for tile entity...");
                        if (Physics.Raycast(aimRay, out RaycastHit hitInfo))
                        {
                            Plugin.Logger.LogInfo("Hit detected...");
                            // Retrieve the tile entity associated with the hit collider
                            // This depends on how you've associated your game objects with entities
                            Entity tileEntity = hitInfo.collider.GetComponent<EntityReference>().EntityTarget;

                            // Now that you have the tile entity, you can proceed with your logic
                            string entityString = tileEntity.Index.ToString() + ", " + tileEntity.Version.ToString();
                            Plugin.Logger.LogInfo($"Hovered tile entity: {entityString}");
                            if (data.TilesPlaced.Contains(entityString))
                            {
                                SystemPatchUtil.Destroy(tileEntity);
                                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, $"Tile destroyed at {entityInput.AimPosition.xy}.");
                            }
                            else
                            {
                                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "The hovered entity was not placed via VBuild and cannot be destroyed this way. For now...");
                            }
                            
                        }
                        else
                        {
                            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "No tile found at the aim position.");
                        }

                        



                    }
                    else
                    {
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Dismantle mode is not enabled.");
                    }
                }
            }
        }
        */
        public class TileConstructor(string name, int tilePrefabHash)
        {
            public string Name { get; set; } = name;
            public int TileGUID { get; set; } = tilePrefabHash;
        }

        public class Building
        {
            public static Dictionary<int, TileConstructor> StaticTiles { get; private set; }

            static Building()
            {
                StaticTiles = new Dictionary<int, TileConstructor>
                {
                    { 10, new TileConstructor("TM_Fortressoflight_Brazier01", VBuild.Data.Prefabs.TM_Fortressoflight_Brazier01.GuidHash) },
                    { 9, new TileConstructor("TM_Brazier_Elris01", VBuild.Data.Prefabs.TM_Brazier_Elris01.GuidHash) },
                    { 8, new TileConstructor("TM_CraftingStation_MetalworkStation", VBuild.Data.Prefabs.TM_CraftingStation_MetalworkStation.GuidHash) },
                    { 7, new TileConstructor("TM_CastleRuins_Pillar_Broken01", VBuild.Data.Prefabs.TM_CastleRuins_Pillar_Broken01.GuidHash) },
                    { 6, new TileConstructor("TM_CraftingStation_ArtisansCorner", VBuild.Data.Prefabs.TM_CraftingStation_ArtisansCorner.GuidHash) },
                    { 5, new TileConstructor("TM_SpecialStation_StablePen", VBuild.Data.Prefabs.TM_SpecialStation_StablePen.GuidHash) },
                    { 4, new TileConstructor("TM_CraftingStation_Altar_Frost", VBuild.Data.Prefabs.TM_CraftingStation_Altar_Frost.GuidHash) },
                    { 3, new TileConstructor("TM_CraftingStation_Altar_Unholy", VBuild.Data.Prefabs.TM_CraftingStation_Altar_Unholy.GuidHash) },
                    { 2, new TileConstructor("TM_CraftingStation_Altar_Spectral", VBuild.Data.Prefabs.TM_CraftingStation_Altar_Spectral.GuidHash) },
                    { 1, new TileConstructor("TM_Workstation_Waypoint_World_UnlockedFromStart", VBuild.Data.Prefabs.TM_Workstation_Waypoint_World_UnlockedFromStart.GuidHash) }
                };
            }

            public Dictionary<int, TileConstructor> Tiles { get; private set; }

            public Building()
            {
                Tiles = StaticTiles;
            }
        }

        public static Dictionary<int, TileConstructor> GetTilesBySet(string setName)
        {
            if (ModelRegistry.tilesBySet.TryGetValue(setName, out var setTiles))
            {
                return setTiles.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            return null;
        }

        public static readonly HashSet<string> adminSets = new(StringComparer.OrdinalIgnoreCase)
        {
            "building",
        };

        public static class ModelRegistry
        {
            public static readonly Dictionary<string, Dictionary<int, TileConstructor>> tilesBySet = new(StringComparer.OrdinalIgnoreCase);

            static ModelRegistry()
            {
                RegisterTiles("Building", new Dictionary<int, TileConstructor>
                {
                    
                    { 10, new TileConstructor("TM_Fortressoflight_Brazier01", VBuild.Data.Prefabs.TM_Fortressoflight_Brazier01.GuidHash) },
                    { 9, new TileConstructor("TM_Brazier_Elris01", VBuild.Data.Prefabs.TM_Brazier_Elris01.GuidHash) },
                    { 8, new TileConstructor("TM_CraftingStation_MetalworkStation", VBuild.Data.Prefabs.TM_CraftingStation_MetalworkStation.GuidHash) },
                    { 7, new TileConstructor("TM_CastleRuins_Pillar_Broken01", VBuild.Data.Prefabs.TM_CastleRuins_Pillar_Broken01.GuidHash) },
                    { 6, new TileConstructor("TM_CraftingStation_ArtisansCorner", VBuild.Data.Prefabs.TM_CraftingStation_ArtisansCorner.GuidHash) },
                    { 5, new TileConstructor("TM_SpecialStation_StablePen", VBuild.Data.Prefabs.TM_SpecialStation_StablePen.GuidHash) },
                    { 4, new TileConstructor("TM_CraftingStation_Altar_Frost", VBuild.Data.Prefabs.TM_CraftingStation_Altar_Frost.GuidHash) },
                    { 3, new TileConstructor("TM_CraftingStation_Altar_Unholy", VBuild.Data.Prefabs.TM_CraftingStation_Altar_Unholy.GuidHash) },
                    { 2, new TileConstructor("TM_CraftingStation_Altar_Spectral", VBuild.Data.Prefabs.TM_CraftingStation_Altar_Spectral.GuidHash) },
                    { 1, new TileConstructor("TM_Workstation_Waypoint_World_UnlockedFromStart", VBuild.Data.Prefabs.TM_Workstation_Waypoint_World_UnlockedFromStart.GuidHash) }
                });
            }

            public static void RegisterTiles(string setName, Dictionary<int, TileConstructor> tiles)
            {
                tilesBySet[setName] = tiles;
            }

            private static Dictionary<int, TileConstructor> GetTilesFromSetChoice(string setChoice)
            {
                // This method should return the appropriate tiles dictionary based on the set choice
                // Example:
                return setChoice switch
                {
                    "building" => Building.StaticTiles,
                    _ => null,
                };
            }
        }

        public class ResourceFunctions
        {
            
            public static unsafe void SearchAndDestroy()
            {
                Plugin.Logger.LogInfo("Entering SearchAndDestroy...");
                EntityManager entityManager = VWorld.Server.EntityManager;
                int counter = 0;
                bool includeDisabled = true;
                var nodeQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                    ComponentType.ReadOnly<YieldResourcesOnDamageTaken>(),
                },
                    Options = includeDisabled ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
                });

                var resourceNodeEntities = nodeQuery.ToEntityArray(Allocator.Temp);
                foreach (var node in resourceNodeEntities)
                {
                    if (ShouldRemoveNodeBasedOnTerritory(node))
                    {

                        counter += 1;
                        SystemPatchUtil.Destroy(node);
                        
                        //node.LogComponentTypes();
                    }
                }
                resourceNodeEntities.Dispose();

                var cleanUp = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[]
                    {
                    ComponentType.ReadOnly<PrefabGUID>(),
                },
                    Options = EntityQueryOptions.IncludeDisabled
                });
                var cleanUpEntities = cleanUp.ToEntityArray(Allocator.Temp);
                foreach (var node in cleanUpEntities)
                {
                    
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(node);
                    string name = prefabGUID.LookupName();
                    if (name.Contains("plant") || name.Contains("fibre") || name.Contains("shrub") || name.Contains("tree") || name.Contains("fiber"))
                    {
                        if (ShouldRemoveNodeBasedOnTerritory(node))
                        {
                            counter += 1;
                            SystemPatchUtil.Destroy(node);
                        }
                    }
                }
                cleanUpEntities.Dispose();


                Plugin.Logger.LogInfo($"{counter} resource nodes destroyed.");
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

            
        }

        public class HorseFunctions
        {
            
            internal static Dictionary<ulong, HorseStasisState> PlayerHorseStasisMap = new();

            [Command("spawnhorse", shortHand: "sh", description: "Spawns a horse with specified stats.", usage: ".sh <Speed> <Acceleration> <Rotation> <isSpectral> <#>", adminOnly: true)]
            public static void SpawnHorse(ChatCommandContext ctx, float speed, float acceleration, float rotation, bool spectral = false, int num = 1)
            {
                var position = Utilities.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
                var prefabGuid = spectral ? Prefabs.CHAR_Mount_Horse_Spectral : Prefabs.CHAR_Mount_Horse;

                for (int i = 0; i < num; i++)
                {
                    UnitSpawnerService.UnitSpawner.SpawnWithCallback(ctx.Event.SenderUserEntity, prefabGuid, position.xz, -1f, horse =>
                    {
                        var mountable = horse.Read<Mountable>() with
                        {
                            MaxSpeed = speed,
                            Acceleration = acceleration,
                            RotationSpeed = rotation * 10f
                        };
                        horse.Write(mountable);
                    });
                }

                var horseType = spectral ? "spectral" : "";
                ctx.Reply($"Spawned {num} {horseType} horse{(num > 1 ? "s" : "")} (with speed: {speed}, accel: {acceleration}, and rotate: {rotation}) near you.");
            }

            [Command("disablehorses", "dh", description: "Disables dead, dominated ghost horses on the server.", adminOnly: true)]
            public static void DisableGhosts(ChatCommandContext ctx)
            {
                var entityManager = VWorld.Server.EntityManager;
                NativeArray<Entity> entityArray = (entityManager).CreateEntityQuery(new EntityQueryDesc()
                {
                    All = (Il2CppStructArray<ComponentType>)new ComponentType[4]
                    {
            ComponentType.ReadWrite<Immortal>(),
            ComponentType.ReadWrite<Mountable>(),
            ComponentType.ReadWrite<BuffBuffer>(),
            ComponentType.ReadWrite<PrefabGUID>()
                    }
                }).ToEntityArray(Allocator.TempJob);

                foreach (var entity in entityArray)
                {
                    DynamicBuffer<BuffBuffer> buffer;
                    VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(entity, out buffer);
                    for (int index = 0; index < buffer.Length; ++index)
                    {
                        if (buffer[index].PrefabGuid.GuidHash == Prefabs.Buff_General_VampireMount_Dead.GuidHash && Utilities.HasComponent<EntityOwner>(entity))
                        {
                            Entity owner = Utilities.GetComponentData<EntityOwner>(entity).Owner;
                            if (Utilities.HasComponent<PlayerCharacter>(owner))
                            {
                                User componentData = Utilities.GetComponentData<User>(Utilities.GetComponentData<PlayerCharacter>(owner).UserEntity);
                                ctx.Reply("Found dead horse owner, disabling...");
                                ulong platformId = componentData.PlatformId;
                                TileSets.HorseFunctions.PlayerHorseStasisMap[platformId] = new TileSets.HorseFunctions.HorseStasisState(entity, true);
                                SystemPatchUtil.Disable(entity);
                            }
                        }
                    }
                }
                entityArray.Dispose();
                ctx.Reply("Placed dead player ghost horses in stasis. They can still be resummoned.");
            }

            [Command("enablehorse", "eh", description: "Reactivates the player's horse.", adminOnly: false)]
            public static void ReactivateHorse(ChatCommandContext ctx)
            {
                ulong platformId = ctx.User.PlatformId;
                if (TileSets.HorseFunctions.PlayerHorseStasisMap.TryGetValue(platformId, out HorseStasisState horseStasisState) && horseStasisState.IsInStasis)
                {
                    SystemPatchUtil.Enable(horseStasisState.HorseEntity);
                    horseStasisState.IsInStasis = false;
                    TileSets.HorseFunctions.PlayerHorseStasisMap[platformId] = horseStasisState;
                    ctx.Reply("Your horse has been reactivated.");
                }
                else
                {
                    ctx.Reply("No horse in stasis found to reactivate.");
                }
            }

            internal struct HorseStasisState
            {
                public Entity HorseEntity;
                public bool IsInStasis;

                public HorseStasisState(Entity horseEntity, bool isInStasis)
                {
                    this.HorseEntity = horseEntity;
                    this.IsInStasis = isInStasis;
                }
            }
            /*
            [Command("critterquery", "cq", description: "Queries for critters.", adminOnly: true)]
            public static void CritterQuery(ChatCommandContext ctx)
            {
                var position = Utilities.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
                var entityManager = VWorld.Server.EntityManager;
                Entity userEntity = ctx.Event.SenderUserEntity;

                // cat group
                NativeArray<Entity> entityArray = entityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = (Il2CppStructArray<ComponentType>)new ComponentType[4]
                    {
            ComponentType.ReadOnly<PrefabGUID>(),
            ComponentType.ReadOnly<UnitSpawnHandler>(),
            ComponentType.ReadOnly<Age>(),
            ComponentType.ReadOnly<LifeTime>()
                    },
                    Options = EntityQueryOptions.IncludeDisabled
                }).ToEntityArray(Allocator.TempJob);
                
                foreach (var entity in entityArray)
                {
                    UnitSpawnHandler unitSpawnHandler = Utilities.GetComponentData<UnitSpawnHandler>(entity);
                    Age age = Utilities.GetComponentData<Age>(entity);
                    LifeTime lifeTime = Utilities.GetComponentData<LifeTime>(entity);
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(entity);
                    if (prefabGUID.LookupName().ToLower().Contains("critter"))
                    {
                        Plugin.Logger.LogInfo($"Critter found: {prefabGUID.LookupName()}");
                        UnitSpawnerService.UnitSpawner.SpawnWithCallback(userEntity, prefabGUID, position.xz, 1f, e =>
                        {
                            Utilities.SetComponentData(e, unitSpawnHandler);
                            Utilities.SetComponentData(e, age);
                            Utilities.SetComponentData(e, lifeTime);

                            Plugin.Logger.LogInfo($"Critter spawned: {prefabGUID.LookupName()}");
                        });
                    }
                }
                entityArray.Dispose();
                
                */

        }
            
        
    }
}