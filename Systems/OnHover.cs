using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VCreate.Core;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using User = ProjectM.Network.User;

namespace VCreate.Systems
{
    public class OnHover
    {
        public static readonly float[] gridSizes = [2.5f, 5f, 7.5f]; // grid sizes to cycle through

        public static void InspectHoveredEntity(Entity userEntity)
        {
            User user = Utilities.GetComponentData<User>(userEntity);

            // Obtain the hovered entity from the player's input
            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;

            // Check if the hovered entity is valid
            if (hoveredEntity != Entity.Null && VWorld.Server.EntityManager.Exists(hoveredEntity))
            {
                hoveredEntity.LogComponentTypes();

                string entityString = hoveredEntity.Index.ToString() + ", " + hoveredEntity.Version.ToString();
                if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(hoveredEntity, out DynamicBuffer<BuffBuffer> buffer))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        string otherMessage = buffer[i].PrefabGuid.LookupName();
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, otherMessage);
                    }
                }
                ulong steamId = user.PlatformId;
                if (VCreate.Core.DataStructures.PlayerSettings.TryGetValue(steamId, out Omnitool data))
                {
                    // Create a unique string reference for the entity or prefab or whatever
                    PrefabGUID prefabGUID = Utilities.GetComponentData<PrefabGUID>(hoveredEntity);
                    data.SetData("Unit", prefabGUID.GuidHash);

                    DataStructures.Save();
                    string copySuccess = $"Inspected hovered entity for components, check log: '{entityString}', {prefabGUID.LookupName()}";
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, copySuccess);
                }
            }
            else
            {
                // Send an error message if no valid entity is hovered
                string message = "No valid entity is being hovered.";
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
            }
        }

        public static void BuffAtHover(Entity userEntity)
        {
            if (VCreate.Core.DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out Omnitool data))
            {
                PrefabGUID buff = new(data.GetData("Buff"));
                Entity entity = userEntity.Read<EntityInput>().HoveredEntity;

                FromCharacter fromCharacter = new() { Character = entity, User = userEntity };
                DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                var debugEvent = new ApplyBuffDebugEvent
                {
                    BuffPrefabGUID = buff,
                };

                if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, entity, buff, out Entity buffEntity))
                {
                    debugEventsSystem.ApplyBuff(fromCharacter, debugEvent);
                    if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, entity, buff, out buffEntity))
                    {
                        if (buffEntity.Has<CreateGameplayEventsOnSpawn>())
                        {
                            buffEntity.Remove<CreateGameplayEventsOnSpawn>();
                        }
                        if (buffEntity.Has<GameplayEventListeners>())
                        {
                            buffEntity.Remove<GameplayEventListeners>();
                        }
                        if (!Utilities.HasComponent<Buff_Persists_Through_Death>(buffEntity))
                        {
                            Utilities.AddComponent<Buff_Persists_Through_Death>(buffEntity);
                        }

                        if (buffEntity.Has<LifeTime>())
                        {
                            var lifetime = buffEntity.Read<LifeTime>();
                            lifetime.Duration = -1;
                            lifetime.EndAction = LifeTimeEndAction.None;
                            buffEntity.Write(lifetime);
                            //buffEntity.Remove<LifeTime>();
                        }
                        if (buffEntity.Has<RemoveBuffOnGameplayEvent>())
                        {
                            buffEntity.Remove<RemoveBuffOnGameplayEvent>();
                        }
                        if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
                        {
                            buffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
                        }
                    }
                }
            }
            else
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Couldn't find omnitool data.");
            }
        }

        public static void BuffNonPlayer(Entity unitEntity, Entity userEntity, PrefabGUID prefabGUID)
        {
            //PlayerService.TryGetCharacterFromName(userEntity.Read<User>().CharacterName.ToString(), out Entity character);
            FromCharacter fromCharacter = new() { Character = unitEntity, User = unitEntity };
            DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            var debugEvent = new ApplyBuffDebugEvent
            {
                BuffPrefabGUID = prefabGUID,
            };
            if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, unitEntity, prefabGUID, out Entity buffEntity))
            {
                debugEventsSystem.ApplyBuff(fromCharacter, debugEvent);
                if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, unitEntity, prefabGUID, out buffEntity))
                {
                    if (buffEntity.Has<CreateGameplayEventsOnSpawn>())
                    {
                        buffEntity.Remove<CreateGameplayEventsOnSpawn>();
                    }
                    if (buffEntity.Has<GameplayEventListeners>())
                    {
                        buffEntity.Remove<GameplayEventListeners>();
                    }
                    if (!Utilities.HasComponent<Buff_Persists_Through_Death>(buffEntity))
                    {
                        Utilities.AddComponent<Buff_Persists_Through_Death>(buffEntity);
                    }

                    if (buffEntity.Has<LifeTime>())
                    {
                        var lifetime = buffEntity.Read<LifeTime>();
                        lifetime.Duration = -1;
                        lifetime.EndAction = LifeTimeEndAction.None;
                        buffEntity.Write(lifetime);
                        //buffEntity.Remove<LifeTime>();
                    }
                    if (buffEntity.Has<RemoveBuffOnGameplayEvent>())
                    {
                        buffEntity.Remove<RemoveBuffOnGameplayEvent>();
                    }
                    if (buffEntity.Has<RemoveBuffOnGameplayEventEntry>())
                    {
                        buffEntity.Remove<RemoveBuffOnGameplayEventEntry>();
                    }
                }
            }
        }

        public static void DestroyAtHover(Entity userEntity)
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

        public static void SummonHelpers(Entity userEntity)
        {
            // want this to spawn all the dummise
            User user = Utilities.GetComponentData<User>(userEntity);
            int index = user.Index;
            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
            PrefabCollectionSystem prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
            PrefabGUID paladin = VCreate.Data.Prefabs.CHAR_ChurchOfLight_Paladin_Servant;
            EntityManager entityManager = VWorld.Server.EntityManager;
            //spawncharmeable on the pet
            //VWorld.Server.EntityManager.AddBuffer<FollowerBuffer>(hoveredEntity);
            FromCharacter fromCharacter = new() { Character = hoveredEntity, User = userEntity };
            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
            EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

            var debugEventEquipment = new SpawnCharmeableDebugEvent
            {
                PrefabGuid = paladin,
                Position = hoveredEntity.Read<LocalToWorld>().Position,
            };
            DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            debugEventsSystem.SpawnCharmeableDebugEvent(index, ref debugEventEquipment, entityCommandBuffer, ref fromCharacter);

            // this will be following the player and will fire the follower hook
        }

        public static void LinkHelper(Entity userEntity)
        {
            //List<Entity> followerEntities = new List<Entity>();
            EntityManager entityManager = VWorld.Server.EntityManager;
            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
            Team userTeam = userEntity.Read<Team>();
            TeamReference teamReference = userEntity.Read<TeamReference>();
            Entity character = userEntity.Read<User>().LocalCharacter._Entity;

            if (VWorld.Server.EntityManager.TryGetBuffer<FollowerBuffer>(character, out var followers))
            {
                for (int i = 0; i < followers.Length; i++)
                {
                    if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(followers[i].Entity._Entity, out var buffs))
                    {
                        foreach (var buff in buffs)
                        {
                            if (buff.PrefabGuid.Equals(VCreate.Data.Buffs.AB_Charm_Active_Human_Buff))
                            {
                                Entity entityToFollow = Entity.Null;
                                // want to link this follower to the follower with a follower buffer
                                foreach (var following in followers)
                                {
                                    if (VWorld.Server.EntityManager.TryGetBufferReadOnly<FollowerBuffer>(following.Entity._Entity, out var buffer))
                                    {
                                        // want to link the follower to the hovered entity if it has a follower buffer
                                        entityToFollow = following.Entity._Entity;
                                        break;
                                    }
                                }
                                if (entityToFollow == Entity.Null)
                                {
                                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "No familar found to link helper to.");
                                    continue; //no familar found to link
                                }
                                else
                                {
                                    Utilities.SetComponentData(hoveredEntity, new Team { Value = userTeam.Value, FactionIndex = userTeam.FactionIndex });
                                    ModifiableEntity modEnt = ModifiableEntity.CreateFixed(entityToFollow);
                                    ModifiableInt modInt = Utilities.GetComponentData<Follower>(hoveredEntity).ModeModifiable;
                                    modInt._Value = (int)FollowMode.Unit;

                                    Utilities.SetComponentData(hoveredEntity, new Follower { Followed = modEnt, ModeModifiable = modInt, Stationary = ModifiableBool.CreateFixed(false) });
                                    Utilities.SetComponentData(hoveredEntity, teamReference);
                                    GetOwnerTranslationOnSpawn getOwnerTranslationOnSpawn = new GetOwnerTranslationOnSpawn { SnapToGround = true, TranslationSource = GetOwnerTranslationOnSpawnComponent.GetTranslationSource.Owner };
                                    Utilities.AddComponentData(hoveredEntity, getOwnerTranslationOnSpawn);
                                    //entityManager.AddBuffer<FollowerBuffer>(hoveredEntity);
                                    // now should be safe to get rid of charm

                                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Linked helper to familiar.");
                                }
                            }
                        }
                        Plugin.Log.LogInfo("Modifying buffs...");
                        DebuffNonPlayer(hoveredEntity);
                        BuffNonPlayer(hoveredEntity, userEntity, VCreate.Data.Buffs.Admin_Observe_Invisible_Buff);
                        //BuffNonPlayer(hoveredEntity, userEntity, VBuild.Data.Buff.AB_InvisibilityAndImmaterial_Buff);
                    }
                }
                Plugin.Log.LogInfo("Attempted to link helper to familar...");
            }
            else
            {
                Plugin.Log.LogInfo("No followers found...");
            }
        }

        public static void ConvertCharacter(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;

            Entity hoveredEntity = userEntity.Read<EntityInput>().HoveredEntity;
            Team userTeam = userEntity.Read<Team>();
            TeamReference teamReference = userEntity.Read<TeamReference>();
            Entity character = userEntity.Read<User>().LocalCharacter._Entity;

            Utilities.SetComponentData(hoveredEntity, new Team { Value = userTeam.Value, FactionIndex = userTeam.FactionIndex });

            ModifiableEntity modifiableEntity = ModifiableEntity.CreateFixed(character);
            ModifiableInt modifiableInt = Utilities.GetComponentData<Follower>(hoveredEntity).ModeModifiable;
            modifiableInt._Value = (int)FollowMode.Patrol;

            Utilities.SetComponentData(hoveredEntity, new Follower { Followed = modifiableEntity, ModeModifiable = modifiableInt });
            Utilities.SetComponentData(hoveredEntity, teamReference);
            GetOwnerTranslationOnSpawn getOwnerTranslationOnSpawn = new GetOwnerTranslationOnSpawn { SnapToGround = true, TranslationSource = GetOwnerTranslationOnSpawnComponent.GetTranslationSource.BuffTarget };
            //hoveredEntity.Write<Translation>(new Translation { Value = new(-218.4665f, 15, -556.69354f) });
            //hoveredEntity.Write<LastTranslation>(new LastTranslation { Value = new(-218.4665f, 15, -556.69354f) });
            OffsetTranslationOnSpawnBlockerSettings offsetTranslationOnSpawnBlockerSettings = new OffsetTranslationOnSpawnBlockerSettings { Filter = ProjectM.Physics.CollisionFilterFlags.All };
            Utilities.AddComponentData(hoveredEntity, getOwnerTranslationOnSpawn);
            entityManager.AddBuffer<FollowerBuffer>(hoveredEntity);
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "Converted entity to your team.");
        }

        public static unsafe void SpawnCopy(Entity userEntity)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Plugin.Log.LogInfo("Cloning Triggered");

            User user = Utilities.GetComponentData<User>(userEntity);
            int index = user.Index;
            PlayerService.TryGetCharacterFromName(user.CharacterName.ToString(), out Entity character);
            FromCharacter fromCharacter = new() { Character = character, User = userEntity };

            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
                EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

                PrefabGUID prefab = new(data.GetData("Unit"));
                var debugEvent = new SpawnCharmeableDebugEvent
                {
                    PrefabGuid = prefab,
                    Position = userEntity.Read<EntityInput>().AimPosition
                };
                DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                debugEventsSystem.SpawnCharmeableDebugEvent(index, ref debugEvent, entityCommandBuffer, ref fromCharacter);
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Spawned last unit inspected/set as charmed.");
            }
            else
            {
                Plugin.Log.LogInfo("Couldn't find omnitool data.");
            }
        }

        public static unsafe void SpawnTileModel(Entity userEntity)
        {
            Plugin.Log.LogInfo("SpawnPrefabModel Triggered");

            if (!Utilities.HasComponent<User>(userEntity))
            {
                return;
            }

            var user = Utilities.GetComponentData<User>(userEntity);
            var steamId = user.PlatformId;
            var aimPosition = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().AimPosition);

            if (!DataStructures.PlayerSettings.TryGetValue(steamId, out Omnitool data))
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "Unable to locate build settings.");
                return;
            }

            HandleBuild(data, aimPosition, userEntity, user);
        }

        private static void HandleBuild(Omnitool data, Nullable_Unboxed<float3> aimPosition, Entity userEntity, User user)
        {
            var prefabEntity = GetPrefabEntity(data);
            if (prefabEntity == Entity.Null)
            {
                Plugin.Log.LogInfo("Prefab entity is null, returning...");
                return;
            }

            Entity tileEntity = DefaultInstantiateBehavior(prefabEntity, aimPosition, data);

            if (tileEntity == Entity.Null)
            {
                Plugin.Log.LogInfo("Tile entity is null, returning...");
                return;
            }
            string entityString = $"{tileEntity.Index}, {tileEntity.Version}";

            data.AddEntity(entityString);
            ApplyTileSettings(tileEntity, aimPosition, data, userEntity, user);
        }

        private static Entity GetPrefabEntity(Omnitool data)
        {
            PrefabGUID prefabGUID = new(data.GetData("Tile"));

            return VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap.TryGetValue(prefabGUID, out Entity entity) ? entity : Entity.Null;
        }

        private static void ApplyTileSettings(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data, Entity userEntity, User user)
        {
            // Apply settings like ImmortalTiles, MapIconToggle, etc.
            ApplyImmortalTilesSetting(tileEntity, data);
            ApplyMapIconSetting(tileEntity, data, user);
            ApplySnappingSetting(tileEntity, aimPosition, data);

            FinalizeTileSpawn(tileEntity, aimPosition, data, user);
        }

        private static Entity DefaultInstantiateBehavior(Entity prefabEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data)
        {
            Entity tileEntity = VWorld.Server.EntityManager.Instantiate(prefabEntity);
            Utilities.SetComponentData(tileEntity, new Translation { Value = aimPosition.Value });

            SetTileRotation(tileEntity, data.GetData("Rotation"));
            return tileEntity;
        }

        private static void SetTileRotation(Entity tileEntity, int rotationDegrees)
        {
            float radians = math.radians(rotationDegrees);
            quaternion rotationQuaternion = quaternion.EulerXYZ(new float3(0, radians, 0));
            Utilities.SetComponentData(tileEntity, new Rotation { Value = rotationQuaternion });
        }

        private static void ApplyImmortalTilesSetting(Entity tileEntity, Omnitool data)
        {
            if (data.GetMode("ImmortalToggle"))
            {
                Utilities.AddComponentData(tileEntity, new Immortal { IsImmortal = true });
            }
        }

        private static void ApplyMapIconSetting(Entity tileEntity, Omnitool data, User user)
        {
            if (data.GetMode("MapIconToggle"))
            {
                if (data.GetData("MapIcon") == 0)
                {
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, "No map icon set.");
                    return;
                }

                var prefabGUID = new PrefabGUID(data.GetData("MapIcon"));
                if (!VWorld.Server.EntityManager.HasComponent<AttachMapIconsToEntity>(tileEntity))
                {
                    VWorld.Server.EntityManager.AddBuffer<AttachMapIconsToEntity>(tileEntity);
                }

                VWorld.Server.EntityManager.GetBuffer<AttachMapIconsToEntity>(tileEntity).Add(new AttachMapIconsToEntity { Prefab = prefabGUID });
            }
        }

        private static void ApplySnappingSetting(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data)
        {
            if (data.GetMode("SnappingToggle"))
            {
                float3 mousePosition = aimPosition.Value;
                // Assuming TileSnap is an int representing the grid size index
                // If TileSnap now refers directly to the size, adjust accordingly
                float gridSize = OnHover.gridSizes[data.GetData("GridSize")-1]; // Adjust this line if the way you access grid sizes has changed
                mousePosition = new float3(
                    math.round(mousePosition.x / gridSize) * gridSize,
                    mousePosition.y,
                    math.round(mousePosition.z / gridSize) * gridSize);
                Utilities.SetComponentData(tileEntity, new Translation { Value = mousePosition });
            }
        }

        private static void FinalizeTileSpawn(Entity tileEntity, Nullable_Unboxed<float3> aimPosition, Omnitool data, User user)
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
            string message = $"Tile spawned at {aimPosition.value.xy} with rotation {data.GetData("Rotation")} degrees clockwise.";
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
            LogTilePlacement(tileEntity);
        }

        private static void LogTilePlacement(Entity tileEntity)
        {
            string entityString = $"{tileEntity.Index}, {tileEntity.Version}";
            Plugin.Log.LogInfo($"Tile placed: {entityString}");
        }

        public static void DebuffAtHover(Entity userEntity)
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
            else
            {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, userEntity.Read<User>(), "No buffs found to remove.");
            }
        }

        public static void DebuffNonPlayer(Entity unitEntity)
        {
            if (VWorld.Server.EntityManager.TryGetBuffer<BuffBuffer>(unitEntity, out DynamicBuffer<BuffBuffer> buffer))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    SystemPatchUtil.Disable(buffer[i].Entity);
                }
            }
        }
    }
}