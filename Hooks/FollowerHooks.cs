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

        try
        {
            foreach (var entity in entities)
            {
                // entity.LogComponentTypes();
                if (!entity.Has<Follower>()) continue;
                if (!entity.Read<Follower>().Followed._Value.Has<PlayerCharacter>() || !entity.Has<BehaviourTreeState>()) continue;

                if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Return)
                {
                    // Plugin.Log.LogInfo($"{entity.Read<BehaviourTreeState>().Value.ToString()}");
                    BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                    behaviourTreeStateChangedEvent.Value = GenericEnemyState.Follow;
                    entity.Write(behaviourTreeStateChangedEvent);
                }
                else if (Utilities.HasComponent<BehaviourTreeState>(entity) && entity.Read<BehaviourTreeState>().Value == GenericEnemyState.Follow)
                {
                    var distance = UnityEngine.Vector3.Distance(entity.Read<LocalToWorld>().Position, entity.Read<Follower>().Followed._Value.Read<LocalToWorld>().Position);
                    if (distance < 5f)
                    {
                        BehaviourTreeState behaviourTreeStateChangedEvent = entity.Read<BehaviourTreeState>();
                        behaviourTreeStateChangedEvent.Value = GenericEnemyState.Idle;
                        entity.Write(behaviourTreeStateChangedEvent);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogInfo($"Exited BehaviorTreeState hook early {e}");
        }
        finally
        {
            // Dispose of the NativeArray properly in the finally block to ensure it's always executed.
            entities.Dispose();
        }
    }
}



