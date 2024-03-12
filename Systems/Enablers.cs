using Bloodstone.API;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM;
using ProjectM.Network;
using ProjectM.Tiles;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Data;
using VCreate.Core;

namespace VCreate.Systems;

internal static class Enablers
{
    public class TileFunctions
    {
        private static NativeArray<Entity> GetTiles()
        {
            var tileQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<PrefabGUID>(),
                ComponentType.ReadOnly<TileModel>()
            },
                //None = new[] { ComponentType.ReadOnly<Dead>(), ComponentType.ReadOnly<DestroyTag>() }
            });

            return tileQuery.ToEntityArray(Allocator.Temp);
        }

        internal static List<Entity> ClosestTiles(ChatCommandContext ctx, float radius, string name)
        {
            try
            {
                var e = ctx.Event.SenderCharacterEntity;
                var tiles = GetTiles();
                var results = new List<Entity>();
                var origin = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(e).Position;
                var prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
                PrefabGUID tileGUID;

                tileGUID = prefabCollectionSystem.NameToPrefabGuidDictionary.TryGetValue(name, out var guid) ? guid : throw new ArgumentException($"Tile name '{name}' not found.", nameof(name));
                foreach (var tile in tiles)
                {
                    // filter for tile models to make this slightly more user-friendly
                    if (!tileGUID.LookupName().ToLower().Contains("tile")) continue;
                    var position = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(tile).Position;
                    var distance = UnityEngine.Vector3.Distance(origin, position);
                    var em = VWorld.Server.EntityManager;
                    var getGuid = em.GetComponentDataFromEntity<PrefabGUID>();
                    if (distance < radius && getGuid[tile] == tileGUID)
                    {
                        results.Add(tile);
                    }
                }
                tiles.Dispose();

                return results;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class ResourceFunctions
    {
        public static unsafe void SearchAndDestroy()
        {
            Plugin.Log.LogInfo("Entering SearchAndDestroy...");
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

            Plugin.Log.LogInfo($"{counter} resource nodes destroyed.");
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

    // this one doesn't really belong here but that's where it's going for now
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
            NativeArray<Entity> entityArray = entityManager.CreateEntityQuery(new EntityQueryDesc()
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
                VWorld.Server.EntityManager.TryGetBuffer(entity, out buffer);
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
                            PlayerHorseStasisMap[platformId] = new HorseStasisState(entity, true);
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
            if (PlayerHorseStasisMap.TryGetValue(platformId, out HorseStasisState horseStasisState) && horseStasisState.IsInStasis)
            {
                SystemPatchUtil.Enable(horseStasisState.HorseEntity);
                horseStasisState.IsInStasis = false;
                PlayerHorseStasisMap[platformId] = horseStasisState;
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
                HorseEntity = horseEntity;
                IsInStasis = isInStasis;
            }
        }
    }
}