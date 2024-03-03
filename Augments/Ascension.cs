using RPGMods;
using RPGMods.Commands;
using VampireCommandFramework;
using VPlus.Core;
using VPlus;
using Plugin = VPlus.Core.Plugin;
using VPlus.Core.Commands;

namespace V.Augments
{
    public class DivineData
    {
        public int Divinity { get; set; }
        public int VPoints { get; set; }
        public DateTime LastConnectionTime { get; private set; }
        public DateTime LastAwardTime { get; private set; }

        public DivineData(int divinity, int vpoints)
        {
            Divinity = divinity;
            VPoints = vpoints;
            LastConnectionTime = DateTime.UtcNow;
            LastAwardTime = DateTime.UtcNow;
        }

        public void OnUserConnected()
        {
            LastConnectionTime = DateTime.UtcNow;
        }

        public void OnUserDisconnected()
        {
            UpdateVPoints();
            LastConnectionTime = DateTime.UtcNow; // Reset for next session
        }

        // This method now only calculates points without updating LastAwardTime
        public void UpdateVPoints()
        {
            TimeSpan timeOnline = DateTime.UtcNow - LastConnectionTime;
            int hoursOnline = (int)timeOnline.TotalHours;
            if (hoursOnline > 0)
            {
                VPoints += hoursOnline * VPlus.Core.Plugin.PointsPerHour;
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
            bool requirementsMet = true; //need to implement this
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
                // Don't give more bonuses after 10 ascensions
                ctx.Reply("You have reached the maximum number of ascensions.");
                return false;
            }

            // Apply the updated stats to the player
            PowerUp.powerUP(ctx, playerName, "add", extraHealth, extraPhysicalPower, extraSpellPower, extraPhysicalResistance, extraSpellResistance);
            return true;
        }



    }
}