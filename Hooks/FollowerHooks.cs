using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Behaviours;
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

[HarmonyPatch(typeof(BehaviourTreeStateChangedEventSystem), nameof(BehaviourTreeStateChangedEventSystem.OnUpdate))]
public static class BehaviourTreeStateChangedEventSystemPatch
{
    public static void Prefix(BehaviourTreeStateChangedEventSystem __instance)
    {
        
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        for (int i = 0; i < entities.Length; i++)
        {
            Entity entity = entities[i];
            //entity.LogComponentTypes();
            if (!entity.Read<Follower>().Followed._Value.Has<PlayerCharacter>()) continue;
            
            if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value==GenericEnemyState.Return)
            {
                //Plugin.Log.LogInfo($"{entity.Read<BehaviourTreeState>().Value.ToString()}");
                BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                behaviourTreeStateChangedEvent.Value = GenericEnemyState.Follow;
                entity.Write(behaviourTreeStateChangedEvent);
            }
            else if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Follow)
            {
                var distance = UnityEngine.Vector3.Distance(entity.Read<LocalToWorld>().Position, entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position);
                if (distance < 3f)
                {
                    BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                    behaviourTreeStateChangedEvent.Value = GenericEnemyState.Idle;
                    entity.Write(behaviourTreeStateChangedEvent);
                }
            }
        }
        entities.Dispose();
    }
}



[HarmonyPatch(typeof(FollowerSystem), nameof(FollowerSystem.OnUpdate))]
public static class FollowerSystemPatch
{
    // proxies for pets
    private static readonly PrefabGUID invulnerable = VCreate.Data.Buffs.Admin_Invulnerable_Buff;
    //private static readonly PrefabGUID invisible = VCreate.Data.Buffs.Admin_Observe_Invisible_Buff;
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
            if (!Utilities.HasComponent<Follower>(entity)) continue; // if entity is not following someone skip, not sure why those pop up in this query
            if (entityManager.TryGetBuffer<FollowerBuffer>(entity, out var followers))
            {
                //Plugin.Log.LogInfo("FollowerSystem Prefix: has buffer, setting helper positions");
                //GetOwnerTranslationOnSpawn getOwnerTranslationOnSpawn = new GetOwnerTranslationOnSpawn { SnapToGround = true, TranslationSource = GetOwnerTranslationOnSpawnComponent.GetTranslationSource.Owner };
                NativeArray<FollowerBuffer> followerEntities = followers.ToNativeArray(Unity.Collections.Allocator.Temp);
                for (int i = 0; i < followerEntities.Length; i++)
                {
                    followers[i].Entity._Entity.Write<LastTranslation>(new LastTranslation { Value = entity.Read<LastTranslation>().Value });
                    followers[i].Entity._Entity.Write<Translation>(new Translation { Value = entity.Read<Translation>().Value });
                }
                followerEntities.Dispose();
            }
            

        }
        entities.Dispose();
    }
}




