using Bloodstone.API;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;

namespace RPGAddOns
{
    [CommandGroup(name: "rpg", shortHand: "rpg")]
    internal class Commands
    {
        [Command(name: "rankup", shortHand: "ru", adminOnly: false, usage: "Resets your rank points and increases your rank.", description: "Resets your rank points and grants a buff.")]
        public static void ResetPointsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            string StringID = SteamID.ToString();

            // Call the ResetPoints method from Prestige

            //PrestigeCommands.ResetPoints(ctx, name, SteamID, StringID);
        }

        [Command(name: "prestige", shortHand: "pr", adminOnly: false, usage: "Use this command to reset your level to 1 after reaching max level to receive extra perks.", description: "Reset your level for extras.")]
        public static void ResetLevelCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();

            // Call the ResetLevel method from ResetLevelRPG

            //EntityManager entityManager = default;
            ResetLevel.ResetPlayerLevel(ctx, name, SteamID);
        }

        [Command(name: "getpoints", shortHand: "gp", adminOnly: false, usage: "Check your current rank points.", description: "Displays the number of points possessed by the player.")]
        public static void CheckPointsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRank.TryGetValue(SteamID, out RankData data))
            {
                double percentage = 100 * ((double)data.Points / ((data.Level * 1000) + 1000));
                string integer = ((int)percentage).ToString();
                ctx.Reply($"You have {data.Points} out of the {(data.Level * 1000) + 1000} points required to increase your rank. ({integer}%)");
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "getrank", shortHand: "gr", adminOnly: false, usage: "Check your current rank level.", description: "Displays the level of rank possessed by the player.")]
        public static void CheckRankCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRank.TryGetValue(SteamID, out RankData data))
            {
                ctx.Reply($"You are rank {data.Level}.");
            }
            else
            {
                ctx.Reply("You don't have a rank yet.");
            }
        }

        [Command(name: "getprestiges", shortHand: "gpr", adminOnly: false, usage: "Check your current prestige count.", description: "Displays the number of times you have reset your level.")]
        public static void CheckResetsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerResetCountsBuffs.TryGetValue(SteamID, out ResetData data))
            {
                ctx.Reply($"Your current reset count is: {data.ResetCount}");
            }
            else
            {
                ctx.Reply("You have not reset your level yet.");
            }
        }

        [Command(name: "getbuffs", shortHand: "gb", adminOnly: false, usage: "Check your current permanent buffs.", description: "Displays the buffs you have received from resets.")]
        public static void CheckBuffsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerResetCountsBuffs.TryGetValue(SteamID, out ResetData data))
            {
                var buffs = data.Buffs.Count > 0 ? string.Join(", ", data.Buffs) : "None";
                ctx.Reply($"Your current buffs are: {buffs}");
            }
            else
            {
                ctx.Reply("You have not received any buffs yet.");
            }
        }

        [Command(name: "wiperesets", shortHand: "wr", adminOnly: true, usage: ".rpg wr <PlayerName>", description: "Resets the specified user's reset count and buffs to the initial state. Does not wipe any buffs they already have but that would probably be good to add here")]
        public static void WipeProgressCommand(ChatCommandContext ctx, string playerName)
        {
            // Find the user's SteamID based on the playerName

            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);
            if (Databases.playerResetCountsBuffs.ContainsKey(SteamID))
            {
                // Reset the user's progress
                Databases.playerResetCountsBuffs[SteamID] = new ResetData(0, []);
                Commands.SavePlayerResets();  // Assuming this method saves the data to a persistent storage

                ctx.Reply($"Progress for player {playerName} has been wiped.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no progress to wipe.");
            }
        }

        [Command(name: "getresetdata", shortHand: "grd", adminOnly: true, usage: "", description: "Retrieves the reset count and buffs for a specified player.")]
        public static void GetPlayerResetDataCommand(ChatCommandContext ctx, string playerName)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);

            if (SteamID != 0 && Databases.playerResetCountsBuffs.TryGetValue(SteamID, out ResetData data))
            {
                var buffsList = data.Buffs.Count > 0 ? string.Join(", ", data.Buffs) : "None";
                ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Reset Count: {data.ResetCount}, Buffs: {buffsList}");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no reset data available.");
            }
        }

        private static EntityManager entityManager = VWorld.Server.EntityManager;

        public static void LoadData()
        {
            if (!File.Exists(Plugin.PlayerPrestigeJson))
            {
                var stream = File.Create(Plugin.PlayerPrestigeJson);
                stream.Dispose();
            }

            string json = File.ReadAllText(Plugin.PlayerPrestigeJson);
            try
            {
                Databases.playerRank = JsonSerializer.Deserialize<Dictionary<ulong, RankData>>(json);
                Plugin.Logger.LogWarning("Player Prestige Populated");
            }
            catch
            {
                Databases.playerRank = new Dictionary<ulong, RankData>();
                Plugin.Logger.LogWarning("Player Prestige Created");
            }
            if (!File.Exists(Plugin.PlayerResetCountsBuffsJson))
            {
                var stream = File.Create(Plugin.PlayerResetCountsBuffsJson);
                stream.Dispose();
            }

            json = File.ReadAllText(Plugin.PlayerResetCountsBuffsJson);
            try
            {
                Databases.playerResetCountsBuffs = JsonSerializer.Deserialize<Dictionary<ulong, ResetData>>(json);
                Plugin.Logger.LogWarning("Player ResetCountsBuffs Populated");
            }
            catch
            {
                Databases.playerResetCountsBuffs = new Dictionary<ulong, ResetData>();
                Plugin.Logger.LogWarning("Player ResetCountsBuffs Created");
            }
        }

        public static void SavePlayerResets()
        {
            File.WriteAllText(Plugin.PlayerResetCountsBuffsJson, JsonSerializer.Serialize(Databases.playerResetCountsBuffs));
        }

        public static void SavePlayerPrestige()
        {
            File.WriteAllText(Plugin.PlayerPrestigeJson, JsonSerializer.Serialize(Databases.playerRank));
        }
    }
}