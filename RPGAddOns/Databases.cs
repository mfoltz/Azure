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
        public static Dictionary<string, ResetData> playerResetCountsBuffs = new Dictionary<string, ResetData>();
        public static Dictionary<string, PrestigeData> playerPrestige = new Dictionary<string, PrestigeData>();

    }
}
