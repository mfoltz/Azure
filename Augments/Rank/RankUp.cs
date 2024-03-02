using ProjectM;
using System.Text.RegularExpressions;
using VPlus.Core;
using VPlus.Core.Commands;
using VampireCommandFramework;
using Bloodstone.API;
using Il2CppSystem;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using VPlus.Core.Services;
using VPlus.Data;
using DateTime = System.DateTime;
using static ProjectM.Tiles.TileConstants;
using VPlus.Core.Toolbox;
using Il2CppSystem.Security.Cryptography;

namespace VPlus.Augments.Rank
{
    public class RankData
    {
        public int Rank { get; set; }
        public int Points { get; set; }
        public List<int> Buffs { get; set; } = new List<int>();

        public DateTime LastAbilityUse { get; set; }

        public int RankSpell { get; set; }

        public List<int> Spells { get; set; } = new List<int>();

        public string ClassChoice { get; set; } = "default";

        public bool FishingPole { get; set; }

        public RankData(int rank, int points, List<int> buffs, int rankSpell, List<int> spells, string classchoice, bool fishingPole)
        {
            Rank = rank;
            Points = points;
            Buffs = buffs;
            LastAbilityUse = DateTime.MinValue; // Initialize to ensure it's always set
            RankSpell = rankSpell;
            Spells = spells;
            ClassChoice = classchoice;
            FishingPole = fishingPole;
        }
    }

    internal class PvERankSystem
    {
        public static void RankUp(ChatCommandContext ctx, string playerName, ulong SteamID, RankData data)
        {
            List<int> playerBuffs = data.Buffs;

            // reset points, increment rank level, grant buff, save data
            if (Plugin.BuffRewardsRankUp)
            {
                var (buffname, buffguid, buffFlag) = BuffCheck(data);
                if (!buffFlag)
                {
                    ctx.Reply("Unable to parse buffs, make sure number of buff prefabs equals the number of max ranks in configuration.");
                    return;
                }
                if (buffname != "0") // this is a way to skip a buff, leave buffs you want skipped as 0s in config
                {
                    Helper.BuffPlayerByName(playerName, buffguid, 0, true);
                    string colorString = FontColors.Green(buffname);
                    ctx.Reply($"You've been granted a permanent buff: {colorString}");
                }
            }
            data.Rank++;
            data.Points = 0;
            data.Buffs = playerBuffs;
            string rankString = FontColors.Yellow(data.Rank.ToString());
            string playerString = FontColors.Blue(playerName);
            ctx.Reply($"Congratulations {playerString}! You have increased your PvE rank to {rankString}.");
            //lightning bolt goes here

            PrefabGUID lightning = new PrefabGUID(838368210);// eye of god
            VPlus.Data.FoundPrefabGuid foundPrefabGuid = new(lightning);
            CastCommand(ctx, foundPrefabGuid, null);
            ChatCommands.SavePlayerRanks();
            return;
        }

        public static (string, PrefabGUID, bool) BuffCheck(RankData data)
        {
            var buffstring = Plugin.BuffPrefabsRankUp;
            List<int> playerBuffs = data.Buffs;
            var buffList = Regex.Matches(buffstring, @"-?\d+")
                               .Cast<Match>()
                               .Select(m => int.Parse(m.Value))
                               .ToList();
            bool buffFlag = false;
            string buffname = "placeholder";

            playerBuffs.Add(buffList[data.Rank]);
            PrefabGUID buffguid = new(buffList[data.Rank]);
            buffname = ECSExtensions.LookupName(buffguid);
            if (buffList[data.Rank] == 0)
            {
                buffname = "0";
            }
            if (buffList.Count == Plugin.MaxRanks)
            {
                buffFlag = true;
                return (buffname, buffguid, buffFlag);
            }
            else
            {
                return (buffname, buffguid, buffFlag);
            }
        }
        
