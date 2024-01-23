using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using RPGAddOns.VeinModules;
using RPGAddOns.VeinModules.Divinity;
using System.Reflection;
using System.Text.Json;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VampireCommandFramework;
using VRising.GameData;
using VRising.GameData.Models;
using WillisCore;

namespace RPGAddOns.Core
{
    [CommandGroup(name: "rpg", shortHand: "rpg")]
    internal class ChatCommands
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
            if (DataStructures.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                // handle applying chosen visual buff
                PrestigeSystem.PrestigeFunctions.BuffCheck(ctx, buff, data);
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

            if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData data))
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
                DataStructures.playerRanks[SteamID] = data;
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
                DataStructures.playerRanks.Add(SteamID, rankData);
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
            if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData data))
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

            if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData data))
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

            if (DataStructures.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
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
            if (DataStructures.playerPrestige.ContainsKey(SteamID))
            {
                // Reset the user's progress
                DataStructures.playerPrestige[SteamID] = new PrestigeData(0, 0);
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
            if (DataStructures.playerRanks.ContainsKey(SteamID))
            {
                // Reset the user's progress
                DataStructures.playerRanks[SteamID] = new RankData(0, 0, []);
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

            if (DataStructures.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
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

            if (DataStructures.playerPrestige.TryGetValue(SteamID, out PrestigeData data))
            {
                data.Prestiges = count;
                ChatCommands.SavePlayerPrestige();
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
            {
                ctx.Reply("An error ocurred while trying to control your original body");
            }
        }

        [Command(name: "test", shortHand: "t", adminOnly: true, usage: "", description: "")]
        public static void OtherTest(ChatCommandContext ctx)
        {
            /*
            ScrambleGameMode_ChunkRemappingSystem chunkRemappingSystem = VWorld.Server.GetExistingSystem<ScrambleGameMode_ChunkRemappingSystem>();
            Plugin.Logger.LogInfo($"{chunkRemappingSystem.Enabled} | {chunkRemappingSystem._Initialized} | {chunkRemappingSystem.ShouldRunSystem()}");
            ServerHostSettings serverHostSettings = new ServerHostSettings();
            serverHostSettings.EnableDangerousDebugEvents = true;
            */
            UnityEngine.Object uiCanvasBase;
            UnityEngine.Object mainGameToolCamera;
            if (OnUserConnectedManagerOld.objectsByType.TryGetValue("UnityEngine.Object", out List<object> unityObjects))
            {
                for (int i = 0; i < unityObjects.Count; i++)
                {
                    UnityEngine.Object unityObject = unityObjects[i] as UnityEngine.Object;
                    if (unityObject.name == "UICanvasBase")
                    {
                        Plugin.Logger.LogInfo($"{unityObject.name} found");
                        uiCanvasBase = unityObject as UICanvasBase;
                        if (uiCanvasBase != null)
                        {
                            LogUICanvasBaseDetails((UICanvasBase)uiCanvasBase);
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("UICanvasBase cast failed. Object is not of type UICanvasBase.");
                        }
                    }
                    if (unityObject.name == "Main_GameToolCamera")
                    {
                        Plugin.Logger.LogInfo($"{unityObject.name} found");
                        mainGameToolCamera = unityObject as Camera;
                        if (mainGameToolCamera != null)
                        {
                            LogCameraDetails((Camera)mainGameToolCamera);
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Main_GameToolCamera cast failed. Object is not of type Camera.");
                        }
                    }
                    if (unityObject.name == "SceneLightingGameObjects")
                    {
                        Plugin.Logger.LogInfo($"{unityObject.name} found");
                        GameObject sceneLightingGameObjects = unityObject as GameObject;
                        foreach (Transform child in sceneLightingGameObjects.transform)
                        {
                            Plugin.Logger.LogInfo($"Child of SceneLightingGameObjects: {child.name}");
                        }
                        ProcessObject(sceneLightingGameObjects);
                    }
                }
            }
            void LogUICanvasBaseDetails(UICanvasBase uiCanvas)
            {
                // Log details about the UICanvasBase object
                if (uiCanvas == null)
                {
                    Plugin.Logger.LogInfo("UICanvasBase is null.");
                    return;
                }
                foreach (Component component in uiCanvas.GetComponents<Component>())
                {
                    Plugin.Logger.LogInfo($"Component on UICanvasBase: {component.GetType().Name}");
                }

                foreach (Transform child in uiCanvas.transform)
                {
                    Plugin.Logger.LogInfo($"Child of UICanvasBase: {child.name}");
                }
            }

            void LogCameraDetails(Camera camera)
            {
                if (camera == null)
                {
                    Plugin.Logger.LogInfo("Camera is null.");
                    return;
                }
                // Log details about the Camera object
                Plugin.Logger.LogInfo($"Camera Field of View: {camera.fieldOfView}");
                Plugin.Logger.LogInfo($"Camera Render Path: {camera.renderingPath}");

                foreach (Component component in camera.GetComponents<Component>())
                {
                    Plugin.Logger.LogInfo($"Component on Camera: {component.GetType().Name}");
                }

                foreach (Transform child in camera.transform)
                {
                    Plugin.Logger.LogInfo($"Child of Camera: {child.name}");
                }
            }
            void ProcessObject(UnityEngine.Object obj, int depth = 0, string indent = "")
            {
                if (obj == null)
                {
                    Plugin.Logger.LogInfo($"{indent}Object is null.");
                    return;
                }

                if (depth > 10)
                {
                    Plugin.Logger.LogInfo($"{indent}Maximum processing depth reached.");
                    return;
                }

                Plugin.Logger.LogInfo($"{indent}Processing object: {obj.name}, Type: {obj.GetType().Name}");

                if (obj is GameObject gameObject)
                {
                    // Check if the GameObject is a prefab
                    if (IsPrefab(gameObject))
                    {
                        Plugin.Logger.LogInfo($"{indent}GameObject is a prefab.");
                    }
                    else
                    {
                        // Process components and children for non-prefab GameObjects
                        foreach (Component component in gameObject.GetComponents<Component>())
                        {
                            ProcessObject(component, depth + 1, indent + "  ");
                        }

                        foreach (Transform child in gameObject.transform)
                        {
                            ProcessObject(child.gameObject, depth + 1, indent + "  ");
                        }
                    }
                }
                else if (obj is Transform transform)
                {
                    ProcessObject(transform.gameObject, depth, indent);
                }
                // Add more cases for other types as needed

                LogPropertiesAndFields(obj, depth, indent);
            }

            bool IsPrefab(GameObject obj)
            {
                if (obj.scene.name == null)
                {
                    // If the GameObject's scene is null, it's likely a prefab
                    return true;
                }
                return false;
            }

            void LogPropertiesAndFields(UnityEngine.Object obj, int depth, string indent)
            {
                Type objType = obj.GetType();
                foreach (PropertyInfo property in objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    try
                    {
                        object value = property.GetValue(obj);
                        Plugin.Logger.LogInfo($"{indent}Property {property.Name} ({property.PropertyType.Name}): {value}");
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger.LogInfo($"{indent}Error accessing property {property.Name}: {ex.Message}");
                    }
                }

                foreach (FieldInfo field in objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    try
                    {
                        object value = field.GetValue(obj);
                        Plugin.Logger.LogInfo($"{indent}Field {field.Name} ({field.FieldType.Name}): {value}");
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger.LogInfo($"{indent}Error accessing field {field.Name}: {ex.Message}");
                    }
                }
            }
        }

        /*
        public static void InitializeScramble(ChatCommandContext ctx)
        {
            Plugin.Logger.LogInfo("Starting scramble...");

            // Accessing the required systems
            EntityManager entityManager = VWorld.Server.EntityManager;
            ScrambleGameModeSystem scrambleGameModeSystem = VWorld.Server.GetExistingSystem<ScrambleGameModeSystem>();
            ScrambleGameMode_ChunkRemappingSystem chunkRemappingSystem = VWorld.Server.GetExistingSystem<ScrambleGameMode_ChunkRemappingSystem>();
            PrefabCollectionSystem prefabCollectionSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();

            Plugin.Logger.LogError("Destroying system...");
            scrambleGameModeSystem.DestroyInstance();
            scrambleGameModeSystem.OnDestroy();

            Plugin.Logger.LogInfo("Recreating system...");
            scrambleGameModeSystem = VWorld.Server.GetOrCreateSystem<ScrambleGameModeSystem>();
            scrambleGameModeSystem.Enabled = true;
            chunkRemappingSystem.Enabled = true;
            // Retrieve or create the SingletonAccessor for ServerGameBalanceSettings
            SingletonAccessor<ServerGameBalanceSettings> settingsAccessor = new SingletonAccessor<ServerGameBalanceSettings>();
            settingsAccessor = SingletonAccessor<ServerGameBalanceSettings>.Create(entityManager);
            if (!settingsAccessor.HasSingleton())
            {
                Plugin.Logger.LogError("ServerGameBalanceSettings singleton not found.");
                return;
            }
            if (!settingsAccessor.TryGetSingleton(out ServerGameBalanceSettings serverGameBalanceSettings))
            {
                Plugin.Logger.LogError("Failed to retrieve ServerGameBalanceSettings.");
                return;
            }

            // Get the PrefabLookupMap from the prefabCollectionSystem
            PrefabLookupMap prefabLookupMap = prefabCollectionSystem.PrefabLookupMap;

            // Create new scramble game mode settings
            Plugin.Logger.LogInfo("Creating new scramble settings...");
            ScrambleGameModeSettings scrambleGameModeSettings = ScrambleGameModeSystem.CreateNewScrambleGameModeSettings(prefabLookupMap, serverGameBalanceSettings, entityManager);

            // Initialize and apply scramble settings
            Plugin.Logger.LogInfo("Applying scramble settings...");
            Plugin.Logger.LogInfo($"{scrambleGameModeSystem.ShouldRunSystem()}");
            scrambleGameModeSystem.InitializeScrambleSettings(serverGameBalanceSettings, scrambleGameModeSettings);
            Plugin.Logger.LogInfo("Checking conversion status...");
            // Ensure everything is converted before applying remappings
            scrambleGameModeSystem.EnsureEverythingConverted();
            Plugin.Logger.LogInfo($"{scrambleGameModeSystem.ShouldRunSystem()}");
            // Apply remappings
            bool hasBeenApplied = false;
            Plugin.Logger.LogInfo("Applying remappings...");
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            ScrambleGameModeSystem.ApplyRemappings(entityManager, commandBuffer, prefabLookupMap, serverGameBalanceSettings, scrambleGameModeSettings, ref hasBeenApplied);
            Plugin.Logger.LogInfo($"{scrambleGameModeSystem.ShouldRunSystem()}");
            scrambleGameModeSystem.Finalize();
            commandBuffer.Playback(entityManager);
            Plugin.Logger.LogInfo("Disposing command buffer...");
            commandBuffer.Dispose();
        }
        */

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
                DataStructures.playerPrestige = JsonSerializer.Deserialize<Dictionary<ulong, PrestigeData>>(json1);
                Plugin.Logger.LogWarning("PlayerPrestige Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                DataStructures.playerPrestige = new Dictionary<ulong, PrestigeData>();
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
                DataStructures.playerRanks = JsonSerializer.Deserialize<Dictionary<ulong, RankData>>(json2);
                Plugin.Logger.LogWarning("PlayerRanks Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                DataStructures.playerRanks = new Dictionary<ulong, RankData>();
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
                DataStructures.playerDivinity = JsonSerializer.Deserialize<Dictionary<ulong, DivineData>>(json3);
                Plugin.Logger.LogWarning("PlayerDivinity populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                DataStructures.playerDivinity = new Dictionary<ulong, DivineData>();
                Plugin.Logger.LogWarning("PlayerDivinity Created");
            }
        }

        public static void SavePlayerPrestige()
        {
            File.WriteAllText(Plugin.PlayerPrestigeJson, JsonSerializer.Serialize(DataStructures.playerPrestige));
        }

        public static void SavePlayerRanks()
        {
            File.WriteAllText(Plugin.PlayerRanksJson, JsonSerializer.Serialize(DataStructures.playerRanks));
        }

        public static void SavePlayerDivinity()
        {
            File.WriteAllText(Plugin.PlayerDivinityJson, JsonSerializer.Serialize(DataStructures.playerDivinity));
        }
    }
}