using RPGMods.Commands;
using VampireCommandFramework;
using Plugin = VPlus.Core.Plugin;
using VPlus.Core.Commands;
using VRising.GameData.Models;
using Unity.Entities;
using ProjectM.Network;
using ProjectM;
using Bloodstone.API;
using VPlus.Hooks;

namespace VPlus.Augments
{
    public class DivineData
    {
        private static readonly string redV = VPlus.Core.Toolbox.FontColors.Red("V");
        public int Divinity { get; set; }
        public int VTokens { get; set; }
        public DateTime LastConnectionTime { get; private set; }
        public DateTime LastAwardTime { get; private set; }

        public bool Spawned { get; set; }

        public DivineData(int divinity, int vtokens)
        {
            Divinity = divinity;
            VTokens = vtokens;
            LastConnectionTime = DateTime.UtcNow;
            LastAwardTime = DateTime.UtcNow;
        }

        public void OnUserConnected()
        {
            LastConnectionTime = DateTime.UtcNow;
        }

        public void OnUserDisconnected(User user, DivineData divineData)
        {
            UpdateVPoints();
            LastConnectionTime = DateTime.UtcNow; // Reset for next session
            EntityManager entityManager = VWorld.Server.EntityManager;
            ServerChatUtils.SendSystemMessageToClient(entityManager, user, $"Your {redV} Tokens have been updated, don't forget to redeem them: {VPlus.Core.Toolbox.FontColors.Yellow(divineData.VTokens.ToString())}");
        }

        public void UpdateVPoints()
        {
            TimeSpan timeOnline = DateTime.UtcNow - LastConnectionTime;
            int minutesOnline = (int)timeOnline.TotalMinutes;
            if (minutesOnline > 0)
            {
                VTokens += minutesOnline * VPlus.Core.Plugin.PointsPerMinute;
                LastAwardTime = DateTime.UtcNow;
            }
        }
    }

    internal class Ascension
    {
        // one method for handling all cases of ascension? also method for checking requirements are met
        public static void AscensionCheck(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
        {
            // check requirements are met and return true if so, false if not
            bool requirementsMet = false; //need to implement this
            requirementsMet = CheckRequirements(ctx, playerName, SteamID, data);
            if (requirementsMet)
            {
                // run thing here, thing return true if works
                if (ApplyAscensionBonuses(ctx, playerName, SteamID, data))
                {
                    data.Divinity++;
                    ChatCommands.SavePlayerDivinity();
                }
            }
            else
            {
                ctx.Reply("You do not meet the requirements to ascend.");
                return;
            }
        }

        public static bool ApplyAscensionBonuses(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
        {
            // Initial stats before ascension bonuses are applied
            int preHealth = 0;
            int prePhysicalPower = 0;
            int preSpellPower = 0;
            int prePhysicalResistance = 0;
            int preSpellResistance = 0;

            // Check if the player has previous power-up data and retrieve it
            if (RPGMods.Utils.Database.PowerUpList.TryGetValue(SteamID, out RPGMods.Utils.PowerUpData preStats))
            {
                preHealth = (int)preStats.MaxHP;
                prePhysicalPower = (int)preStats.PATK;
                preSpellPower = (int)preStats.SATK;
                prePhysicalResistance = (int)preStats.PDEF;
                preSpellResistance = (int)preStats.SDEF;
            }

            // Set stat bonus values and add pre-existing bonuses for continuity

            int extraHealth = preHealth + Plugin.AscensionHealthBonus;
            int extraPhysicalPower = prePhysicalPower + Plugin.AscensionPhysicalPowerBonus * Plugin.divineMultiplier;
            int extraSpellPower = preSpellPower + Plugin.AscensionSpellPowerBonus * Plugin.divineMultiplier;
            float extraPhysicalResistance = (float)(prePhysicalResistance + Plugin.AscensionPhysicalResistanceBonus);
            float extraSpellResistance = (float)(preSpellResistance + Plugin.AscensionSpellResistanceBonus);

            // Example condition to limit the maximum number of ascensions
            if (data.Divinity == Plugin.MaxAscensions)
            {
                ctx.Reply("You have reached the maximum number of ascensions.");
                return false;
            }

            // Apply the updated stats to the player
            PowerUp.powerUP(ctx, playerName, "add", extraHealth, extraPhysicalPower, extraSpellPower, extraPhysicalResistance, extraSpellResistance);
            return true;
        }

        public enum AscensionLevel
        {
            Level0,
            Level1,
            Level2,
            Level3,
            Level4
        }

        public static List<int> ParsePrefabIdentifiers(string prefabIds)
        {
            // Removing the brackets at the start and end, then splitting by commas
            var ids = prefabIds.Trim('[', ']').Split(',');
            return ids.Select(int.Parse).ToList();
        }

        public static bool CheckRequirements(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
        {
            AscensionLevel ascensionLevel = (AscensionLevel)(data.Divinity);
            List<int> prefabIds;

            // Determine the prefab IDs based on the ascension level
            switch (ascensionLevel)
            {
                case AscensionLevel.Level0:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsFirstAscension);
                    break;

                case AscensionLevel.Level1:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsSecondAscension);
                    break;

                case AscensionLevel.Level2:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsThirdAscension);
                    break;

                case AscensionLevel.Level3:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsFourthAscension);
                    break;
                case AscensionLevel.Level4:
                    //ctx.Reply("You have reached the maximum number of ascensions.");
                    return false;
                default:
                    throw new InvalidOperationException("Unknown Ascension Level");
            }

            return CheckLevelRequirements(ctx, data, prefabIds);
        }

        public static bool CheckLevelRequirements(ChatCommandContext ctx, DivineData _, List<int> prefabIds)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            var user = ctx.User;
            UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(user.PlatformId);
            Entity characterEntity = userModel.FromCharacter.Character;

            // Aggregate required quantities for each PrefabGUID
            var requiredQuantities = prefabIds
                .Select((id, index) => new PrefabGUID(id))
                .GroupBy(guid => guid)
                .ToDictionary(group => group.Key, group => group.Count());

            // Aggregate actual quantities in the user's inventory
            var actualQuantities = userModel.Inventory.Items
                .GroupBy(item => item.Item.PrefabGUID)
                .ToDictionary(group => group.Key, group => group.Sum(item => item.Stacks));

            // Check if all required items with their quantities are present in the inventory
            foreach (var requirement in requiredQuantities)
            {
                if (!actualQuantities.TryGetValue(requirement.Key, out var actualQuantity) || actualQuantity < requirement.Value)
                {
                    return false; // Requirement not met, return early
                }
            }

            return true;
        }
    }
}