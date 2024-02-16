using RPGAddOnsEx.Augments;
using RPGAddOnsEx.Augments.RankUp;

using System.Text.Json;

namespace RPGAddOnsEx.Core
{
    public class DataStructures
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