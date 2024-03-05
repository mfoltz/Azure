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
using VBuild.Data;
using VBuild.Core.Toolbox;
using DateTime = System.DateTime;
using static ProjectM.Tiles.TileConstants;
using VPlus.Core.Toolbox;
using Il2CppSystem.Security.Cryptography;
using ECSExtensions = VPlus.Core.Toolbox.ECSExtensions;
using Databases = VPlus.Data.Databases;
using VBuild.Core.Converters;

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

            PrefabGUID lightning = VBuild.Data.Prefabs.AB_Militia_BishopOfDunley_SummonEyeOfGod_AbilityGroup;
            VBuild.Core.Converters.FoundPrefabGuid foundPrefabGuid = new(lightning);
            VBuild.Core.CoreCommands.CastCommand(ctx, foundPrefabGuid,null);
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
        
       
        public class Nightmarshal
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Nightmarshal()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
                    { 5, new RankSpellConstructor("BatSwarm", VBuild.Data.Prefabs.AB_BatVampire_BatSwarm_AbilityGroup.GuidHash, 5) },
                    { 4, new RankSpellConstructor("NightDash", VBuild.Data.Prefabs.AB_BatVampire_NightDash_AbilityGroup.GuidHash, 4) },
                    { 3, new RankSpellConstructor("BatStorm", VBuild.Data.Prefabs.AB_BatVampire_BatStorm_AbilityGroup.GuidHash, 3) },
                    { 2, new RankSpellConstructor("MeleeAttack", VBuild.Data.Prefabs.AB_BatVampire_MeleeAttack_AbilityGroup.GuidHash, 2) },
                    { 1, new RankSpellConstructor("BatWhirlwind", VBuild.Data.Prefabs.AB_BatVampire_Whirlwind_AbilityGroup.GuidHash, 1) },
                };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }
        public class Paladin
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Paladin()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
                    { 5, new RankSpellConstructor("HolySpinners", VBuild.Data.Prefabs.AB_ChurchOfLight_Paladin_HolySpinners_AbilityGroup.GuidHash, 5) },
                    { 4, new RankSpellConstructor("DivineRays", VBuild.Data.Prefabs.AB_ChurchOfLight_Paladin_DivineRays_AbilityGroup.GuidHash, 4) },
                    { 3, new RankSpellConstructor("HolyFlackCannon", VBuild.Data.Prefabs.AB_ChurchOfLight_Paladin_HolyFlackCannon_AbilityGroup.GuidHash, 3) },
                    { 2, new RankSpellConstructor("ChargedSwing", VBuild.Data.Prefabs.AB_ChurchOfLight_Paladin_ChargedSwing_AbilityGroup.GuidHash, 2) },
                    { 1, new RankSpellConstructor("EmpoweredMelee", VBuild.Data.Prefabs.AB_ChurchOfLight_Paladin_EmpoweredMelee_AbilityGroup.GuidHash, 1) },
                };
            }

            public bool TryGetSpell(int choice, out RankSpellConstructor spellConstructor)
            {
                return Spells.TryGetValue(choice, out spellConstructor);
            }
        }

        public class Default
        {
            public Dictionary<int, RankSpellConstructor> Spells { get; }

            public Default()
            {
                Spells = new Dictionary<int, RankSpellConstructor>
                {
                    { 5, new RankSpellConstructor("DivineRays", VBuild.Data.Prefabs.AB_ChurchOfLight_Paladin_DivineRays_AbilityGroup.GuidHash, 5) },
                    { 4, new RankSpellConstructor("NightDash", VBuild.Data.Prefabs.AB_BatVampire_NightDash_Dash_AbilityGroup.GuidHash, 4) },
                    { 3, new RankSpellConstructor("HealBomb", VBuild.Data.Prefabs.AB_ChurchOfLight_Priest_HealBomb_AbilityGroup.GuidHash, 3) },
                    { 2, new RankSpellConstructor("MeleeAlt", VBuild.Data.Prefabs.AB_BatVampire_MeleeAttack_AbilityGroup.GuidHash, 2) },
                    { 1, new RankSpellConstructor("BatWhirlwind", VBuild.Data.Prefabs.AB_BatVampire_Whirlwind_AbilityGroup.GuidHash, 1) },
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
            public int SpellGUID { get; set; }
            public int RequiredRank { get; set; }

            public RankSpellConstructor(string name, int spellGUID, int requiredRank)
            {
                Name = name;
                SpellGUID = spellGUID;
                RequiredRank = requiredRank;
            }
        }

        public static class ClassFactory
        {
            public static object CreateClassInstance(string className)
            {
                switch (className.ToLower())
                {
                    case "nightmarshal": return new Nightmarshal();
                    case "paladin": return new Paladin();
                    case "default": return new Default();
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
                var classInstance = ClassFactory.CreateClassInstance(rankData.ClassChoice);
                if (classInstance is Nightmarshal nightmarshal)
                {
                    if (nightmarshal.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                }
                else if (classInstance is Paladin paladin)
                {
                    if (paladin.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Paladin
                }
                else if (classInstance is Default defaultClass)
                {
                    if (defaultClass.Spells.TryGetValue(choice, out RankSpellConstructor spellConstructor) && rankData.Rank >= spellConstructor.RequiredRank)
                    {
                        // Logic to apply the spell
                        rankData.RankSpell = spellConstructor.SpellGUID;
                        ChatCommands.SavePlayerRanks();
                        ctx.Reply($"Rank spell set to {spellConstructor.Name}.");
                    }
                    else
                    {
                        ctx.Reply($"Invalid spell choice or rank requirement not met. ({spellConstructor.RequiredRank})");
                    }
                    // Similar logic for Default
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

        

        

        [Command(name: "chooseClass", shortHand: "cc", adminOnly: true, usage: ".cc <className>", description: "Sets class to use spells from.")]
        public static void ChooseClass(ChatCommandContext ctx, string className)
        {
            string nameClass = className.ToLower();
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData rankData))
            {
                // Check if the class name provided is valid
                var classInstance = ClassFactory.CreateClassInstance(nameClass);
                if (classInstance != null)
                {
                    // Update the player's class choice
                    rankData.ClassChoice = nameClass;
                    ctx.Reply($"Class set to {nameClass}. You can now use spells associated with this class.");
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
        
       
    }
}
