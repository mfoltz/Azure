using System.Text.Json;

namespace WorldBuild.Core
{
    public class Databases
    {
        public static void SaveBuildSettings()
        {
            File.WriteAllText(Plugin.BuildSettingsJson, JsonSerializer.Serialize(Databases.playerBuildSettings));
        }

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

        public static Dictionary<ulong, BuildSettings> playerBuildSettings = new();
    }
}