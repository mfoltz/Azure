﻿using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using System.Runtime.CompilerServices;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Converters;
using VCreate.Core.Toolbox;
using static VCreate.Core.Services.PlayerService;
using VCreate.Data;
using UnityEngine;
using VCreate.Systems;
using VRising.GameData.Models;
using VCreate.Hooks;
using static VCreate.Systems.Enablers.HorseFunctions;
using ProjectM.Scripting;
using static VCreate.Hooks.PetSystem.UnitTokenSystem;
using Unity.Transforms;
using Unity.Collections;

namespace VCreate.Core.Commands
{
    internal class PetCommands
    {
        internal static Dictionary<ulong, FamiliarStasisState> PlayerFamiliarStasisMap = [];

        [Command(name: "setUnlocked", shortHand: "set", adminOnly: false, usage: ".set [#]", description: "Sets familiar to attempt binding to from unlocked units.")]
        public static void MethodMinusOne(ChatCommandContext ctx, int choice)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.UnlockedPets.TryGetValue(platformId, out var data))
            {
                if (choice < 1 || choice > data.Count)
                {
                    ctx.Reply($"Invalid choice, please use 1 to {data.Count}.");
                    return;
                }
                if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
                {
                    settings.Familiar = data[choice - 1];
                    DataStructures.PlayerSettings[platformId] = settings;
                    DataStructures.SavePlayerSettings();
                    PrefabGUID prefabGUID = new(data[choice - 1]);
                    string colorfam = VCreate.Core.Toolbox.FontColors.Pink(prefabGUID.LookupName());
                    ctx.Reply($"Familiar to attempt binding to set: {colorfam}");
                }
                else
                {
                    ctx.Reply("Couldn't find data to set unlocked.");
                    return;
                }
            }
            else
            {
                ctx.Reply("You don't have any unlocked familiars yet.");
            }
        }

        [Command(name: "removeUnlocked", shortHand: "remove", adminOnly: false, usage: ".remove [#]", description: "Removes choice from list of unlocked familiars to bind to.")]
        public static void RemoveUnlocked(ChatCommandContext ctx, int choice)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.UnlockedPets.TryGetValue(platformId, out var data))
            {
                if (choice < 1 || choice > data.Count)
                {
                    ctx.Reply($"Invalid choice, please use 1 to {data.Count} for removing.");
                    return;
                }
                if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
                {
                    var toRemove = data[choice - 1];
                    if (data.Contains(toRemove))
                    {
                        data.Remove(toRemove);
                        DataStructures.UnlockedPets[platformId] = data;
                        DataStructures.SaveUnlockedPets();

                        ctx.Reply($"Familiar removed from list of unlocked units.");
                    }
                    else
                    {
                        ctx.Reply("Failed to remove unlocked unit.");
                        return;
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find data to remove unlocked unit.");
                    return;
                }
            }
            else
            {
                ctx.Reply("You don't have any unlocked familiars yet.");
            }
        }

        [Command(name: "listFamiliars", shortHand: "listfam", adminOnly: false, usage: ".listfam", description: "Lists unlocked familiars.")]
        public static void MethodZero(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.UnlockedPets.TryGetValue(platformId, out var data))
            {
                if (data.Count == 0)
                {
                    ctx.Reply("You don't have any unlocked familiars yet.");
                    return;
                }
                int counter = 0;
                foreach (var unlock in data)
                {
                    counter++;
                    string colornum = VCreate.Core.Toolbox.FontColors.Green(counter.ToString());
                    PrefabGUID prefabGUID = new(unlock);
                    // want real name from guid
                    string colorfam = VCreate.Core.Toolbox.FontColors.Pink(prefabGUID.LookupName());
                    ctx.Reply($"{colornum}: {colorfam}");
                }
            }
            else
            {
                ctx.Reply("You don't have any unlocked familiars yet.");
                return;
            }
        }

        [Command(name: "bindFamiliar", shortHand: "bind", adminOnly: false, usage: ".bind", description: "Binds familiar from first soulgem found in inventory and sets profile to active.")]
        public static void MethodOne(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            // verify states before proceeding, make sure no active profiles and no familiars in stasis
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var data))
            {
                int limit = 9;
                var profiles = data.Values;
                if (profiles.Count > limit)
                {
                    ctx.Reply("You have too many familiar profiles to bind to another, you'll have to remove one first.");
                    return;
                }
                foreach (var profile in profiles)
                {
                    if (profile.Active)
                    {
                        ctx.Reply("You already have an active familiar profile. Unbind it before binding to another.");
                        return;
                    }
                }
                if (PlayerFamiliarStasisMap.TryGetValue(platformId, out var familiarStasisState) && familiarStasisState.IsInStasis)
                {
                    ctx.Reply("You have a familiar in stasis. If you want to bind to another, summon it and unbind first.");
                    return;
                }
            }
            bool flag = false;
            if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
            {
                Entity unlocked = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[new(settings.Familiar)];
                EntityCategory unitCategory = unlocked.Read<EntityCategory>();
                Plugin.Log.LogInfo(unitCategory.UnitCategory.ToString());
                PrefabGUID gem;
                if (unlocked.Read<PrefabGUID>().LookupName().ToLower().Contains("vblood"))
                {
                    gem = new(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[UnitToGemMapping.UnitType.VBlood]);
                }
                else
                {
                    gem = new(PetSystem.UnitTokenSystem.UnitToGemMapping.UnitCategoryToGemPrefab[(UnitToGemMapping.UnitType)unitCategory.UnitCategory]);
                }

                //Plugin.Log.LogInfo(gem.LookupName());
                //Plugin.Log.LogInfo(gem.GuidHash.ToString());
                UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(platformId);
                var inventory = userModel.Inventory.Items;
                foreach (var item in inventory)
                {
                    if (item.Item.PrefabGUID.GuidHash == gem.GuidHash)
                    {
                        flag = true;
                        InventoryUtilitiesServer.TryRemoveItem(VWorld.Server.EntityManager, ctx.Event.SenderCharacterEntity, gem, 1);
                        break;
                    }
                }
                if (flag)
                {
                    if (DataStructures.PlayerSettings.TryGetValue(platformId, out var Settings))
                    {
                        Settings.Binding = true;

                        OnHover.SummonFamiliar(ctx.Event.SenderCharacterEntity.Read<PlayerCharacter>().UserEntity, new(settings.Familiar));
                    }
                }
                else
                {
                    ctx.Reply("Couldn't find perfect gem to bind to familiar type.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find data to bind familiar.");
                return;
            }

            // check for correct gem to take away for binding to familiar
        }

        [Command(name: "unbindFamiliar", shortHand: "unbind", adminOnly: false, usage: ".unbind", description: "Deactivates familiar profile and lets you bind to a different familiar.")]
        public static void MethodTwo(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;

            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                if (PlayerFamiliarStasisMap.TryGetValue(platformId, out FamiliarStasisState familiarStasisState) && familiarStasisState.IsInStasis)
                {
                    ctx.Reply("You have a familiar in stasis. Summon it before unbinding.");
                    return;
                }

                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (!familiar.Equals(Entity.Null) && data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    UnitStats stats = familiar.Read<UnitStats>();
                    Health health = familiar.Read<Health>();
                    float maxhealth = health.MaxHealth._Value;
                    float attackspeed = stats.AttackSpeed._Value;
                    float primaryattackspeed = stats.PrimaryAttackSpeed._Value;
                    float physicalpower = stats.PhysicalPower._Value;
                    float spellpower = stats.SpellPower._Value;
                    float physcritchance = stats.PhysicalCriticalStrikeChance._Value;
                    float physcritdamage = stats.PhysicalCriticalStrikeDamage._Value;
                    float spellcritchance = stats.SpellCriticalStrikeChance._Value;
                    float spellcritdamage = stats.SpellCriticalStrikeDamage._Value;
                    profile.Stats.Clear();
                    profile.Stats.AddRange([maxhealth, attackspeed, primaryattackspeed, physicalpower, spellpower, physcritchance, physcritdamage, spellcritchance, spellcritdamage]);
                    profile.Active = false;
                    profile.Combat = true;
                    data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;
                    DataStructures.PlayerPetsMap[platformId] = data;
                    DataStructures.SavePetExperience();
                    SystemPatchUtil.Destroy(familiar);
                    ctx.Reply("Familiar profile deactivated, stats saved and familiar unbound. You may now bind to another.");
                }
                else if (familiar.Equals(Entity.Null))
                {
                    var profiles = data.Keys;
                    foreach (var key in profiles)
                    {
                        if (data[key].Active)
                        {
                            // remember if code gets here it means familiar also not in stasis so probably has been lost, unbind it
                            data.TryGetValue(key, out PetExperienceProfile dataprofile);
                            dataprofile.Active = false;
                            data[key] = dataprofile;
                            DataStructures.PlayerPetsMap[platformId] = data;
                            DataStructures.SavePetExperience();
                            ctx.Reply("Unable to locate familiar and not in stasis, assuming dead and unbinding.");
                        }
                    }
                }
                else
                {
                    ctx.Reply("You don't have an active familiar to unbind.");
                }
            }
            else
            {
                ctx.Reply("You don't have a familiar to unbind.");
                return;
            }
        }

        [Command(name: "enableFamiliar", shortHand: "call", usage: ".call", description: "Summons familar if found in stasis.", adminOnly: false)]
        public static void EnableFamiliar(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                if (PlayerFamiliarStasisMap.TryGetValue(platformId, out FamiliarStasisState familiarStasisState) && familiarStasisState.IsInStasis)
                {
                    SystemPatchUtil.Enable(familiarStasisState.FamiliarEntity);
                    Follower follower = familiarStasisState.FamiliarEntity.Read<Follower>();
                    follower.Followed._Value = ctx.Event.SenderCharacterEntity;
                    familiarStasisState.FamiliarEntity.Write(follower);
                    familiarStasisState.FamiliarEntity.Write(new Translation { Value = ctx.Event.SenderCharacterEntity.Read<Translation>().Value });
                    familiarStasisState.FamiliarEntity.Write(new LastTranslation { Value = ctx.Event.SenderCharacterEntity.Read<Translation>().Value });
                    familiarStasisState.IsInStasis = false;
                    familiarStasisState.FamiliarEntity = Entity.Null;
                    PlayerFamiliarStasisMap[platformId] = familiarStasisState;
                    ctx.Reply("Your familiar has been summoned.");
                }
                else
                {
                    ctx.Reply("No familiars in stasis to enable.");
                }
            }
            else
            {
                ctx.Reply("No familiars found.");
            }
        }

        [Command(name: "disableFamiliar", shortHand: "dismiss", adminOnly: false, usage: ".dismiss", description: "Puts summoned familiar in stasis.")]
        public static void MethodThree(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (familiar.Equals(Entity.Null) || !familiar.Has<PrefabGUID>())
                {
                    ctx.Reply("You don't have any familiars to disable.");
                }
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    Follower follower = familiar.Read<Follower>();
                    follower.Followed._Value = Entity.Null;
                    familiar.Write(follower);
                    SystemPatchUtil.Disable(familiar);
                    PlayerFamiliarStasisMap[platformId] = new FamiliarStasisState(familiar, true);
                    ctx.Reply("Your familiar has been put in stasis.");
                    //DataStructures.SavePetExperience();
                }
                else
                {
                    ctx.Reply("You don't have an active familiar to disable.");
                }
            }
            else
            {
                ctx.Reply("You don't have any familiars to disable.");
                return;
            }
        }

        [Command(name: "setFamiliarFocus", shortHand: "focus", adminOnly: false, usage: ".focus [#]", description: "Sets the stat your familiar will specialize in when leveling up.")]
        public static void MethodFour(ChatCommandContext ctx, int stat)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    int toSet = stat - 1;
                    if (toSet < 0 || toSet > PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count - 1)
                    {
                        ctx.Reply($"Invalid choice, please use 1 to {PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count}.");
                        return;
                    }
                    profile.Focus = toSet;
                    data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;

                    DataStructures.SavePetExperience();
                    ctx.Reply($"Familiar focus set to {PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap[toSet]}.");
                    return;
                }
                else
                {
                    ctx.Reply("You don't have an active familiar.");
                }
            }
        }

        [Command(name: "combatModeToggle", shortHand: "combat", adminOnly: false, usage: ".combat", description: "Toggles combat mode for familiar.")]
        public static void MethodFive(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            var buffs = ctx.Event.SenderCharacterEntity.ReadBuffer<BuffBuffer>();
            foreach (var buff in buffs)
            {
                if (buff.PrefabGuid.GuidHash == VCreate.Data.Prefabs.Buff_InCombat.GuidHash)
                {
                    ctx.Reply("You cannot toggle combat mode during combat.");
                    return;
                }
            }
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out Dictionary<string, PetExperienceProfile> data))
            {
                ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
                BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
                EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
                EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
                Entity familiar = FindPlayerFamiliar(ctx.Event.SenderCharacterEntity);
                if (familiar.Equals(Entity.Null))
                {
                    ctx.Reply("Summon your familiar before toggling this.");
                    return;
                }
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
                    profile.Combat = !profile.Combat; // this will be false when first triggered
                    FactionReference factionReference = familiar.Read<FactionReference>();
                    PrefabGUID ignored = new(-1430861195);
                    PrefabGUID player = new(1106458752);
                    if (!profile.Combat)
                    {
                        factionReference.FactionGuid._Value = ignored;
                    }
                    else
                    {
                        factionReference.FactionGuid._Value = player;
                    }

                    //familiar.Write(new Immortal { IsImmortal = !profile.Combat });

                    familiar.Write(factionReference);
                    BufferFromEntity<BuffBuffer> bufferFromEntity = VWorld.Server.EntityManager.GetBufferFromEntity<BuffBuffer>();
                    if (profile.Combat)
                    {
                        BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff, familiar);
                        BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, VCreate.Data.Prefabs.Admin_Invulnerable_Buff, familiar);
                    }
                    else
                    {
                        OnHover.BuffNonPlayer(familiar, VCreate.Data.Prefabs.Admin_Invulnerable_Buff);
                        OnHover.BuffNonPlayer(familiar, VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff);
                    }

                    data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;
                    DataStructures.PlayerPetsMap[platformId] = data;
                    DataStructures.SavePetExperience();
                    if (!profile.Combat)
                    {
                        string disabledColor = VCreate.Core.Toolbox.FontColors.Pink("disabled");
                        ctx.Reply($"Combat for familiar is {disabledColor}. It cannot die and won't participate, however, no experience will be gained.");
                    }
                    else
                    {
                        string enabledColor = VCreate.Core.Toolbox.FontColors.Green("enabled");
                        ctx.Reply($"Combat for familiar is {enabledColor}. It will fight till glory or death and gain experience.");
                    }
                }
            }
            else
            {
                ctx.Reply("You don't have any familiars.");
                return;
            }
        }

        internal struct FamiliarStasisState
        {
            public Entity FamiliarEntity;
            public bool IsInStasis;

            public FamiliarStasisState(Entity familiar, bool isInStasis)
            {
                FamiliarEntity = familiar;
                IsInStasis = isInStasis;
            }
        }

        public static Entity FindPlayerFamiliar(Entity characterEntity)
        {
            var followers = characterEntity.ReadBuffer<FollowerBuffer>();
            foreach (var follower in followers)
            {
                if (!follower.Entity._Entity.Has<PrefabGUID>()) continue;
                PrefabGUID prefabGUID = follower.Entity._Entity.Read<PrefabGUID>();
                ulong platformId = characterEntity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId;
                if (DataStructures.PlayerSettings.TryGetValue(platformId, out var data))
                {
                    if (data.Familiar.Equals(prefabGUID.GuidHash))
                    {
                        return follower.Entity._Entity;
                    }
                }
            }

            return Entity.Null;
        }
    }
}