using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using RPGAddOnsEx.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;
using WillisCore;
using Plugin = RPGAddOnsEx.Core.Plugin;

#nullable disable
/*

namespace RPGAddOnsEx.Hooks_WIP
{
    [HarmonyPatch(typeof(SyncAbilityGroupSlotBufferSystem), "OnUpdate")]
    public class SyncAbilityGroupSlotBufferSystem_Patch
    {
        private static void Prefix(SyncAbilityGroupSlotBufferSystem __instance)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entityArray = __instance.__SyncAbilityGroupSlotBuffer_entityQuery.ToEntityArray(Allocator.Temp);
            Plugin.Logger.LogInfo("SyncAbilityGroupSlotBufferSystem Prefix called...");
            foreach (Entity entity in entityArray)
            {
                Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!entityManager.HasComponent<PlayerCharacter>(owner))
                {
                    return;
                }
                entity.LogComponentTypes();
            }
        }
    }
}
*/