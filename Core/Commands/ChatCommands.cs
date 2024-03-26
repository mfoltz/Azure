using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Entities;
using UnityEngine;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VampireCommandFramework;
using VRising.GameData;
using VRising.GameData.Models;
using VPlus.Core.Toolbox;
using Databases = VPlus.Data.Databases;
using System.Text;
using VCreate.Core.Toolbox;
using static VCreate.Core.Services.PlayerService;
using VRising.GameData.Utils;
using static VPlus.Augments.Ascension;
using RPGMods.Utils;
using VRising.GameData.Methods;
using Helper = VCreate.Core.Toolbox.Helper;

namespace VPlus.Core.Commands
{
    public class ChatCommands
    {


        private static readonly string redV = VPlus.Core.Toolbox.FontColors.Red("V");


        [Command(name: "starterKit", shortHand: "start", adminOnly: false, usage: ".start", description: "Provides starting kit.")]

        public static void KitMe(ChatCommandContext ctx)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            if (Databases.playerDivinity.TryGetValue(ctx.Event.User.PlatformId, out DivineData data) && !data.Spawned)
            {
                data.Spawned = true;
                Databases.playerDivinity[ctx.Event.User.PlatformId] = data;
                ChatCommands.SavePlayerDivinity();
                UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(ctx.Event.User.PlatformId);
                foreach (var item in VPlus.Hooks.ReplaceAbilityOnSlotSystem_Patch.keyValuePairs.Keys)
                {
                    userModel.TryGiveItem(item, VPlus.Hooks.ReplaceAbilityOnSlotSystem_Patch.keyValuePairs[item], out var _);
                }
                ServerChatUtils.SendSystemMessageToClient(entityManager, ctx.Event.User, "You've received a starting kit with blood essence, stone, wood, coins, and health potions!");
            }
            else
            {
                ctx.Reply("You've already received your starting kit.");
            }
            
        }
        [Command(name: "resetVision", shortHand: "vision", adminOnly: false, usage: ".vision", description: "Removes farsight.")]

