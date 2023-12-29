using Il2CppSystem.Runtime.Serialization.Formatters.Binary;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VampireCommandFramework;

namespace RPGAddOns
{
    public class ResetData
    {
        public int ResetCount { get; set; }
        public List<int> Buffs { get; set; }

        public ResetData(int count, List<int> buffs)
        {
            ResetCount = count;
            Buffs = buffs;
        }
    }
    public class ResetLevel
    {
        public static void ResetPlayerLevel(ChatCommandContext ctx, string playerName, ulong SteamID, string StringID)
        {
            if (ExperienceSystem.getLevel(SteamID) >= ExperienceSystem.MaxLevel)
            {

            }
            else
            {
                ctx.Reply("You have not reached the maximum level yet.");
                return;
            }

        }
        public class ResetLevelFunctions
        {
            public static bool BuffFlag()
            {
                return false;
            }
            public static bool ItemFlag()
            {
                return false;
            }
            
        } 
    }
}
