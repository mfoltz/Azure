﻿using Bloodstone.API;
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
                        else if (killer.Has<Follower>() && killer.Has<FactionReference>())
                        {
                            if (killer.Read<Follower>().Followed._Value.Has<PlayerCharacter>() && !killer.Read<FactionReference>().FactionGuid.Equals(VCreate.Data.Prefabs.Faction_Players)) DeathEventHandlers.HandlePetKill(killer, died); // if pet, handle pet experience
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
                    // check for servantpower on follower, give exp to pet when found
                    if (follower.Read<Team>().Value.Equals(killer.Read<Team>().Value) && DataStructures.PetExperience.TryGetValue(killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out PetExperience petExperience))
                    {
                        UnitLevel unitLevel = died.Read<UnitLevel>();
                        float baseExp = Math.Max(unitLevel.Level - petExperience.Level, 1);

                        petExperience.CurrentExperience += (int)baseExp;

                        double toNext = 1.25 * Math.Pow(petExperience.Level, 3);

                        if (petExperience.CurrentExperience >= toNext && petExperience.Level < 80)
                        {
                            petExperience.CurrentExperience = 0;
                            petExperience.Level++;

                            Plugin.Log.LogInfo("Pet level up! Setting unit level...");
                            follower.Write<UnitLevel>(new UnitLevel { Level = petExperience.Level });
                            UnitStatSet(follower);
                            DataStructures.PetExperience[killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = petExperience;
                            DataStructures.SavePetExperience();
                            break;
                        }
                        else
                        {
                            Plugin.Log.LogInfo("Giving pet experience...");

                            DataStructures.PetExperience[killer.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = petExperience;
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
                if (DataStructures.PetExperience.TryGetValue(platformId, out PetExperience petExperience))
                {
                    UnitLevel unitLevel = died.Read<UnitLevel>();
                    float baseExp = Math.Max(unitLevel.Level - petExperience.Level, 1);

                    petExperience.CurrentExperience += (int)baseExp;

                    double toNext = 1.25 * Math.Pow(petExperience.Level, 3);

                    if (petExperience.CurrentExperience >= toNext && petExperience.Level < 80)
                    {
                        petExperience.CurrentExperience = 0;
                        petExperience.Level++;

                        Plugin.Log.LogInfo("Pet level up! Setting unit level/power...");
                        pet.Write<UnitLevel>(new UnitLevel { Level = petExperience.Level });
                        try
                        {
                            Utilities.AddComponentData(entity, new UnitLevel { Level = petExperience.Level });
                        }
                        catch (Exception e)
                        {
                            Plugin.Log.LogError(e);
                        }

                        //UnitStats unitStats = pet.Read<UnitStats>();
                        UnitStatSet(pet);

                        DataStructures.PetExperience[platformId] = petExperience;
                        DataStructures.SavePetExperience();
                    }
                    else
                    {
                        Plugin.Log.LogInfo("Giving pet experience...");

                        DataStructures.PetExperience[platformId] = petExperience;
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

                // Increase the selected stat by 5%. This assumes there's a way to modify the UnitStats component directly.
                // Note: You might need to adjust how you access and modify the stats based on your actual implementation.
                switch (selectedStat)
                {
                    case FocusToStatMap.StatType.MaxHealth:
                        health.MaxHealth._Value *= 1.05f;
                        break;
                    case FocusToStatMap.StatType.HealthRegen:
                        unitStats.PassiveHealthRegen._Value += 0.01f;
                        break;
                    case FocusToStatMap.StatType.AttackSpeed:
                        unitStats.AttackSpeed._Value *= 1.05f;
                        break;
                    case FocusToStatMap.StatType.PrimaryAttackSpeed:
                        unitStats.PrimaryAttackSpeed.Value *= 1.05f;
                        break;
                    case FocusToStatMap.StatType.PhysicalPower:
                        unitStats.PhysicalPower._Value *= 1.05f;
                        break;
                    case FocusToStatMap.StatType.SpellPower:
                        unitStats.SpellPower._Value *= 1.05f;
                        break;
                }

                // Assuming entity.Write<UnitStats>(unitStats) is a way to save the modified UnitStats back to the entity.
                entity.Write(unitStats);
            }

        }

        public class UnitTokenSystem
        {
            private static readonly float chance = 0.5f; // testing
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
                    { UnitType.Human, -2020212226 }, // Item_Ingredient_Gem_Sapphire_T04
                    { UnitType.Undead, 1354115931 }, // Item_Ingredient_Gem_Emerald_T04
                    { UnitType.Demon, 750542699 }, // Item_Ingredient_Gem_Miststone_T04
                    { UnitType.Mechanical, -1983566585 }, // Item_Ingredient_Gem_Topaz_T04
                    { UnitType.Beast, -106283194 }, // Item_Ingredient_Gem_Amethyst_T04
                    { UnitType.VBlood, 188653143 } // Item_Ingredient_Gem_Ruby_T04
                };
            }

            public static void HandleTokenDrop(Entity killer, Entity died)
            {
                // get died category
                Plugin.Log.LogInfo("Handling token drop...");
                PrefabGUID gem;
                EntityCategory diedCategory = died.Read<EntityCategory>();

                if ((int)diedCategory.UnitCategory < 5 && !died.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    gem = new(UnitToGemMapping.UnitCategoryToGemPrefab[(UnitToGemMapping.UnitType)diedCategory.UnitCategory]);
                    HandleRoll(gem, chance, died, killer);
                }
                else if (died.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    gem = new(UnitToGemMapping.UnitCategoryToGemPrefab[UnitToGemMapping.UnitType.VBlood]);
                    HandleRoll(gem, chance / 3, died, killer);
                }
                else
                {
                    Plugin.Log.LogInfo("No token drop for this unit.");
                    return;
                }
            }

            public static void HandleRoll(PrefabGUID gem, float dropChance, Entity died, Entity killer)
            {
                try
                {
                    if (RollForChance(gem, dropChance, died))
                    {
                        //want to give player the item here
                        UserModel userModel = VRising.GameData.GameData.Users.GetUserByCharacterName(killer.Read<PlayerCharacter>().Name.ToString());
                        if (Helper.AddItemToInventory(userModel.FromCharacter.Character, gem, 1, out Entity _, false))
                        {
                            //Plugin.Log.LogInfo("Item entity is null... (going once)");
                            var items = userModel.Inventory.Items;
                            foreach (var item in items)
                            {
                                if (item.Item.PrefabGUID.Equals(gem))
                                {
                                    Plugin.Log.LogInfo("Trying item modification...");
                                    Entity testing = item.Item.Entity;
                                    ItemData itemData = testing.Read<ItemData>();
                                    itemData.ItemTypeGUID = died.Read<PrefabGUID>();
                                    itemData.MaxAmount = 1;
                                    testing.Write(itemData);
                                    Plugin.Log.LogInfo($"Item modified.{itemData.ItemTypeGUID.LookupName()} stored...");
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

        public class PerfectGemSystem
        {
            private static readonly List<PrefabGUID> perfectGems =
            [
                new PrefabGUID(-2020212226), // Item_Ingredient_Gem_Sapphire_T04
                new PrefabGUID(1354115931), // Item_Ingredient_Gem_Emerald_T04
                new PrefabGUID(750542699), // Item_Ingredient_Gem_Miststone_T04
                new PrefabGUID(-1983566585), // Item_Ingredient_Gem_Topaz_T04
                new PrefabGUID(-106283194), // Item_Ingredient_Gem_Amethyst_T04
                new PrefabGUID(188653143) // Item_Ingredient_Gem_Ruby_T04
            ];

            public static void ModifyPerfectGems()
            {
                EntityManager entityManager = VWorld.Server.EntityManager;

                foreach (PrefabGUID prefabGUID in perfectGems)
                {
                    Entity gemEntity = Utilities.GetPrefabEntityByPrefabGUID(prefabGUID, entityManager);
                    
                    if (gemEntity != Entity.Null)
                    {
                        var itemData = Utilities.GetComponentData<ItemData>(gemEntity);
                        itemData.RemoveOnConsume = false;
                        itemData.ItemCategory = ItemCategory.BloodBound;
                        //Consumable consumable = new();
                        //var conditionBuffer = entityManager.AddBuffer<ConsumableCondition>(gemEntity);
                        //var buffer = entityManager.GetBuffer<ConsumableCondition>(test_consumable);
                        //foreach (var item in buffer)
                        //{
                        //   conditionBuffer.Add(item);
                        //}
                        CastAbilityOnConsume castAbilityOnConsume = new() { AbilityGuid = VCreate.Data.Prefabs.AB_ChurchOfLight_Paladin_SummonAngel_Cast };
                        //Utilities.AddComponentData(gemEntity, consumable);
                        Utilities.AddComponentData(gemEntity, castAbilityOnConsume);
                        Utilities.SetComponentData(gemEntity, itemData);
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
                    HealthRegen,
                    AttackSpeed,
                    PrimaryAttackSpeed,
                    PhysicalPower,
                    SpellPower
                }

                public static readonly Dictionary<int, StatType> FocusStatMap = new()
                {
                    { 0, StatType.MaxHealth },
                    { 1, StatType.HealthRegen },
                    { 2, StatType.AttackSpeed },
                    { 3, StatType.PrimaryAttackSpeed },
                    { 4, StatType.PhysicalPower },
                    { 5, StatType.SpellPower }
                    
                };
            }
        }
    }
}