using System.Text.Json;

namespace RPGAddOns
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
        public static Dictionary<ulong, ResetData> playerResetCountsBuffs = new Dictionary<ulong, ResetData>();
        public static Dictionary<ulong, PrestigeData> playerPrestige = new Dictionary<ulong, PrestigeData>();

    }
}
