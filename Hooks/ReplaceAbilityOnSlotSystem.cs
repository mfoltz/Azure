using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using V.Augments.Rank;
using V.Core;
using V.Core.Commands;
using Plugin = V.Core.Plugin;

#nullable disable
// almost ready for live maybe

namespace RPGAddOnsEx.Hooks
{
    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
    public class ReplaceAbilityOnSlotSystem_Patch
    {
        private static void Prefix(ReplaceAbilityOnSlotSystem __instance)
        {

            try
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

                            // should be 3 items in the buffer for an iron+ weapon
                            // 2 items for copper
                            // 1 item for bone
                            //yeah iron is a fair unlock for this, that works, but need a check for bone weapons

                            DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                            if (buffer.Length == 3)
                            {
                                Plugin.Logger.LogInfo("Player equipping iron<= weapon, adding rank spell to shift...");
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
                                        PrefabGUID prefabGUID = new(data.RankSpell); //
                                        newItem.NewGroupId = prefabGUID;
                                        newItem.Slot = 3;
                                        // if this doesn't work change back
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
                                    // this should be unarmed based on the buffer length
                                    // or a bone weapon -_-

                                    // it could also be equipping a fishing pole, so we want to modify unarmed when unequipping the fishing pole specifically
                                    ReplaceAbilityOnSlotBuff item = buffer[0];
                                    ReplaceAbilityOnSlotBuff newItem = item;
                                    Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                                    User user = entityManager.GetComponentData<User>(userEntity);
                                    ulong steamID = user.PlatformId;
                                    if (entityManager.TryGetComponentData(entity, out WeaponLevel component))
                                    {
                                        PrefabGUID prefabGUID = new(-1016182556); //fishing pole
                                        if (buffer[0].NewGroupId == prefabGUID)
                                        {
                                            // fishing pole equipped
                                            // if I want to do it like this will need to keep track of player's last equipped weapon but only in the case
                                            // of a fishing pole being equipped which is right here, neat
                                            if (Databases.playerRanks.TryGetValue(steamID, out RankData rankData))
                                            {
                                                rankData.FishingPole = true;
                                                ChatCommands.SavePlayerRanks();
                                                return;
                                            }
                                            else
                                            {
                                                Plugin.Logger.LogInfo("Player rank not found.");
                                                return;
                                            }
                                        }
                                    }
                                    Plugin.Logger.LogInfo("Player unequipping weapon, modifying slots if fishing pole was equipped...");

                                    if (Databases.playerRanks.TryGetValue(steamID, out RankData data))
                                    {
                                        //Plugin.Logger.LogInfo($"Player rank: {data}");
                                        if (!data.FishingPole)
                                        {
                                            // if not true that means player is not unequipping a fishing rod, return
                                            Plugin.Logger.LogInfo("Player not unequipping fishing pole, returning...");
                                            return;
                                        }

                                        try
                                        {
                                            Plugin.Logger.LogInfo("3");
                                            PrefabGUID spell1 = new(data.Spells[0]);
                                            PrefabGUID spell2 = new(data.Spells[1]);
                                            newItem.Slot = 1;
                                            newItem.NewGroupId = spell1;
                                            buffer.Add(newItem);
                                            newItem.Slot = 4;
                                            newItem.NewGroupId = spell2;
                                            buffer.Add(newItem);
                                            Plugin.Logger.LogInfo("Modification complete.");
                                            data.FishingPole = false;
                                        }
                                        catch (System.Exception ex)
                                        {
                                            Plugin.Logger.LogInfo(ex.Message);
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
                            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                            User user = entityManager.GetComponentData<User>(userEntity);
                            ulong steamID = user.PlatformId;
                            // spell equip
                            // intercept this and keep record of last 2 spell choices a player has made to use in unarmed slots? and stick rank spell on shift
                            // activate this by unequipping fishingpole lol
                            DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                            Plugin.Logger.LogInfo("Spell change detected...");
                            //Plugin.Logger.LogInfo($"Buffer length: {buffer.Length}");

                            if (buffer[0].Slot == 5)
                            {
                                //Plugin.Logger.LogInfo("1");
                                // add to playerdata for unarmed spell
                                if (Databases.playerRanks.TryGetValue(steamID, out RankData data))
                                {
                                    if (data.Spells == null)
                                    {
                                        RankData newData = data;
                                        newData.Spells = [0, 0];
                                    }

                                    data.Spells[0] = buffer[0].NewGroupId.GuidHash;
                                    ChatCommands.SavePlayerRanks();
                                }
                                else
                                {
                                    Plugin.Logger.LogInfo("Player rank not found.");
                                }
                            }

                            if (buffer[0].Slot == 6)
                            {
                                //Plugin.Logger.LogInfo("2");
                                if (Databases.playerRanks.TryGetValue(steamID, out RankData data))
                                {
                                    if (data.Spells == null)
                                    {
                                        RankData newData = data;
                                        newData.Spells = [0, 0];
                                    }
                                    data.Spells[1] = buffer[0].NewGroupId.GuidHash;
                                    ChatCommands.SavePlayerRanks();
                                }
                                else
                                {
                                    Plugin.Logger.LogInfo("Player rank not found.");
                                }
                            }
                        }
                    }
                }
                entityArray.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogInfo(ex.Message);
            }
        }
    }
}