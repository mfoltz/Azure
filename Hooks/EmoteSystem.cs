using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VBuild.BuildingSystem;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using VBuild.Data;
using VRising.GameData;
using VRising.GameData.Models;
using static VBuild.Core.Services.PlayerService;
using static VCF.Core.Basics.RoleCommands;
using User = ProjectM.Network.User;

namespace VBuild.Hooks;

[HarmonyPatch]
internal class EmoteSystemPatch
{
    private static readonly string enabledColor = VBuild.Core.Toolbox.FontColors.Green("enabled");
    private static readonly string disabledColor = VBuild.Core.Toolbox.FontColors.Red("disabled");

    private static readonly Dictionary<int, Action<Player, ulong>> emoteActions = new Dictionary<int, Action<Player, ulong>>()
        {
            { -658066984, ToggleSnapping }, // Beckon
            { -1462274656, ToggleCopyMode }, // Bow
            { -26826346, ToggleMapIconPlacement }, // Clap
            //{ 1048364815,  }, // Dance
            { -452406649, ToggleInspectMode }, // Point
            { -53273186, ToggleKillMode }, // No
            { -370061286, ToggleImmortalTiles }, // Salute
            { -578764388, UndoLastTilePlacement }, // Shrug
            { 808904257, CycleGridSize }, // Sit
            //{ -1064533554, ToggleSurrenderSetting }, // Surrender
            //{ -158502505, MoveClosestToMouseToggle }, // Taunt
            { 1177797340, ToggleTileRotation }, // Wave
            { -1525577000, ToggleBuildMode } // Yes
        };

    static EmoteSystemPatch()
    {
        emoteActions = new Dictionary<int, Action<Player, ulong>>()
        {
            { -658066984, ToggleSnapping }, // Beckon
            { -1462274656, ToggleCopyMode }, // Bow
            { -26826346, ToggleMapIconPlacement }, // Clap
            //{ 1048364815, TBD }, // Dance
            { -452406649, ToggleInspectMode }, // Point
            { -53273186, ToggleKillMode }, // No
            { -370061286, ToggleImmortalTiles }, // Salute
            { -578764388, UndoLastTilePlacement }, // Shrug
            { 808904257, CycleGridSize }, // Sit
            //{ -1064533554, TBD }, // Surrender
            //{ -158502505, TBD }, // Taunt
            { 1177797340, ToggleTileRotation }, // Wave
            { -1525577000, ToggleBuildMode } // Yes
    };
    }

    [HarmonyPatch(typeof(EmoteSystem), nameof(EmoteSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void OnUpdate_Emote(ProjectM.EmoteSystem __instance)
    {
        var _entities = __instance.__UseEmoteJob_entityQuery.ToEntityArray(Allocator.Temp);

        foreach (var _entity in _entities)
        {
            var _event = _entity.Read<UseEmoteEvent>();
            var _from = _entity.Read<FromCharacter>();

            Player _player = new Player(_from.User);
            ulong _playerId = _player.SteamID;
            if (!_player.IsAdmin) continue;
            if (emoteActions.TryGetValue(_event.Action.GuidHash, out var action))
            {
                // Execute the associated action
                action.Invoke(_player, _playerId);
            }
        }

        _entities.Dispose();
    }

    private static void ToggleCopyMode(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // Toggle the CopyModeToggle value
            bool currentValue = settings.GetToggle("CopyToggle");
            settings.SetToggle("CopyToggle", !currentValue);

            // Update the player's build settings in the database
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Copy Mode is now {stateMessage}");
        }
    }

    private static void ToggleKillMode(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // Toggle the CopyModeToggle value
            bool currentValue = settings.GetToggle("KillToggle");
            settings.SetToggle("KillToggle", !currentValue);

            // Update the player's build settings in the database
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Kill Mode is now {stateMessage}");
        }
    }

    private static void ToggleBuildMode(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            bool currentValue = settings.GetToggle("BuildMode");
            settings.SetToggle("BuildMode", !currentValue);
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Build Mode is now {stateMessage}");
        }
    }

    private static void ToggleInspectMode(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            bool currentValue = settings.GetToggle("InspectToggle");
            settings.SetToggle("InspectToggle", !currentValue);
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Inspect Mode is now {stateMessage}");
        }
    }

    private static void ToggleImmortalTiles(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            bool currentValue = settings.GetToggle("ImmortalTiles");
            settings.SetToggle("ImmortalTiles", !currentValue);
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Immortal Tiles are now {stateMessage}");
        }
    }

    private static void CycleGridSize(Player player, ulong playerId)
    {
        // Assuming you keep the TileSnap as an int and not move it to the toggles dictionary
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            settings.TileSnap = (settings.TileSnap + 1) % TileSets.gridSizes.Length;
            Databases.playerBuildSettings[playerId] = settings;
            float currentGridSize = TileSets.gridSizes[settings.TileSnap];
            Databases.SaveBuildSettings();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Grid size is now set to {currentGridSize} units.");
        }
    }

    private static void ToggleMapIconPlacement(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            bool currentValue = settings.GetToggle("MapIconToggle");
            settings.SetToggle("MapIconToggle", !currentValue);
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Map Icon Placement is now {stateMessage}");
        }
    }

    private static void ToggleSnapping(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            bool currentValue = settings.GetToggle("SnappingToggle");
            settings.SetToggle("SnappingToggle", !currentValue);
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Grid snapping is now {stateMessage}.");
        }
    }


    private static void ToggleTileRotation(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            switch (settings.TileRotation)
            {
                case 0:
                    settings.TileRotation = 90;
                    break;

                case 90:
                    settings.TileRotation = 180;
                    break;

                case 180:
                    settings.TileRotation = 270;
                    break;

                case 270:
                    settings.TileRotation = 0;
                    break;

                default:
                    settings.TileRotation = 0; // Reset to 0 if somehow an invalid value is set
                    break;
            }

            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            // Assuming you have a similar utility method for sending messages as in your base example
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Tile Rotation is now set to {settings.TileRotation} degrees");
        }
    }

    private static void UndoLastTilePlacement(Player player, ulong playerId)
    {
        EntityManager entityManager = VWorld.Server.EntityManager;
        ulong platformId = playerId; // Assuming playerId maps directly to platformId in your context

        if (Databases.playerBuildSettings.TryGetValue(platformId, out var settings))
        {
            string lastTileRef = settings.PopLastTilePlaced();
            if (!string.IsNullOrEmpty(lastTileRef))
            {
                string[] parts = lastTileRef.Split(", ");
                if (parts.Length == 2 && int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int version))
                {
                    Entity tileEntity = new Entity { Index = index, Version = version };
                    if (entityManager.Exists(tileEntity))
                    {
                        SystemPatchUtil.Destroy(tileEntity);
                        ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Successfully destroyed last tile placed.");
                        Databases.SaveBuildSettings();
                    }
                    else
                    {
                        ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "The tile could not be found or has already been modified.");
                    }
                }
                else
                {
                    ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Failed to parse the reference to the last tile placed.");
                }
            }
            else
            {
                ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "You have not placed any tiles yet or all undos have been used.");
            }
        }
        else
        {
            ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "You have not placed any tiles yet.");
        }
    }
}