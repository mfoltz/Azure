using Bloodstone.API;
using WorldBuild.Data;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Terrain;
using ProjectM.Tiles;
using ProjectM.UI;
using Stunlock.Core;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AssetBundlePatching;
using UnityEngine.SceneManagement;
using VampireCommandFramework;
using Il2CppSystem;
using static ProjectM.Tiles.TileCellEnumReader;
using static WorldBuild.BuildingSystem.TileSets;
using UnityEngine.Networking.Match;
using WorldBuild.BuildingSystem;
using WorldBuild.Core.Services;
using WorldBuild.Core.Toolbox;

namespace WorldBuild.Core
{
    [CommandGroup(name: "WorldBuild", shortHand: "wb")]
    public class Commands
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

            [Command(name: "toggleworldbuild", shortHand: "twb", adminOnly: true, usage: ".wb twb", description: "Toggles worldbuild debug settings.")]
            public static void ToggleBuildDebugCommand(ChatCommandContext ctx)
            {
                User user = ctx.Event.User;
                // want to disable resource nodes in active player territories here to avoid overgrowth

                DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                if (!wbFlag)
                {
                    wbFlag = true;
                    BuildingCostsDebugSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingCostsDebugSetting);

                    BuildingPlacementRestrictionsDisabledSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingPlacementRestrictionsDisabledSetting);

                    string enabledColor = FontColors.Green("enabled");
                    ctx.Reply($"freebuild: {enabledColor}");
                    ctx.Reply($"BuildingCostsDisabled: {WorldBuildToggle.BuildingCostsDebugSetting.Value} | BuildingPlacementRestrictionsDisabled: {WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value}");
                }
                else
                {
                    wbFlag = false;
                    BuildingCostsDebugSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingCostsDebugSetting);

                    BuildingPlacementRestrictionsDisabledSetting.Value = wbFlag;
                    existingSystem.SetDebugSetting(user.Index, ref BuildingPlacementRestrictionsDisabledSetting);

                    string disabledColor = FontColors.Red("disabled");
                    ctx.Reply($"freebuild: {disabledColor}");
                    ctx.Reply($"BuildingCostsDisabled: {WorldBuildToggle.BuildingCostsDebugSetting.Value} | BuildingPlacementRestrictionsDisabled: {WorldBuildToggle.BuildingPlacementRestrictionsDisabledSetting.Value}");
                }
            }
        }

        [Command(name: "togglebuildmode", shortHand: "bm", adminOnly: true, usage: ".wb bm", description: "Toggles build mode (tiles will be placed when pressing shift at mouse hover).")]
        public static void BuildModeCommand(ChatCommandContext ctx)
        {
            User user = ctx.Event.User;
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.BuildMode = !settings.BuildMode;

                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Build mode: {(settings.BuildMode ? enabledColor : disabledColor)}");
                Databases.SaveBuildSettings();
            }
        }

        [Command(name: "tilepermissions", shortHand: "tp", adminOnly: true, usage: ".wb tp <Name>", description: "Toggles tile permissions for a player.")]
        public static void ToggleEditTilesCommand(ChatCommandContext ctx, string name)
        {
            User setter = ctx.Event.User;
            PlayerService.TryGetUserFromName(name, out Entity userEntity);
            User user = VWorld.Server.EntityManager.GetComponentData<User>(userEntity);
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.CanEditTiles = !settings.CanEditTiles;
                Databases.SaveBuildSettings();
                string enabledColor = FontColors.Green("enabled");
                string disabledColor = FontColors.Red("disabled");
                ctx.Reply($"Edit tiles outside of territories: {(settings.CanEditTiles ? enabledColor : disabledColor)}");
            }
            else
            {
                // create new settings for user
                BuildSettings newSettings = new BuildSettings(false, false, 0, 0, "", "");
                newSettings.CanEditTiles = true;
                Databases.playerBuildSettings.Add(user.PlatformId, newSettings);
                Databases.SaveBuildSettings();
                ctx.Reply($"Created new build settings and set tile permissions to true.");
            }
        }

        [Command(name: "tilerotation", shortHand: "tr", adminOnly: false, usage: ".wb tr [0/90/180/270]", description: "Sets rotation of tiles placed in buildmode.")]
        public static void SetTileRotationCommand(ChatCommandContext ctx, int rotation)
        {
            if (rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270)
            {
                ctx.Reply("Invalid rotation. Please use 0, 90, 180, or 270 degrees.");
                return;
            }

            User user = ctx.Event.User;
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
            {
                settings.TileRotation = rotation;
                Databases.SaveBuildSettings();
                ctx.Reply($"Tile rotation set to: {rotation} degrees.");
            }
        }

        [Command(name: "listset", shortHand: "ls", adminOnly: true, usage: ".wb ls", description: "Lists available tiles from current set.")]
        public static void ListTilesCommand(ChatCommandContext ctx)
        {
            ulong SteamID = ctx.Event.User.PlatformId;
            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
            {
                if (!ModelRegistry.tilesBySet.ContainsKey(data.TileSet))
                {
                    ctx.Reply("Invalid set name.");
                    return;
                }
                var tiles = TileSets.GetTilesBySet(data.TileSet);
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

        [Command(name: "tileset", shortHand: "ts", adminOnly: false, usage: ".wb ts <name>", description: "Sets tile set to use tiles from.")]
        public static void TileSetChoice(ChatCommandContext ctx, string choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            string lowerCaseChoice = choice.ToLower();
            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
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

        [Command(name: "tilemodel", shortHand: "tm", adminOnly: false, usage: ".wb tm <#>", description: "Sets tile model to use.")]
        public static void SetTile(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
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

        [Command(name: "tilemodelprefab", shortHand: "tmp", adminOnly: false, usage: ".wb tmp <PrefabGUID>", description: "Manually set tile model to use.")]
        public static void SetTileByPrefab(ChatCommandContext ctx, int choice)
        {
            Entity character = ctx.Event.SenderCharacterEntity;
            ulong SteamID = ctx.Event.User.PlatformId;

            if (Databases.playerBuildSettings.TryGetValue(SteamID, out BuildSettings data))
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

        [Command(name: "undotile", shortHand: "undo", adminOnly: true, usage: ".wb undo", description: "Destroys the last tile placed.")]
        public static void UndoLastTilePlacedCommand(ChatCommandContext ctx)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            User user = ctx.Event.User;
            if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings data))
            {
                string lastTileRef = data.LastTilePlaced;
                // Assuming lastTileRef is in the format "index, version"
                string[] parts = lastTileRef.Split(", ");
                if (parts.Length == 2 && int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int version))
                {
                    // Reconstruct the entity from index and version
                    Entity tileEntity = new Entity { Index = index, Version = version };

                    // Verify if the entity still exists and matches the version
                    if (entityManager.Exists(tileEntity) && tileEntity.Version == version)
                    {
                        // Implement your logic to undo or destroy the tile entity
                        // Example: EntityManager.DestroyEntity(tileEntity);
                        SystemPatchUtil.Destroy(tileEntity);
                        ctx.Reply($"Successfully destroyed last tile placed.");
                        data.LastTilePlaced = "";
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
                ctx.Reply("You have not placed any tiles yet.");
            }
        }
    }
}