using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Steamworks;
using Unity.Collections;
using Unity.Entities;
using VPlus.Augments.Rank;
using VPlus.Core;
using VPlus.Core.Commands;
using VBuild.BuildingSystem;
using VPlus.Augments.Rank;
using VPlus.Core;
using VPlus.Core.Commands;
using Plugin = VPlus.Core.Plugin;
using VPlus.Core.Toolbox;

#nullable disable

// almost ready for live maybe
// wow, famoust last words huh ^
namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
    public class ReplaceAbilityOnSlotSystem_Patch
    {
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

            if (bufferLength == 3)
            {
                // I think the buffer here refers to the abilities possessed by the weapon (primary auto, weapon skill 1, and weapon skill 2)
                EquipIronOrHigherWeapon(entityManager, entity, owner, buffer);
            }
            else if (bufferLength == 1)
            {
                // unequip or equipping bone weapon here

                PrefabGUID prefabGUID = new(-1016182556); //fishing pole
                if (buffer[0].NewGroupId == prefabGUID)
                {
                    // fishing pole equipped
                    User user = Utilities.GetComponentData<User>(entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity);
                    if (Databases.playerRanks.TryGetValue(user.PlatformId, out RankData rankData))
                    {
                        rankData.FishingPole = true;
                        ChatCommands.SavePlayerRanks();
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Player rank not found.");
                    }
                }
                HandleFishingPole(entityManager, entity, owner, buffer);
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
            Plugin.Logger.LogInfo("Player unequipping weapon, modifying slots if fishing pole was equipped...");
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
            int slot = buffer[0].Slot;

            if (Databases.playerRanks.TryGetValue(steamID, out RankData data))
            {
                data.Spells ??= [];

                // Assuming slot 5 and 6 are relevant for spell changes
                if (slot == 5 || slot == 6)
                {
                    data.Spells[slot == 5 ? 0 : 1] = buffer[0].NewGroupId.GuidHash;
                    ChatCommands.SavePlayerRanks();
                    Plugin.Logger.LogInfo($"Spell change for slot {slot} recorded.");
                }
            }
            else
            {
                Plugin.Logger.LogInfo("Player rank not found.");
            }
        }
    }
}