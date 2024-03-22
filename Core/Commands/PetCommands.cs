using Bloodstone.API;
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

namespace VCreate.Core.Commands
{
    internal class PetCommands
    {
        internal static Dictionary<ulong, FamiliarStasisState> PlayerFamiliarStasisMap = [];

        [Command(name: "bindFamiliar", shortHand: "bind", adminOnly: false, usage: ".bind", description: "Binds familiar from first soulgem found in inventory and sets profile to active.")]
        public static void MethodOne(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            // verify states before proceeding, make sure no active profiles and no familiars in stasis
            if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var data))
            {
                var profiles = data.Values;
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
            OnHover.SummonFamiliar(ctx.Event.SenderCharacterEntity.Read<PlayerCharacter>().UserEntity);
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
                    profile.Stats.Clear();
                    profile.Stats.AddRange([maxhealth, attackspeed, primaryattackspeed, physicalpower, spellpower]);
                    profile.Active = false;
                    profile.Combat = true;
                    data[familiar.Read<PrefabGUID>().LookupName().ToString()] = profile;
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
                    familiarStasisState.IsInStasis = false;
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
                if (data.TryGetValue(familiar.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && profile.Active)
                {
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
                    if (!familiar.Has<Immortal>())
                    {
                        Utilities.AddComponentData(familiar, new Immortal { IsImmortal = false });
                    }

                    familiar.Write(new Immortal { IsImmortal = !profile.Combat });

                    familiar.Write(factionReference);
                    BufferFromEntity<BuffBuffer> bufferFromEntity = VWorld.Server.EntityManager.GetBufferFromEntity<BuffBuffer>();
                    if (profile.Combat)
                    {
                        BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff, familiar);
                    }
                    else
                    {
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
            Dictionary<Entity, bool> keyValuePairs = [];
            var followers = characterEntity.ReadBuffer<FollowerBuffer>();
            foreach (var follower in followers)
            {
                keyValuePairs.Add(follower.Entity._Entity, false);
                var buffs = follower.Entity._Entity.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffs)
                {
                    if (buff.PrefabGuid.GuidHash == VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff.GuidHash)
                    {
                        DataStructures.PlayerPetsMap.TryGetValue(characterEntity.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var data);
                        if (data.TryGetValue(follower.Entity._Entity.Read<PrefabGUID>().LookupName().ToString(), out PetExperienceProfile profile) && !profile.Combat)
                        {
                            // if charmed and not in combat mode probably familiar
                            continue;
                        }
                        else
                        {
                            keyValuePairs[follower.Entity._Entity] = true;
                        }
                        
                    }
                }
            }
            foreach (var pair in keyValuePairs)
            {
                if (!pair.Value)
                {
                    return pair.Key;
                }
            }

            return Entity.Null;
        }
    }
}