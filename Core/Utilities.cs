using System;
using System.Runtime.InteropServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM.CastleBuilding;
using ProjectM;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using ProjectM.Shared;
using FreeBuild.Core;
using static PlayerService;
namespace FreeBuild.Core
{
    public static class Utilities
    {
        public static Il2CppSystem.Type Il2CppTypeGet(Type type)
        {
            return Il2CppSystem.Type.GetType(type.ToString());
        }

        public static ComponentType ComponentTypeGet(string component)
        {
            return ComponentType.ReadOnly(Il2CppSystem.Type.GetType(component));
        }

        // alternative for Entitymanager.HasComponent
        public static bool HasComponent<T>(Entity entity) where T : struct
        {
            return VWorld.Server.EntityManager.HasComponent(entity, ComponentTypeOther<T>());
        }

        // more convenient than Entitymanager.AddComponent
        public static bool AddComponent<T>(Entity entity) where T : struct
        {
            return VWorld.Server.EntityManager.AddComponent(entity, ComponentTypeOther<T>());
        }

        // alternative for Entitymanager.AddComponentData
        public static void AddComponentData<T>(Entity entity, T componentData) where T : struct
        {
            AddComponent<T>(entity);
            SetComponentData(entity, componentData);
        }

        // alternative for Entitymanager.RemoveComponent
        public static bool RemoveComponent<T>(Entity entity) where T : struct
        {
            return VWorld.Server.EntityManager.RemoveComponent(entity, ComponentTypeOther<T>());
        }

        // alternative for EntityMManager.GetComponentData
        public static unsafe T GetComponentData<T>(Entity entity) where T : struct
        {
            void* rawPointer = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, ComponentTypeIndex<T>());
            return Marshal.PtrToStructure<T>(new System.IntPtr(rawPointer));
        }

        // alternative for EntityManager.SetComponentData
        public static unsafe void SetComponentData<T>(Entity entity, T componentData) where T : struct
        {
            var size = Marshal.SizeOf(componentData);
            //byte[] byteArray = new byte[size];
            var byteArray = StructureToByteArray(componentData);
            fixed (byte* data = byteArray)
            {
                //UnsafeUtility.CopyStructureToPtr(ref componentData, data);
                VWorld.Server.EntityManager.SetComponentDataRaw(entity, ComponentTypeIndex<T>(), data, size);
            }
        }

        private static ComponentType ComponentTypeOther<T>()
        {
            return new ComponentType(Il2CppType.Of<T>());
        }

        private static int ComponentTypeIndex<T>()
        {
            return ComponentTypeOther<T>().TypeIndex;
        }

        private static byte[] StructureToByteArray<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] byteArray = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, true);
                Marshal.Copy(ptr, byteArray, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return byteArray;
        }
    }

    public static class CastleTerritoryCache
    {
        public static Dictionary<int2, Entity> BlockTileToTerritory = new();
        public static int TileToBlockDivisor = 10;

        public static void Initialize()
        {
            var entities = Helper.GetEntitiesByComponentTypes<CastleTerritoryBlocks>();
            foreach (var entity in entities)
            {
                entity.LogComponentTypes();
                var buffer = entity.ReadBuffer<CastleTerritoryBlocks>();
                foreach (var block in buffer)
                {
                    //Plugin.Logger.LogInfo($"{block.BlockCoordinate}");
                    BlockTileToTerritory[block.BlockCoordinate] = entity;
                }
            }
        }

        public static bool TryGetCastleTerritory(Player player, out Entity territoryEntity)
        {
            return TryGetCastleTerritory(player.Character, out territoryEntity);
        }

        public static bool TryGetCastleTerritory(Entity entity, out Entity territoryEntity)
        {
            if (entity.Has<TilePosition>())
            {
                return BlockTileToTerritory.TryGetValue(entity.Read<TilePosition>().Tile / TileToBlockDivisor, out territoryEntity);
            }
            territoryEntity = default;
            return false;
        }

        public static void AddTerritory(Entity territoryEntity, EntityManager entityManager)
        {
            try
            {
                if (entityManager.HasComponent<CastleTerritoryBlocks>(territoryEntity))
                {
                    var buffer = entityManager.GetBuffer<CastleTerritoryBlocks>(territoryEntity);
                    foreach (var block in buffer)
                    {
                        // Calculate the tile coordinate from block coordinate.
                        float2 tileCoordinate = block.BlockCoordinate / TileToBlockDivisor;

                        // Update or add the territory entity associated with this tile coordinate.
                        BlockTileToTerritory[(int2)tileCoordinate] = territoryEntity;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo($"Unable to remove territory from cache {ex}");
            }
        }

        public static void RemoveTerritory(Entity territoryEntity, EntityManager entityManager)
        {
            try
            {
                if (entityManager.HasComponent<CastleTerritoryBlocks>(territoryEntity))
                {
                    var buffer = entityManager.GetBuffer<CastleTerritoryBlocks>(territoryEntity);
                    foreach (var block in buffer)
                    {
                        // Calculate the tile coordinate from block coordinate.
                        float2 tileCoordinate = block.BlockCoordinate / TileToBlockDivisor;

                        // Remove the territory entity associated with this tile coordinate if it exists.
                        BlockTileToTerritory.Remove((int2)tileCoordinate);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo($"Unable to remove territory from cache {ex}");
            }
        }
    }

    public static class SystemPatchUtil
    {
        public static void CancelJob(Entity entity)
        {
            VWorld.Server.EntityManager.AddComponent<Disabled>(entity);
            DestroyUtility.CreateDestroyEvent(VWorld.Server.EntityManager, entity, DestroyReason.Default, DestroyDebugReason.ByScript);
        }
    }
}