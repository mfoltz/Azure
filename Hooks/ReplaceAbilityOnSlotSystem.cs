using AdminCommands;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using ProjectM.Shared;
using RPGAddOnsEx.Augments.RankUp;
using RPGAddOnsEx.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;
using Plugin = RPGAddOnsEx.Core.Plugin;

#nullable disable
// almost ready for live maybe

namespace RPGAddOnsEx.Hooks
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
                    if (entityManager.HasComponent<WeaponLevel>(entity))
                    {
                        Plugin.Logger.LogInfo("Player equipping weapon, adding rank spell to shift...");
                        // should be 3 items in the buffer for a weapon
                        DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                        ReplaceAbilityOnSlotBuff item = buffer[2];
                        ReplaceAbilityOnSlotBuff newItem = item;
                        // reaper check
                        // also need an unarmed check to verify unequip vs equip
                        PrefabGUID reaperMeleeAttack = new(784360484);
                        Plugin.Logger.LogInfo($"ReplaceGroupId: {buffer[0].ReplaceGroupId.GuidHash.ToString()}");
                        if (buffer[0].ReplaceGroupId == reaperMeleeAttack)
                        {
                            Plugin.Logger.LogInfo("Reaper equipped, replacing abilities...");
                            ReplaceAbilityOnSlotBuff primaryWeaponSkill = buffer[1];
                            ReplaceAbilityOnSlotBuff newPrimaryWeaponSkill = primaryWeaponSkill;
                            newPrimaryWeaponSkill.NewGroupId = new PrefabGUID(1952822626);
                            buffer[1] = newPrimaryWeaponSkill;
                            ReplaceAbilityOnSlotBuff secondaryWeaponSkill = buffer[2];
                            ReplaceAbilityOnSlotBuff newSecondaryWeaponSkill = secondaryWeaponSkill;
                            newSecondaryWeaponSkill.NewGroupId = new PrefabGUID(-135509259);
                            buffer[2] = newSecondaryWeaponSkill;
                        }
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        ulong steamID = user.PlatformId;
                        if (DataStructures.playerRanks.TryGetValue(steamID, out RankData data))
                        {
                            Plugin.Logger.LogInfo($"Player rank: {data}");
                            if (data.RankSpell == 0)
                            {
                                // no rank spell
                                return;
                            }
                            else
                            {
                                Plugin.Logger.LogInfo("Adding chosen rank spell to shift...");
                                PrefabGUID prefabGUID = new(data.RankSpell); //
                                newItem.NewGroupId = prefabGUID;
                                newItem.Slot = 3;
                                buffer.Add(newItem);
                                Plugin.Logger.LogInfo("Modification complete.");
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Player rank not found.");
                        }
                    }
                    /*
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
                    */
                }
            }
            entityArray.Dispose();
        }
    }
}