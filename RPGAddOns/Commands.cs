using Bloodstone.API;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;

namespace RPGAddOns
{
    [CommandGroup(name: "rpg", shortHand: "rpg")]
    internal class Commands
    {
        [Command(name: "setrankpoints", shortHand: "sp", adminOnly: true, usage: ".rpg sp <PlayerName> <Points>", description: "Sets the rank points for a specified player.")]
        public static void SetRankPointsCommand(ChatCommandContext ctx, string playerName, int points)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);

            if (SteamID != 0 && Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                // Set the user's rank points, prevent more points than rank allows
                data.Points = points;
                if (points < 0)
                {
                    ctx.Reply("Points cannot be negative.");
                    return;
                }
                if (data.Points > (data.Rank * 1000) + 1000)
                {
                    data.Points = (data.Rank * 1000) + 1000;
                }
                Databases.playerRanks[SteamID] = data;
                Commands.SavePlayerRanks();  // Save the updated rank data

                ctx.Reply($"Rank points for player {playerName} have been set to {points}.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no rank data available.");
            }
        }

        [Command(name: "rankup", shortHand: "ru", adminOnly: false, usage: ".rpg ru", description: "Resets your rank points and increases your rank, granting a buff if applicable.")]
        public static void ResetPointsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            string StringID = SteamID.ToString();
            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                PvERankSystem.RankUp(ctx, name, SteamID, data);
            }
            else
            {
                double percentage = 100 * ((double)data.Points / ((data.Rank * 1000) + 1000));
                string integer = ((int)percentage).ToString();
                ctx.Reply($"You have {data.Points} out of the {(data.Rank * 1000) + 1000} points required to increase your rank. ({integer}%)");
            }
            // Call the ResetPoints method from Prestige
        }

        [Command(name: "prestige", shortHand: "pr", adminOnly: false, usage: ".rpg pr", description: "Resets your level to 1 after reaching max level, offering extra perks.")]
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

        [Command(name: "getpoints", shortHand: "gp", adminOnly: false, usage: ".rpg gp", description: "Displays your current rank points and progress towards the next rank.")]
        public static void CheckPointsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                double percentage = 100 * ((double)data.Points / ((data.Rank * 1000) + 1000));
                string integer = ((int)percentage).ToString();
                ctx.Reply($"You have {data.Points} out of the {(data.Rank * 1000) + 1000} points required to increase your rank. ({integer}%)");
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "getranks", shortHand: "gr", adminOnly: false, usage: ".rpg gr", description: "Shows your current rank level.")]
        public static void CheckRankCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                ctx.Reply($"You are rank {data.Rank}.");
            }
            else
            {
                ctx.Reply("You don't have a rank yet.");
            }
        }

        [Command(name: "getprestiges", shortHand: "gpr", adminOnly: false, usage: ".rpg gpr", description: "Displays the number of times you've reset your level (prestige count).")]
        public static void CheckResetsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerPrestiges.TryGetValue(SteamID, out PrestigeData data))
            {
                ctx.Reply($"Your current reset count is: {data.ResetCount}");
            }
            else
            {
                ctx.Reply("You have not reset your level yet.");
            }
        }

        [Command(name: "getrankbuffs", shortHand: "grb", adminOnly: false, usage: ".rpg grb", description: "Checks and displays the buffs received from your current rank.")]
        public static void CheckRankBuffsCommand(ChatCommandContext ctx)

        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                var buffs = data.Buffs.Count > 0 ? string.Join(", ", data.Buffs) : "None";
                ctx.Reply($"Your current rank buffs are: {buffs}");
            }
            else
            {
                ctx.Reply("You have not received any rank buffs yet.");
            }
        }

        [Command(name: "getprestigebuffs", shortHand: "gpb", adminOnly: false, usage: ".rpg gpb", description: "Shows the permanent buffs you've gained from prestige resets.")]
        public static void CheckBuffsCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerPrestiges.TryGetValue(SteamID, out PrestigeData data))
            {
                var buffs = data.Buffs.Count > 0 ? string.Join(", ", data.Buffs) : "None";
                ctx.Reply($"Your current buffs are: {buffs}");
            }
            else
            {
                ctx.Reply("You have not received any buffs yet.");
            }
        }

        [Command(name: "wiperesets", shortHand: "wr", adminOnly: true, usage: ".rpg wr <PlayerName>", description: "Resets a player's reset count and buffs to their initial state.")]
        public static void WipeProgressCommand(ChatCommandContext ctx, string playerName)
        {
            // Find the user's SteamID based on the playerName

            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);
            if (Databases.playerPrestiges.ContainsKey(SteamID))
            {
                // Reset the user's progress
                Databases.playerPrestiges[SteamID] = new PrestigeData(0, []);
                Commands.SavePlayerPrestiges();  // Assuming this method saves the data to a persistent storage

                ctx.Reply($"Progress for player {playerName} has been wiped.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no progress to wipe.");
            }
        }

        [Command(name: "getresetdata", shortHand: "grd", adminOnly: true, usage: ".rpg grd <PlayerName>", description: "Retrieves the reset count and buffs for a specified player.")]
        public static void GetPlayerResetDataCommand(ChatCommandContext ctx, string playerName)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);

            if (SteamID != 0 && Databases.playerPrestiges.TryGetValue(SteamID, out PrestigeData data))
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
            if (!File.Exists(Plugin.PlayerPrestigesJson))
            {
                var stream = File.Create(Plugin.PlayerPrestigesJson);
                stream.Dispose();
            }

            string json = File.ReadAllText(Plugin.PlayerPrestigesJson);
            try
            {
                Databases.playerRanks = JsonSerializer.Deserialize<Dictionary<ulong, RankData>>(json);
                Plugin.Logger.LogWarning("PlayerRanks Created");
            }
            catch
            {
                Databases.playerRanks = new Dictionary<ulong, RankData>();
                Plugin.Logger.LogWarning("PlayerRanks Created");
            }
            if (!File.Exists(Plugin.PlayerRanksJson))
            {
                var stream = File.Create(Plugin.PlayerRanksJson);
                stream.Dispose();
            }

            json = File.ReadAllText(Plugin.PlayerRanksJson);
            try
            {
                Databases.playerPrestiges = JsonSerializer.Deserialize<Dictionary<ulong, PrestigeData>>(json);
                Plugin.Logger.LogWarning("PlayerPrestiges Populated");
            }
            catch
            {
                Databases.playerPrestiges = new Dictionary<ulong, PrestigeData>();
                Plugin.Logger.LogWarning("PlayerPrestiges Created");
            }
        }

        public static void SavePlayerPrestiges()
        {
            File.WriteAllText(Plugin.PlayerPrestigesJson, JsonSerializer.Serialize(Databases.playerPrestiges));
        }

        public static void SavePlayerRanks()
        {
            File.WriteAllText(Plugin.PlayerRanksJson, JsonSerializer.Serialize(Databases.playerRanks));
        }
    }
}