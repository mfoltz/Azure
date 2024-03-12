using Bloodstone.API;
using ProjectM.Network;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VampireCommandFramework;
using VCreate.Core.Toolbox;

namespace VCreate.Core.Commands
{
    public class WorldBuildToggle
    {
        private static bool wbFlag = false;

        public static bool WbFlag
        {
            get { return wbFlag; }
        }

        private static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)5,
            Value = false
        };

        private static SetDebugSettingEvent BuildingPlacementRestrictionsDisabledSetting = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)16,
            Value = false
        };

        private static SetDebugSettingEvent CastleHeartConnectionRequirementDisabled = new SetDebugSettingEvent()
        {
            SettingType = (DebugSettingType)27,
            Value = false
        };

        [Command(name: "toggleWorldBuild", shortHand: "twb", adminOnly: true, usage: ".twb", description: "Toggles worldbuilding debug settings for no-cost building anywhere.")]
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
        private static bool buildingCostsFlag = false;

        private static SetDebugSettingEvent BuildingCostsDebugSetting = new SetDebugSettingEvent()
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
            SettingType = (DebugSettingType)27,
            Value = false
        };

        [Command(name: "toggleCastleHeartConnectionRequirement", shortHand: "tch", adminOnly: true, usage: ".tch", description: "Toggles the Castle Heart connection requirement for structures.")]
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
}
