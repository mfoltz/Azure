using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Core;
using System.Text.Json;
using System.Text.RegularExpressions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AssetBundlePatching;
using UnityEngine.SceneManagement;
using VampireCommandFramework;

namespace RPGAddOnsEx.Core
{
    [CommandGroup(name: "ddcommands", shortHand: "dd")]
    internal class ChatCommands
    {
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

        public static SetDebugSettingEvent CastleLimitsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)31,
            Value = false
        };

        [Command(name: "togglefreebuild", shortHand: "tfb", adminOnly: true, usage: ".dd tfb", description: "Toggles freebuild debug settings.")]
        public static void ToggleBuildDebugCommand(ChatCommandContext ctx)
        {
            User user = ctx.Event.User;
            DebugEventsSystem existingSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            ChatCommands.BuildingCostsDebugSetting.Value = !ChatCommands.BuildingCostsDebugSetting.Value;
            existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingCostsDebugSetting);
            ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value = !ChatCommands.BuildingPlacementRestrictionsDisabledSetting.Value;
            existingSystem.SetDebugSetting(user.Index, ref ChatCommands.BuildingPlacementRestrictionsDisabledSetting);
            ChatCommands.CastleLimitsDisabledSetting.Value = !ChatCommands.CastleLimitsDisabledSetting.Value;
            existingSystem.SetDebugSetting(user.Index, ref ChatCommands.CastleLimitsDisabledSetting);
            if (ChatCommands.BuildingCostsDebugSetting.Value)
            {
                string enabledColor = DismantleDenier.Core.FontColors.Green("enabled");
                ctx.Reply($"freebuild: {enabledColor}");
            }
            else
            {
                string disabledColor = DismantleDenier.Core.FontColors.Red("enabled");
                ctx.Reply($"freebuild: {disabledColor}");
            }
        }
    }
}