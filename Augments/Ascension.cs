using RPGAddOnsEx.Core;
using Steamworks;
using VampireCommandFramework;

namespace RPGAddOnsEx.Augments
{
    public class DivineData
    {
        public int Divinity { get; set; }
        public int Path { get; set; }
        // 1 for phys 2 for spell, 0 for not there yet

        public DivineData(int divinity, int path)
        {
            Divinity = divinity;
            Path = path;
        }
    }

    internal class Ascension
    {
        // one method for handling all cases of ascension? also method for checking requirements are met
        public static bool AscensionCheck(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
        {
            // check requirements are met and return true if so, false if not
            bool requirementsMet = false;
            if (requirementsMet)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
        public static void AscendPlayer(ChatCommandContext ctx, string playerName, ulong SteamID, DivineData data)
        {
            int preHealth = 0;
            int prePhysicalPower = 0;
            int preSpellPower = 0;
            int prePhysicalResistance = 0;
            int preSpellResistance = 0;

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (RPGMods.Utils.Database.PowerUpList.ContainsKey(SteamID) != null)
            {
                if (RPGMods.Utils.Database.PowerUpList.TryGetValue(SteamID, out RPGMods.Utils.PowerUpData preStats))
                {
                    preHealth = (int)preStats.MaxHP;
                    prePhysicalPower = (int)preStats.PATK;
                    preSpellPower = (int)preStats.SATK;
                    prePhysicalResistance = (int)preStats.PDEF;
                    preSpellResistance = (int)preStats.SDEF;
                }
            }
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

            // set stat bonus values and add pre-existing bonuses for continuity
            int extraHealth = Plugin.ExtraHealth + preHealth;
            int extraPhysicalPower = Plugin.ExtraPhysicalPower + prePhysicalPower;
            int extraSpellPower = Plugin.ExtraSpellPower + preSpellPower;
            int extraPhysicalResistance = Plugin.ExtraPhysicalResistance + prePhysicalResistance;
            int extraSpellResistance = Plugin.ExtraSpellResistance + preSpellResistance;

            ChatCommands.SavePlayerDivinity();
        }
        */
    }
}