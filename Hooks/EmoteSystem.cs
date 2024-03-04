using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using System;
using Unity.Collections;
using Unity.Entities;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using VBuild.Data;
using static VBuild.Core.Services.PlayerService;
using static VCF.Core.Basics.RoleCommands;
using User = ProjectM.Network.User;

namespace VBuild.Hooks;

[HarmonyPatch]
internal class EmoteSystemPatch
{
    private static readonly string enabledColor = VBuild.Core.Toolbox.FontColors.Green("enabled");
    private static readonly string disabledColor = VBuild.Core.Toolbox.FontColors.Red("disabled");
    [HarmonyPatch(typeof(EmoteSystem), nameof(EmoteSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void OnUpdate_Emote(ProjectM.EmoteSystem __instance)
    {
        
        
        Dictionary<int, Action<Player, ulong>> emoteActions = new Dictionary<int, Action<Player, ulong>>()
        {
            { -658066984, ToggleSnapping }, // Beckon
            //{ -1462274656,  }, // Bow
            { -26826346, ToggleMapIconPlacement }, // Clap
            //{ 1048364815,  }, // Dance
            { -452406649, ToggleInspectMode }, // Point
            { -53273186, ToggleKillMode }, // No
            { -370061286, ToggleImmortalTiles }, // Salute
            { -578764388, UndoLastTilePlacement }, // Shrug
            //{ 808904257, ToggleSitSetting }, // Sit
            //{ -1064533554, ToggleSurrenderSetting }, // Surrender
            //{ -158502505, ToggleFollower }, // Taunt
            //{ 1177797340, ToggleWaveSetting }, // Wave
            { -1525577000, ToggleBuildMode } // Yes
        };

        
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
    private static void ToggleKillMode(Player player, ulong playerId)
    {
        // Hypothetical dictionary to hold player settings, similar to playerBuildSettings
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // Toggle the DestroyToggle property within the settings
            settings.KillToggle = !settings.KillToggle;

            // Update the player's settings in the dictionary
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            // Send a message to the client indicating the new state of Destroy Mode

            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Destroy Mode is now {(settings.KillToggle ? enabledColor : disabledColor)}");
        }
    }



    private static void ToggleBuildMode(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            settings.BuildMode = !settings.BuildMode;
            Databases.playerBuildSettings[playerId] = settings;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Build Mode is now {(settings.BuildMode ? enabledColor : disabledColor)}");
        }
    }

    private static void ToggleInspectMode(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            settings.InspectToggle = !settings.InspectToggle;
            Databases.playerBuildSettings[playerId] = settings;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Inspect Mode is now {(settings.InspectToggle ? enabledColor : disabledColor)}");
        }
    }

    private static void ToggleImmortalTiles(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            settings.ImmortalTiles = !settings.ImmortalTiles;
            Databases.playerBuildSettings[playerId] = settings;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Immortal Tiles are now {(settings.ImmortalTiles ? enabledColor : disabledColor)}");
        }
    }

    private static void ToggleMapIconPlacement(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            settings.MapIconToggle = !settings.MapIconToggle;
            Databases.playerBuildSettings[playerId] = settings;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Map Icon Placement is now {(settings.MapIconToggle ? enabledColor : disabledColor)}");
        }
    }
    private static void ToggleSnapping(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            settings.SnappingToggle = !settings.SnappingToggle;
            Databases.playerBuildSettings[playerId] = settings;
            
            
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"Snapping placement is now {(settings.SnappingToggle ? enabledColor : disabledColor)}");
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
                    if (entityManager.Exists(tileEntity) && tileEntity.Version == version)
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
