using Bloodstone.API;
using Gee.External.Capstone.X86;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using LibCpp2IL.BinaryStructures;
using MS.Internal.Xml.XPath;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
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
using Activator = System.Activator;
using StringComparer = System.StringComparer;
using User = ProjectM.Network.User;

namespace VBuild.BuildingSystem
{
    internal class TileSets
    {
        public static readonly float[] gridSizes = new float[] { 2.5f, 5f, 10f }; // Example grid sizes to cycle through
        // can activate this by monitoring for ability player gets to use with shift key to place a tile at mouse location
        // use charm/siege interact T02 or something, monitor for abilitycast finishes that match the prefab and run this method

        public static unsafe void InspectHoveredEntity(Entity userEntity)
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
                    string copySuccess = $"Inspected hovered entity for components, check log: {entityString}, {prefabGUID.LookupName()}";
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

        public static unsafe void KillHoveredEntity(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            
            User user = Utilities.GetComponentData<User>(userEntity);

            // Obtain the hovered entity from the player's input
            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
            PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
            // Check if the hovered entity is valid
            if (Utilities.HasComponent<VampireTag>(hoveredEntity))
            {

                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Using this on vampires is not allowed.");
                return;
            }
            if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
            {
                if (!Utilities.HasComponent<Dead>(hoveredEntity))
                {
                    Utilities.AddComponentData(hoveredEntity, new Dead { DoNotDestroy = false });
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Target destroyed.");
                }
                else
                {
                    Utilities.SetComponentData(hoveredEntity, new Dead { DoNotDestroy = false });
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Target destroyed.");
                }
                    
                    
                
            }
                
            
        }

        public static unsafe void CopyHoveredEntity(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Plugin.Logger.LogInfo("Cloning Triggered");
            
                    User user = Utilities.GetComponentData<User>(userEntity);

                    // Obtain the hovered entity from the player's input
                    Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
                    // Check if the hovered entity is valid
                    if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
                    {
                        Entity prefabEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[prefabGUID];
                        //hopefully this counts as a modifiable prefab
                        // time for cloning
                        entityManager.Instantiate(prefabEntity);
                        CopyComponentData<IComponentData>(hoveredEntity, prefabEntity, entityManager, true);

                        
                        string message = "Cloned hovered entity.";
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                    }
                
            
        }

        public static void CopyComponentData<T>(Entity source, Entity destination, EntityManager entityManager, bool isReadOnly = false) where T : IComponentData
        {
            ComponentDataFromEntity<IComponentData> componentDataFromEntity = VWorld.Server.EntityManager.GetComponentDataFromEntity<IComponentData>(isReadOnly);

            if (componentDataFromEntity.HasComponent(source))
            {
                T componentData = (T)componentDataFromEntity[source];

                if (entityManager.HasComponent<T>(destination))
                {
                    entityManager.SetComponentData(destination, componentData);
                }
                else
                {
                    entityManager.AddComponentData(destination, componentData);
                }
            }
        }

        public static unsafe void SpawnTileModel(Entity userEntity)
        {
            Plugin.Logger.LogInfo("SpawnPrefabModel Triggered");

            if (!Utilities.HasComponent<User>(userEntity))
            {
                return;
            }

            

            var user = Utilities.GetComponentData<User>(userEntity);
            var steamId = user.PlatformId;
            var aimPosition = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().AimPosition);

            if (!Databases.playerBuildSettings.TryGetValue(steamId, out BuildSettings data))
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Unable to locate build settings.");
                return;
            }

