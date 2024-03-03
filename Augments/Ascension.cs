using RPGMods;
using VampireCommandFramework;
using VPlus;

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


        
    }
}