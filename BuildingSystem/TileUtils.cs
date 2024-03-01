using System;
using System.Collections.Generic;
using Bloodstone.API;
using ProjectM;
using ProjectM.Tiles;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace WorldBuild.BuildingSystem;

internal static class TileUtil
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
                var position = VWorld.Server.EntityManager.GetComponentData<LocalToWorld>(tile).Position;
                var distance = UnityEngine.Vector3.Distance(origin, position);
                var em = VWorld.Server.EntityManager;
                var getGuid = em.GetComponentDataFromEntity<PrefabGUID>();
                if (distance < radius && getGuid[tile] == tileGUID)
                {
                    results.Add(tile);
                }
            }

            return results;
        }
        catch (Exception)
        {
            return null;
        }
    }
}