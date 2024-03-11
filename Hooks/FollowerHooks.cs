using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
using ProjectM.UI;
using Steamworks;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VBuild.BuildingSystem;
using VBuild.Core;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;


[HarmonyPatch(typeof(RepairDoubleVBloodSpawnedSystem), nameof(RepairDoubleVBloodSpawnedSystem.OnUpdate))]
public static class RepairDoubleVBloodSpawnedSystemPatch
{
    public static bool Prefix(RepairDoubleVBloodSpawnedSystem __instance)
    {
        Plugin.Logger.LogInfo("RepairDoubleVBloodSpawnedSystem Prefix called...");
        return false;
    }
}



[HarmonyPatch(typeof(FollowerSystem), nameof(FollowerSystem.OnUpdate))]
public static class FollowerSystemPatch
{
    // proxies for pets
    private static readonly PrefabGUID invulnerable = VBuild.Data.Buff.Admin_Invulnerable_Buff;
    private static readonly PrefabGUID invisible = VBuild.Data.Buff.Admin_Observe_Invisible_Buff;
    private static readonly PrefabGUID servant = VBuild.Data.Prefabs.CHAR_ChurchOfLight_Paladin_Servant;
    private static readonly PrefabGUID horse = VBuild.Data.Prefabs.CHAR_Mount_Horse;
    private static readonly PrefabGUID trader = VBuild.Data.Prefabs.CHAR_Trader_Farbane_Knowledge_T01;
    public static void Prefix(FollowerSystem __instance)
    {
        EntityManager entityManager = VWorld.Server.EntityManager;
        //Plugin.Logger.LogInfo("FollowerSystem Prefix called...");
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        foreach (var entity in entities)
        {
            //if (!entityManager.TryGetBuffer<FollowerBuffer>(entity, out var followers)) continue;
            if (!Utilities.HasComponent<Follower>(entity)) continue;
            //Plugin.Logger.LogInfo("FollowerSystem Prefix called...");
            if (entityManager.TryGetBuffer<FollowerBuffer>(entity, out var followers))
            {
                Plugin.Logger.LogInfo("FollowerSystem Prefix: has buffer, setting helper positions");
                entity.Write<LastTranslation>(new LastTranslation { Value = entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position });
                NativeArray<FollowerBuffer> followerEntities = followers.ToNativeArray(Unity.Collections.Allocator.Temp);
                for (int i = 0; i < followerEntities.Length; i++)
                {
                    
                    //followers[i].Entity._Entity.Write<Translation>(new Translation { Value = entity.Read<LocalToWorld>().Position });
                    //followers[i].Entity._Entity.Write<LastTranslation>(new LastTranslation { Value = entity.Read<LocalToWorld>().Position });
                    followers[i].Entity._Entity.Write<LocalToWorld>(new LocalToWorld { Value = entity.Read<LocalToWorld>().Value });
                }
                followerEntities.Dispose();
            }
            else if (entity.Read<Follower>().Followed._Value.Has<PlayerCharacter>())
            {
                
                Plugin.Logger.LogInfo("FollowerSystem Prefix: no buffer, following player");
                //entity.Write<LastTranslation>(new LastTranslation { Value = entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position });
                //entity.Write<Translation>(new Translation { Value = entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position });
                OffsetTranslationOnSpawn offsetTranslationOnSpawn = new OffsetTranslationOnSpawn { Offset = entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position };
                Utilities.AddComponentData(entity, offsetTranslationOnSpawn);
            }
            
        }
        entities.Dispose();
    }
}


[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class ServerBootstrapSystem_Patch
{
    public static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
    {
        Plugin.Logger.LogInfo("ServerBootstrapSystem Prefix called...");
        var em = __instance.EntityManager;
        var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
        var serverClient = __instance._ApprovedUsersLookup[userIndex];
        var userEntity = serverClient.UserEntity;
        var userData = __instance.EntityManager.GetComponentData<User>(userEntity);

        
        try
        {
            var buffer = userData.LocalCharacter._Entity.ReadBuffer<FollowerBuffer>();
            for (int i = 0; i < buffer.Length; i++)
            {
                LocalToWorld localToWorld = userEntity.Read<LocalToWorld>();
                var follower = buffer[i];
                if (follower.Entity._Entity.Has<LastTranslation>())
                {
                    
                    follower.Entity._Entity.Write<LastTranslation>(new LastTranslation { Value = localToWorld.Position });
                }
                if (follower.Entity._Entity.Has<Translation>())
                {
                    follower.Entity._Entity.Write<Translation>(new Translation { Value = localToWorld.Position });
                }
                
                

            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e.Message);
        }
        
    }
}