            HandleBuildSettings(data, aimPosition, userEntity, user);
        }

        private static void HandleBuildSettings(BuildSettings data, Nullable_Unboxed<float3> aimPosition, Entity userEntity, User user)
        {
            var prefabEntity = GetPrefabEntity(data);
            if (prefabEntity == Entity.Null)
            {
                return;
            }

            Entity tileEntity = InstantiateTilePrefab(prefabEntity, aimPosition, data, userEntity, user);

            if (tileEntity == Entity.Null)
            {
                Plugin.Logger.LogInfo("Tile entity is null in handle build settings, returning...");
                return;
            }
            string entityString = $"{tileEntity.Index}, {tileEntity.Version}";

            data.LastTilesPlaced.Push(entityString);
            ApplyTileSettings(tileEntity, aimPosition, data, userEntity, user);
        }

        private static Entity GetPrefabEntity(BuildSettings data)
        {
            PrefabGUID prefabGUID = new(data.TileModel);
            return VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap.TryGetValue(prefabGUID, out Entity entity) ? entity : Entity.Null;
        }

        private static readonly Dictionary<string, System.Func<Entity, Entity>> specialWordHandlers = new Dictionary<string, System.Func<Entity, Entity>>()
            {
                {"char", (entity) => HandleCharPrefab(entity)},
                // Other special words and their handlers can be added here
            };

        private static Entity InstantiateTilePrefab(Entity prefabEntity, Nullable_Unboxed<float3> aimPosition, BuildSettings data, Entity userEntity, User user)
        {
            PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(prefabEntity);
            string prefabName = prefabGUID.LookupName().ToLower();

            foreach (var handler in specialWordHandlers)
            {
                if (prefabName.Contains(handler.Key))
                {
                    Plugin.Logger.LogInfo("Character spawn attempted...");
                    //return handler.Value(prefabEntity);
                }
            }

            // Default behavior if no special words are found
            return DefaultInstantiateBehavior(prefabEntity, aimPosition, data);
        }

        // Example handler method
        private static Entity HandleCharPrefab(Entity prefabEntity)
        {
            // Specific handling for character prefabs
            return prefabEntity;
        }

        private static void ApplyTileSettings(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, BuildSettings data, Entity userEntity, User user)
        {
            // Apply settings like ImmortalTiles, MapIconToggle, etc.
            ApplyImmortalTilesSetting(tileEntity, data);
            ApplyMapIconSetting(tileEntity, data, user);
            ApplySnappingSetting(tileEntity, aimPosition, data);

            FinalizeTileSpawn(tileEntity, aimPosition, data, user);
        }

        private static Entity DefaultInstantiateBehavior(Entity prefabEntity, Nullable_Unboxed<float3> aimPosition, BuildSettings data)
        {
            Entity tileEntity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
            Utilities.SetComponentData(tileEntity, new Translation { Value = aimPosition.Value });

            SetTileRotation(tileEntity, data.TileRotation);
            return tileEntity;
        }

        private static void SetTileRotation(Entity tileEntity, int rotationDegrees)
        {
            float radians = math.radians(rotationDegrees);
            quaternion rotationQuaternion = quaternion.EulerXYZ(new float3(0, radians, 0));
            Utilities.SetComponentData(tileEntity, new Rotation { Value = rotationQuaternion });
        }

        private static void ApplyImmortalTilesSetting(Entity tileEntity, BuildSettings data)
        {
            if (data.GetToggle("ImmortalTiles"))
            {
                Utilities.AddComponentData(tileEntity, new Immortal { IsImmortal = true });
            }
        }

        private static void ApplyMapIconSetting(Entity tileEntity, BuildSettings data, User user)
        {
            if (data.GetToggle("MapIconToggle"))
            {
                if (data.MapIcon == 0)
                {
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "No map icon selected.");
                    return;
                }

                var prefabGUID = new PrefabGUID(data.MapIcon);
                if (!VWorld.Server.EntityManager.HasComponent<AttachMapIconsToEntity>(tileEntity))
                {
                    VWorld.Server.EntityManager.AddBuffer<AttachMapIconsToEntity>(tileEntity);
                }

                VWorld.Server.EntityManager.GetBuffer<AttachMapIconsToEntity>(tileEntity).Add(new AttachMapIconsToEntity { Prefab = prefabGUID });
            }
        }

        private static void ApplySnappingSetting(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, BuildSettings data)
        {
            if (data.GetToggle("SnappingToggle"))
            {
                float3 mousePosition = aimPosition.Value;
                // Assuming TileSnap is an int representing the grid size index
                // If TileSnap now refers directly to the size, adjust accordingly
                float gridSize = TileSets.gridSizes[data.TileSnap]; // Adjust this line if the way you access grid sizes has changed
                mousePosition = new float3(
                    math.round(mousePosition.x / gridSize) * gridSize,
                    mousePosition.y,
                    math.round(mousePosition.z / gridSize) * gridSize);
                Utilities.SetComponentData(tileEntity, new Translation { Value = mousePosition });
            }
        }

        private static void FinalizeTileSpawn(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, BuildSettings data, User user)
        {
            string message = $"Tile spawned at {aimPosition.value.xy} with rotation {data.TileRotation} degrees clockwise.";
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
            LogTilePlacement(tileEntity);
        }

        private static void LogTilePlacement(Entity tileEntity)
        {
            string entityString = $"{tileEntity.Index}, {tileEntity.Version}";
            Plugin.Logger.LogInfo($"Tile placed: {entityString}");
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
                    { 9, new TileConstructor("TM_Castle_Floor_Royal01", VBuild.Data.Prefabs.TM_Castle_Floor_Foundation_Stone01_DLCVariant01.GuidHash) },
                    { 8, new TileConstructor("TM_GloomRot_LightningRod_Refinement_01", VBuild.Data.Prefabs.TM_GloomRot_LightningRod_Refinement_01.GuidHash) },
                    { 7, new TileConstructor("TM_Castle_Fortification_Stone_Wall01", VBuild.Data.Prefabs.TM_Castle_Fortification_Stone_Wall01.GuidHash) },
                    { 6, new TileConstructor("TM_Castle_Fortification_Pillar_Base", VBuild.Data.Prefabs.TM_Castle_Fortification_Pillar_Base.GuidHash) },
                    { 5, new TileConstructor("TM_Castle_Fortification_Stone_Entrance01", VBuild.Data.Prefabs.TM_Castle_Fortification_Stone_Entrance01.GuidHash) },
                    { 4, new TileConstructor("Test_TM_Castle_Floor_Entryway_Stairs_Long_Repaired", VBuild.Data.Prefabs.Test_TM_Castle_Floor_Entryway_Stairs_Long_Repaired.GuidHash) },
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
                    { 9, new TileConstructor("TM_Castle_Floor_Royal01", VBuild.Data.Prefabs.TM_Castle_Floor_Foundation_Stone01_DLCVariant01.GuidHash) },
                    { 8, new TileConstructor("TM_GloomRot_LightningRod_Refinement_01", VBuild.Data.Prefabs.TM_GloomRot_LightningRod_Refinement_01.GuidHash) },
                    { 7, new TileConstructor("TM_Castle_Fortification_Stone_Wall01", VBuild.Data.Prefabs.TM_Castle_Fortification_Stone_Wall01.GuidHash) },
                    { 6, new TileConstructor("TM_Castle_Fortification_Pillar_Base", VBuild.Data.Prefabs.TM_Castle_Fortification_Pillar_Base.GuidHash) },
                    { 5, new TileConstructor("TM_Castle_Fortification_Stone_Entrance01", VBuild.Data.Prefabs.TM_Castle_Fortification_Stone_Entrance01.GuidHash) },
                    { 4, new TileConstructor("Test_TM_Castle_Floor_Entryway_Stairs_Long_Repaired", VBuild.Data.Prefabs.Test_TM_Castle_Floor_Entryway_Stairs_Long_Repaired.GuidHash) },
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
                    if (name.Contains("plant") || name.Contains("fibre") || name.Contains("shrub") || name.Contains("tree") || name.Contains("fiber") || name.Contains("bush") || name.Contains("tree)"))
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
        }
    }
}