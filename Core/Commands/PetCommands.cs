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

namespace VCreate.Core.Commands
{
    internal class PetCommands
    {
        internal static Dictionary<ulong, FamiliarStasisState> PlayerFamiliarStasisMap = [];

        [Command(name: "bindFamiliar", shortHand: "soul", adminOnly: false, usage: ".soul", description: "Summons familiar from first soulgem found in inventory.")]
        public static void MethodOne(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (PlayerFamiliarStasisMap.TryGetValue(platformId, out FamiliarStasisState familiarStasisState) && familiarStasisState.IsInStasis)
            {
                ctx.Reply("You already have a familiar in stasis. If you would like to bind to a new one, destroy the one in stasis first.");
            }
            OnHover.SummonFamiliar(ctx.Event.SenderCharacterEntity.Read<PlayerCharacter>().UserEntity);
        }

        [Command(name: "unbindFamiliar", shortHand: "unbindsoul", adminOnly: false, usage: ".unbindsoul", description: "Destroys familiar if you have one in stasis.")]
        public static void MethodTwo(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (PlayerFamiliarStasisMap.TryGetValue(platformId, out FamiliarStasisState familiarStasisState) && familiarStasisState.IsInStasis)
            {
                Entity entity = familiarStasisState.FamiliarEntity;
                SystemPatchUtil.Enable(entity);
                familiarStasisState.IsInStasis = false;
                PlayerFamiliarStasisMap[platformId] = familiarStasisState;
                SystemPatchUtil.Destroy(entity);
                ctx.Reply("Your familiar has been unbound.");
            }
        }

        [Command(name: "enableFamiliar", shortHand: "summon", usage: ".summon", description: "Summons familar if found in stasis.", adminOnly: false)]
        public static void EnableFamiliar(ChatCommandContext ctx)
        {
            ulong platformId = ctx.User.PlatformId;
            if (PlayerFamiliarStasisMap.TryGetValue(platformId, out FamiliarStasisState familiarStasisState) && familiarStasisState.IsInStasis)
            {
                SystemPatchUtil.Enable(familiarStasisState.FamiliarEntity);
                familiarStasisState.IsInStasis = false;
                PlayerFamiliarStasisMap[platformId] = familiarStasisState;
                ctx.Reply("Your familiar has been enabled.");
            }
            else
            {
                ctx.Reply("No familiar in stasis found to enable.");
            }
        }

        [Command(name: "disableFamiliar", shortHand: "dismiss", adminOnly: false, usage: ".dismiss", description: "Puts familiar in stasis.")]
        public static void MethodThree(ChatCommandContext ctx)
        {
            Dictionary<Entity, bool> keyValuePairs = [];
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PetExperience.TryGetValue(platformId, out PetExperience data) && data.Active)
            {
                var followers = ctx.Event.SenderCharacterEntity.ReadBuffer<FollowerBuffer>();
                foreach (var follower in followers)
                {
                    //string entityString = follower.Entity._Entity.ToString();
                    keyValuePairs.Add(follower.Entity._Entity, false);
                    var buffs = follower.Entity._Entity.ReadBuffer<BuffBuffer>();
                    foreach (var buff in buffs)
                    {
                        if (buff.PrefabGuid.GuidHash == VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff.GuidHash)
                        {
                            keyValuePairs[follower.Entity._Entity] = true;
                        }
                    }
                }
                foreach (var pair in keyValuePairs)
                {
                    if (!pair.Value)
                    {
                        // disable the familiar
                        Entity entity = pair.Key;
                        SystemPatchUtil.Disable(entity);
                        data.Active = false;
                        DataStructures.PetExperience[platformId] = data;
                        DataStructures.SavePetExperience();

                        PlayerFamiliarStasisMap[platformId] = new FamiliarStasisState(entity, true);
                        SystemPatchUtil.Disable(entity);
                        ctx.Reply("Familiar disabled.");
                        return;
                    }
                }
            }
            else
            {
                ctx.Reply("You don't have an active familiar.");
                return;
            }
        }

        [Command(name: "setFamiliarFocus", shortHand: "focus", adminOnly: false, usage: ".focus [#]", description: "Sets the stat your familiar will improve specialize in when leveling up.")]
        public static void MethodFour(ChatCommandContext ctx, int stat)
        {
            ulong platformId = ctx.User.PlatformId;
            if (DataStructures.PetExperience.TryGetValue(platformId, out PetExperience data) && data.Active)
            {
                // validate input
                int toSet = stat - 1;
                if (toSet < 0 || toSet > PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count - 1)
                {
                    ctx.Reply($"Invalid choice, please use 1 to {PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap.Count}.");
                    return;
                }
                data.Focus = toSet;
                DataStructures.PetExperience[platformId] = data;
                DataStructures.SavePetExperience();
                ctx.Reply($"Familiar focus set to {PetSystem.PetFocusSystem.FocusToStatMap.FocusStatMap[toSet]}.");
                return;
            }
            else
            {
                ctx.Reply("You don't have an active familiar.");
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
    }
}