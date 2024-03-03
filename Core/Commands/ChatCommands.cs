using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VPlus.Augments;
using VPlus.Augments.Rank;
using VPlus.Core;
using VBuild.Data;
using VPlus.Data;
using VampireCommandFramework;
using VRising.GameData;
using VRising.GameData.Models;
using VPlus.Augments;
using VPlus.Core.Toolbox;
using VBuild.Core.Toolbox;
using Databases = VPlus.Data.Databases;
using V.Augments;
using VBuild.Core.Services;

namespace VPlus.Core.Commands
{
    [CommandGroup(name: "VPlus", shortHand: "v")]
    public class ChatCommands
    {

        [Command(name: "redeemPoints", shortHand: "redeem", adminOnly: false, usage: ".v redeem", description: "Redeems all VPoints for the crystal equivalent, drops if inventory full.")]
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
                    ctx.Reply($"You need at least {VPlus.Core.Toolbox.FontColors.Yellow(Plugin.RewardFactor.ToString())} VPoints to redeem for a crystal. ({VPlus.Core.Toolbox.FontColors.White(data.VTokens.ToString())})");
                    return;
                }
                int reward = data.VTokens / Plugin.RewardFactor;

                // Calculate the exact cost in VPoints for those rewards
                int cost = reward * Plugin.RewardFactor;

                // Subtract the cost from the player's VPoints
                PrefabGUID prefabGUID = new PrefabGUID(Plugin.VTokensItemPrefab);
                bool success = Helper.AddItemToInventory(characterEntity, prefabGUID, reward, out Entity entity);
                if (!success)
                {
                    //inventory full probably
                    InventoryUtilitiesServer.CreateDropItem(VWorld.Server.EntityManager, characterEntity, prefabGUID, reward, entity);
                }

                data.VTokens -= cost;
                int remainder = data.VTokens;
                ctx.Reply($"VPoints redeemed for {VPlus.Core.Toolbox.FontColors.White(reward.ToString())} {VPlus.Core.Toolbox.FontColors.Pink("crystals")}.");
            }
            else
            {
                ctx.Reply("You don't have any VPoints to redeem yet.");
            }
        }

        [Command(name: "wipeplayerranks", shortHand: "wpr", adminOnly: true, usage: ".v wpr <Player>", description: "Resets a player's rank count.")]
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

        [Command(name: "setrankpoints", shortHand: "srp", adminOnly: true, usage: ".v srp <Player> <Points>", description: "Sets the rank points for a specified player.")]
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

        [Command(name: "setplayerrank", shortHand: "spr", adminOnly: true, usage: ".v spr <Player> <#>", description: "Sets the rank for a specified player.")]
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
                        ctx.Reply($"Rank for player {playerName} has been set to {rank}, they can use .rpg bs to apply their buffs.");
                    }
                }
            }
            else
            {
                //couldn't find player
                ctx.Reply("Player not found.");
            }
        }

        [Command(name: "rankup", shortHand: "ru", adminOnly: false, usage: ".v ru", description: "Resets your rank points and increases your rank, granting a buff if applicable.")]
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
            string StringID = SteamID.ToString();

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                if (data.Rank >= Plugin.MaxRanks)
                {
                    ctx.Reply("You have reached the maximum rank.");
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

        [Command(name: "buffsync", shortHand: "bs", adminOnly: false, usage: ".v bs", description: "Checks which buffs you should have from rank and apply them if you don't have them.")]
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

        [Command(name: "getrank", shortHand: "gr", adminOnly: false, usage: ".v gr", description: "Displays your current rank points and progress towards the next rank along with current rank.")]
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
                ctx.Reply($"You have {colorPoints1} out of the {colorPoints2} points required to increase your rank. ({colorString}%)");
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "getplayerrank", shortHand: "gpr", adminOnly: true, usage: ".v gpr <Player>", description: "Helps admins check player rank data.")]
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
        [Command(name: "visualbuff", shortHand: "vb", adminOnly: false, usage: ".v vb <#>", description: "Applies a visual buff you've earned through prestige.")]
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

        [Command(name: "prestige", shortHand: "pr", adminOnly: false, usage: ".v pr", description: "Resets your level to 1 after reaching max level, offering extra perks.")]
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

        [Command(name: "getprestige", shortHand: "gp", adminOnly: false, usage: ".v gp", description: "Displays the number of times you've prestiged.")]
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

        [Command(name: "wipeplayerprestige", shortHand: "wpp", adminOnly: true, usage: ".v wpp <Player>", description: "Resets a player's prestige count.")]
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

                        ctx.Reply($"Prestige data for player {playerName} has been reset.");
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

        [Command(name: "getplayerprestige", shortHand: "gpp", adminOnly: true, usage: ".v gpp <Player>", description: "Retrieves the prestige count and buffs for a specified player.")]
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

        [Command(name: "setplayerprestige", shortHand: "spp", adminOnly: true, usage: ".v spp <Player> <#>", description: "Sets player prestige level for specified player.")]
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

        
        [Command(name: "playerascend", shortHand: "asc", adminOnly: false, usage: ".v asc", description: "Ascends player if requirements are met.")]
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
            
        }

        [Command(name: "resetdivinity", shortHand: "rd", adminOnly: true, usage: ".v rd <PlayerName>", description: "Resets player divinity data.")]
        public static void ResetDivinityCommand(ChatCommandContext ctx, string playerName)
        {
            if (!Plugin.PlayerAscension)
            {
                ctx.Reply("Ascension is disabled.");
                return;
            }
            PlayerService.TryGetPlayerFromString(playerName, out var player);
            ulong SteamID = player.SteamID;
            if (Databases.playerDivinity.ContainsKey(SteamID))
            {
                Databases.playerDivinity[SteamID] = new DivineData(0, 0); // Reset the divinity data
                SavePlayerDivinity(); // Save changes
                ctx.Reply($"Divinity data for {playerName} has been reset.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no divinity data to wipe.");
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