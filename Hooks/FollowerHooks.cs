using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared.Systems;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VCreate.Core;
using VCreate.Core.Toolbox;


[HarmonyPatch(typeof(RepairDoubleVBloodSpawnedSystem), nameof(RepairDoubleVBloodSpawnedSystem.OnUpdate))]
public static class RepairDoubleVBloodSpawnedSystemPatch
{
    public static bool Prefix(RepairDoubleVBloodSpawnedSystem __instance)
    {
        Plugin.Log.LogInfo("RepairDoubleVBloodSpawnedSystem Prefix called...");
        return false;
    }
}



[HarmonyPatch(typeof(FollowerSystem), nameof(FollowerSystem.OnUpdate))]
public static class FollowerSystemPatch
{
    // proxies for pets
    private static readonly PrefabGUID invulnerable = VCreate.Data.Buffs.Admin_Invulnerable_Buff;
    private static readonly PrefabGUID invisible = VCreate.Data.Buffs.Admin_Observe_Invisible_Buff;
    private static readonly PrefabGUID servant = VCreate.Data.Prefabs.CHAR_ChurchOfLight_Paladin_Servant;
    private static readonly PrefabGUID horse = VCreate.Data.Prefabs.CHAR_Mount_Horse;
    private static readonly PrefabGUID trader = VCreate.Data.Prefabs.CHAR_Trader_Farbane_Knowledge_T01;
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
                Plugin.Log.LogInfo("FollowerSystem Prefix: has buffer, setting helper positions");
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

                Plugin.Log.LogInfo("FollowerSystem Prefix: no buffer, following player");
                //entity.Write<LastTranslation>(new LastTranslation { Value = entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position });
                //entity.Write<Translation>(new Translation { Value = entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position });
                OffsetTranslationOnSpawn offsetTranslationOnSpawn = new OffsetTranslationOnSpawn { Offset = entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position };
                Utilities.AddComponentData(entity, offsetTranslationOnSpawn);
            }

        }
        entities.Dispose();
    }
}




