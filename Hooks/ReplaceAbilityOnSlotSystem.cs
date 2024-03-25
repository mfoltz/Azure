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
using VPlus.Data;
using VCreate.Core.Toolbox;
using Bloodstone.API;
using Utilities = VPlus.Core.Toolbox.Utilities;
using VPlus.Augments;
using VRising.GameData.Models;
using VRising.GameData.Methods;

// almost ready for live maybe
// wow, famoust last words huh ^
namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
    public class ReplaceAbilityOnSlotSystem_Patch
    {
        public static readonly Dictionary<PrefabGUID, int> keyValuePairs = new()
            {
                { new(862477668), 2500 },
                { new(-1531666018), 2500 },
                { new(-1593377811), 2500 },
                { new(429052660), 25 },
                { new(28625845), 200 }
            };

        private static readonly PrefabGUID fishingPole = new(-1016182556); //as you might have guessed, this is -REDACTED-

        private static void Prefix(ReplaceAbilityOnSlotSystem __instance)
        {
            NativeArray<Entity> entities = __instance.__Spawn_entityQuery.ToEntityArray(Allocator.Temp);
            try
            {
                EntityManager entityManager = __instance.EntityManager;

                //Plugin.Logger.LogInfo("ReplaceAbilityOnSlotSystem Prefix called...");

                foreach (Entity entity in entities)
                {
                    ProcessEntity(entityManager, entity);
                }

                entities.Dispose();
            }
            catch (System.Exception ex)
            {
                entities.Dispose();
                Plugin.Logger.LogInfo(ex.Message);
            }
        }

        private static void ProcessEntity(EntityManager entityManager, Entity entity)
        {
            Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
            if (!entityManager.HasComponent<PlayerCharacter>(owner)) return;
            ulong steamdId = owner.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            //ulong steamID = user.PlatformId;
            if (Databases.playerDivinity.TryGetValue(steamdId, out DivineData data) && !data.Spawned)
            {
                data.Spawned = true;
                Databases.playerDivinity[steamdId] = data;
                ChatCommands.SavePlayerDivinity();
                UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(user.PlatformId);
                foreach (var item in keyValuePairs.Keys)
                {
                    userModel.TryGiveItem(item, keyValuePairs[item], out var _);
                }
                ServerChatUtils.SendSystemMessageToClient(entityManager, user, "You've received a starting kit with blood essence, stone, wood, coins, and health potions!");
            }
            else if (entityManager.HasComponent<WeaponLevel>(entity))
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
                // if necro want to return here

                EquipIronOrHigherWeapon(entityManager, entity, owner, buffer);
            }
        }

        private static void EquipIronOrHigherWeapon(EntityManager entityManager, Entity _, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
        {
            Plugin.Logger.LogInfo("Player equipping iron<= weapon, adding rank spell to shift if not necrodagger...");
            if (buffer[0].NewGroupId.GuidHash == VCreate.Data.Prefabs.AB_NecromancyDagger_Primary_AbilityGroup.GuidHash) return; //necro already OP, no shift spell for necro
            ReplaceAbilityOnSlotBuff newItem = buffer[2]; // shift slot
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);

            if (Databases.playerRanks.TryGetValue(user.PlatformId, out RankData data) && data.RankSpell != 0)
            {
                PrefabGUID prefabGUID = new PrefabGUID(data.RankSpell);
                newItem.NewGroupId = prefabGUID;

                newItem.Slot = 3; // Assuming slot 3 is where the rank spell should go
                buffer.Add(newItem);

                Plugin.Logger.LogInfo("Ability added, attempting to modify cooldown...");
                try
                {
                    Entity abilityEntity = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID];
                    //if (!abilityEntity.Has<AbilityGroupStartAbilitiesBuffer>() || abilityEntity.ReadBuffer<AbilityGroupStartAbilitiesBuffer>().Length == 0) return;

                    AbilityGroupStartAbilitiesBuffer bufferItem = abilityEntity.ReadBuffer<AbilityGroupStartAbilitiesBuffer>()[0];
                    Entity castEntity = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[bufferItem.PrefabGUID];
                    AbilityCooldownData abilityCooldownData = castEntity.Read<AbilityCooldownData>();
                    AbilityCooldownState abilityCooldownState = castEntity.Read<AbilityCooldownState>();
                    abilityCooldownState.CurrentCooldown = 30f; // Reset the last used time
                    castEntity.Write(abilityCooldownState);

                    abilityCooldownData.Cooldown._Value = 30f; // Set the cooldown to 30 seconds
                    castEntity.Write(abilityCooldownData);
                    Plugin.Logger.LogInfo("Cooldown modified.");
                    // need to get the ability cast entity to modify the cooldown, so first get the cast for an ability group somehow
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.LogInfo("Error setting cooldown." + ex.Message);
                }
            }
            else
            {
                Plugin.Logger.LogInfo("Player rank not found.");
            }
        }

        private static void HandleFishingPole(EntityManager entityManager, Entity _, Entity owner, DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer)
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

                        if (rankData.Rank < 1)// first and second slot locked until rank 1 and 3 for now
                        {
                            rankData.FishingPole = false;
                            ChatCommands.SavePlayerRanks();
                            return;
                        }
                        newItem.NewGroupId = firstSpellGUID;
                        newItem.Slot = 1;
                        buffer.Add(newItem);

                        if (rankData.Rank < 3)
                        {
                            rankData.FishingPole = false;
                            ChatCommands.SavePlayerRanks();
                            return;
                        }
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
            //Plugin.Logger.LogInfo("Spell change detected...");
            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
            User user = entityManager.GetComponentData<User>(userEntity);
            ulong steamID = user.PlatformId;

            DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
            int slotIndex = buffer[0].Slot == 5 ? 0 : buffer[0].Slot == 6 ? 1 : -1; // Determine if we're dealing with slot 5 or 6, otherwise set to -1

            if (slotIndex != -1) // Proceed only if it's slot 5 or 6
            {
                if (Databases.playerRanks.TryGetValue(steamID, out RankData data))
                {
                    // Ensure Spells list is initialized and has at least 2 elements to accommodate both slots.
                    if (data.Spells == null)
                    {
                        data.Spells = new List<int> { 0, 0 }; // Initialize with two default values
                    }
                    else if (data.Spells.Count < 2)
                    {
                        // Ensure there are two slots available, padding with 0 if necessary
                        while (data.Spells.Count < 2)
                        {
                            data.Spells.Add(0);
                        }
                    }

                    // Now safely assign value to the corresponding slot
                    data.Spells[slotIndex] = buffer[0].NewGroupId.GuidHash;
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