        public static void ResetVision(ChatCommandContext ctx)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            Vision vision = character.Read<Vision>();
            vision.Range._Value = 40f;
            character.Write(vision);
            ctx.Reply("Farsight removed.");
        }

        [Command(name: "redeemPoints", shortHand: "redeem", adminOnly: false, usage: ".redeem", description: "Redeems all VTokens for the crystal equivalent, drops if inventory full.")]
        public static void RedeemPoints(ChatCommandContext ctx)
        {
            if (!Plugin.VTokens)
            {
                ctx.Reply("VPoints is disabled.");
                return;
            }
            // Find the user's SteamID based on the playerName
            User user = ctx.Event.User;
            string playerName = user.CharacterName.ToString();
            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            Entity characterEntity = userModel.FromCharacter.Character;
            ulong SteamID = user.PlatformId;
            if (Databases.playerDivinity.TryGetValue(SteamID, out DivineData data))
            {
                if (data.VTokens < Plugin.RewardFactor)
                {
                    ctx.Reply($"You need at least {VPlus.Core.Toolbox.FontColors.Yellow(Plugin.RewardFactor.ToString())} VTokens to redeem for a crystal. ({VPlus.Core.Toolbox.FontColors.White(data.VTokens.ToString())})");
                    return;
                }
                int reward = data.VTokens / Plugin.RewardFactor;

                // Calculate the exact cost in VPoints for those rewards
                int cost = reward * Plugin.RewardFactor;

                // Subtract the cost from the player's VPoints
                PrefabGUID prefabGUID = new(Plugin.VTokensItemPrefab);
                bool success = Helper.AddItemToInventory(characterEntity, prefabGUID, reward, out Entity entity);
                if (!success)
                {
                    //inventory full probably
                    InventoryUtilitiesServer.CreateDropItem(VWorld.Server.EntityManager, characterEntity, prefabGUID, reward, entity);
                }

                data.VTokens -= cost;
                int remainder = data.VTokens;
                ctx.Reply($"{redV}Tokens redeemed for {VPlus.Core.Toolbox.FontColors.White(reward.ToString())} {VPlus.Core.Toolbox.FontColors.Pink("crystal(s)")}.");
            }
            else
            {
                ctx.Reply($"You don't have any {redV}Tokens to redeem yet.");
            }
        }

        [Command(name: "wipePlayerRank", shortHand: "wpr", adminOnly: true, usage: ".wpr [Player]", description: "Resets a player's rank count.")]
        public static void WipeRanksCommand(ChatCommandContext ctx, string playerName)
        {
            if (!Plugin.PlayerRankUp)
            {
                ctx.Reply("PvE Rank is disabled.");
                return;
            }
            // Find the user's SteamID based on the playerName
            ulong SteamID;
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity player, out Entity user))
            {
                if (VWorld.Server.EntityManager.TryGetComponentData(user, out User component))
                {
                    SteamID = component.PlatformId;

                    if (Databases.playerRanks.ContainsKey(SteamID))
                    {
                        // Reset the user's progress
                        var buffsToWipe = Databases.playerRanks[SteamID].Buffs;
                        Databases.playerRanks[SteamID] = new RankData(0, 0, [], 0, [0, 0], "none", false);
                        foreach (var buff in buffsToWipe)
                        {
                            PrefabGUID buffguid = new(buff);
                            Helper.UnbuffCharacter(player, buffguid);
                        }

                        SavePlayerRanks();

                        ctx.Reply($"Progress for player {playerName} has been wiped.");
                    }
                    else
                    {
                        ctx.Reply($"Player {playerName} has no progress to wipe.");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        [Command(name: "setRankPoints", shortHand: "srp", adminOnly: true, usage: ".srp [Player] [Points]", description: "Sets the rank points for a player.")]
        public static void SetRankPointsCommand(ChatCommandContext ctx, string playerName, int points)
        {
            if (!Plugin.PlayerRankUp)
            {
                ctx.Reply("PvE Rank is disabled.");
                return;
            }

            ulong SteamID;
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity player, out Entity user))
            {
                if (VWorld.Server.EntityManager.TryGetComponentData(user, out User component))
                {
                    SteamID = component.PlatformId;

                    if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
                    {
                        // Set the user's rank points, prevent more points than rank allows
                        data.Points = points;
                        if (points < 0)
                        {
                            ctx.Reply("Points cannot be negative.");
                            return;
                        }
                        if (data.Points > data.Rank * 1000 + 1000)
                        {
                            data.Points = data.Rank * 1000 + 1000;
                        }
                        Databases.playerRanks[SteamID] = data;
                        SavePlayerRanks();
                        // Save the updated rank data

                        ctx.Reply($"Rank points for player {playerName} have been set to {points}.");
                    }
                    else
                    {
                        if (points < 0)
                        {
                            ctx.Reply("Points cannot be negative.");
                            return;
                        }
                        // make data for them if none found
                        RankData rankData = new(0, points, [], 0, [0, 0], "none", false);
                        if (rankData.Points > rankData.Rank * 1000 + 1000)
                        {
                            rankData.Points = rankData.Rank * 1000 + 1000;
                        }
                        Databases.playerRanks.Add(SteamID, rankData);
                        SavePlayerRanks();
                        ctx.Reply($"Rank points for player {playerName} have been set to {points}.");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        [Command(name: "setPlayerRank", shortHand: "spr", adminOnly: true, usage: ".spr [Player] [#]", description: "Sets the rank for a player if they don't have any data.")]
        public static void SetRankCommand(ChatCommandContext ctx, string playerName, int rank)
        {
            if (Plugin.PlayerRankUp == false)
            {
                ctx.Reply("PvE Rank is disabled.");
                return;
            }

            ulong SteamID;
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity player, out Entity user))
            {
                if (VWorld.Server.EntityManager.TryGetComponentData(user, out User component))
                {
                    SteamID = component.PlatformId;
                    if (rank < 0)
                    {
                        ctx.Reply("Rank cannot be negative.");
                        return;
                    }
                    if (rank > Plugin.MaxRanks)
                    {
                        ctx.Reply("Rank cannot exceed the maximum rank.");
                        return;
                    }
                    List<int> playerBuffs = [];
                    var buffstring = Plugin.BuffPrefabsRankUp;
                    var buffList = Regex.Matches(buffstring, @"-?\d+")
                                           .Cast<Match>()
                                           .Select(m => int.Parse(m.Value))
                                           .ToList();
                    var counter = 0;
                    if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
                    {
                        if (data.Rank > 0)
                        {
                            ctx.Reply("Player already has a rank, use .rpg wpr to reset their rank.");
                            return;
                        }
                        data.Rank = rank;
                        data.Points = 0;
                        /*
                        for (int i = 0; i <= rank; i++)
                        {
                            playerBuffs.Add(buffList[i]);
                        }
                        */
                        foreach (var buff in buffList)
                        {
                            PrefabGUID buffguid = new(buff);
                            counter += 1;

                            playerBuffs.Add(buff);
                            if (counter == rank)
                            {
                                break;
                            }
                        }
                        data.Buffs = playerBuffs;

                        Databases.playerRanks[SteamID] = data;

                        SavePlayerRanks();
                        ctx.Reply($"Rank for player {playerName} has been set to {rank}, they can use .rpg bs to apply their buffs.");
                    }
                    else
                    {
                        RankData rankData = new(rank, 0, [], 0, [0, 0], "none", false);
                        /*
                        for (int i = 0; i <= rank; i++)
                        {
                            playerBuffs.Add(buffList[i]);
                        }
                        */
                        foreach (var buff in buffList)
                        {
                            PrefabGUID buffguid = new(buff);
                            counter += 1;

                            playerBuffs.Add(buff);
                            if (counter == rank)
                            {
                                break;
                            }
                        }
                        data.Buffs = playerBuffs;
                        Databases.playerRanks.Add(SteamID, rankData);

                        SavePlayerRanks();
                        ctx.Reply($"Rank for player {playerName} has been set to {rank}, they can use .sync to apply their buffs.");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        [Command(name: "rankUp", shortHand: "rankup", adminOnly: false, usage: ".rankup", description: "Resets your rank points and increases your rank, granting any applicable rewards.")]
        public static void RankUpCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerRankUp == false)
            {
                ctx.Reply("PvE Rank is disabled.");
                return;
            }
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                if (data.Rank >= Plugin.MaxRanks)
                {
                    ctx.Reply($"You have reached the maximum rank. ({Plugin.MaxRanks})");
                    return;
                }
                if (data.Points >= data.Rank * 1000 + 1000)
                {
                    PvERankSystem.RankUp(ctx, name, SteamID, data);
                }
                else
                {
                    double percentage = 100 * ((double)data.Points / (data.Rank * 1000 + 1000));
                    string integer = ((int)percentage).ToString();
                    var colorString = FontColors.Yellow(integer);
                    string colorPoints1 = FontColors.White(data.Points.ToString());
                    string colorPoints2 = FontColors.White((data.Rank * 1000 + 1000).ToString());
                    ctx.Reply($"You have {colorPoints1} out of the {colorPoints2} points required to increase your rank. ({colorString}%)");
                }
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "syncRankBuffs", shortHand: "sync", adminOnly: false, usage: ".sync", description: "Syncs your buffs to your rank and shows you which buffs you should have.")]
        public static void BuffSyncCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerRankUp == false)
            {
                ctx.Reply("PvE Rank is disabled.");
                return;
            }
            var user = ctx.Event.User;
            var player = ctx.Event.SenderCharacterEntity;
            string name = user.CharacterName.ToString();
            var SteamID = user.PlatformId;
            string StringID = SteamID.ToString();

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                //

                try
                {
                    var playerBuffs = data.Buffs;
                    foreach (var buff in playerBuffs)
                    {
                        PrefabGUID buffguid = new(buff);

                        Helper.BuffPlayerByName(ctx.Name, buffguid, 0, true);
                        string colorString = FontColors.Cyan(buffguid.LookupName());
                        ctx.Reply($"Applying {colorString} if not already present...");
                    }
                    ctx.Reply("Rank buffs synced.");
                }
                catch (Exception ex)
                {
                    ctx.Reply($"Error syncing rank buffs: {ex}");
                }
            }
            else
            {
                ctx.Reply("You don't have any rank data yet.");
            }
        }

        [Command(name: "getRank", shortHand: "getrank", adminOnly: false, usage: ".getrank", description: "Displays your current rank points and progress towards the next rank along with current rank.")]
        public static void GetRankCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerRankUp == false)
            {
                ctx.Reply("PvE Rank is disabled.");
                return;
            }
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                double percentage = 100 * ((double)data.Points / (data.Rank * 1000 + 1000));
                string integer = ((int)percentage).ToString();
                var colorString = FontColors.Yellow(integer);
                string colorPoints1 = FontColors.White(data.Points.ToString());
                string colorPoints2 = FontColors.White((data.Rank * 1000 + 1000).ToString());
                string colorString1 = FontColors.Red("max");
                if (data.Rank >= Plugin.MaxRanks)
                {
                    ctx.Reply($"You have reached {colorString1} rank.");
                    return;
                }
                string color = FontColors.Purple(data.Rank.ToString());
                ctx.Reply($"You are rank {color} and have {colorPoints1} out of the {colorPoints2} points required to increase your rank. ({colorString}%)");
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "getPlayerRank", shortHand: "gpr", adminOnly: true, usage: ".gpr [Player]", description: "Helps admins check player rank data.")]
        public static void GetPlayerRankCommand(ChatCommandContext ctx, string playerName)
        {
            if (Plugin.PlayerRankUp == false)
            {
                ctx.Reply("PvE Rank is disabled.");
                return;
            }
            ulong SteamID;
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity player, out Entity user))
            {
                if (VWorld.Server.EntityManager.TryGetComponentData(user, out User component))
                {
                    SteamID = component.PlatformId;

                    if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
                    {
                        ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Rank: {data.Rank}, Points: {data.Points}");
                    }
                    else
                    {
                        ctx.Reply($"Player {playerName} has no rank data available.");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        // prestige commands for players and admins should all be in this block unless otherwise mentioned
        [Command(name: "visualBuff", shortHand: "visual", adminOnly: false, usage: ".visual [#]", description: "Applies a visual buff you've earned through prestige.")]
        public static void VisualBuffCommand(ChatCommandContext ctx, int buff)
        {
            if (Plugin.BuffRewardsPrestige == false)
            {
                ctx.Reply("Visual buffs are disabled.");
                return;
            }
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;

            // check if player has prestiged
            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                // handle applying chosen visual buff
                PrestigeSystem.PrestigeFunctions.BuffChecker(ctx, buff, data);
            }
            else
            {
                ctx.Reply($"You haven't prestiged yet.");
            }
        }

        [Command(name: "playerPrestige", shortHand: "prestige", adminOnly: false, usage: ".prestige", description: "Resets your level to 1 after reaching max, offering extra perks.")]
        public static void PrestigeCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerPrestige == false)
            {
                ctx.Reply("Prestige is disabled.");
                return;
            }
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();

            PrestigeSystem.PrestigeCheck(ctx, name, SteamID);
        }

        [Command(name: "getPrestige", shortHand: "getprestige", adminOnly: false, usage: ".getprestige", description: "Displays the number of times you've prestiged.")]
        public static void GetPrestigeCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerPrestige == false)
            {
                ctx.Reply("Prestige is disabled.");
                return;
            }
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                ctx.Reply($"Your current prestige count is: {data.Prestiges}");
            }
            else
            {
                ctx.Reply("You have not prestiged yet.");
            }
        }

        [Command(name: "wipePlayerPrestige", shortHand: "wpp", adminOnly: true, usage: ".wpp [Player]", description: "Resets a player's prestige data.")]
        public static void WipePrestigeCommand(ChatCommandContext ctx, string playerName)
        {
            if (Plugin.PlayerPrestige == false)
            {
                ctx.Reply("Prestige is disabled.");
                return;
            }

            ulong SteamID;
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity player, out Entity user))
            {
                if (VWorld.Server.EntityManager.TryGetComponentData(user, out User component))
                {
                    SteamID = component.PlatformId;

                    if (Databases.playerPrestige.ContainsKey(SteamID))
                    {
                        // Reset the user's progress
                        Databases.playerPrestige[SteamID] = new PrestigeData(0, 0);
                        SavePlayerPrestige();

                        ctx.Reply($"Prestige data for {playerName} has been reset.");
                    }
                    else
                    {
                        ctx.Reply($"No progress to wipe.");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        [Command(name: "getTopPlayers", shortHand: "getranks", adminOnly: false, usage: ".getranks", description: "Shows the top 10 players by PvE rank and points.")]
        public static void GetTopPlayersCommand(ChatCommandContext ctx)
        {
            
            List<RankData> allRanks = [.. Databases.playerRanks.Values]; 

            if (allRanks == null || allRanks.Count == 0)
            {
                ctx.Reply("No rank data available.");
                return;
            }

            // Sorting by rank in ascending order and taking the top 10
            //var topRanks = allRanks.OrderBy(rankData => rankData.Rank).Take(10);
            // count rank as 1000 points per level, plus points for ease of ranking
            var topRanks = allRanks.OrderByDescending(rankData => rankData.Rank * 1000 + rankData.Points).Take(10);
            StringBuilder replyMessage = new("Top 10 Players by Rank:\n");
            foreach (var rankInfo in topRanks)
            {
                // Assuming there's a way to get the player's name from RankData or through another system
                // Since RankData does not include a player name, you might need a method to map rank or player ID to player names
                string playerName = GetPlayerNameFromRankData(rankInfo); // Placeholder, implement accordingly
                replyMessage.AppendLine($"Player {playerName} - Rank: {rankInfo.Rank}, Points: {rankInfo.Points}");
            }

            ctx.Reply(replyMessage.ToString());
        }

        public static string GetPlayerNameFromRankData(RankData rankData)
        {
            // we find the player's unique identifier (ulong) that matches the given RankData
            var playerID = Databases.playerRanks.FirstOrDefault(x => x.Value == rankData).Key;

            // Assuming a method that can translate playerID to playerName exists
            string playerName = GetPlayerNameById(playerID);

            return playerName;
        }

        public static string GetPlayerNameById(ulong steamID)
        {
            // Iterate through all players, looking for the one with the matching SteamID
            foreach (var playerEntry in Databases.playerRanks)
            {
                if (playerEntry.Key == steamID)
                {
                    // Once the matching ID is found, use it to get the player's name
                    // Assuming PlayerService can resolve player names from SteamID
                    Player player;
                    if (TryGetPlayerFromString(steamID.ToString(), out player))
                    {
                        return player.Name;
                    }
                    break;
                }
            }

            // Return a default or placeholder name if the player is not found
            return "Unknown Player";
        }

        [Command(name: "getPlayerPrestige", shortHand: "gpp", adminOnly: true, usage: ".gpp [Player]", description: "Retrieves the prestige count and buffs for a specified player.")]
        public static void GetPlayerPrestigeCommand(ChatCommandContext ctx, string playerName)
        {
            if (Plugin.PlayerPrestige == false)
            {
                ctx.Reply("Prestige is disabled.");
                return;
            }
            ulong SteamID;
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity player, out Entity user))
            {
                if (VWorld.Server.EntityManager.TryGetComponentData(user, out User component))
                {
                    SteamID = component.PlatformId;

                    if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
                    {
                        ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Prestige Count: {data.Prestiges}, Visual: {data.PlayerBuff}");
                    }
                    else
                    {
                        ctx.Reply($"No prestige data available.");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        [Command(name: "setPlayerPrestige", shortHand: "spp", adminOnly: true, usage: ".spp [Player] [#]", description: "Sets player prestige level for specified player.")]
        public static void SetPlayerPrestigeCommand(ChatCommandContext ctx, string playerName, int count)
        {
            if (Plugin.PlayerPrestige == false)
            {
                ctx.Reply("Prestige is disabled.");
                return;
            }
            if (count < 0)
            {
                ctx.Reply("Prestige count cannot be negative.");
                return;
            }
            if (Plugin.MaxPrestiges != -1 && count > Plugin.MaxPrestiges)
            {
                ctx.Reply("Prestige count cannot exceed the maximum number of prestiges.");
                return;
            }

            ulong SteamID;
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity player, out Entity user))
            {
                if (VWorld.Server.EntityManager.TryGetComponentData(user, out User component))
                {
                    SteamID = component.PlatformId;

                    if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
                    {
                        data.Prestiges = count;
                        SavePlayerPrestige();
                        ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Prestige Count: {data.Prestiges}");
                    }
                    else
                    {
                        // create new data for player
                        PrestigeData prestigeData = new PrestigeData(count, 0);
                        Databases.playerPrestige.Add(SteamID, prestigeData);
                        //data.Prestiges = count;
                        SavePlayerPrestige();
                        ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Prestige Count: {count}");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        [Command(name: "playerAscend", shortHand: "ascend", adminOnly: false, usage: ".ascend", description: "Ascends player if requirements are met.")]
        public static void PlayerAscendCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerAscension == false)
            {
                ctx.Reply("Ascension is disabled.");
                return;
            }
            var user = ctx.Event.User;

            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();
            if (Databases.playerDivinity.TryGetValue(SteamID, out DivineData data))
            {
                Ascension.AscensionCheck(ctx, name, SteamID, data);
            }
            else
            {
                ctx.Reply("Couldn't find ascension data.");
            }
        }

        [Command(name: "getAscension", shortHand: "getasc", adminOnly: false, usage: ".getasc", description: "Gets current ascension level and bonus stats.")]
        public static void GetPlayerAscendCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerAscension == false)
            {
                ctx.Reply("Ascension is disabled.");
                return;
            }
            var user = ctx.Event.User;

            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();
            if (Databases.playerDivinity.TryGetValue(SteamID, out DivineData data))
            {
                if (RPGMods.Utils.Database.PowerUpList.TryGetValue(SteamID, out var powerUp))
                {
                    int health = (int)powerUp.MaxHP;
                    int phys = (int)powerUp.PATK;
                    int spell = (int)powerUp.SATK;
                    float pdef = powerUp.PDEF;
                    float sdef = powerUp.SDEF;

                    string colorHealth = VPlus.Core.Toolbox.FontColors.Green(health.ToString());
                    string colorPhys = VPlus.Core.Toolbox.FontColors.Red(phys.ToString());
                    string colorSpell = VPlus.Core.Toolbox.FontColors.Cyan(spell.ToString());
                    string colorPdef = VPlus.Core.Toolbox.FontColors.Yellow(string.Format("{0:P0}", pdef));
                    string colorSdef = VPlus.Core.Toolbox.FontColors.White(string.Format("{0:P0}", sdef));

                    int level = data.Divinity;
                    string colorLevel = VPlus.Core.Toolbox.FontColors.Pink(level.ToString());

                    ctx.Reply($"Ascension Level: |{colorLevel}|");
                    ctx.Reply($"MaxHealth: |{colorHealth}| PhysicalPower: |{colorPhys}| SpellPower: |{colorSpell}| PhysicalResistance: |{colorPdef}| SpellResistance: |{colorSdef}|");
                    ReplyItemsForAscLevel(ctx, name, SteamID, data);
                }
                else
                {
                    ctx.Reply("You haven't ascended yet.");
                }
            }
            else
            {
                ctx.Reply("Couldn't find ascension data.");
            }
        }

        // need to add commands for ascension that show current level and stats gained from current level, also need to add a command to show items needed for level
        [Command(name: "getAscensionRequirements", shortHand: "getreq", adminOnly: false, usage: ".getreq", description: "Lists items required for next level of ascension.")]
        public static void GetAscendRequirementsCommand(ChatCommandContext ctx)
        {
            if (Plugin.PlayerAscension == false)
            {
                ctx.Reply("Ascension is disabled.");
                return;
            }
            var user = ctx.Event.User;

            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();
            if (Databases.playerDivinity.TryGetValue(SteamID, out DivineData data))
            {
                ReplyItemsForAscLevel(ctx, name, SteamID, data);
            }
            else
            {
                ctx.Reply("Couldn't find ascension data.");
            }
        }

        [Command(name: "wipePlayerAscension", shortHand: "wpa", adminOnly: true, usage: ".wpa [Player]", description: "Resets player ascension level and removes powerup stats.")]
        public static void WipePlayerAscension(ChatCommandContext ctx, string name)
        {
            if (Plugin.PlayerAscension == false)
            {
                ctx.Reply("Ascension is disabled.");
                return;
            }
            User setter = ctx.Event.User;
            if (!TryGetUserFromName(name, out Entity player))
            {
                ctx.Reply("Player not found.");
                return;
            }
            ulong SteamID = player.Read<User>().PlatformId;
            if (VPlus.Data.Databases.playerDivinity.TryGetValue(SteamID, out DivineData data))
            {
                if (data.Divinity == 0)
                {
                    ctx.Reply("Player has not ascended yet.");
                    return;
                }
                if (RPGMods.Utils.Database.PowerUpList.TryGetValue(SteamID, out var powerUp))
                {
                    float health = powerUp.MaxHP;
                    float phys = powerUp.PATK;
                    float spell = powerUp.SATK;
                    float pdef = powerUp.PDEF;
                    float sdef = powerUp.SDEF;
                    RPGMods.Commands.PowerUp.powerUP(ctx, name, "remove", health, phys, spell, pdef, sdef);
                    data.Divinity = 0;
                    ChatCommands.SavePlayerDivinity();
                    ctx.Reply("Player ascension level and stats have been reset.");
                }
            }
            else
            {
                ctx.Reply("Player not found.");
            }
        }

        public static void ReplyItemsForAscLevel(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
        {
            AscensionLevel ascensionLevel = (AscensionLevel)(data.Divinity);
            List<int> prefabIds;

            // Determine the prefab IDs based on the ascension level
            switch (ascensionLevel)
            {
                case AscensionLevel.Level0:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsFirstAscension);
                    break;

                case AscensionLevel.Level1:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsSecondAscension);
                    break;

                case AscensionLevel.Level2:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsThirdAscension);
                    break;

                case AscensionLevel.Level3:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsFourthAscension);
                    break;

                case AscensionLevel.Level4:
                    ctx.Reply("You have reached the maximum ascension level.");
                    return;

                default:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsFirstAscension);
                    break;
            }

            for (int i = 0; i < prefabIds.Count; i++)
            {
                if (prefabIds[i] == 0)
                {
                    continue;
                }
                PrefabGUID prefab = new(prefabIds[i]);
                string name = VPlus.Core.Toolbox.FontColors.White(prefab.GetPrefabName());
                string quantity = VPlus.Core.Toolbox.FontColors.Yellow((i + 1).ToString());
                ctx.Reply($"Item: {name}x{quantity}");
            }
        }

        /*
        [Command(name: "test", shortHand: "t", adminOnly: true, usage: "", description: "testing")]
        public unsafe void TestCommand(ChatCommandContext ctx)
        {
            SpawnUIPrefabOnLoad spawnUIPrefabOnLoad = new SpawnUIPrefabOnLoad();
            GameObject prefabInstance = GameObject.Instantiate(spawnUIPrefabOnLoad.ReferencePrefab);
            Plugin.Logger.LogInfo($"{prefabInstance.name}");
            Plugin.Logger.LogInfo($"Test complete.");
        }
        */

        public static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> FindAllObjects()
        {
            Plugin.Logger.LogInfo("Getting UnityEngine.Object as Il2CppType");

            Il2CppSystem.Type objectType = Il2CppSystem.Type.GetType("UnityEngine.Object, UnityEngine.CoreModule");
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> allUnityObjects = Resources.FindObjectsOfTypeAll(objectType);

            return allUnityObjects;
        }

        public static void SavePlayerPrestige()
        {
            File.WriteAllText(Plugin.PlayerPrestigeJson, JsonSerializer.Serialize(Databases.playerPrestige));
        }

        public static void SavePlayerRanks()
        {
            File.WriteAllText(Plugin.PlayerRanksJson, JsonSerializer.Serialize(Databases.playerRanks));
        }

        public static void SavePlayerDivinity()
        {
            File.WriteAllText(Plugin.PlayerDivinityJson, JsonSerializer.Serialize(Databases.playerDivinity));
        }
    }
}