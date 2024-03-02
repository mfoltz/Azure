using System;
using System.Collections.Generic;
using Bloodstone.API;
using ProjectM;
using ProjectM.Tiles;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace VBuild.BuildingSystem;

internal static class TileUtils
{
    public static class MapIconFunctions
    {
        public static Dictionary<int, int> mapIcons = new Dictionary<int, int>
        {
            {1, VBuild.Data.Prefabs.MapIcon_CastleObject_Anvil.GuidHash },
            {2, VBuild.Data.Prefabs.MapIcon_DraculasCastle.GuidHash },
            {3, VBuild.Data.Prefabs.MapIcon_Siege_Summon_T01.GuidHash },
            {4, VBuild.Data.Prefabs.MapIcon_Siege_Summon_T02.GuidHash },
            {5, VBuild.Data.Prefabs.MapIcon_CastleObject_CastleHeart.GuidHash },
            {6, VBuild.Data.Prefabs.MapIcon_CastleObject_BloodAltar.GuidHash },
            {7, VBuild.Data.Prefabs.MapIcon_WorldWaypoint_Active.GuidHash },
            {8, VBuild.Data.Prefabs.MapIcon_Crypt.GuidHash },
            {9, VBuild.Data.Prefabs.MapIcon_PlayerPathDot.GuidHash },
            {10, VBuild.Data.Prefabs.MapIcon_Cave_Entryway.GuidHash }
        };
    }



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


    