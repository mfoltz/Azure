using System.Text.Json;
using VCreate.Systems;
namespace VCreate.Core
{
    public class DataStructures
    {
        // Encapsulated fields with properties
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = false,
            IncludeFields = true
        };

        private static readonly JsonSerializerOptions prettyJsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        private static Dictionary<ulong, Omnitool> playerSettings = new Dictionary<ulong, Omnitool>();

        private static Dictionary<ulong, PetExperience> petExperience = new Dictionary<ulong, PetExperience>();

        // Property for playerSettings if external access or modification is required
        public static Dictionary<ulong, Omnitool> PlayerSettings
        {
            get => playerSettings;
            set => playerSettings = value;
        }
        public static Dictionary<ulong, PetExperience> PetExperience
        {
            get => petExperience;
            set => petExperience = value;
        }
        public static void Save()
        {
            try
            {
                //string json = JsonSerializer.Serialize(playerSettings, prettyJsonOptions); // Consider using prettyJsonOptions if you want the output to be indented.
                File.WriteAllText(Plugin.PlayerSettingsJson, JsonSerializer.Serialize(DataStructures.PlayerSettings));
                File.WriteAllText(Plugin.PetDataJson, JsonSerializer.Serialize(DataStructures.PetExperience));
            }
            catch (IOException ex)
            {
                // Handle file write exceptions
                Plugin.Log.LogInfo($"An error occurred saving settings: {ex.Message}");

            }
            catch (JsonException ex)
            {
                // Handle JSON serialization exceptions
                Plugin.Log.LogInfo($"An error occurred during JSON serialization: {ex.Message}");
            }
        }
    }
}