using Bloodstone.API;
using HarmonyLib;
using MS.Internal.Xml.XPath;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using static Il2CppSystem.Data.Common.ObjectStorage;
using static VCreate.Hooks.PetSystem.PetFocusSystem;

namespace VCreate.Hooks
{
    internal class PetSystem
    {
        [HarmonyPatch]
        public class DeathEventListenerSystem_PetPatch
        {
            [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
            [HarmonyPostfix]
            public static void Postfix(DeathEventListenerSystem __instance)
            {
                NativeArray<Entity> entities = __instance._DeathEventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                try
                {
                    // faction token drops for obtaining pets?
                    foreach (Entity entity in entities)
                    {
                        if (!entity.Has<DeathEvent>()) continue; // not sure why something in this query wouldn't have this but better safe than sorry

                        DeathEvent deathEvent = entity.Read<DeathEvent>();

                        Entity killer = deathEvent.Killer; // want to check follower buffer if player character
                        Entity died = deathEvent.Died;
                        if (!died.Has<UnitLevel>()) continue; // if no level, continue
                        if (killer.Has<PlayerCharacter>()) DeathEventHandlers.HandlePlayerKill(killer, died);  // if player, handle token drop
                        else if (killer.Has<Follower>())
                        {
                            if (killer.Read<Follower>().Followed._Value.Has<PlayerCharacter>()) DeathEventHandlers.HandlePetKill(killer, died); // if pet, handle pet experience
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
                finally
                {
                    entities.Dispose();
                }
            }
        }

        public class DeathEventHandlers
        {
            public static void HandlePlayerKill(Entity killer, Entity died)
            {
                UnitTokenSystem.HandleTokenDrop(killer, died);
                UpdatePetExperiencePlayerKill(killer, died);
            }

            public static void HandlePetKill(Entity killer, Entity died)
            {
                UpdatePetExperiencePetKill(killer, died);
            }

            public static void UpdatePetExperiencePlayerKill(Entity killer, Entity died)
            {
                if (!killer.Has<FollowerBuffer>()) return; // if doesn't have a follower buffer, return

                var followers = killer.ReadBuffer<FollowerBuffer>();

                var enumerator = followers.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    // also shinies
                    Entity follower = enumerator.Current.Entity._Entity;
                    if (follower.Read<Team>().Value.Equals(killer.Read<Team>().Value) && DataStructures.PlayerPetsMap.TryGetValue(killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var profiles))
                    {
                        if (!profiles.TryGetValue(follower.Read<PrefabGUID>().LookupName().ToString(), out var profile) || !profile.Combat) continue;
                        UnitLevel unitLevel = died.Read<UnitLevel>();
                        float baseExp = Math.Max(unitLevel.Level - profile.Level, 1);

                        profile.CurrentExperience += (int)baseExp;

                        double toNext = 1.25 * Math.Pow(profile.Level, 3);

                        if (profile.CurrentExperience >= toNext && profile.Level < 80)
                        {
                            profile.CurrentExperience = 0;
                            profile.Level++;

                            Plugin.Log.LogInfo("Pet level up! Setting level and saving stats...");
                            follower.Write<UnitLevel>(new UnitLevel { Level = profile.Level });
                            UnitStatSet(follower); // need to review

                            UnitStats unitStats = follower.Read<UnitStats>();
                            Health health = follower.Read<Health>();
                            float maxhealth = health.MaxHealth._Value;
                            float attackspeed = unitStats.AttackSpeed._Value;
                            float primaryattackspeed = unitStats.PrimaryAttackSpeed._Value;
                            float physicalpower = unitStats.PhysicalPower._Value;
                            float spellpower = unitStats.SpellPower._Value;
                            profile.Stats.Clear();
                            profile.Stats.AddRange([maxhealth, attackspeed, primaryattackspeed, physicalpower, spellpower]);
                            profiles[follower.Read<PrefabGUID>().LookupName().ToString()] = profile;
                            DataStructures.PlayerPetsMap[killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = profiles;
                            DataStructures.SavePetExperience();
                            break;
                        }
                        else
                        {
                            Plugin.Log.LogInfo("Giving pet experience...");

                            profiles[follower.Read<PrefabGUID>().LookupName().ToString()] = profile;
                            DataStructures.PlayerPetsMap[killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = profiles;
                            DataStructures.SavePetExperience();
                            break;
                        }
                    }
                }
            }

            public static void UpdatePetExperiencePetKill(Entity killer, Entity died)
            {
                // also shinies
                Entity pet = killer;
                Entity entity = killer.Read<Follower>().Followed._Value; // player
                ulong platformId = entity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var profiles))
                {
                    if (!profiles.TryGetValue(pet.Read<PrefabGUID>().LookupName().ToString(), out var profile)) return;
                    if (!profile.Combat) return;
                    UnitLevel unitLevel = died.Read<UnitLevel>();
                    float baseExp = Math.Max(unitLevel.Level - profile.Level, 1);
                    // fix stat permanencesa
                    profile.CurrentExperience += (int)baseExp;

                    double toNext = 1.25 * Math.Pow(profile.Level, 3);

                    if (profile.CurrentExperience >= toNext && profile.Level < 80)
                    {
                        profile.CurrentExperience = 0;
                        profile.Level++;

                        Plugin.Log.LogInfo("Pet level up! Setting level and saving stats...");
                        pet.Write<UnitLevel>(new UnitLevel { Level = profile.Level });
                        

                        //UnitStats unitStats = pet.Read<UnitStats>();
                        UnitStatSet(pet);

                        UnitStats unitStats = pet.Read<UnitStats>();
                        Health health = pet.Read<Health>();
                        float maxhealth = health.MaxHealth._Value;
                        float attackspeed = unitStats.AttackSpeed._Value;
                        float primaryattackspeed = unitStats.PrimaryAttackSpeed._Value;
                        float physicalpower = unitStats.PhysicalPower._Value;
                        float spellpower = unitStats.SpellPower._Value;
                        profile.Stats.Clear();
                        profile.Stats.AddRange([maxhealth, attackspeed, primaryattackspeed, physicalpower, spellpower]);
                        profiles[pet.Read<PrefabGUID>().LookupName().ToString()] = profile;
                        DataStructures.PlayerPetsMap[platformId] = profiles;
                        DataStructures.SavePetExperience();
                    }
                    else
                    {
                        Plugin.Log.LogInfo("Giving pet experience...");
                        profiles[pet.Read<PrefabGUID>().LookupName().ToString()] = profile;
                        DataStructures.PlayerPetsMap[platformId] = profiles;
                        DataStructures.SavePetExperience();
                    }
                }
            }

            public static void UnitStatSet(Entity entity)
            {
                // Assuming entity.Read<UnitStats>() is a way to access the UnitStats component of the entity.
                UnitStats unitStats = entity.Read<UnitStats>();
                Health health = entity.Read<Health>();
                // Generate a random index to select a stat.
                //Random random = new Random();
                int randomIndex = UnitTokenSystem.Random.Next(FocusToStatMap.FocusStatMap.Count);
                FocusToStatMap.StatType selectedStat = FocusToStatMap.FocusStatMap[randomIndex];
                ulong playerId = entity.Read<Follower>().Followed._Value.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                if (DataStructures.PlayerPetsMap.TryGetValue(playerId, out var profiles))
                {
                    if (profiles.TryGetValue(entity.Read<PrefabGUID>().LookupName().ToString(), out var profile))
                    {
                        FocusToStatMap.StatType otherStat = FocusToStatMap.FocusStatMap[profile.Focus];
                        switch (otherStat)
                        {
                            case FocusToStatMap.StatType.MaxHealth:
                                health.MaxHealth._Value += health.MaxHealth._Value*0.05f;
                                break;
                            case FocusToStatMap.StatType.AttackSpeed:
                                unitStats.AttackSpeed._Value += 0.01f;
                                break;
                            case FocusToStatMap.StatType.PrimaryAttackSpeed:
                                unitStats.PrimaryAttackSpeed.Value += 0.02f;
                                break;
                            case FocusToStatMap.StatType.PhysicalPower:
                                unitStats.PhysicalPower._Value += 2.5f;
                                break;
                            case FocusToStatMap.StatType.SpellPower:
                                unitStats.SpellPower._Value += 2.5f;
                                break;
                        }
                    }
                }
                switch (selectedStat)
                {
                    case FocusToStatMap.StatType.MaxHealth:
                        health.MaxHealth._Value += health.MaxHealth._Value*0.05f;
                        break;
                
                    case FocusToStatMap.StatType.AttackSpeed:
                        unitStats.AttackSpeed._Value += 0.01f;
                        break;
                    case FocusToStatMap.StatType.PrimaryAttackSpeed:
                        unitStats.PrimaryAttackSpeed.Value += 0.02f;
                        break;
                    case FocusToStatMap.StatType.PhysicalPower:
                        unitStats.PhysicalPower._Value += 1f;
                        break;
                    case FocusToStatMap.StatType.SpellPower:
                        unitStats.SpellPower._Value += 1f;
                        break;
                }

                // Assuming entity.Write<UnitStats>(unitStats) is a way to save the modified UnitStats back to the entity.
                entity.Write(health);
                entity.Write(unitStats);
            }

        }

        public class UnitTokenSystem
        {
            private static readonly float chance = 0.01f; // testing
            public static readonly Random Random = new();

            public class UnitToGemMapping
            {
                public enum UnitType
                {
                    Human,
                    Undead,
                    Demon,
                    Mechanical,
                    Beast,
                    VBlood
                }

                public static readonly Dictionary<UnitType, int> UnitCategoryToGemPrefab = new()
                {
                    // all siege stones because perfect gems hate me and refuse to not stack
                    { UnitType.Human, 2076358520 }, // Item_Ingredient_Gem_Sapphire_T04
                    { UnitType.Undead, 2076358520 }, // Item_Ingredient_Gem_Emerald_T04
                    { UnitType.Demon, 2076358520 }, // Item_Ingredient_Gem_Miststone_T04
                    { UnitType.Mechanical, 2076358520 }, // Item_Ingredient_Gem_Topaz_T04
                    { UnitType.Beast, 2076358520 }, // Item_Ingredient_Gem_Amethyst_T04
                    { UnitType.VBlood, 2076358520 } // Item_Ingredient_Gem_Ruby_T04
                };
            }

            public static void HandleTokenDrop(Entity killer, Entity died)
            {
                // get died category
                Plugin.Log.LogInfo("Handling token drop...");
                PrefabGUID stone;
                EntityCategory diedCategory = died.Read<EntityCategory>();
                if (died.Read<PrefabGUID>().GuidHash.Equals(VCreate.Data.Prefabs.CHAR_Mount_Horse.GuidHash)) return;
                
                    
                if ((int)diedCategory.UnitCategory < 5 && !died.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    stone = new(UnitToGemMapping.UnitCategoryToGemPrefab[(UnitToGemMapping.UnitType)diedCategory.UnitCategory]);
                    HandleRoll(stone, 1, died, killer);
                }
                else if (died.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    stone = new(UnitToGemMapping.UnitCategoryToGemPrefab[UnitToGemMapping.UnitType.VBlood]);
                    HandleRoll(stone, 1, died, killer);
                }
                else
                {
                    Plugin.Log.LogInfo("No token drop for this unit.");
                    return;
                }
            }

            public static void HandleRoll(PrefabGUID stone, float dropChance, Entity died, Entity killer)
            {
                try
                {
                    if (RollForChance(stone, dropChance, died))
                    {
                        //want to give player the item here
                        UserModel userModel = VRising.GameData.GameData.Users.GetUserByCharacterName(killer.Read<PlayerCharacter>().Name.ToString());
                        if (Helper.AddItemToInventory(userModel.FromCharacter.Character, stone, 1, out Entity _, false))
                        {
                            //Plugin.Log.LogInfo("Item entity is null... (going once)");
                            var items = userModel.Inventory.Items;
                            foreach (var item in items)
                            {
                                if (item.Item.PrefabGUID.Equals(stone))
                                {
                                    //Plugin.Log.LogInfo("Trying item modification...");
                                    Entity itemEntity = item.Item.Entity;

                                    //ItemData itemData = itemEntity.Read<ItemData>();
                                    //if (itemData.ItemTypeGUID.LookupName().ToLower().Contains("char")) continue; //skip if stone has been modified already
                                    //itemData.ItemTypeGUID = died.Read<PrefabGUID>();
                                    CastAbilityOnConsume castAbilityOnConsume = itemEntity.Read<CastAbilityOnConsume>();
                                    if (!castAbilityOnConsume.AbilityGuid.GuidHash.Equals(0)) continue; //skip if stone has been modified already
                                    castAbilityOnConsume.AbilityGuid = died.Read<PrefabGUID>();
                                    itemEntity.Write(castAbilityOnConsume);
                                    //itemEntity.Write(itemData);
                                    Plugin.Log.LogInfo($"Created soul gem for {castAbilityOnConsume.AbilityGuid.LookupName()}");
                                }
                            }

                            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, killer.Read<PlayerCharacter>().UserEntity.Read<User>(), "Your bag feels slightly heavier...");
                        }

                        // maybe it immediately gets synced and then destroyed after syncing?
                        //NetworkedEntityUtil.TryFindEntity()

                        //CastAbilityOnConsume castAbilityOnConsume = new CastAbilityOnConsume { AbilityGuid = VCreate.Data.Prefabs.AB_ChurchOfLight_Paladin_SummonAngel_Cast };
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogInfo(e.Message);
                }
            }

            public static bool RollForChance(PrefabGUID gem, float chance, Entity died)
            {
                //Random random = new();
                float roll = (float)Random.NextDouble(); // Generates a random number between 0.0 and 1.0

                if (roll < chance)
                {
                    Plugin.Log.LogInfo($"Roll for {gem.LookupName()} from {died.Read<PrefabGUID>().LookupName()} was successful");
                    return true; // The roll is successful, within the chance
                }
                else
                {
                    Plugin.Log.LogInfo($"Roll for {gem.LookupName()} from {died.Read<PrefabGUID>().LookupName()} was unsuccessful");
                    return false; // The roll is not successful
                }
            }
        }

        public class SoulStoneSystem
        {
            private static readonly List<PrefabGUID> perfectGems =
            [
                new PrefabGUID(2076358520), // Item_Building_SiegeStone_T01
            ];

            public static void ModifySiegeStone()
            {
                EntityManager entityManager = VWorld.Server.EntityManager;

                foreach (PrefabGUID prefabGUID in perfectGems)
                {
                    Entity stoneEntity = Utilities.GetPrefabEntityByPrefabGUID(prefabGUID, entityManager);
                    
                    if (stoneEntity != Entity.Null)
                    {
                        var itemData = Utilities.GetComponentData<ItemData>(stoneEntity);
                        itemData.ItemCategory = ItemCategory.BloodBound;
                        //itemData.ItemType = ItemType.Equippable;
                        //itemData.MaxAmount = 1;
                        //Consumable consumable = new();
                        //var conditionBuffer = entityManager.AddBuffer<ConsumableCondition>(gemEntity);
                        //var buffer = entityManager.GetBuffer<ConsumableCondition>(test_consumable);
                        //foreach (var item in buffer)
                        //{
                        //   conditionBuffer.Add(item);
                        //}
                        CastAbilityOnConsume castAbilityOnConsume = new() { AbilityGuid = new(0) };
                        Utilities.AddComponentData(stoneEntity, castAbilityOnConsume);
                        Utilities.SetComponentData(stoneEntity, itemData);
                        Plugin.Log.LogInfo($"Modified {prefabGUID.LookupName()}");
                    }
                    else
                    {
                        Plugin.Log.LogWarning($"Could not find prefab entity for GUID: {prefabGUID.GuidHash}");
                    }
                }
            }
        }

        public class PetFocusSystem
        {
            public class FocusToStatMap
            {
                public enum StatType
                {
                    MaxHealth,
                    AttackSpeed,
                    PrimaryAttackSpeed,
                    PhysicalPower,
                    SpellPower
                }

                public static readonly Dictionary<int, StatType> FocusStatMap = new()
                {
                    { 0, StatType.MaxHealth },
                    { 1, StatType.AttackSpeed },
                    { 2, StatType.PrimaryAttackSpeed },
                    { 3, StatType.PhysicalPower },
                    { 4, StatType.SpellPower }
                    
                };
            }
        }
    }
}