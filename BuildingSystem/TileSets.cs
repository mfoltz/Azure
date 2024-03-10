using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Network;
using ProjectM.Pathfinding;
using ProjectM.Shared;
using ProjectM.Shared.Systems;
using ProjectM.Tiles;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.Tilemaps;
using VBuild.Core;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using VBuild.Data;
using static ProjectM.Behaviours.CastOptionCandidateSorters;
using static RootMotion.FinalIK.InteractionObject;
using static UnityEngine.UI.Image;
using static VBuild.BuildingSystem.TileSets;
using static VBuild.Core.Services.UnitSpawnerService;
using static VCF.Core.Basics.RoleCommands;
using Buff = VBuild.Data.Buff;
using Exception = System.Exception;
using ServantData = ProjectM.ServantData;
using StringComparer = System.StringComparer;
using StringComparison = System.StringComparison;
using User = ProjectM.Network.User;

namespace VBuild.BuildingSystem
{
    public class TileSets
    {
        public static readonly float[] gridSizes = new float[] { 2.5f, 5f, 10f }; // grid sizes to cycle through

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
                // component details here
                //List<ComponentType> componentTypes = hoveredEntity.GetComponentTypes();

                string entityString = hoveredEntity.Index.ToString() + ", " + hoveredEntity.Version.ToString();
                if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(hoveredEntity, out DynamicBuffer<BuffBuffer> buffer))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        //SystemPatchUtil.Destroy(buffer[i].Entity);
                        string otherMessage = buffer[i].PrefabGuid.LookupName();
                        //string colorMessage = VBuild.Core.Toolbox.FontColors.Cyan(otherMessage);
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, otherMessage);
                    }
                }
                // so I want it to follow me and be on my team, could also remove the charm debuff when being possessed and add back when done
                // need to figure out how to make everything able to attack by default to and if the combat buffs are universal or apply to them specifically

                ulong steamId = user.PlatformId;
                if (Databases.playerBuildSettings.TryGetValue(steamId, out BuildSettings settings))
                {
                    // Create a unique string reference for the entity or prefab or whatever
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
                    settings.TileModel = prefabGUID.GuidHash;

                    // Add this reference to the LastTilesPlaced stack
                    Databases.SaveBuildSettings();
                    string copySuccess = $"Inspected hovered entity for components, check log: '{entityString}', {prefabGUID.LookupName()}";
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, copySuccess);
                }
                else
                {
                    Plugin.Logger.LogInfo("Couldn't find player build settings for eye-dropper.");
                }
            }
            else
            {
                // Send an error message if no valid entity is hovered
                string message = "No valid entity is being hovered. Please hover over an entity to inspect.";
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
            }
        }

        public static void BuffAtHover(Entity userEntity)
        {
            if (VPlus.Data.Databases.playerPrestige.TryGetValue(userEntity.Read<User>().PlatformId, out var data))
            {
                PrefabGUID shiny = new(data.PlayerBuff);
                Entity entity = userEntity.Read<EntityInput>().HoveredEntity;
                PlayerService.TryGetCharacterFromName(userEntity.Read<User>().CharacterName.ToString(), out Entity character);
                FromCharacter fromCharacter = new() { Character = character, User = userEntity };
                if (entity != Entity.Null && VWorld.Server.EntityManager.Exists(entity))
                {
                    var buffer = entity.ReadBuffer<BuffBuffer>();
                    buffer.Add(new BuffBuffer { Entity = entity, PrefabGuid = shiny });
                    DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                    var debugEvent = new ApplyBuffDebugEvent
                    {
                        BuffPrefabGUID = shiny,
                    };
                    
                    
                    BufferFromEntity<BuffBuffer> bufferFromEntity = VWorld.Server.EntityManager.GetBufferFromEntity<BuffBuffer>();
                    if (BuffUtility.TryGetBuff(entity, shiny, bufferFromEntity, out var result))
                    {
                        debugEventsSystem.ApplyBuff(fromCharacter, debugEvent);
                        if (result.Has<CreateGameplayEventsOnSpawn>())
                        {
                            result.Remove<CreateGameplayEventsOnSpawn>();
                        }

                        if (result.Has<GameplayEventListeners>())
                        {
                            result.Remove<GameplayEventListeners>();
                        }

                        result.Add<Buff_Persists_Through_Death>();
                        if (result.Has<RemoveBuffOnGameplayEvent>())
                        {
                            result.Remove<RemoveBuffOnGameplayEvent>();
                        }

                        if (result.Has<RemoveBuffOnGameplayEventEntry>())
                        {
                            result.Remove<RemoveBuffOnGameplayEventEntry>();
                        }

                        if (result.Has<LifeTime>())
                        {
                            LifeTime componentData2 = result.Read<LifeTime>();
                            componentData2.Duration = -1f;
                            componentData2.EndAction = LifeTimeEndAction.None;
                            result.Write(componentData2);
                        }

                        if (result.Has<RemoveBuffOnGameplayEvent>())
                        {
                            result.Remove<RemoveBuffOnGameplayEvent>();
                        }

                        if (result.Has<RemoveBuffOnGameplayEventEntry>())
                        {
                            result.Remove<RemoveBuffOnGameplayEventEntry>();
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Buff failed");
                    }
                }
            }
            else
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Couldn't find data.");
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

        public static Entity ServantComponents(Entity hoveredEntity)
        {
            //begin transplant
            PrefabGUID prefabGUID = VBuild.Data.Prefabs.CHAR_ChurchOfLight_Paladin_Servant;
            Entity entity = VWorld.Server.EntityManager.Instantiate(VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[prefabGUID]);

            try
            {
                ServantEquipment servantEquipment = entity.Read<ServantEquipment>();

                entity.Remove<ServantEquipment>();

                Utilities.AddComponentData(hoveredEntity, servantEquipment);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
                SystemPatchUtil.Destroy(entity);
                return hoveredEntity;
            }
            SystemPatchUtil.Destroy(entity);
            return hoveredEntity;
        }

        public static Entity RemoveComponents(Entity hoveredEntity)
        {
            try
            {
                if (hoveredEntity.Has<VBloodConsumeSource>())
                {
                    hoveredEntity.Remove<VBloodConsumeSource>();
                }
                else if (hoveredEntity.Has<VBloodUnitSpawnSource>())
                {
                    hoveredEntity.Remove<VBloodUnitSpawnSource>();
                }
                else if (hoveredEntity.Has<VBloodUnit>())
                {
                    hoveredEntity.Remove<VBloodUnit>();
                }
                else if (hoveredEntity.Has<VBloodUnlockTechBuffer>())
                {
                    hoveredEntity.Remove<VBloodUnlockTechBuffer>();
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
                return hoveredEntity;
            }
            return hoveredEntity;
        }

        public static Entity HorseComponents(Entity hoveredEntity)
        {
            //begin transplant
            PrefabGUID prefabGUID = VBuild.Data.Prefabs.CHAR_Mount_Horse;
            Entity entity = VWorld.Server.EntityManager.Instantiate(VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[prefabGUID]);

            try
            {
                CanPreventDisableWhenNoPlayersInRange canPreventDisableWhenNoPlayersInRange = new CanPreventDisableWhenNoPlayersInRange { CanDisable = new ModifiableBool { _Value = false } };

                FeedableInventory feedableInventory = entity.Read<FeedableInventory>();

                Interactable interactable = entity.Read<Interactable>();

                NameableInteractable nameableInteractable = entity.Read<NameableInteractable>();

                GetOwnerTranslationOnSpawn getOwnerTranslationOnSpawn = new GetOwnerTranslationOnSpawn { SnapToGround = true, TranslationSource = GetOwnerTranslationOnSpawnComponent.GetTranslationSource.Owner };

                Mountable mountable = new Mountable { };
                mountable.Acceleration = 30f;
                mountable.MaxSpeed = 15f;
                mountable.RotationSpeed = 20f;
                mountable.AccelerationRange = 5f;
                mountable.RotationSpeedRange = 5f;
                mountable.MountBuff = VBuild.Data.Prefabs.MOUNT_Wolf_Boss_Standard;
                mountable.HasNearbyUsers = true;
                mountable.Mounter = hoveredEntity.Read<Follower>().Followed.Value.Read<PlayerCharacter>().UserEntity;

                DeadSequence deadSequence = entity.Read<DeadSequence>();
                DynamicCollision dynamicCollision = entity.Read<DynamicCollision>();
                PhysicsCollider physicsCollider = entity.Read<PhysicsCollider>();
                var buffer = entity.ReadBuffer<InteractAbilityBuffer>();
                //
                entity.Remove<FeedableInventory>();
                entity.Remove<Interactable>();
                entity.Remove<NameableInteractable>();
                entity.Remove<DeadSequence>();
                entity.Remove<DynamicCollision>();
                entity.Remove<PhysicsCollider>();
                entity.Remove<InteractAbilityBuffer>();
                //entity.Remove<GetOwnerTranslationOnSpawn>();
                //
                Utilities.AddComponentData(hoveredEntity, canPreventDisableWhenNoPlayersInRange);
                Utilities.AddComponentData(hoveredEntity, feedableInventory);
                Utilities.SetComponentData(hoveredEntity, interactable);
                Utilities.AddComponentData(hoveredEntity, nameableInteractable);
                Utilities.AddComponentData(hoveredEntity, getOwnerTranslationOnSpawn);
                Utilities.AddComponentData(hoveredEntity, mountable);
                Utilities.SetComponentData(hoveredEntity, deadSequence);
                Utilities.SetComponentData(hoveredEntity, dynamicCollision);
                Utilities.SetComponentData(hoveredEntity, physicsCollider);
                Utilities.SetComponentData(hoveredEntity, buffer);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
                SystemPatchUtil.Destroy(entity);
                return hoveredEntity;
            }
            SystemPatchUtil.Destroy(entity);
            return hoveredEntity;
        }

        public static void ConvertCharacter(Entity userEntity)
        {
            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
            Team userTeam = userEntity.Read<Team>();
            TeamReference teamReference = userEntity.Read<TeamReference>();
            Entity character = userEntity.Read<User>().LocalCharacter._Entity;
            Utilities.SetComponentData(hoveredEntity, new Team { Value = userTeam.Value, FactionIndex = userTeam.FactionIndex });
            ModifiableEntity modifiableEntity = new ModifiableEntity { Value = character };
            ModifiableInt modifiableInt = Utilities.GetComponentData<Follower>(hoveredEntity).ModeModifiable;
            modifiableInt._Value = (int)FollowMode.Patrol;
            Utilities.SetComponentData(hoveredEntity, new Follower { Followed = modifiableEntity, ModeModifiable = modifiableInt });
            Utilities.SetComponentData(hoveredEntity, teamReference);

            ServantComponents(hoveredEntity);
            HorseComponents(hoveredEntity);
            RemoveComponents(hoveredEntity);
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Converted entity to your team.");
        }

        public static unsafe void SpawnCopy(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Plugin.Logger.LogInfo("Cloning Triggered");

            User user = Utilities.GetComponentData<User>(userEntity);
            int index = user.Index;
            PlayerService.TryGetCharacterFromName(user.CharacterName.ToString(), out Entity character);
            FromCharacter fromCharacter = new() { Character = character, User = userEntity };

            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
                EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

                PrefabGUID prefab = new(settings.TileModel);
                var debugEvent = new SpawnCharmeableDebugEvent
                {
                    PrefabGuid = prefab,
                    Position = userEntity.Read<EntityInput>().AimPosition
                };
                DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                debugEventsSystem.SpawnCharmeableDebugEvent(index, ref debugEvent, entityCommandBuffer, ref fromCharacter);
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Spawned last unit inspected as charmed.");
            }
            else
            {
                Plugin.Logger.LogInfo("Couldn't find settings for spawn.");
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

            data.AddTilePlaced(entityString);
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
            if (!Utilities.HasComponent<InteractedUpon>(tileEntity))
            {
                Utilities.AddComponentData(tileEntity, new InteractedUpon { BlockBuildingDisassemble = true, BlockBuildingMovement = true });
            }
            else
            {
                InteractedUpon interactedUpon = tileEntity.Read<InteractedUpon>();
                interactedUpon.BlockBuildingDisassemble = true;
                interactedUpon.BlockBuildingMovement = true;
                Utilities.SetComponentData(tileEntity, interactedUpon);
            }
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

        public static void DebuffTileModel(Entity userEntity)
        {
            //var Position = userEntity.Read<EntityInput>().AimPosition;
            Entity entity = userEntity.Read<EntityInput>().HoveredEntity;
            if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(entity, out DynamicBuffer<BuffBuffer> buffer))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    SystemPatchUtil.Destroy(buffer[i].Entity);
                }
            }
        }

        public static PrefabGUID FindInCombatBuff(string characterPrefabName)
        {
            // Strip the specified literals from the character prefab name
            string strippedName = characterPrefabName.Replace("CHAR", "").Replace("VBlood", "");

            // Use reflection to get all static fields of the Buff class that might match the modified prefab name
            var fields = typeof(Buff).GetFields(BindingFlags.Public | BindingFlags.Static)
                                      .Where(field => field.FieldType == typeof(PrefabGUID));

            foreach (var field in fields)
            {
                if (field.Name.Replace("VBlood", "").IndexOf(strippedName, StringComparison.OrdinalIgnoreCase) >= 0
                    && field.Name.IndexOf("InCombat", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Returns the first matching PrefabGUID if an InCombat buff is found.
                    return (PrefabGUID)field.GetValue(null);
                }
            }

            return new PrefabGUID { GuidHash = 0 }; // If no matching InCombat buff is found.
        }

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
                    if (name.Contains("plant") || name.Contains("fibre") || name.Contains("shrub") || name.Contains("tree") || name.Contains("fiber") || name.Contains("bush") || name.Contains("grass") || name.Contains("sapling"))
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
    }
}