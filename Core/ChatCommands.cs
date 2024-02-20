using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using RPGAddOnsEx.Augments;
using RPGAddOnsEx.Augments.RankUp;
using Stunlock.Core;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AssetBundlePatching;
using UnityEngine.SceneManagement;
using VampireCommandFramework;
using VRising.GameData;
using VRising.GameData.Models;
using WillisCore;

namespace RPGAddOnsEx.Core
{
    [CommandGroup(name: "rpg", shortHand: "rpg")]
    internal class ChatCommands
    {
        // rank commands for players and admins should all be in this block unless otherwise mentioned
        [Command(name: "wipeplayerranks", shortHand: "wpr", adminOnly: true, usage: ".rpg wpr <PlayerName>", description: "Resets a player's rank count.")]
        public static void WipeRanksCommand(ChatCommandContext ctx, string playerName)
        {
            if (Plugin.PlayerRankUp == false)
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
                        Databases.playerRanks[SteamID] = new RankData(0, 0, [], 0, [0, 0], false);
                        foreach (var buff in buffsToWipe)
                        {
                            PrefabGUID buffguid = new(buff);
                            WillisCore.Helper.UnbuffCharacter(player, buffguid);
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

        [Command(name: "setrankpoints", shortHand: "srp", adminOnly: true, usage: ".rpg srp <PlayerName> <Points>", description: "Sets the rank points for a specified player.")]
        public static void SetRankPointsCommand(ChatCommandContext ctx, string playerName, int points)
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
                        ChatCommands.SavePlayerRanks();
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
                        RankData rankData = new(0, points, [], 0, [0, 0], false);
                        if (rankData.Points > rankData.Rank * 1000 + 1000)
                        {
                            rankData.Points = rankData.Rank * 1000 + 1000;
                        }
                        Databases.playerRanks.Add(SteamID, rankData);
                        ChatCommands.SavePlayerRanks();
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

        [Command(name: "setplayerrank", shortHand: "spr", adminOnly: true, usage: ".rpg spr <PlayerName> <Rank>", description: "Sets the rank for a specified player.")]
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

                            //WillisCore.Helper.BuffPlayerByName(playerName, buffguid, 0, true);
                        }
                        data.Buffs = playerBuffs;

                        Databases.playerRanks[SteamID] = data;

                        SavePlayerRanks();
                        ctx.Reply($"Rank for player {playerName} has been set to {rank}, they can use .rpg bs to apply their buffs.");
                    }
                    else
                    {
                        RankData rankData = new(rank, 0, [], 0, [0, 0], false);
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

                            //WillisCore.Helper.BuffPlayerByName(playerName, buffguid, 0, true);
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

        [Command(name: "rankup", shortHand: "ru", adminOnly: false, usage: ".rpg ru", description: "Resets your rank points and increases your rank, granting a buff if applicable.")]
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
                    var colorString = RPGAddOnsEx.Core.FontColors.Yellow(integer);
                    string colorPoints1 = RPGAddOnsEx.Core.FontColors.White(data.Points.ToString());
                    string colorPoints2 = RPGAddOnsEx.Core.FontColors.White((data.Rank * 1000 + 1000).ToString());
                    ctx.Reply($"You have {colorPoints1} out of the {colorPoints2} points required to increase your rank. ({colorString}%)");
                }
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "buffsync", shortHand: "bs", adminOnly: false, usage: ".rpg bs", description: "Attempts to check which buffs you should have from rank and apply them if you don't have them.")]
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
                    //WillisCore.Helper.ClearExtraBuffs(player);
                    foreach (var buff in playerBuffs)
                    {
                        PrefabGUID buffguid = new(buff);

                        WillisCore.Helper.BuffPlayerByName(ctx.Name, buffguid, 0, true);
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

        [Command(name: "getrank", shortHand: "gr", adminOnly: false, usage: ".rpg gr", description: "Displays your current rank points and progress towards the next rank along with current rank.")]
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
                var colorString = RPGAddOnsEx.Core.FontColors.Yellow(integer);
                string colorPoints1 = RPGAddOnsEx.Core.FontColors.White(data.Points.ToString());
                string colorPoints2 = RPGAddOnsEx.Core.FontColors.White((data.Rank * 1000 + 1000).ToString());
                string colorString1 = RPGAddOnsEx.Core.FontColors.Red("max");
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

        [Command(name: "getplayerrank", shortHand: "gpr", adminOnly: true, usage: ".rpg gpr <PlayerName>", description: "Helps admins check player rank data.")]
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
        [Command(name: "visualbuff", shortHand: "vb", adminOnly: false, usage: ".rpg vb <#>", description: "Applies a visual buff you've earned through prestige.")]
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

        [Command(name: "prestige", shortHand: "pr", adminOnly: false, usage: ".rpg p", description: "Resets your level to 1 after reaching max level, offering extra perks.")]
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

        [Command(name: "getprestige", shortHand: "gp", adminOnly: false, usage: ".rpg gp", description: "Displays the number of times you've prestiged.")]
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

        [Command(name: "wipeplayerprestige", shortHand: "wpp", adminOnly: true, usage: ".rpg wpp <PlayerName>", description: "Resets a player's prestige count.")]
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

        [Command(name: "getplayerprestige", shortHand: "gpp", adminOnly: true, usage: ".rpg gpp <PlayerName>", description: "Retrieves the prestige count and buffs for a specified player.")]
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

        [Command(name: "setplayerprestige", shortHand: "spp", adminOnly: true, usage: ".rpg spp <PlayerName> <#>", description: "Sets player prestige level for specified player.")]
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
                        ChatCommands.SavePlayerPrestige();
                        ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Prestige Count: {data.Prestiges}");
                    }
                    else
                    {
                        // create new data for player
                        PrestigeData prestigeData = new PrestigeData(count, 0);
                        Databases.playerPrestige.Add(SteamID, prestigeData);
                        //data.Prestiges = count;
                        ChatCommands.SavePlayerPrestige();
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

        // ascension commands for players and admins should all be in this block unless otherwise mentioned

        [Command(name: "playerascend", shortHand: "asc", adminOnly: false, usage: ".rpg asc", description: "Ascends player if requirements are met.")]
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
                if (Ascension.AscensionCheck(ctx, name, SteamID, data))
                {
                    // if requirements are met handle ascension appropriately for path, level, etc
                    //Ascension.AscendPlayer(ctx, name, SteamID, data);
                }
                else
                {
                    ctx.Reply("You don't meet the requirements for ascending yet.");
                }
            }
            else
            {
                //ctx.Reply("You haven't ascended yet.");
                // handle creating new data for player
                DivineData divineData = new(0, 0);

                Databases.playerDivinity.Add(SteamID, divineData);
                // check if requirements are met and ascend them if yes, if not reply with not met
                if (Ascension.AscensionCheck(ctx, name, SteamID, divineData))
                {
                    //Ascension.AscendPlayer(ctx, name, SteamID, divineData);
                }
                else
                {
                    ctx.Reply("You don't meet the requirements for ascending yet.");
                }
                SavePlayerDivinity();
            }
        }

        [Command(name: "resetdivinity", shortHand: "rd", adminOnly: true, usage: ".rpg rd <PlayerName>", description: "Resets player divinity data.")]
        public static void ResetDivinityCommand(ChatCommandContext ctx, string playerName)
        {
            if (Plugin.PlayerAscension == false)
            {
                ctx.Reply("Ascension is disabled.");
                return;
            }
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = ctx.User.PlatformId;
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

        [Command(name: "setdivinity", shortHand: "sd", adminOnly: true, usage: ".rpg sd <PlayerName> <DivinityLevel> <Path>", description: "Sets the player's divinity level and path (1 for phys and 2 for spell, 0 by default).")]
        public static void SetDivinityCommand(ChatCommandContext ctx, string playerName, int divinityLevel, int path)
        {
            if (Plugin.PlayerAscension == false)
            {
                ctx.Reply("Ascension is disabled.");
                return;
            }
            // Attempt to find the player and get their SteamID
            if (RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity))
            {
                ulong SteamID = ctx.User.PlatformId;
                // Check if divinity data exists for the player and update or create as necessary
                if (Databases.playerDivinity.ContainsKey(SteamID))
                {
                    // Update existing divinity data
                    Databases.playerDivinity[SteamID] = new DivineData(divinityLevel, path);
                    ctx.Reply($"Divinity level for {playerName} set to {divinityLevel} with path {path}.");
                }
                else
                {
                    // Create new divinity data
                    Databases.playerDivinity.Add(SteamID, new DivineData(divinityLevel, path));
                    ctx.Reply($"Divinity level for {playerName} initialized to {divinityLevel} with path {path}.");
                }
                SavePlayerDivinity(); // Save changes to persistent storage
            }
            else
            {
                ctx.Reply($"Player {playerName} not found.");
            }
        }

        // other commands I'm leaving in will be here and should probably be admin only

        [Command(name: "getposition", shortHand: "pos", adminOnly: true, usage: "", description: "Returns position coordinates of player in console.")]
        public static void GetPosition(ChatCommandContext ctx)
        {
            // choose skill based on VBlood tracking?
            // need small dictionary of VBloodTracked:VBloodSkill
            // so people could make custom weapons... man that's too fucking sick
            EntityManager entityManager = VWorld.Server.EntityManager;
            UserModel usermodel = GameData.Users.GetUserByCharacterName(ctx.Name);
            Entity player = usermodel.FromCharacter.Character;
            float3 playerPosition = usermodel.Position;
            Plugin.Logger.LogError($"{playerPosition}");
        }

        [Command(name: "control", shortHand: "ctrl", adminOnly: true, usage: "", description: "Possesses VBloods or other entities, use with care.")]
        public static void ControlCommand(ChatCommandContext ctx)
        {
            Entity senderUserEntity = ctx.Event.SenderUserEntity;
            Entity Character = ctx.Event.SenderCharacterEntity;
            FromCharacter fromCharacter = new FromCharacter()
            {
                User = senderUserEntity,
                Character = Character
            };
            DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            if (Character.Read<EntityInput>().HoveredEntity.Index > 0)
            {
                Entity hoveredEntity = senderUserEntity.Read<EntityInput>().HoveredEntity;
                if (!hoveredEntity.Has<PlayerCharacter>())
                {
                    ControlDebugEvent controlDebugEvent = new ControlDebugEvent()
                    {
                        EntityTarget = hoveredEntity,
                        Target = senderUserEntity.Read<EntityInput>().HoveredEntityNetworkId
                    };
                    existingSystem.ControlUnit(fromCharacter, controlDebugEvent);
                    ctx.Reply("Controlling hovered unit");
                    return;
                }
            }
            if (PlayerService.TryGetCharacterFromName(senderUserEntity.Read<User>().CharacterName.ToString(), out Character))
            {
                ControlDebugEvent controlDebugEvent = new ControlDebugEvent()
                {
                    EntityTarget = Character,
                    Target = Character.Read<NetworkId>()
                };
                existingSystem.ControlUnit(fromCharacter, controlDebugEvent);
                ctx.Reply("Controlling self");
            }
            else
            {
                ctx.Reply("An error ocurred while trying to control your original body");
            }
        }

        [Command(name: "addNoctumSet", shortHand: "ans", adminOnly: true, usage: "", description: "adds noctum set to inventory if not already present")]
        public static void addNoctumCommand(ChatCommandContext ctx)
        {
            // want to get ModifyUnitStatsBuff_DOTS from EquipBuff_Gloves_Base or something similar
            var user = ctx.Event.User;
            var player = ctx.Event.SenderCharacterEntity;
            string name = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            EntityManager entityManager = VWorld.Server.EntityManager;

            List<PrefabGUID> noctumSet = new List<PrefabGUID>
            {
                new PrefabGUID(1076026390), // Chest
                new PrefabGUID(735487676), // Boots
                new PrefabGUID(-810609112),  // Legs
                new PrefabGUID(776192195),  // Gloves
            };
            var userModel = GameData.Users.GetUserByCharacterName(name);
            var inventoryModel = userModel.Inventory;
            var inventoryItemData = inventoryModel.Items;
            if (InventoryUtilities.TryGetInventoryEntity(entityManager, player, out Entity inventoryEntity))
            {
                foreach (var prefabGUID in noctumSet)
                {
                    bool check = InventoryUtilitiesServer.TryRemoveItem(entityManager, inventoryEntity, prefabGUID, 1);
                    // going to assume that returns true if present/removed and false if not present
                    if (check)
                    {
                        // item was present and removed, add it back
                        RPGAddOnsEx.Hooks.VBloodSystem.AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                    else
                    {
                        // item was not present and should be added
                        RPGAddOnsEx.Hooks.VBloodSystem.AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                }
            }
        }

        [Command(name: "addDeathSet", shortHand: "ads", adminOnly: true, usage: "", description: "adds death set to inventory if not already present")]
        public static void addDeathCommand(ChatCommandContext ctx)
        {
            // want to get ModifyUnitStatsBuff_DOTS from EquipBuff_Gloves_Base or something similar
            var user = ctx.Event.User;
            var player = ctx.Event.SenderCharacterEntity;
            string name = user.CharacterName.ToString();
            var SteamID = user.PlatformId;

            EntityManager entityManager = VWorld.Server.EntityManager;

            List<PrefabGUID> deathSet = new List<PrefabGUID>
            {
                new PrefabGUID(1055898174), // Chest
                new PrefabGUID(1400688919), // Boots
                new PrefabGUID(125611165),  // Legs
                new PrefabGUID(-204401621),  // Gloves
            };
            var userModel = GameData.Users.GetUserByCharacterName(name);
            var inventoryModel = userModel.Inventory;
            var inventoryItemData = inventoryModel.Items;
            if (InventoryUtilities.TryGetInventoryEntity(entityManager, player, out Entity inventoryEntity))
            {
                foreach (var prefabGUID in deathSet)
                {
                    bool check = InventoryUtilitiesServer.TryRemoveItem(entityManager, inventoryEntity, prefabGUID, 1);
                    // going to assume that returns true if present/removed and false if not present
                    if (check)
                    {
                        // item was present and removed, add it back
                        RPGAddOnsEx.Hooks.VBloodSystem.AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                    else
                    {
                        // item was not present and should be added
                        RPGAddOnsEx.Hooks.VBloodSystem.AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                }
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

        public static void LoadData()
        {
            if (!File.Exists(Plugin.PlayerPrestigeJson))
            {
                var stream = File.Create(Plugin.PlayerPrestigeJson);
                stream.Dispose();
            }

            string json1 = File.ReadAllText(Plugin.PlayerPrestigeJson);
            Plugin.Logger.LogWarning($"PlayerPrestige found: {json1}");
            try
            {
                Databases.playerPrestige = JsonSerializer.Deserialize<Dictionary<ulong, PrestigeData>>(json1);
                Plugin.Logger.LogWarning("PlayerPrestige Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerPrestige = new Dictionary<ulong, PrestigeData>();
                Plugin.Logger.LogWarning("PlayerPrestige Created");
            }
            if (!File.Exists(Plugin.PlayerRanksJson))
            {
                var stream = File.Create(Plugin.PlayerRanksJson);
                stream.Dispose();
            }

            string json2 = File.ReadAllText(Plugin.PlayerRanksJson);
            Plugin.Logger.LogWarning($"PlayerRanks found: {json2}");

            try
            {
                Databases.playerRanks = JsonSerializer.Deserialize<Dictionary<ulong, RankData>>(json2);
                Plugin.Logger.LogWarning("PlayerRanks Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerRanks = new Dictionary<ulong, RankData>();
                Plugin.Logger.LogWarning("PlayerRanks Created");
            }
            if (!File.Exists(Plugin.PlayerDivinityJson))
            {
                var stream = File.Create(Plugin.PlayerDivinityJson);
                stream.Dispose();
            }
            string json3 = File.ReadAllText(Plugin.PlayerDivinityJson);
            Plugin.Logger.LogWarning($"PlayerDivinity found: {json3}");

            try
            {
                Databases.playerDivinity = JsonSerializer.Deserialize<Dictionary<ulong, DivineData>>(json3);
                Plugin.Logger.LogWarning("PlayerDivinity populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerDivinity = new Dictionary<ulong, DivineData>();
                Plugin.Logger.LogWarning("PlayerDivinity Created");
            }
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