        public interface ICharacterClass
        {
            bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor);
        }
        public class Nightmarshal : ICharacterClass
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Nightmarshal()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
                    { 5, new RankSpellConstructor("Batswarm", VPlus.Data.Prefabs.AB_BatVampire_BatSwarm_AbilityGroup, 5) },
                    { 4, new RankSpellConstructor("Nightdash", VPlus.Data.Prefabs.AB_BatVampire_NightDash_AbilityGroup, 4) },
                    { 3, new RankSpellConstructor("Batstorm", VPlus.Data.Prefabs.AB_BatVampire_BatStorm_AbilityGroup, 3) },
                    { 2, new RankSpellConstructor("Batleap", VPlus.Data.Prefabs.AB_BatVampire_SummonMinions_AbilityGroup, 2) },
                    { 1, new RankSpellConstructor("Batwhirlwind", VPlus.Data.Prefabs.AB_BatVampire_Whirlwind_AbilityGroup, 1) },
                };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }
        public class Paladin : ICharacterClass
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Paladin()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
                    { 5, new RankSpellConstructor("HolySpinners", VPlus.Data.Prefabs.AB_ChurchOfLight_Paladin_HolySpinners_AbilityGroup, 5) },
                    { 4, new RankSpellConstructor("DivineRays", VPlus.Data.Prefabs.AB_ChurchOfLight_Paladin_DivineRays_AbilityGroup, 4) },
                    { 3, new RankSpellConstructor("HolyFlackCannon", VPlus.Data.Prefabs.AB_ChurchOfLight_Paladin_HolyFlackCannon_AbilityGroup, 3) },
                    { 2, new RankSpellConstructor("ChargedSwing", VPlus.Data.Prefabs.AB_ChurchOfLight_Paladin_ChargedSwing_AbilityGroup, 2) },
                    { 1, new RankSpellConstructor("AngelicAscent", VPlus.Data.Prefabs.AB_ChurchOfLight_Paladin_AngelicAscent_AbilityGroup, 1) },
                };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class Default : ICharacterClass
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Default()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
                    { 5, new RankSpellConstructor("LightningStornm", VPlus.Data.Prefabs.AB_Monster_LightningStorm_AbilityGroup, 5) },
                    { 4, new RankSpellConstructor("WispDance", VPlus.Data.Prefabs.AB_Cursed_MountainBeast_GhostCall_AbilityGroup, 4) },
                    { 3, new RankSpellConstructor("Heal", VPlus.Data.Prefabs.AB_Nun_VBlood_HealCommand_AbilityGroup, 3) },
                    { 2, new RankSpellConstructor("LightningShield", VPlus.Data.Prefabs.AB_Monster_LightningShieldV2_AbilityGroup, 2) },
                    { 1, new RankSpellConstructor("ChaosWave", VPlus.Data.Prefabs.AB_Bandit_Tourok_VBlood_ChaosWave_AbilityGroup, 1) },
                };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }
        public class RankSpellConstructor
        {
            public string Name { get; set; }
            public PrefabGUID SpellGUID { get; set; }
            public int RequiredRank { get; set; }

            public RankSpellConstructor(string name, PrefabGUID spellGUID, int requiredRank)
            {
                Name = name;
                SpellGUID = spellGUID;
                RequiredRank = requiredRank;
            }
        }

        public static class ClassFactory
        {
            // Method to create class instance based on class choice
            public static ICharacterClass CreateClassInstance(string className)
            {
                switch (className.ToLower())
                {
                    case "nightmarshal": return new Nightmarshal();
                    case "paladin": return new Paladin();
                    // Add more cases as needed
                    default: return null;
                }
            }
        }


        [Command(name: "chooseSpell", shortHand: "cs", adminOnly: false, usage: ".cs <#>", description: "Sets class spell to shift.")]

        public static void SpellChoice(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData rankData))
            {
                // Use factory to get the class instance
                var classInstance = ClassFactory.CreateClassInstance(rankData.ClassChoice);
                if (classInstance != null && classInstance.TryGetSpell(choice, out RankSpellConstructor spellConstructor))
                {
                    if (rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        PrefabGUID newSpell = spellConstructor.SpellGUID;
                        VPlus.Data.FoundPrefabGuid foundPrefabGuid = new VPlus.Data.FoundPrefabGuid(newSpell);
                        rankData.RankSpell = newSpell.GuidHash;
                        // Assuming CastCommand is a method to cast the spell
                        // CastCommand(ctx, foundPrefabGuid, null);
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                        ChatCommands.SavePlayerRanks();
                    }
                    else
                    {
                        ctx.Reply($"You must be at least rank {spellConstructor.RequiredRank} to use this ability.");
                    }
                }
                else
                {
                    ctx.Reply("Invalid class or spell choice.");
                }
            }
            else
            {
                ctx.Reply("Your rank data could not be found.");
            }
        }

        [Command(name: "chooseClass", shortHand: "cc", adminOnly: false, usage: ".cc <className>", description: "Sets class to use spells from.")]
        public static void ChooseClass(ChatCommandContext ctx, string className)
        {
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData rankData))
            {
                // Check if the class name provided is valid
                var classInstance = ClassFactory.CreateClassInstance(className);
                if (classInstance != null)
                {
                    // Update the player's class choice
                    rankData.ClassChoice = className.ToLower();
                    ctx.Reply($"Class set to {className}. You can now use spells associated with this class.");
                    ChatCommands.SavePlayerRanks();
                }
                else
                {
                    ctx.Reply($"Invalid class name: {className}. Please choose a valid class.");
                }
            }
            else
            {
                ctx.Reply("Your rank data could not be found.");
            }
        }
        public static void CastCommand(ChatCommandContext ctx, FoundPrefabGuid prefabGuid, FoundPlayer player = null)
        {
            PlayerService.Player player1;
            Entity entity1;
            if ((object)player == null)
            {
                entity1 = ctx.Event.SenderUserEntity;
            }
            else
            {
                player1 = player.Value;
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
                player1 = player.Value;
                entity3 = player1.Character;
            }
            Entity entity4 = entity3;
            FromCharacter fromCharacter = new FromCharacter()
            {
                User = entity2,
                Character = entity4
            };
            DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            CastAbilityServerDebugEvent serverDebugEvent = new CastAbilityServerDebugEvent()
            {
                AbilityGroup = prefabGuid.Value,
                AimPosition = new Nullable_Unboxed<float3>(entity2.Read<EntityInput>().AimPosition),
                Who = entity4.Read<NetworkId>()
            };
            existingSystem.CastAbilityServerDebugEvent(entity2.Read<User>().Index, ref serverDebugEvent, ref fromCharacter);
        }

    }
}
