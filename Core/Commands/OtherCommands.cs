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
        public static Dictionary<Entity, bool> AutoReviveDictionary = new Dictionary<Entity, bool>();
        public static Dictionary<Entity, Entity> AutoReviveCharactersToUsers = new Dictionary<Entity, Entity>();

        [Command("revive", adminOnly: true)]
        public void ReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;

            Helper.ReviveCharacter(Character, User);

            ctx.Reply("Revived");
        }

        [Command("autorevive", adminOnly: true)]
        public void AutoReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            if (AutoReviveDictionary.ContainsKey(Character))
            {
                AutoReviveDictionary[Character] = !AutoReviveDictionary[Character];
                if (AutoReviveDictionary[Character])
                {
                    Helper.ReviveCharacter(Character, User);
                    ctx.Reply($"Enabled autorevive for {player?.Value.Name ?? "you"}.");
                }
                else
                {
                    ctx.Reply($"Disabled autorevive for {player?.Value.Name ?? "you"}.");
                }
            }
            else
            {
                Helper.ReviveCharacter(Character, User);
                AutoReviveDictionary[Character] = true;
                AutoReviveCharactersToUsers[Character] = User;
                ctx.Reply($"Enabled autorevive for {player?.Value.Name ?? "you"}.");
            }
        }
    }

    internal class CommandsCommands
    {
        private static void MakePlayerImmaterial(Entity User, Entity Character)
        {
            Helper.BuffPlayer(Character, User, Data.Buffs.AB_Blood_BloodRite_Immaterial, Helper.NO_DURATION, true);
            if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, Character, Prefabs.AB_Blood_BloodRite_Immaterial, out Entity buffEntity))
            {
                var modifyMovementSpeedBuff = buffEntity.Read<ModifyMovementSpeedBuff>();
                modifyMovementSpeedBuff.MoveSpeed = 1; //bloodrite makes you accelerate forever, disable this
                buffEntity.Write(modifyMovementSpeedBuff);
            }
        }

        private static void MakePlayerMaterial(Entity Character)
        {
            Helper.UnbuffCharacter(Character, Data.Buffs.AB_Blood_BloodRite_Immaterial);
        }

        private static bool ToggleImmaterial(Entity User, Entity Character)
        {
            if (!BuffUtility.HasBuff(VWorld.Server.EntityManager, Character, Data.Buffs.AB_Blood_BloodRite_Immaterial))
            {
                MakePlayerImmaterial(User, Character);
            }
            else
            {
                MakePlayerMaterial(Character);
            }
            return false;
        }

        [Command(name: "demigod", shortHand: "deus", adminOnly: true, usage: ".deus", description: "Minor godhood. The bare essentials, if you will :P")]
        public void DemigodCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            var User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            var Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;

            MakePlayerImmaterial(User, Character);
            Helper.BuffPlayer(Character, User, Prefabs.EquipBuff_ShroudOfTheForest, -1, true);
            ctx.Reply("Granted you the powers of a (minor) god! Use debuffMode to return to normal.");
        }



        [Command(name: "unlock", shortHand: "ul", adminOnly: true, usage: ".ul [PlayerName]", description: "Unlocks all the things.")]
        public void UnlockCommand(ChatCommandContext ctx, string playerName, string unlockCategory = "all")
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
                switch (unlockCategory)
                {
                    case "all":
                        Helper.UnlockAll(fromCharacter);
                        ChatCommandContext chatCommandContext1 = ctx;
                        string str1;
                        if ((object)player == null)
                        {
                            str1 = null;
                        }
                        else
                        {
                            player1 = player;
                            str1 = player1.Name;
                        }
                        if (str1 == null)
                            str1 = "you";
                        string str2 = "Unlocked everything for " + str1 + ".";
                        chatCommandContext1.Reply(str2);
                        break;

                    case "vbloods":
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
                        chatCommandContext2.Reply(str4);
                        break;

                    case "achievements":
                        Helper.UnlockAchievements(fromCharacter);
                        ChatCommandContext chatCommandContext3 = ctx;
                        string str5;
                        if ((object)player == null)
                        {
                            str5 = null;
                        }
                        else
                        {
                            player1 = player;
                            str5 = player1.Name;
                        }
                        if (str5 == null)
                            str5 = "you";
                        string str6 = "Unlocked achievements for " + str5 + ".";
                        chatCommandContext3.Reply(str6);
                        break;

                    case "research":
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
                        chatCommandContext4.Reply(str8);
                        break;

                    case "dlc":
                        Helper.UnlockContent(fromCharacter);
                        ChatCommandContext chatCommandContext5 = ctx;
                        string str9;
                        if ((object)player == null)
                        {
                            str9 = null;
                        }
                        else
                        {
                            player1 = player;
                            str9 = player1.Name;
                        }
                        if (str9 == null)
                            str9 = "you";
                        string str10 = "Unlocked dlc for " + str9 + ".";
                        chatCommandContext5.Reply(str10);
                        break;

                    default:
                        ctx.Reply("Invalid unlock type specified.");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ctx.Error(ex.ToString());
            }
        }

        [Command("bloodmerlot", "bm", ".bm [Type] [quantity] [quality]", "Provides a blood merlot as ordered.", null, true)]
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
        public static void PingCommand(ChatCommandContext ctx, string mode = "")
        {
            var ping = (int)(ctx.Event.SenderCharacterEntity.Read<Latency>().Value * 1000);
            ctx.Reply($"Your latency is <color=#ffff00>{ping}</color>ms");
        }
    }
}