using System.Text.Json;
using RPGAddOns.Prestige;
using RPGAddOns.PvERank;

namespace RPGAddOns.Core
{
    public class Databases
    {
        public static JsonSerializerOptions JSON_options = new()
        {
            WriteIndented = false,
            IncludeFields = false
        };

        public static JsonSerializerOptions Pretty_JSON_options = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public static Dictionary<ulong, PrestigeData> playerPrestiges = new();
        public static Dictionary<ulong, RankData> playerRanks = new();
    }
}