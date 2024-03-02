using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VPlus.Augments.Rank;
using VPlus.Core;
using VPlus.Core.Commands;
using Plugin = VPlus.Core.Plugin;
using VPlus.Core.Toolbox;
using ProjectM.UI;



// almost ready for live maybe
// wow, famoust last words huh ^
namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
    public class ReplaceAbilityOnSlotSystem_Patch
    {
        private static readonly PrefabGUID fishingPole = new(-1016182556); //as you might have guessed, this is -REDACTED-
        
        private static void Prefix(ReplaceAbilityOnSlotSystem __instance)
        {
            
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> entities = __instance.__Spawn_entityQuery.ToEntityArray(Allocator.Temp);
                Plugin.Logger.LogInfo("ReplaceAbilityOnSlotSystem Prefix called...");

                foreach (Entity entity in entities)
                {
                    ProcessEntity(entityManager, entity);
                }

                entities.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogInfo(ex.Message);
            }
        }

        private static void ProcessEntity(EntityManager entityManager, Entity entity)
        {
            Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
            if (!entityManager.HasComponent<PlayerCharacter>(owner)) return;

            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;

            if (entityManager.HasComponent<WeaponLevel>(entity))
            {
                HandleWeaponEquipOrUnequip(entityManager, entity, owner);
            }
            else
            {
                HandleSpellChange(entityManager, entity, owner);
            }
        }

        private static void HandleWeaponEquipOrUnequip(EntityManager entityManager, Entity entity, Entity owner)
        {
            DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
            int bufferLength = buffer.Length;
            User user = Utilities.GetComponentData<User>(entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity);
            ulong steamID = user.PlatformId;
            if (bufferLength == 1)
            {
                
                // unequip or equipping bone weapon here or fishing pole
                if (Databases.playerRanks.TryGetValue(steamID, out RankData rankData) && rankData.FishingPole)
                {
                    HandleFishingPole(entityManager, entity, owner, buffer);
                    return;
                }
                else if (buffer[0].NewGroupId == fishingPole)
                {
                    // fishing pole equipped
                    
                    if (Databases.playerRanks.TryGetValue(user.PlatformId, out RankData data))
                    {
                        data.FishingPole = true;
                        ChatCommands.SavePlayerRanks();
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Player rank not found for setting spells.");
                        return;
                    }
                }
                //HandleFishingPole(entityManager, entity, owner, buffer);
            }
            else if (bufferLength == 3)
            {
                // I think the buffer here refers to the abilities possessed by the weapon (primary auto, weapon skill 1, and weapon skill 2)
                EquipIronOrHigherWeapon(entityManager, entity, owner, buffer);
            }
            
        }

        private static void EquipIronOrHigherWeapon(EntityManager entityManager, Entity entity, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
        {
            Plugin.Logger.LogInfo("Player equipping iron<= weapon, adding rank spell to shift...");
            ReplaceAbilityOnSlotBuff newItem = buffer[2]; // Assuming iron or higher weapon adds to the third slot
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(steamID, out RankData data) && data.RankSpell != 0)
            {
                
                PrefabGUID prefabGUID = new PrefabGUID(data.RankSpell);
                newItem.NewGroupId = prefabGUID;
                
                newItem.Slot = 3; // Assuming slot 3 is where the rank spell should go
                buffer.Add(newItem);
                Plugin.Logger.LogInfo("Modification complete.");
            }
            else
            {
                Plugin.Logger.LogInfo("Player rank not found.");
            }
        }

        private static void HandleFishingPole(EntityManager entityManager, Entity entity, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
        {
            Plugin.Logger.LogInfo("Fishing pole unequipped, modifiying unarmed slots...");
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(steamID, out RankData rankData))
            {
                if (!rankData.FishingPole)
                {
                    Plugin.Logger.LogInfo("No adjustments needed, fishing pole not previously equipped.");
                    return;
                }
                else
                {
                    // Adjust abilities based on the player's spell choices
                    if (rankData.RankSpell != 0)
                    {
                        ReplaceAbilityOnSlotBuff item = buffer[0];
                        ReplaceAbilityOnSlotBuff newItem = item;

                        PrefabGUID firstSpellGUID = new PrefabGUID(rankData.Spells.First());
                        PrefabGUID secondSpellGUID = new PrefabGUID(rankData.Spells.Last());

                        newItem.NewGroupId = firstSpellGUID;
                        newItem.Slot = 1;
                        buffer.Add(newItem);

                        newItem.NewGroupId = secondSpellGUID;
                        newItem.Slot = 4;
                        buffer.Add(newItem);
                        rankData.FishingPole = false; // Reset the flag as the fishing pole is being unequipped
                        ChatCommands.SavePlayerRanks();
                        // Optionally, add more logic here for additional adjustments
                        Plugin.Logger.LogInfo("Abilities adjusted.");
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("No spells to adjust.");
                    }
                }
                return;
            }
        }

        private static void HandleSpellChange(EntityManager entityManager, Entity entity, Entity owner)
        {
            Plugin.Logger.LogInfo("Spell change detected...");
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;

            DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
            if (!(buffer.Length > 0))
            {
                Plugin.Logger.LogInfo("Buffer length not great than zero, skipping...");
                return;
            }
            int slot = buffer[0].Slot;

            
            // Assuming slot 5 and 6 are relevant for spell changes
            if (slot == 5 || slot == 6)
            {
                // Assume RankData is a class and Databases.playerRanks[steamID] directly returns a reference.
                RankData rankData = Databases.playerRanks.ContainsKey(steamID) ? Databases.playerRanks[steamID] : new RankData(0, 0, [], 0, [],"", false);
                
                // Initialize Spells if null
                rankData.Spells ??= [0, 0];

                // Update the specific spell slot
                rankData.Spells[slot == 5 ? 0 : 1] = buffer[0].NewGroupId.GuidHash;

                // Update or add the modified RankData back to the dictionary
                Databases.playerRanks[steamID] = rankData;

                Plugin.Logger.LogInfo($"Spell change recorded for slot {slot}.");
            }
            
        }
    }
}