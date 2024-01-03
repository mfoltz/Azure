using AdminCommands;
using Bloodstone.API;
using Cpp2IL.Core.InstructionSets;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using RPGAddOns.Prestige;
using RPGAddOns.PvERank;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;
using VRising.GameData;
using VRising.GameData.Models;
using static ProjectM.BuffUtility;

namespace RPGAddOns.Core
{
    [CommandGroup(name: "rpg", shortHand: "rpg")]
    internal class Commands
    {
        [Command(name: "setrankpoints", shortHand: "sp", adminOnly: true, usage: ".rpg sp <PlayerName> <Points>", description: "Sets the rank points for a specified player.")]
        public static void SetRankPointsCommand(ChatCommandContext ctx, string playerName, int points)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = ctx.User.PlatformId;

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
                SavePlayerRanks();  // Save the updated rank data

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
                RankData rankData = new(0, points, []);
                if (rankData.Points > (rankData.Rank * 1000) + 1000)
                {
                    rankData.Points = rankData.Rank * 1000 + 1000;
                }
                Databases.playerRanks.Add(SteamID, rankData);
                SavePlayerRanks();
                ctx.Reply($"Rank points for player {playerName} have been set to {points}.");
            }
        }

        [Command(name: "rankup", shortHand: "ru", adminOnly: false, usage: ".rpg ru", description: "Resets your rank points and increases your rank, granting a buff if applicable.")]
        public static void RankUpCommand(ChatCommandContext ctx)
        {
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
                PvERankSystem.RankUp(ctx, name, SteamID, data);
            }
            else
            {
                double percentage = 100 * ((double)data.Points / (data.Rank * 1000 + 1000));
                string integer = ((int)percentage).ToString();
                ctx.Reply($"You have {data.Points} out of the {data.Rank * 1000 + 1000} points required to increase your rank. ({integer}%)");
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

        [Command(name: "getrank", shortHand: "gr", adminOnly: false, usage: ".rpg gr", description: "Displays your current rank points and progress towards the next rank along with current rank.")]
        public static void GetRankCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            ulong SteamID = user.PlatformId;

            if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
            {
                double percentage = 100 * ((double)data.Points / (data.Rank * 1000 + 1000));
                string integer = ((int)percentage).ToString();
                ctx.Reply($"You have {data.Points} out of the {data.Rank * 1000 + 1000} points required to increase your rank. ({integer}%)");
            }
            else
            {
                ctx.Reply("You don't have any points yet.");
            }
        }

        [Command(name: "getprestige", shortHand: "gp", adminOnly: false, usage: ".rpg gp", description: "Displays the number of times you've prestiged.")]
        public static void GetPrestigeCommand(ChatCommandContext ctx)
        {
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

        /*
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

            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                var buffs = data.Buffs.Count > 0 ? string.Join(", ", data.Buffs) : "None";
                ctx.Reply($"Your current buffs are: {buffs}");
            }
            else
            {
                ctx.Reply("You have not received any buffs yet.");
            }
        }
        */

        [Command(name: "wipeprestige", shortHand: "wpr", adminOnly: true, usage: ".rpg wpr <PlayerName>", description: "Resets a player's prestige count.")]
        public static void WipePrestigeCommand(ChatCommandContext ctx, string playerName)
        {
            // Find the user's SteamID based on the playerName

            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);
            if (Databases.playerPrestige.ContainsKey(SteamID))
            {
                // Reset the user's progress
                Databases.playerPrestige[SteamID] = new PrestigeData(0, []);
                SavePlayerPrestige();  // Assuming this method saves the data to a persistent storage

                ctx.Reply($"Progress for player {playerName} has been wiped.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no progress to wipe.");
            }
        }

        [Command(name: "wiperanks", shortHand: "wr", adminOnly: true, usage: ".rpg wr <PlayerName>", description: "Resets a player's rank count.")]
        public static void WipeRanksCommand(ChatCommandContext ctx, string playerName)
        {
            // Find the user's SteamID based on the playerName

            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);
            if (Databases.playerRanks.ContainsKey(SteamID))
            {
                // Reset the user's progress
                Databases.playerRanks[SteamID] = new RankData(0, 0, []);
                SavePlayerPrestige();  // Assuming this method saves the data to a persistent storage

                ctx.Reply($"Progress for player {playerName} has been wiped.");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no progress to wipe.");
            }
        }

        [Command(name: "getplayerprestige", shortHand: "gpr", adminOnly: true, usage: ".rpg gpr <PlayerName>", description: "Retrieves the prestige count and buffs for a specified player.")]
        public static void GetPlayerResetDataCommand(ChatCommandContext ctx, string playerName)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = (ulong)VWorld.Server.EntityManager.GetComponentData<PlatformID>(playerEntity);

            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Reset Count: {data.Prestiges}, Buffs: {data.Buffs}");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no reset data available.");
            }
        }

        [Command(name: "bloodforge", shortHand: "bf", adminOnly: false, usage: ".bf", description: "Bloodforges your equipped weapon, imbuing it with the latent essence of slain VBloods.")]
        public static void InfuseWeapon(ChatCommandContext ctx)
        {
            // choose skill based on VBlood tracking?
            // need small dictionary of VBloodTracked:VBloodSkill
            // so people could make custom weapons... man that's too fucking sick
            EntityManager entityManager = VWorld.Server.EntityManager;
            UserModel usermodel = GameData.Users.GetUserByCharacterName(ctx.Name);
            Entity player = usermodel.FromCharacter.Character;
            //Entity character = ctx.Event.SenderCharacterEntity;
            if (entityManager.TryGetComponentData(player, out Equipment equipment))
            {
                Plugin.Logger.LogError($"Equipment check");
                Entity weaponEntity = equipment.WeaponSlotEntity._Entity;
                Plugin.Logger.LogError($"Weapon check");

                if (entityManager.TryGetComponentData<EquippableData>(weaponEntity, out EquippableData data))
                {
                    Plugin.Logger.LogError($"EquippableData check");

                    // item 0 auto attack, item 1 primary, item 2 secondary

                    PrefabGUID equipBuff = data.BuffGuid;
                    PrefabCollectionSystem prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
                    Entity equipBuffEntity = prefabCollectionSystem._PrefabGuidToEntityMap[equipBuff];
                    Plugin.Logger.LogError($"Buff check");

                    PrefabGUID main = AdminCommands.Data.Prefabs.AB_ChurchOfLight_Paladin_Melee_AbilityGroup;
                    PrefabGUID secondary = AdminCommands.Data.Prefabs.AB_ChurchOfLight_Paladin_AngelicAscent_AbilityGroup;
                    if (entityManager.TryGetComponentData<ReplaceAbilityOnSlotBuff>(equipBuffEntity, out ReplaceAbilityOnSlotBuff slotBuff))
                    {
                        //slotBuff.ReplaceGroupId = main;
                        slotBuff.NewGroupId = main;
                        Plugin.Logger.LogError($"slotBuff check");

                        try
                        {
                            entityManager.SetComponentData(equipBuffEntity, slotBuff);
                            ctx.Reply("Your weapon has been bloodforged.");
                        }
                        catch (Exception e)
                        {
                            Plugin.Logger.LogError($"Error setting component data: {e}");
                        }
                    }
                    else
                    {
                        ctx.Reply("Your weapon cannot be infused...");
                    }
                }
                else
                {
                    ctx.Reply("Your weapon cannot be infused..");
                }
            }
            else
            {
                ctx.Reply("Your weapon cannot be infused.");
            }
        }

        [Command("control", null, null, "Takes control over hovered NPC (Unstable, work-in-progress)", null, true)]
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
                ctx.Reply("An error ocurred while trying to control your original body");
        }

        private static EntityManager entityManager = VWorld.Server.EntityManager;

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
        }

        public static void SavePlayerPrestige()
        {
            File.WriteAllText(Plugin.PlayerPrestigeJson, JsonSerializer.Serialize(Databases.playerPrestige));
        }

        public static void SavePlayerRanks()
        {
            File.WriteAllText(Plugin.PlayerRanksJson, JsonSerializer.Serialize(Databases.playerRanks));
        }
    }
}