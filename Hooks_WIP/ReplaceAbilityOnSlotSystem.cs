using AdminCommands;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using ProjectM.Shared;
using RPGAddOnsEx.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;
using Plugin = RPGAddOnsEx.Core.Plugin;

#nullable disable
// almost ready for live maybe

namespace RPGAddOnsEx.Hooks_WIP
{
    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
    public class ReplaceAbilityOnSlotSystem_Patch
    {
        private static void Prefix(ReplaceAbilityOnSlotSystem __instance)
        {
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entityArray = __instance.__Spawn_entityQuery.ToEntityArray(Allocator.Temp);
            Plugin.Logger.LogInfo("ReplaceAbilityOnSlotSystem Prefix called...");
            foreach (Entity entity in entityArray)
            {
                Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!entityManager.HasComponent<PlayerCharacter>(owner))
                {
                    return;
                }
                else
                {
                    // if legendaryspellmodcomponent then weapon, if not then spell drag
                    if (entityManager.HasComponent<WeaponLevel>(entity))
                    {
                        Plugin.Logger.LogInfo("Player equipping weapon, attempting to replace ability in buffer...");
                        // should be 3 items in the buffer for a weapon
                        DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                        ReplaceAbilityOnSlotBuff item = buffer[2];
                        ReplaceAbilityOnSlotBuff newItem = item;
                        PrefabGUID prefabGUID = new(1438305657); // slashers camoflauge
                        newItem.NewGroupId = prefabGUID;
                        buffer[2] = newItem;
                        newItem.Slot = 3;
                        buffer.Add(newItem);
                        Plugin.Logger.LogInfo("Modification complete.");
                    }
                    else
                    {
                        // spell drag?
                        Plugin.Logger.LogInfo("Player equipping spell, attempting to replace ability in buffer...");
                        DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                        ReplaceAbilityOnSlotBuff item = buffer[0];
                        ReplaceAbilityOnSlotBuff newItem = item;
                        PrefabGUID prefabGUID = AdminCommands.Data.Prefabs.AB_ChurchOfLight_Paladin_SummonAngel_AbilityGroup; //
                        newItem.NewGroupId = prefabGUID;
                        Plugin.Logger.LogInfo(item.Slot.ToString());
                        buffer[0] = newItem;
                        //newItem.Slot = 3;
                        //buffer.Add(newItem);
                        Plugin.Logger.LogInfo("Modification complete.");
                    }
                }
            }
            entityArray.Dispose();
        }
    }
}