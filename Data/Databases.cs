
using V.Augments.Rank;
using System.Text.Json;
using V.Augments;

namespace V.Core
{
    public class Databases
    {
        public static JsonSerializerOptions JSON_options = new()
        {
            WriteIndented = false,
            IncludeFields = true
        };

        public static JsonSerializerOptions Pretty_JSON_options = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public static Dictionary<ulong, PrestigeData> playerPrestige = new();
        public static Dictionary<ulong, RankData> playerRanks = new();
        public static Dictionary<ulong, DivineData> playerDivinity = new();
    }
}