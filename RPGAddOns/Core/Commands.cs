using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using RPGAddOns.Divinity;
using RPGAddOns.Prestige;
using RPGAddOns.PvERank;
using System.Text.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using VampireCommandFramework;
using VRising.GameData;
using VRising.GameData.Models;
using WillisCore;

namespace RPGAddOns.Core
{
    [CommandGroup(name: "rpg", shortHand: "rpg")]
    internal class Commands
    {
        [Command(name: "visualbuff", shortHand: "vb", adminOnly: true, usage: ".rpg vb <#>", description: "Applies a visual buff you've earned through prestige.")]
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
                Prestige.PrestigeSystem.PrestigeFunctions.BuffCheck(ctx, buff, data);
            }
            else
            {
                ctx.Reply($"You haven't prestiged yet.");
            }
        }

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
        public static void PrestigeCommand(ChatCommandContext ctx)
        {
            var user = ctx.Event.User;
            string name = user.CharacterName.ToString();
            ulong SteamID = user.PlatformId;
            string StringID = SteamID.ToString();

            // Call the ResetLevel method from ResetLevelRPG

            //EntityManager entityManager = default;
            PrestigeSystem.PrestigeCheck(ctx, name, SteamID);
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

        [Command(name: "wipeprestige", shortHand: "wpr", adminOnly: true, usage: ".rpg wpr <PlayerName>", description: "Resets a player's prestige count.")]
        public static void WipePrestigeCommand(ChatCommandContext ctx, string playerName)
        {
            // Find the user's SteamID based on the playerName

            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = ctx.User.PlatformId;
            if (Databases.playerPrestige.ContainsKey(SteamID))
            {
                // Reset the user's progress
                Databases.playerPrestige[SteamID] = new PrestigeData(0, 0);
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
        public static void GetPlayerPrestigeCommand(ChatCommandContext ctx, string playerName)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = ctx.User.PlatformId;

            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Reset Count: {data.Prestiges}, Buffs: {data.Buffs}");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no reset data available.");
            }
        }

        [Command(name: "setplayerprestige", shortHand: "spr", adminOnly: true, usage: ".rpg spr <PlayerName> <#>", description: "Retrieves the prestige count and buffs for a specified player.")]
        public static void SetPlayerPrestigeCommand(ChatCommandContext ctx, string playerName, int count)
        {
            RPGMods.Utils.Helper.FindPlayer(playerName, false, out Entity playerEntity, out Entity userEntity);
            ulong SteamID = ctx.User.PlatformId;

            if (Databases.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                data.Prestiges = count;
                Commands.SavePlayerPrestige();
                ctx.Reply($"Player {playerName} (SteamID: {SteamID}) - Reset Count: {data.Prestiges}, Buffs: {data.Buffs}");
            }
            else
            {
                ctx.Reply($"Player {playerName} not found or no reset data available.");
            }
        }

        [Command(name: "getposition", shortHand: "pos", adminOnly: true, usage: "", description: "")]
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

        [Command(name: "test", shortHand: "t", adminOnly: true, usage: "", description: "")]
        public static void TestCommandPleaseIgnore(ChatCommandContext ctx)
        {
            Entity senderUserEntity = ctx.Event.SenderUserEntity;
            Entity Character = ctx.Event.SenderCharacterEntity;
            EntityManager entityManager = VWorld.Server.EntityManager;
            //DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            /*
            existingSystem.SetDebugSetting
            existingSystem.CastAbilityServerDebugEvent
            existingSystem.GiveEvent
            existingSystem.JumpToNextBloodMoon
            existingSystem.SetUserContentDebugEvent
            existingSystem.SpawnCharmeableDebugEvent
            existingSystem.
                        //var testZar = senderUserEntity.Read<SyncedInputState>().InputsPressed.ToInputFlag();
            //Timer timer = new Timer(5000);
            //var testZar = senderUserEntity.Read<ProjectM.UI.VBloodTrackingEntry>(); // only active client side probably, need to test and see if event can be intercepted
            //var testWar = senderUserEntity.Read<ProjectM.UI.SocialMenu>();
            */

            //var testXar = senderUserEntity.Read<ProjectM.EntityInput>().State.InputsPressed.ToInputFlag();
            WillisCore.ECSExtensions.LogComponentTypes(Character);
            WillisCore.ECSExtensions.LogComponentTypes(senderUserEntity);
            Plugin.Logger.LogInfo($"Begin test");

            //serverbootstrapsystem serverclient events when new clients join server
            ProjectM.UI.SocialMenu socialMenu = new ProjectM.UI.SocialMenu();
            ServerBootstrapSystem serverBootstrapSystem = VWorld.Server.GetExistingSystem<ServerBootstrapSystem>();
            Plugin.Logger.LogInfo($"{serverBootstrapSystem}");
            if (serverBootstrapSystem == null)
            {
                Plugin.Logger.LogInfo($"ServerBootstrapSystem is null");
            }
            else
            {
                Plugin.Logger.LogInfo($"ServerBootstrapSystem is not null");
            }
            // GameClientSettings
            GameBootstrap gameBootstrap = serverBootstrapSystem.GameBootstrap;
            Plugin.Logger.LogInfo($"{gameBootstrap}");
            if (gameBootstrap == null)
            {
                Plugin.Logger.LogInfo($"GameBootstrapSystem is null");
            }
            else
            {
                Plugin.Logger.LogInfo($"GameBootstrapSystem is not null");
            }

            // try to get bar entity from hovering or query for it even if the entity doesnt have the component at the time
            //ctx.Reply($"{testZar}");            ctx.Reply($"{testXar}");

            var queryChar = entityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<ProjectM.UI.CharacterHUDEntry>(),
                        //ComponentType.ReadOnly<Team>(),
                        //ComponentType.ReadOnly<CastleHeartConnection>(),
                        //ComponentType.ReadOnly<BlueprintData>(),
                        //ComponentType.ReadOnly<BlobAssetOwner>(),
                        //ComponentType.ReadOnly<TileModelRegistrationState>(),
                },
                Options = true ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
            });
            var queryCanv = entityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<ProjectM.UI.UICanvasBase>(),
                },
                Options = true ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
            });
            var queryUIdata = entityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<ProjectM.UI.UIDataSystem>(),
                },
                Options = true ? EntityQueryOptions.IncludeDisabled : EntityQueryOptions.Default
            });
            var charQuery = queryChar.ToEntityArray(Allocator.TempJob);
            var canvQuery = queryCanv.ToEntityArray(Allocator.TempJob);
            var uiDataQuery = queryUIdata.ToEntityArray(Allocator.TempJob);
            var characterEntry = new Queue<Entity>(charQuery.ToArray());
            var canvasEntry = new Queue<Entity>(canvQuery.ToArray());
            var uiDataEntry = new Queue<Entity>(uiDataQuery.ToArray());
            Plugin.Logger.LogInfo($"{characterEntry} || {canvasEntry} || {uiDataEntry}");
        }

        [Command(name: "testing", shortHand: "t1", adminOnly: true, usage: "", description: "")]
        public static void TestCommandPleaseIgnoreTheSecond(ChatCommandContext ctx)
        {
            // scramble game mode hype also test HUD modding and everything else
            // uhhh try it on current world and see what happens? lol
            ScrambleGameModeSystem scrambleGameModeSystem = VWorld.Server.GetExistingSystem<ScrambleGameModeSystem>();
            //var HUD = GameObject.FindObjectOfType<UICanvasBase>();
            CanvasFinder.ServerSceneManagement.FindScenes();
            CanvasFinder.ServerSceneManagement.InteractWithUIEntryPoint();
            CanvasFinder.ServerSceneManagement.LoadUIScene();
            //var scrambleGameMode = scrambleGameModeSystem.Enabled;
            Plugin.Logger.LogInfo($" || {scrambleGameModeSystem.Enabled} || {scrambleGameModeSystem.ScrambleSettings} || {scrambleGameModeSystem.Pointer}");
            // use that for on update
        }

        public class CanvasFinder : MonoBehaviour
        {
            public class ServerSceneManagement
            {
                public static void FindScenes()
                {
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);
                        if (scene.isLoaded)
                        {
                            Plugin.Logger.LogInfo("Active Scene: " + scene.name);
                        }
                    }
                }

                public static void InteractWithUIEntryPoint()
                {
                    // Find the UIEntryPoint scene
                    Scene uiEntryPointScene = SceneManager.GetSceneByName("UIEntryPoint");
                    if (!uiEntryPointScene.isLoaded)
                    {
                        Plugin.Logger.LogInfo("UIEntryPoint scene is not loaded.");
                        return;
                    }

                    // Assuming you know the names of the GameObjects you want to interact with
                    // For example, finding a GameObject named "MainCanvas"
                    foreach (GameObject obj in uiEntryPointScene.GetRootGameObjects())
                    {
                        if (obj.name == "MainCanvas")
                        {
                            // Interact with the MainCanvas or its components
                            Canvas canvasComponent = obj.GetComponent<Canvas>();
                            if (canvasComponent != null)
                            {
                                // Perform actions on the canvas
                                // For example, enabling/disabling it
                                canvasComponent.enabled = !canvasComponent.enabled;
                            }

                            // If you need to find a child GameObject
                            Transform childTransform = obj.transform.Find("ChildObjectName");
                            if (childTransform != null)
                            {
                                GameObject childObject = childTransform.gameObject;
                                // Perform actions on the child object
                                Plugin.Logger.LogInfo($"{childObject.name}");
                            }

                            break;
                        }
                    }
                }

                public static void LoadUIScene()
                {
                    string sceneName = "UIEntryPoint"; // Name of the scene you want to load

                    // Check if the scene is already loaded
                    if (SceneManager.GetSceneByName(sceneName).isLoaded)
                    {
                        Plugin.Logger.LogInfo($"{sceneName} scene is already loaded.");
                        return;
                    }

                    // Load the scene
                    try
                    {
                        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive); // Using Additive mode to keep existing scenes
                        Plugin.Logger.LogInfo($"{sceneName} scene has been loaded.");
                    }
                    catch (System.Exception ex)
                    {
                        Plugin.Logger.LogInfo($"Failed to load {sceneName} scene: {ex.Message}");
                    }
                }
            }
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