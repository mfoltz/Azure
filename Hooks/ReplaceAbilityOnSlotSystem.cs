using AdminCommands;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using ProjectM.Shared;
using RPGAddOnsEx.Augments.RankUp;
using RPGAddOnsEx.Core;
using Steamworks;
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
                        // this also fires when unequipping a weapon, need to check for that
                        // use buffer length to determine appropriate modification
                        // if buffer length is 3 then it's a weapon equip, if buffer length is 1 then it's a weapon unequip

                        // should be 3 items in the buffer for a weapon

                        DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                        if (buffer.Length == 3)
                        {
                            Plugin.Logger.LogInfo("Player equipping weapon, adding rank spell to shift...");
                            ReplaceAbilityOnSlotBuff item = buffer[2];
                            ReplaceAbilityOnSlotBuff newItem = item;
                            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                            User user = entityManager.GetComponentData<User>(userEntity);
                            ulong steamID = user.PlatformId;
                            if (Databases.playerRanks.TryGetValue(steamID, out RankData data))
                            {
                                //Plugin.Logger.LogInfo($"Player rank: {data}");

                                if (data.RankSpell == 0)
                                {
                                    // no rank spell
                                    return;
                                }
                                else
                                {
                                    Plugin.Logger.LogInfo("Adding rank spell to shift...");
                                    PrefabGUID prefabGUID = new(data.RankSpell); //
                                    newItem.NewGroupId = prefabGUID;
                                    newItem.Slot = 3;
                                    buffer.Add(newItem);
                                    Plugin.Logger.LogInfo("Modification complete.");
                                    return;
                                }
                            }
                            else
                            {
                                Plugin.Logger.LogInfo("Player rank not found.");
                            }
                        }
                        else
                        {
                            if (buffer.Length == 1)
                            {
                                Plugin.Logger.LogInfo("Player unequipping weapon, replacing unarmed melee attack...");
                                ReplaceAbilityOnSlotBuff item = buffer[0];
                                ReplaceAbilityOnSlotBuff newItem = item;
                                Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                                User user = entityManager.GetComponentData<User>(userEntity);
                                ulong steamID = user.PlatformId;
                                if (Databases.playerRanks.TryGetValue(steamID, out RankData data))
                                {
                                    //Plugin.Logger.LogInfo($"Player rank: {data}");
                                    if (buffer.Length == 1)
                                    {
                                        // this should be unarmed
                                        if (data.RankSpell == 0)
                                        {
                                            // no rank spell
                                            return;
                                        }
                                        else
                                        {
                                            PrefabGUID prefabGUID = new(data.RankSpell); //
                                            newItem.NewGroupId = prefabGUID;
                                            buffer[0] = newItem;
                                            newItem.Slot = 1;
                                            newItem.NewGroupId = new(-358319417);
                                            buffer.Add(newItem);
                                            prefabGUID = new(-2053450457);
                                            // copy spells equipped to unarmed bar, then player could swap other skills?
                                            //
                                            newItem.NewGroupId = prefabGUID;
                                            newItem.Slot = 4;
                                            buffer.Add(newItem);
                                            //newItem.Slot = 4;
                                            //buffer.Add(newItem);
                                            Plugin.Logger.LogInfo("Modification complete.");
                                        }
                                    }
                                }
                                else
                                {
                                    Plugin.Logger.LogInfo("Player rank not found.");
                                }
                            }
                        }
                    }
                    else
                    {
                        // spell drag
                        // intercept this and keep record of last 2 spell choices a player has made that they to use in unarmed slots?
                        DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                        Plugin.Logger.LogInfo("Spell change detected..."); // try to check for ultimate being equipped? use that as trigger for this
                        Plugin.Logger.LogInfo($"Buffer length: {buffer.Length}");
                        //Plugin.Logger.LogInfo($"Slot: {buffer[0].Slot}");
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            Plugin.Logger.LogInfo($"Slot: {buffer[i].Slot}");
                        }
                        entity.LogComponentTypes();
                        //ReplaceAbilityOnSlotBuff item = buffer[0];
                        //ReplaceAbilityOnSlotBuff newItem = item;
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        ulong steamID = user.PlatformId;

                        return;
                    }
                }
            }
            entityArray.Dispose();
        }
    }
}