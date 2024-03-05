using RPGMods;
using RPGMods.Commands;
using VampireCommandFramework;
using VPlus.Core;
using VPlus;
using Plugin = VPlus.Core.Plugin;
using VPlus.Core.Commands;
using VRising.GameData.Models;
using Epic.OnlineServices.Sanctions;
using VBuild.Core.Services;
using Unity.Entities;
using ProjectM.Network;
using ProjectM;
using Bloodstone.API;
using VPlus.Hooks;
using VBuild.Core.Toolbox;
using Unity.Entities.UniversalDelegates;

namespace V.Augments
{
    public class DivineData
    {
        private static readonly string redV = VPlus.Core.Toolbox.FontColors.Red("V");
        public int Divinity { get; set; }
        public int VTokens { get; set; }
        public DateTime LastConnectionTime { get; private set; }
        public DateTime LastAwardTime { get; private set; }

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
            if (data.Divinity > Plugin.MaxAscensions)
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
            Level1,
            Level2,
            Level3,
            Level4
        }

        private static List<int> ParsePrefabIdentifiers(string prefabIds)
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
                case AscensionLevel.Level1:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsFirstAscension);
                    break;

                case AscensionLevel.Level2:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsSecondAscension);
                    break;

                case AscensionLevel.Level3:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsThirdAscension);
                    break;

                case AscensionLevel.Level4:
                    prefabIds = ParsePrefabIdentifiers(Plugin.ItemPrefabsFourthAscension);
                    break;

                default:
                    throw new InvalidOperationException("Unknown Ascension Level");
            }

            return CheckLevelRequirements(ctx, data, prefabIds);
        }

        public static bool CheckLevelRequirements(ChatCommandContext ctx, DivineData data, List<int> prefabIds)
        {
            bool itemCheck = true;
            EntityManager entityManager = VWorld.Server.EntityManager;
            var user = ctx.User;
            UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(user.PlatformId);
            Entity characterEntity = userModel.FromCharacter.Character;
            List<PrefabGUID> prefabGUIDs = prefabIds.Select(id => new PrefabGUID(id)).ToList();

            for (int i = 0; i < prefabGUIDs.Count; i++)
            {
                
                int prefabQuantity = i + 1; // cost multiplier per prefab based on position in the list
                if (prefabGUIDs[i].GuidHash == 0)
                {
                    continue;
                }

                if (InventoryUtilities.TryGetInventoryEntity(entityManager, characterEntity, out Entity inventoryEntity))
                {
                    if (!InventoryUtilitiesServer.TryRemoveItem(entityManager, inventoryEntity, prefabGUIDs[i], prefabQuantity))
                    {
                        itemCheck = false;
                        // give back
                        // go through list in reverse
                        break; // Exit the loop if any required item is missing
                    }
                }
            }

            if (!itemCheck)
            {
                ctx.Reply("You do not have the required items to ascend.");
                // give back here
                if (InventoryUtilities.TryGetInventoryEntity(entityManager, characterEntity, out Entity inventoryEntity))
                {
                    for (int i = 0; i < prefabGUIDs.Count; i++)
                    {
                        
                        int prefabQuantity = i + 1; // cost multiplier per prefab based on position in the list
                        if (prefabGUIDs[i].GuidHash == 0)
                        {
                            continue;
                        }
                        VBloodSystemPatch.AddItemToInventory(prefabGUIDs[i], prefabQuantity, userModel);
                    }
                }

                return false;
            }

            // Since we're ignoring the bloodline check for now, we assume it's always true
            return true;
        }
    }
}