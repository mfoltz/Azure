using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using static VCreate.Core.Services.PlayerService;
using User = ProjectM.Network.User;

namespace VCreate.Hooks;

[HarmonyPatch]
internal class EmoteSystemPatch
{
    private static readonly string enabledColor = VCreate.Core.Toolbox.FontColors.Green("enabled");
    private static readonly string disabledColor = VCreate.Core.Toolbox.FontColors.Red("disabled");

    public static readonly Dictionary<int, Action<Player, ulong>> emoteActions = new Dictionary<int, Action<Player, ulong>>()
        {
            { -658066984, ToggleTileMode }, // Beckon
            { -1462274656, ToggleTileRotation }, // Bow
            { -26826346, ToggleConvert }, // Clap
            { -452406649, ToggleInspectMode }, // Point
            { -53273186, ToggleKillMode }, // No
            { -370061286, ToggleCopyMode }, // Salute
            { -578764388, ToggleImmortalTiles }, // Shrug
            { 808904257, ToggleBuffMode }, // Sit
            { -1064533554, ToggleMapIconPlacement}, // Surrender
            { -158502505, ToggleDebuffMode }, // Taunt
            { 1177797340, ResetToggles }, // Wave
            { -1525577000, ToggleSnapping } // Yes
        };

    static EmoteSystemPatch()
    {
        emoteActions = new Dictionary<int, Action<Player, ulong>>()
        {
            { -658066984, ToggleTileMode }, // Beckon
            { -1462274656, ToggleTileRotation }, // Bow
            { -26826346, ToggleConvert }, // Clap
            { -452406649, ToggleInspectMode }, // Point
            { -53273186, ToggleKillMode }, // No
            { -370061286, ToggleCopyMode }, // Salute
            { -578764388, ToggleImmortalTiles }, // Shrug
            { 808904257, ToggleBuffMode }, // Sit
            { -1064533554, ToggleMapIconPlacement}, // Surrender
            { -158502505, ToggleDebuffMode }, // Taunt
            { 1177797340, ResetToggles }, // Wave
            { -1525577000, ToggleSnapping } // Yes
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
            if (DataStructures.PlayerSettings.TryGetValue(_playerId, out Omnitool data) && !data.Emotes) continue;
            
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
        ResetAllToggles(playerId, "CopyToggle");
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            string stateMessage = settings.GetMode("CopyToggle") ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"CopyMode: |{stateMessage}|");
        }
    }

    private static void ToggleBuffMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "BuffToggle");
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            string stateMessage = settings.GetMode("BuffToggle") ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"BuffMode: |{stateMessage}|");
        }
    }

    private static void ToggleLinkMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "LinkToggle");
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            string stateMessage = settings.GetMode("LinkToggle") ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"LinkMode: |{stateMessage}|");
        }
    }

    private static void ToggleTileMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "TileToggle");
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            string stateMessage = settings.GetMode("TileToggle") ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"TileMode: |{stateMessage}|");
        }
    }

    private static void ToggleEquipMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "EquipToggle");
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            string stateMessage = settings.GetMode("EquipToggle") ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"EquipMode: |{stateMessage}|");
        }
    }

    private static void ToggleKillMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "KillToggle");
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            bool currentValue = settings.GetMode("KillToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"KillMode: |{stateMessage}|");
        }
    }

    private static void ToggleInspectMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "InspectToggle");

        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            bool currentValue = settings.GetMode("InspectToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            DataStructures.Save();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"InspectMode: |{stateMessage}|");
        }
    }

    private static void ToggleDebuffMode(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "DebuffToggle");

        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            // The actual value is set in ResetAllToggles; here, we just trigger UI update and messaging
            bool currentValue = settings.GetMode("DebuffToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"DebuffMode: |{stateMessage}|");
        }
    }

    private static void ToggleConvert(Player player, ulong playerId)
    {
        ResetAllToggles(playerId, "ConvertToggle");

        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            bool currentValue = settings.GetMode("ConvertToggle");
            string stateMessage = currentValue ? enabledColor : disabledColor; // Notice the change due to toggle reset behavior
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"ConvertMode: |{stateMessage}|");
        }
    }

    private static void ToggleImmortalTiles(Player player, ulong playerId)
    {
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            bool currentValue = settings.GetMode("ImmortalToggle");
            settings.SetMode("ImmortalToggle", !currentValue);
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            DataStructures.PlayerSettings[playerId] = settings;
            DataStructures.Save();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"ImmortalTiles: |{stateMessage}|");
        }
    }

    private static void CycleGridSize(Player player, ulong playerId)
    {
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out Omnitool settings))
        {
            settings.SetData("GridSize", (settings.GetData("GridSize") + 1) % OnHover.gridSizes.Length);
            //settings.TileSnap = (settings.TileSnap + 1) % OnHover.gridSizes.Length;
            DataStructures.PlayerSettings[playerId] = settings;
            float currentGridSize = OnHover.gridSizes[settings.GetData("GridSize")];
            string colorFloat = VCreate.Core.Toolbox.FontColors.Cyan(currentGridSize.ToString());
            DataStructures.Save();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"GridSize: {colorFloat}u");
        }
    }

    private static void ToggleMapIconPlacement(Player player, ulong playerId)
    {
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out var settings))
        {
            bool currentValue = settings.GetMode("MapIconToggle");
            settings.SetMode("MapIconToggle", !currentValue);
            DataStructures.PlayerSettings[playerId] = settings;
            DataStructures.Save();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"MapIcons: |{stateMessage}|");
        }
    }

    private static void ToggleSnapping(Player player, ulong playerId)
    {
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out var settings))
        {
            bool currentValue = settings.GetMode("SnappingToggle");
            settings.SetMode("SnappingToggle", !currentValue);
            DataStructures.PlayerSettings[playerId] = settings;
            DataStructures.Save();
            string stateMessage = !currentValue ? enabledColor : disabledColor;
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"GridSnapping: |{stateMessage}|");
        }
    }

    private static void ToggleTileRotation(Player player, ulong playerId)
    {
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out var settings))
        {
            switch (settings.GetData("Rotation"))
            {
                case 0:
                    settings.SetData("Rotation", 90);
                    break;

                case 90:
                    settings.SetData("Rotation", 180);
                    break;

                case 180:
                    settings.SetData("Rotation", 270);
                    break;

                case 270:
                    settings.SetData("Rotation", 0);
                    break;

                default:
                    settings.SetData("Rotation", 0); // Reset to 0 if somehow an invalid value is set
                    break;
            }

            DataStructures.PlayerSettings[playerId] = settings;
            DataStructures.Save();
            string colorString = VCreate.Core.Toolbox.FontColors.Cyan(settings.GetData("Rotation").ToString());
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
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out var settings))
        {
            settings.SetData("Rotation", rotation);
            DataStructures.PlayerSettings[playerId] = settings;
            DataStructures.Save();
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), $"TileRotation: {rotation}°");
        }
    }
    //[Command(name: "returnToBody", shortHand: "return", adminOnly: true, usage: ".return", description: "Backup method to return to body on hover.")]
    

    private static void ResetToggles(Player player, ulong playerId)
    {
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out var settings))
        {
            // Default all toggles to false
            settings.SetMode("KillToggle", false);
            settings.SetMode("TileToggle", false);
            settings.SetMode("InspectToggle", false);
            settings.SetMode("SnappingToggle", false);
            settings.SetMode("ImmortalToggle", false);
            settings.SetMode("MapIconToggle", false);
            settings.SetMode("CopyToggle", false);
            settings.SetMode("DebuffToggle", false);
            settings.SetMode("ConvertToggle", false);
            settings.SetMode("BuffToggle", false);


            // Enable the exceptToggle, if specified

            // Update the player's build settings in the database
            ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, player.User.Read<User>(), "All toggles reset.");
            DataStructures.PlayerSettings[playerId] = settings;
            DataStructures.Save();
        }
    }

    private static void ResetAllToggles(ulong playerId, string exceptToggle)
    {
        if (DataStructures.PlayerSettings.TryGetValue(playerId, out var settings))
        {
            // Default all toggles to false
            settings.SetMode("KillToggle", false);
            settings.SetMode("TileToggle", false);
            settings.SetMode("InspectToggle", false);
            settings.SetMode("SnappingToggle", false);
            settings.SetMode("ImmortalToggle", false);
            settings.SetMode("MapIconToggle", false);
            settings.SetMode("CopyToggle", false);
            settings.SetMode("DebuffToggle", false);
            settings.SetMode("ConvertToggle", false);
            settings.SetMode("BuffToggle", false);

            // Enable the exceptToggle, if specified
            if (!string.IsNullOrEmpty(exceptToggle))
            {
                settings.SetMode(exceptToggle, true);
            }

            // Update the player's build settings in the database
            DataStructures.PlayerSettings[playerId] = settings;
            DataStructures.Save();
        }
    }

    

    private static void UndoLastTilePlacement(Player player, ulong playerId)
    {
        EntityManager entityManager = VWorld.Server.EntityManager;
        ulong platformId = playerId; // Assuming playerId maps directly to platformId in your context

        if (DataStructures.PlayerSettings.TryGetValue(platformId, out var settings))
        {
            string lastTileRef = settings.PopEntity();
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
                        DataStructures.Save();
                    }
                    /*
                    else
                    {
                        try
                        {
                            Entity tile = new Entity { Index = index, Version = version + 1 };
                            if (entityManager.Exists(tileEntity))
                            {
                                SystemPatchUtil.Destroy(tileEntity);
                                ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "Successfully destroyed last tile placed.");
                                DataStructures.Save();
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
                                    DataStructures.Save();
                                }
                            }
                            catch (Exception d)
                            {
                                Plugin.Log.LogInfo(d);
                            }
                        }
                        ServerChatUtils.SendSystemMessageToClient(entityManager, player.User.Read<User>(), "The tile could not be found or has already been modified more than twice.");
                    }
                    */
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