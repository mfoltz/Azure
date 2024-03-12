﻿using Bloodstone.API;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;
using VBuild.BuildingSystem;
using VBuild.Core.Converters;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using VBuild.Data;
using VCreate.Systems;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using static VBuild.Core.Services.PlayerService;
using static VCreate.Core.Converters.CommandParser;
using Exception = System.Exception;

namespace VCreate.Core.Commands
{
    [CommandGroup(name: "VBuild", shortHand: "vb")]
    public class CoreCommands
    {

        public class WorldBuildToggle
        {
            public static bool wbFlag = false;

            public static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
            {
                SettingType = (DebugSettingType)5,
                Value = false
            };

            public static SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
            {
                SettingType = (DebugSettingType)16,
                Value = false
            };
            public static SetDebugSettingEvent CastleHeartConnectionRequirementDisabled = new SetDebugSettingEvent()
            {
                SettingType = (DebugSettingType)27,
                Value = false
            };

            [Command(name: "toggleWorldBuild", shortHand: "twb", adminOnly: true, usage: ".twb", description: "Toggles worldbuild debug settings for no-cost building anywhere.")]
            public static void ToggleBuildDebugCommand(ChatCommandContext ctx)
            {
                User user = ctx.Event.User;


                DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                if (!wbFlag)
                {
                    // want to disable resource nodes in active player territories here to avoid overgrowth

                    //ResourceFunctions.SearchAndDestroy();
                    wbFlag = true;
                    BuildingCostsDebugSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingCostsDebugSetting);

                    BuildingPlacementRestrictionsDisabledSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingPlacementRestrictionsDisabledSetting);

                    CastleHeartConnectionRequirementDisabled.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref CastleHeartConnectionRequirementDisabled);

                    string enabledColor = FontColors.Green("enabled");
                    ctx.Reply($"freebuild: {enabledColor}");
                    ctx.Reply($"BuildingCostsDisabled: {BuildingCostsDebugSetting.Value} | BuildingPlacementRestrictionsDisabled: {BuildingPlacementRestrictionsDisabledSetting.Value} | CastleHeartConnectionRequirement: {CastleHeartConnectionRequirementDisabled}");
                }
                else
                {
                    wbFlag = false;
                    BuildingCostsDebugSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingCostsDebugSetting);

                    BuildingPlacementRestrictionsDisabledSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingPlacementRestrictionsDisabledSetting);

                    CastleHeartConnectionRequirementDisabled.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref CastleHeartConnectionRequirementDisabled);

                    string disabledColor = FontColors.Red("disabled");
                    ctx.Reply($"freebuild: {disabledColor}");
                    ctx.Reply($"BuildingCostsDisabled: {BuildingCostsDebugSetting.Value} | BuildingPlacementRestrictionsDisabled: {BuildingPlacementRestrictionsDisabledSetting.Value} | CastleHeartConnectionRequirement: {CastleHeartConnectionRequirementDisabled}");
                }
            }
        }
        public class BuildingCostsToggle
        {
            public static bool buildingCostsFlag = false;

            public static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
            {
                SettingType = (DebugSettingType)5, // Assuming this is the correct DebugSettingType for building costs
                Value = false
            };

            [Command(name: "toggleBuildingCosts", shortHand: "tbc", adminOnly: true, usage: ".tbc", description: "Toggles building costs for no-cost building.")]
            public static void ToggleBuildingCostsCommand(ChatCommandContext ctx)
            {
                User user = ctx.Event.User;

                DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                buildingCostsFlag = !buildingCostsFlag; // Toggle the flag

                BuildingCostsDebugSetting.Value = buildingCostsFlag;
                existingSystem.SetDebugSetting(user.Index, ref BuildingCostsDebugSetting);

                string toggleColor = buildingCostsFlag ? FontColors.Green("enabled") : FontColors.Red("disabled");
                ctx.Reply($"Building costs {toggleColor}");
                ctx.Reply($"BuildingCostsDisabled: {BuildingCostsDebugSetting.Value}");
            }
        }
        public class CastleHeartConnectionToggle
        {
            public static bool castleHeartConnectionRequirementFlag = false;

            public static SetDebugSettingEvent CastleHeartConnectionDebugSetting = new SetDebugSettingEvent()
            {
                SettingType = (DebugSettingType)27, // Replace X with the correct DebugSettingType for Castle Heart connection requirement
                Value = false
            };

            [Command(name: "toggleCastleHeartConnection", shortHand: "tchc", adminOnly: true, usage: ".tchc", description: "Toggles the Castle Heart connection requirement for no-requirement building.")]
            public static void ToggleCastleHeartConnectionCommand(ChatCommandContext ctx)
            {
                User user = ctx.Event.User;

                DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                castleHeartConnectionRequirementFlag = !castleHeartConnectionRequirementFlag; // Toggle the flag

                CastleHeartConnectionDebugSetting.Value = castleHeartConnectionRequirementFlag;
                existingSystem.SetDebugSetting(user.Index, ref CastleHeartConnectionDebugSetting);

                string toggleColor = castleHeartConnectionRequirementFlag ? FontColors.Green("enabled") : FontColors.Red("disabled");
                ctx.Reply($"Castle Heart connection requirement {toggleColor}");
                ctx.Reply($"CastleHeartConnectionRequirementDisabled: {CastleHeartConnectionDebugSetting.Value}");
            }
        }


        [Command(name: "tilePermissions", shortHand: "perms", adminOnly: true, usage: ".vb perms <Name>", description: "Toggles tile permissions for a player (allows moving or dismantling things outside of their territory if it is something that can be moved or disabled).")]
        public static void ToggleEditTilesCommand(ChatCommandContext ctx, string name)
        {
            User setter = ctx.Event.User;
            TryGetUserFromName(name, out Entity userEntity);
            User user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            if (DataStructures.PlayerSettings.TryGetValue(user.PlatformId, out Omnitool data))
            {
                // Toggle the CanEditTiles value
                bool currentCanEditTiles = data.GetMode("CanEditTiles");
                settings.SetMode("CanEditTiles", !currentCanEditTiles);

                Databases.SaveBuildSettings();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Edit tiles outside of territories: {(currentCanEditTiles ? disabledColor : enabledColor)}");
            }
            else
            {
                // Create new settings for user
                Omnitool newSettings = new();
                newSettings.("CanEditTiles", true);

                // Assuming you have a method to add or update settings in your Databases object
                Databases.playerSettings.Add(user.PlatformId, newSettings);
                Databases.SaveBuildSettings();
                ctx.Reply($"Created new build settings and set tile permissions to true.");
            }
        }


        [Command(name: "tileRotation", shortHand: "tr", adminOnly: false, usage: ".vb tr [0/90/180/270]", description: "Sets rotation of tiles placed.")]
        public static void SetTileRotationCommand(ChatCommandContext ctx, int rotation)
        {
            if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
            {
                ctx.Reply("Invalid rotation. Please use 0, 90, 180, or 270 degrees.");
                return;
            }

            User user = ctx.Event.User;
            if (Databases.playerSettings.TryGetValue(user.PlatformId, out Tools settings))
            {
                settings.TileRotation = rotation;
                Databases.SaveBuildSettings();
                ctx.Reply($"Tile rotation set to: {rotation} degrees.");
            }
        }

        [Command(name: "listSet", shortHand: "ls", adminOnly: true, usage: ".vb ls", description: "Lists available tiles from current set.")]
        public static void ListTilesCommand(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Databases.playerSettings.TryGetValue(SteamID, out Tools data))
            {
                if (!ModelRegistry.tilesBySet.ContainsKey(data.TileSet))
                {
                    ctx.Reply("Invalid set name.");
                    return;
                }
                var tiles = OnHover.GetTilesBySet(data.TileSet);
                if (tiles == null)
                {
                    ctx.Reply($"No tiles available for '{data.TileSet}'.");
                    return;
                }

                foreach (var tile in tiles.OrderBy(kv => kv.Key))
                {
                    ctx.Reply($"-{tile.Key}: {tile.Value.Name}");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }

        [Command(name: "chooseSet", shortHand: "cs", adminOnly: false, usage: ".vb cs <tileSetName>", description: "Sets tile set to use tiles from.")]
        public static void TileSetChoice(ChatCommandContext ctx, string choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            string lowerCaseChoice = choice.ToLower();
            if (Databases.playerSettings.TryGetValue(SteamID, out Tools data))
            {
                // want to compare to lowercase version of dictionary keys

                if (ModelRegistry.tilesBySet.ContainsKey(lowerCaseChoice))
                {
                    if (adminSets.Contains(lowerCaseChoice) && !ctx.Event.User.IsAdmin)
                    {
                        ctx.Reply("You must be an admin to use this set.");
                        return;
                    }

                    data.TileSet = lowerCaseChoice;
                    ctx.Reply($"Class set to {choice}.");
                    Databases.SaveBuildSettings();
                }
                else
                {
                    ctx.Reply("Invalid set choice.");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }
        [Command(name: "chooseMapPrefab", shortHand: "cmp", adminOnly: true, usage: ".vb cmp <PrefabGUID>", description: "Sets map icon to prefab.")]

        public static void SetMapIcon(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerSettings.TryGetValue(SteamID, out Tools data))
            {
                // Assuming there's a similar check for map icons as there is for tile models
                if (Prefabs.FindPrefab.CheckForMatch(choice))
                {
                    PrefabGUID prefabGUID = new PrefabGUID(choice);
                    if (prefabGUID.LookupName().ToLower().Contains("map"))
                    {
                        ctx.Reply($"Map icon set.");
                        data.MapIcon = choice;
                        Databases.SaveBuildSettings();
                    }
                    else
                    {
                        ctx.Reply("Invalid map icon choice.");
                    }

                }
                else
                {
                    ctx.Reply("Invalid map icon choice.");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found, create some by giving yourself map icon permissions.");
            }
        }

        [Command(name: "chooseModel", shortHand: "cm", adminOnly: false, usage: ".vb cm <#>", description: "Sets tile model to use, list available tiles with '.vb ls'.")]
        public static void SetTile(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerSettings.TryGetValue(SteamID, out Tools data))
            {
                var setChoice = data.TileSet;
                Dictionary<int, TileConstructor> tiles = GetTilesBySet(setChoice);

                if (tiles != null && tiles.TryGetValue(choice, out TileConstructor tile))
                {
                    ctx.Reply($"Tile model set to {tile.Name}.");
                    data.TileModel = tile.TileGUID;
                    Databases.SaveBuildSettings();
                }
                else
                {
                    ctx.Reply("Invalid tile choice.");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }

        [Command(name: "setTileModelByPrefab", shortHand: "tmp", adminOnly: false, usage: ".vb tmp <PrefabGUID>", description: "Manually set tile model to use.")]
        public static void SetTileByPrefab(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerSettings.TryGetValue(SteamID, out Tools data))
            {
                if (Prefabs.FindPrefab.CheckForMatch(choice))
                {
                    ctx.Reply($"Tile model set.");
                    data.TileModel = choice;
                    Databases.SaveBuildSettings();
                }
                else
                {
                    ctx.Reply("Invalid tile choice.");
                }
            }
            else
            {
                ctx.Reply("Your build data could not be found, create some by giving yourself tile permissions.");
            }
        }

        //[Command(name: "undotile", shortHand: "undo", adminOnly: true, usage: ".vb undo", description: "Destroys the last tile placed (works on last 10 tiles placed).")]
        public static void UndoLastTilePlacedCommand(ChatCommandContext ctx)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            User user = ctx.Event.User;
            if (Databases.playerSettings.TryGetValue(user.PlatformId, out Tools data))
            {
                string lastTileRef = data.PopEntity();
                if (!string.IsNullOrEmpty(lastTileRef))
                {
                    string[] parts = lastTileRef.Split(", ");
                    if (parts.Length == 2 && int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int version))
                    {
                        Entity tileEntity = new Entity { Index = index, Version = version };
                        if (entityManager.Exists(tileEntity) && tileEntity.Version == version)
                        {
                            SystemPatchUtil.Destroy(tileEntity);
                            ctx.Reply($"Successfully destroyed last tile placed.");
                            Databases.SaveBuildSettings();
                        }
                        else
                        {
                            ctx.Reply("The tile could not be found or has already been modified.");
                        }
                    }
                    else
                    {
                        ctx.Reply("Failed to parse the reference to the last tile placed.");
                    }
                }
                else
                {
                    ctx.Reply("You have not placed any tiles yet or all undos have been used.");
                }
            }
            else
            {
                ctx.Reply("You have not placed any tiles yet.");
            }
        }




        [Command(name: "chooseMapIcon", shortHand: "cmi", adminOnly: true, usage: ".vb cmi <#>", description: "Choose map icon to add to tiles placed.")]
        public static void ChooseMapIcon(ChatCommandContext ctx, int choice)
        {
            User user = ctx.Event.User;
            if (Databases.playerSettings.TryGetValue(user.PlatformId, out Tools settings))
            {

                if (Enablers.MapIconFunctions.mapIcons.TryGetValue(choice, out int mapIcon))
                {
                    settings.MapIcon = mapIcon;
                    Databases.SaveBuildSettings();
                    ctx.Reply($"Map icon set to {choice}.");
                }
                else
                {
                    ctx.Reply($"Invalid map icon choice, must be greater than 0 and less than {Enablers.MapIconFunctions.mapIcons.Count}.");

                }
            }
            else
            {
                ctx.Reply("Your build data could not be found.");
            }
        }
        [Command(name: "listMapIcons", shortHand: "lmi", adminOnly: true, usage: ".vb lmi", description: "List all available map icons.")]
        public static void ListMapIcons(ChatCommandContext ctx)
        {
            // Building the list message
            StringBuilder listMessage = new StringBuilder();
            listMessage.AppendLine("Available Map Icons:");

            foreach (var iconEntry in Enablers.MapIconFunctions.mapIcons)
            {
                // Assuming you have a method to get a friendly name for the map icon by its hash
                PrefabGUID prefabGUID = new PrefabGUID(iconEntry.Value);
                string iconName = prefabGUID.LookupName(); // Implement this method based on your needs
                listMessage.AppendLine($"{iconEntry.Key}: {iconName}");
            }

            // Sending the compiled list as a reply in chat
            ctx.Reply(listMessage.ToString());
        }





        [Command(name: "destroyResources", shortHand: "dr", adminOnly: true, usage: ".vb dr", description: "Destroys resources in player territories. Only use this after disabling worldbuild.")]
        public static void DestroyResourcesCommand(ChatCommandContext ctx)
        {
            ResourceFunctions.SearchAndDestroy();
            ctx.Reply("Resource nodes in player territories destroyed. Probably.");
        }


        [Command("destroyTiles", shortHand: "dt", adminOnly: true, description: "Destroys tiles in entered radius matching entered PrefabGUID.",
        usage: "Usage: .vb dt [PrefabGUID] [radius]")]
        public static void DestroyTiles(ChatCommandContext ctx, string name, float radius = 25f)
        {
            // Check if a name is not provided or is empty
            if (string.IsNullOrEmpty(name))
            {
                ctx.Error("You need to specify a tile name!");
                return;
            }

            var tiles = Enablers.ClosestTiles(ctx, radius, name);

            foreach (var tile in tiles)
            {
                SystemPatchUtil.Destroy(tile);
                ctx.Reply(name + " destroyed!");

            }

            if (tiles.Count < 1)
            {
                ctx.Error("Failed to destroy any tiles, are there any in range?");
            }
            else
            {
                ctx.Reply("Tiles have been destroyed!");
            }
        }

        /*
        [Command(name: "unlockVbloodFeature", shortHand: "uvf", adminOnly: true, usage: ".uvf <featureType>", description: "Unlocks a specified VBlood featureType for the player.")]
        public static void UnlockVBloodFeaturesCommand(ChatCommandContext ctx, string input)
        {
            VBloodFeatureType type;
            // Assuming Entity and FromCharacter are similar to the ControlCommand
            if (Enum.TryParse<VBloodFeatureType>(input, true, out var result))
            {
                 type = result;
            }
            else
            {
                ctx.Reply("Invalid feature type.");
                return;
            }
           
            
            Entity senderUserEntity = ctx.Event.SenderUserEntity;
            Entity Character = ctx.Event.SenderCharacterEntity;
            FromCharacter fromCharacter = new FromCharacter()
            {
                User = senderUserEntity,
                Character = Character
            };

            // BuffSpawnerSystemData is assumed to be required and obtained similarly
            // This might need to be fetched or constructed based on the context or predefined data
            
            BuffUtility.BuffSpawnerSystemData buffSpawnerData = new BuffUtility.BuffSpawnerSystemData()
            {
                // Initialization based on required data
            };
            // Obtaining the system that contains the UnlockVBloodFeatures method
            // Adjust the system type according to where UnlockVBloodFeatures is implemented
            DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            string playerName = ctx.Event.User.CharacterName.ToString();
            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            BaseEntityModel baseEntityModel = userModel.Internals;
            Unity.Entities.SystemBase systemBase = new Unity.Entities.SystemBase();
            
            // Execute the UnlockVBloodFeatures function
            DebugEventsSystem.UnlockVBloodFeatures(systemBase, buffSpawnerData, fromCharacter, type);

            // Provide feedback to the command issuer
            ctx.Reply($"Unlocked VBlood feature: {input}");
        }
        */

        [Command("reset", "r", "Instantly reset cooldown and hp for the player.", adminOnly: true)]
        public static void ResetCommand(ChatCommandContext ctx, FoundPlayer player = null)
        {
            Entity User = player?.Value.User ?? ctx.Event.SenderUserEntity;
            Entity Character = player?.Value.Character ?? ctx.Event.SenderCharacterEntity;
            string name = player?.Value.Name.ToString() ?? ctx.Name;

            Helper.ResetCharacter(Character);

            ctx.Reply($"Player \"{name}\" reset.");
        }
        /*
        [Command(name: "control", shortHand: "ctrl", adminOnly: true, usage: ".v ctrl", description: "Possesses VBloods or other entities, use with care.")]
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
        */

        [Command(name: "addNoctumSet", shortHand: "ans", adminOnly: true, usage: ".v ans", description: "adds noctum set to inventory if not already present.")]
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
                        AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                    else
                    {
                        // item was not present and should be added
                        AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                }
            }
        }

        [Command(name: "addDeathSet", shortHand: "ads", adminOnly: true, usage: ".v ads", description: "adds death set to inventory if not already present.")]
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
                        AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                    else
                    {
                        // item was not present and should be added
                        AddItemToInventory(prefabGUID, 1, userModel);
                        //InventoryUtilities_Events.SendTryEquipItem(entityManager, prefabGUID, 0, true);
                    }
                }
            }
        }
        

        [Command(name: "unlock", shortHand: "u", adminOnly: true, usage: ".v u <Player>", description: "Unlocks all the things.")]
        public void UnlockCommand(ChatCommandContext ctx, string playerName, string unlockCategory = "all")
        {
            TryGetPlayerFromString(playerName, out Player player);
            Player player1;
            Entity entity1;
            if ((object)player == null)
            {
                entity1 = ctx.Event.SenderUserEntity;
            }
            else
            {
                player1 = player;
                entity1 = player1.User;
            }
            Entity entity2 = entity1;
            Entity entity3;
            if ((object)player == null)
            {
                entity3 = ctx.Event.SenderCharacterEntity;
            }
            else
            {
                player1 = player;
                entity3 = player1.Character;
            }
            Entity entity4 = entity3;
            try
            {
                VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                FromCharacter fromCharacter = new FromCharacter()
                {
                    User = entity2,
                    Character = entity4
                };
                switch (unlockCategory)
                {
                    case "all":
                        Helper.UnlockAll(fromCharacter);
                        ChatCommandContext chatCommandContext1 = ctx;
                        string str1;
                        if ((object)player == null)
                        {
                            str1 = null;
                        }
                        else
                        {
                            player1 = player;
                            str1 = player1.Name;
                        }
                        if (str1 == null)
                            str1 = "you";
                        string str2 = "Unlocked everything for " + str1 + ".";
                        chatCommandContext1.Reply(str2);
                        break;

                    case "vbloods":
                        Helper.UnlockVBloods(fromCharacter);
                        ChatCommandContext chatCommandContext2 = ctx;
                        string str3;
                        if ((object)player == null)
                        {
                            str3 = null;
                        }
                        else
                        {
                            player1 = player;
                            str3 = player1.Name;
                        }
                        if (str3 == null)
                            str3 = "you";
                        string str4 = "Unlocked VBloods for " + str3 + ".";
                        chatCommandContext2.Reply(str4);
                        break;

                    case "achievements":
                        Helper.UnlockAchievements(fromCharacter);
                        ChatCommandContext chatCommandContext3 = ctx;
                        string str5;
                        if ((object)player == null)
                        {
                            str5 = null;
                        }
                        else
                        {
                            player1 = player;
                            str5 = player1.Name;
                        }
                        if (str5 == null)
                            str5 = "you";
                        string str6 = "Unlocked achievements for " + str5 + ".";
                        chatCommandContext3.Reply(str6);
                        break;

                    case "research":
                        Helper.UnlockResearch(fromCharacter);
                        ChatCommandContext chatCommandContext4 = ctx;
                        string str7;
                        if ((object)player == null)
                        {
                            str7 = null;
                        }
                        else
                        {
                            player1 = player;
                            str7 = player1.Name;
                        }
                        if (str7 == null)
                            str7 = "you";
                        string str8 = "Unlocked research for " + str7 + ".";
                        chatCommandContext4.Reply(str8);
                        break;

                    case "dlc":
                        Helper.UnlockContent(fromCharacter);
                        ChatCommandContext chatCommandContext5 = ctx;
                        string str9;
                        if ((object)player == null)
                        {
                            str9 = null;
                        }
                        else
                        {
                            player1 = player;
                            str9 = player1.Name;
                        }
                        if (str9 == null)
                            str9 = "you";
                        string str10 = "Unlocked dlc for " + str9 + ".";
                        chatCommandContext5.Reply(str10);
                        break;

                    default:
                        ctx.Reply("Invalid unlock type specified.");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ctx.Error(ex.ToString());
            }
        }

        [Command("bloodpotion", "bp", "{Blood Name} [quantity=1] [quality=100]", "Creates a Potion with specified Blood Type, Quality, and Quantity", null, true)]
        public static void GiveBloodPotionCommand(ChatCommandContext ctx, BloodType type = BloodType.frailed, int quantity = 1, float quality = 100f)
        {
            quality = Mathf.Clamp(quality, 0.0f, 100f);
            int num;
            Entity entity;
            for (num = 0; num < quantity && Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, Prefabs.Item_Consumable_PrisonPotion_Bloodwine, 1, out entity); ++num)
            {
                StoredBlood componentData = new StoredBlood()
                {
                    BloodQuality = quality,
                    BloodType = new PrefabGUID((int)type)
                };
                entity.Write(componentData);
            }
            ChatCommandContext chatCommandContext = ctx;
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(81, 3);
            interpolatedStringHandler.AppendLiteral("Got ");
            interpolatedStringHandler.AppendFormatted(num);
            interpolatedStringHandler.AppendLiteral(" Blood Potion(s) Type <color=#ff0>");
            interpolatedStringHandler.AppendFormatted(type);
            interpolatedStringHandler.AppendLiteral("</color> with <color=#ff0>");
            interpolatedStringHandler.AppendFormatted(quality);
            interpolatedStringHandler.AppendLiteral("</color>% quality");
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            chatCommandContext.Reply(stringAndClear);
        }



        [Command("ping", "p", null, "Shows your latency.", null, false)]
        public static void PingCommand(ChatCommandContext ctx, string mode = "")
        {
            int num = (int)(ctx.Event.SenderCharacterEntity.Read<Latency>().Value * 1000.0);
            ChatCommandContext chatCommandContext = ctx;
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
            interpolatedStringHandler.AppendLiteral("Your latency is <color=#ffff00>");
            interpolatedStringHandler.AppendFormatted(num);
            interpolatedStringHandler.AppendLiteral("</color>ms");
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            chatCommandContext.Reply(stringAndClear);
        }

        public static void CastCommand(ChatCommandContext ctx, FoundPrefabGuid prefabGuid, FoundPlayer player = null)
        {
            Player player1;
            Entity entity1;
            if ((object)player == null)
            {
                entity1 = ctx.Event.SenderUserEntity;
            }
            else
            {
                player1 = player.Value;
                entity1 = player1.User;
            }
            Entity entity2 = entity1;
            Entity entity3;
            if ((object)player == null)
            {
                entity3 = ctx.Event.SenderCharacterEntity;
            }
            else
            {
                player1 = player.Value;
                entity3 = player1.Character;
            }
            Entity entity4 = entity3;
            FromCharacter fromCharacter = new FromCharacter()
            {
                User = entity2,
                Character = entity4
            };
            DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            CastAbilityServerDebugEvent serverDebugEvent = new CastAbilityServerDebugEvent()
            {
                AbilityGroup = prefabGuid.Value,
                AimPosition = new Nullable_Unboxed<float3>(entity2.Read<EntityInput>().AimPosition),
                Who = entity4.Read<NetworkId>()
            };
            existingSystem.CastAbilityServerDebugEvent(entity2.Read<User>().Index, ref serverDebugEvent, ref fromCharacter);
        }
        public static void AddItemToInventory(PrefabGUID guid, int amount, UserModel user)
        {
            unsafe
            {
                user.TryGiveItem(guid, 1, out Entity itemEntity);
                return;
            }
        }
    }
}