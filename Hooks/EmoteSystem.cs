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
using UnityEngine;
using VampireCommandFramework;
using VBuild.BuildingSystem;
using VBuild.Core;
using VBuild.Core.Converters;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using VBuild.Data;
using VRising.GameData;
using VRising.GameData.Models;
using static VBuild.Core.Services.PlayerService;
using static VBuild.Core.Services.UnitSpawnerService;
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
            { -1462274656, ToggleBuildMode }, // Bow
            { -26826346, ToggleConvert }, // Clap
            { -452406649, ToggleInspectMode }, // Point
            { -53273186, ToggleKillMode }, // No
            { -370061286, ToggleCopyMode }, // Salute
            { -578764388, UndoLastTilePlacement }, // Shrug
            { 808904257, ToggleBuffMode }, // Sit
            { -1064533554, ToggleTileRotation}, // Surrender
            { -158502505, ToggleDebuffMode }, // Taunt
            { 1177797340, ResetToggles }, // Wave
            { -1525577000, ToggleControlMode } // Yes
        };

    static EmoteSystemPatch()
    {
        emoteActions = new Dictionary<int, Action<Player, ulong>>()
        {
            { -658066984, ToggleSnapping }, // Beckon
            { -1462274656, ToggleBuildMode }, // Bow
            { -26826346, ToggleConvert }, // Clap
            { -452406649, ToggleInspectMode }, // Point
            { -53273186, ToggleKillMode }, // No
            { -370061286, ToggleCopyMode }, // Salute so for multiple wheels it'd be something like use copy mode, replace with toggles for mode with option to exit mode
            { -578764388, UndoLastTilePlacement }, // Shrug
            { 808904257, ToggleBuffMode }, // Sit
            { -1064533554, ToggleTileRotation }, // Surrender
            { -158502505, ToggleDebuffMode }, // Taunt
            { 1177797340, ResetToggles }, // Wave
            { -1525577000, ToggleControlMode } // Yes
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
            ResetAllToggles(playerId, "CopyToggle");
            // Toggle the CopyModeToggle value
            //bool currentValue = settings.GetToggle("CopyToggle");
            //settings.SetToggle("CopyToggle", !currentValue);
            // Update the player's build settings in the database
        
            string stateMessage = settings.GetToggle("CopyToggle") ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"CopyMode: |{stateMessage}|");
        }
    }
    private static void ToggleBuffMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "BuffToggle");
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            ResetAllToggles(playerId, "BuffToggle");
            // Toggle the CopyModeToggle value
            //bool currentValue = settings.GetToggle("CopyToggle");
            //settings.SetToggle("CopyToggle", !currentValue);
            // Update the player's build settings in the database

            string stateMessage = settings.GetToggle("BuffToggle") ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"BuffMode: |{stateMessage}|");
        }
    }

    private static void ToggleKillMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "KillToggle");

        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            bool currentValue = settings.GetToggle("KillToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            Databases.SaveBuildSettings();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"KillMode: |{stateMessage}|");
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
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"InteractSkills: |{stateMessage}|");
        }
    }

    private static void ToggleInspectMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "InspectToggle");

        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            bool currentValue = settings.GetToggle("InspectToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            Databases.SaveBuildSettings();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"InspectMode: |{stateMessage}|");
        }
    }

    private static void ToggleDebuffMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "DebuffToggle");

        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            bool currentValue = settings.GetToggle("DebuffToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            Databases.SaveBuildSettings();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"DebuffMode: |{stateMessage}|");
        }
    }
    private static void ToggleConvert(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "ConvertToggle");

        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            bool currentValue = settings.GetToggle("ConvertToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            Databases.SaveBuildSettings();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"ConvertMode: |{stateMessage}|");
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
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"ImmortalTiles: |{stateMessage}|");
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
            string colorFloat = VBuild.Core.Toolbox.FontColors.Cyan(currentGridSize.ToString());
            Databases.SaveBuildSettings();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"GridSize: {colorFloat}u");
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
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"MapIcons: |{stateMessage}|");
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
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"GridSnap: |{stateMessage}|");
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
            string colorString = VBuild.Core.Toolbox.FontColors.Cyan(settings.TileRotation.ToString());
            // Assuming you have a similar utility method for sending messages as in your base example
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"TileRotatiom: {colorString}°");
        }
    }

    private static void SetTileRotationTo0(Player player, ulong playerId)
    {
        SetTileRotation(player, playerId, 0);
    }

    private static void SetTileRotationTo90(Player player, ulong playerId)
    {
        SetTileRotation(player, playerId, 90);
    }

    private static void SetTileRotationTo180(Player player, ulong playerId)
    {
        SetTileRotation(player, playerId, 180);
    }
    private static void SetTileRotationTo270(Player player, ulong playerId)
    {
        SetTileRotation(player, playerId, 270);
    }

    // General method to set tile rotation
    private static void SetTileRotation(Player player, ulong playerId, int rotation)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            settings.TileRotation = rotation;
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"TileRotation: {rotation}°");
        }
    }

    public static void ControlCommand(Entity senderUserEntity)
    {
        PlayerService.TryGetCharacterFromName(senderUserEntity.Read<User>().CharacterName.ToString(), out Entity Character);
        FromCharacter fromCharacter = new FromCharacter()
        {
            User = senderUserEntity,
            Character = Character
        };
        DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
        if (Databases.playerBuildSettings.TryGetValue(senderUserEntity.Read<User>().PlatformId, out var settings))
        {
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
                    PrefabGUID prefabGUID = hoveredEntity.Read<PrefabGUID>();
                    string colorString = VBuild.Core.Toolbox.FontColors.Cyan(prefabGUID.LookupName());
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, senderUserEntity.Read<User>(), $"Controlling: {colorString})");
                    settings.OriginalBody = Character.Index + ", " + Character.Version;
                    Databases.SaveBuildSettings();
                    return;
                }
            }
        }
        else
        {
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, senderUserEntity.Read<User>(), "Couldn't find create settings.");
        }
            
    
        
        
        
        
        
        
    }
    private static void ResetToggles(Player player, ulong playerId)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // Default all toggles to false
            settings.SetToggle("ControlToggle", false);
            settings.SetToggle("KillToggle", false);
            settings.SetToggle("BuildMode", false);
            settings.SetToggle("InspectToggle", false);
            settings.SetToggle("SnappingToggle", false);
            settings.SetToggle("ImmortalTiles", false);
            settings.SetToggle("MapIconToggle", false);
            settings.SetToggle("CopyToggle", false);
            settings.SetToggle("DebuffToggle", false);



            // Enable the exceptToggle, if specified


            // Update the player's build settings in the database
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "All toggles reset.");
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
        }
    }

    private static void ResetAllToggles(ulong playerId, string exceptToggle)
    {
        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            // Default all toggles to false
            settings.SetToggle("ControlToggle", false);
            settings.SetToggle("KillToggle", false);
            settings.SetToggle("BuildMode", false);
            settings.SetToggle("InspectToggle", false);
            settings.SetToggle("SnappingToggle", false);
            settings.SetToggle("ImmortalTiles", false);
            settings.SetToggle("MapIconToggle", false);
            settings.SetToggle("CopyToggle", false);
            settings.SetToggle("DebuffToggle", false);



            // Enable the exceptToggle, if specified
            if (!string.IsNullOrEmpty(exceptToggle))
            {
                settings.SetToggle(exceptToggle, true);
            }

            // Update the player's build settings in the database
            Databases.playerBuildSettings[playerId] = settings;
            Databases.SaveBuildSettings();
        }
    }

    // Example of modifying the ToggleControlMode to use the new method
    private static void ToggleControlMode(Player player, ulong playerId)
    {

        // First, reset all toggles except the one being toggled
        ResetAllToggles(playerId, "ControlToggle");

        if (Databases.playerBuildSettings.TryGetValue(playerId, out var settings))
        {
            if (settings.OriginalBody.Length > 0)
            {
                string[] parts = settings.OriginalBody.Split(", ");
                if (parts.Length == 2 && int.TryParse(parts[0], out int index) && int.TryParse(parts[1], out int version))
                {
                    Entity originalBody = new Entity { Index = index, Version = version };
                    if (VWorld.Server.EntityManager.Exists(originalBody))
                    {
                        ControlDebugEvent controlDebugEvent = new ControlDebugEvent()
                        {
                            EntityTarget = originalBody,
                            Target = originalBody.Read<NetworkId>()
                        };
                        DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
                        existingSystem.ControlUnit(new FromCharacter() { User = player.User, Character = player.Character }, controlDebugEvent);
                        settings.OriginalBody = "";
                        Databases.SaveBuildSettings();
                        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "Returned to original body.");
                        
                        return;
                    }
                }
            }
            else
            {
                bool currentValue = settings.GetToggle("ControlToggle");
                string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
                Databases.SaveBuildSettings();
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"ControlMode: |{stateMessage}|");
            }
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            
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
                        try
                        {
                            Entity tile = new Entity { Index = index, Version = version+1 };
                            if (entityManager.Exists(tileEntity))
                            {
                                SystemPatchUtil.Destroy(tileEntity);
                                ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Successfully destroyed last tile placed.");
                                Databases.SaveBuildSettings();
                            }
                        }
                        catch 
                        {
                            try
                            {
                                Entity tile = new Entity { Index = index, Version = version + 2 };
                                if (entityManager.Exists(tileEntity))
                                {
                                    SystemPatchUtil.Destroy(tileEntity);
                                    ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Successfully destroyed last tile placed.");
                                    Databases.SaveBuildSettings();
                                }
                            }
                            catch (Exception d)
                            {
                                Plugin.Logger.LogInfo(d);
                            }
                        }
                        ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "The tile could not be found or has already been modified more than twice.");
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