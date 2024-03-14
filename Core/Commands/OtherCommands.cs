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

namespace VCreate.Core.Commands
{
    internal class GiveItemCommands
    {
        [Command(name: "give", shortHand: "gv", adminOnly: true, usage: ".gv [ItemName] [Quantity]", description: "Gives the specified item w/quantity.")]
        public static void GiveItem(ChatCommandContext ctx, GivenItem item, int quantity = 1)
        {
            if (Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, item.Value, quantity, out var entity))
            {
                var prefabSys = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
                prefabSys.PrefabGuidToNameDictionary.TryGetValue(item.Value, out var name);
                ctx.Reply($"Gave {quantity} {name}");
            }
        }

        public record struct GivenItem(PrefabGUID Value);

        internal class GiveItemConverter : CommandArgumentConverter<GivenItem>
        {
            public override GivenItem Parse(ICommandContext ctx, string input)
            {
                PrefabGUID prefabGUID;
                if (Helper.TryGetItemPrefabGUIDFromString(input, out prefabGUID))
                    return new GivenItem(prefabGUID);
                throw ctx.Error("Could not find item: " + input);
            }
        }
    }

    internal class ReviveCommands
    {
        [Command(name: "revive", shortHand: "rev", adminOnly: true, usage: ".rev [PlayerName]", description: "Revives self or player.")]
        public void ReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;

            Helper.ReviveCharacter(Character, User);

            ctx.Reply("Revived");
        }
    }

    internal class MiscCommands
    {
        [Command(name: "Demigod", shortHand: "deus", adminOnly: true, usage: ".deus", description: "Activates demigod mode. Use debuff mode to clear from self.")]
        public static void DemigodCommand(ChatCommandContext ctx)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (DataStructures.PlayerSettings.TryGetValue(SteamID, out Omnitool data))
            {
                Helper.BuffCharacter(character, VCreate.Data.Buffs.Admin_Invulnerable_Buff, -1, false);
                //OnHover.BuffNonPlayer(ctx.Event.SenderCharacterEntity, VCreate.Data.Buffs.Buff_General_VBlood_Ghost_Timer);
                data.SetData("Debuff", VCreate.Data.Buffs.Admin_Invulnerable_Buff.GuidHash);
                ctx.Reply("You're now invulnerable. Use debuff mode to return to normal.");
            }
            else
            {
                ctx.Reply("Couldn't find omnitool data.");
            }
        }
        [Command(name: "unlock", shortHand: "ul", adminOnly: true, usage: ".ul [PlayerName]", description: "Unlocks all the things.")]
        public void UnlockCommand(ChatCommandContext ctx, string playerName)
        {
            TryGetPlayerFromString(playerName, out Player player);
            Player player1;
            Entity entity1;
            if ((object)player == null)
            {
                entity1 = ctx.Event.SenderUserEntity;
            }
            else
            {
                player1 = player;
                entity1 = player1.User;
            }
            Entity entity2 = entity1;
            Entity entity3;
            if ((object)player == null)
            {
                entity3 = ctx.Event.SenderCharacterEntity;
            }
            else
            {
                player1 = player;
                entity3 = player1.Character;
            }
            Entity entity4 = entity3;
            try
            {
                VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                FromCharacter fromCharacter = new FromCharacter()
                {
                    User = entity2,
                    Character = entity4
                };

                Helper.UnlockVBloods(fromCharacter);
                ChatCommandContext chatCommandContext2 = ctx;
                string str3;
                if ((object)player == null)
                {
                    str3 = null;
                }
                else
                {
                    player1 = player;
                    str3 = player1.Name;
                }
                if (str3 == null)
                    str3 = "you";
                string str4 = "Unlocked VBloods for " + str3 + ".";
                //chatCommandContext2.Reply(str4);

                Helper.UnlockResearch(fromCharacter);
                ChatCommandContext chatCommandContext4 = ctx;
                string str7;
                if ((object)player == null)
                {
                    str7 = null;
                }
                else
                {
                    player1 = player;
                    str7 = player1.Name;
                }
                if (str7 == null)
                    str7 = "you";
                string str8 = "Unlocked research for " + str7 + ".";
                //chatCommandContext4.Reply(str8);
            }
            catch (Exception ex)
            {
                throw ctx.Error(ex.ToString());
            }
        }

        [Command("bloodMerlot", "bm", ".bm [Type] [Quantity] [Quality]", "Provides a blood merlot as ordered.", null, true)]
        public static void GiveBloodPotionCommand(ChatCommandContext ctx, VCreate.Data.Prefabs.BloodType type = VCreate.Data.Prefabs.BloodType.frailed, int quantity = 1, float quality = 100f)
        {
            quality = Mathf.Clamp(quality, 0, 100);
            int i;
            for (i = 0; i < quantity; i++)
            {
                if (Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, Prefabs.Item_Consumable_PrisonPotion_Bloodwine, 1, out var bloodPotionEntity))
                {
                    var blood = new StoredBlood()
                    {
                        BloodQuality = quality,
                        BloodType = new PrefabGUID((int)type)
                    };

                    bloodPotionEntity.Write(blood);
                }
                else
                {
                    break;
                }
            }

            ctx.Reply($"Got {i} Blood Potion(s) Type <color=#ff0>{type}</color> with <color=#ff0>{quality}</color>% quality");
        }

        [Command("ping", "!", null, "Shows your latency.", null, false)]
        public static void PingCommand(ChatCommandContext ctx)
        {
            var ping = (int)(ctx.Event.SenderCharacterEntity.Read<Latency>().Value * 1000);
            ctx.Reply($"Your latency is <color=#ffff00>{ping}</color>ms");
        }
    }
}