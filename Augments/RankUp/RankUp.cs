﻿using AdminCommands.Commands.Converters;
using ProjectM;
using RPGAddOnsEx.Core;
using System.Text.RegularExpressions;
using VampireCommandFramework;

namespace RPGAddOnsEx.Augments.RankUp
{
    public class RankData
    {
        public int Rank { get; set; }
        public int Points { get; set; }
        public List<int> Buffs { get; set; } = new List<int>();

        public DateTime LastAbilityUse { get; set; }

        public int RankSpell { get; set; }

        public List<int> Spells { get; set; } = new List<int>();

        public RankData(int rank, int points, List<int> buffs, int rankSpell, List<int> spells)
        {
            Rank = rank;
            Points = points;
            Buffs = buffs;
            LastAbilityUse = DateTime.MinValue; // Initialize to ensure it's always set
            RankSpell = rankSpell;
            Spells = spells;
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
                    WillisCore.Helper.BuffPlayerByName(playerName, buffguid, 0, true);
                    ctx.Reply($"You've been granted a permanent buff: {buffname}");
                }
            }
            data.Rank++;
            data.Points = 0;
            data.Buffs = playerBuffs;
            ctx.Reply($"Congratulations {playerName}! You have increased your PvE rank to {data.Rank}.");
            //lightning bolt goes here

            PrefabGUID lightning = new PrefabGUID(1365358996);
            FoundPrefabGuid foundPrefabGuid = new(lightning);
            CastCommands.CastCommand(ctx, foundPrefabGuid, null);
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
            buffname = AdminCommands.ECSExtensions.LookupName(buffguid);
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
    }
}