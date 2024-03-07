using ProjectM;
using System.Text.RegularExpressions;
using VBuild.Core;
using VampireCommandFramework;
using VBuild.BuildingSystem;

namespace VBuild.BuildingSystem
{
    public class BuildSettings
    {
        public readonly Dictionary<string, bool> toggles = new Dictionary<string, bool>();

        public int TileRotation { get; set; }
        public int TileModel { get; set; }
        public int TileSnap { get; set; }
        public string TileSet { get; set; }
        public Stack<string> LastTilesPlaced { get; set; } = new Stack<string>();
        public int MapIcon { get; set; }
        public string OriginalBody { get; set; }

        // Constructor
        public BuildSettings()
        {
            // Initialize default values for toggles
            SetToggle("CanEditTiles", false);
            SetToggle("BuildMode", false);
            SetToggle("InspectToggle", false);
            SetToggle("SnappingToggle", false);
            SetToggle("ImmortalTiles", false);
            SetToggle("MapIconToggle", false);
            SetToggle("KillToggle", false);
            SetToggle("CopyToggle", false);
            SetToggle("ControlToggle", false);
            
        }

        // Method to set a toggle value
        public void SetToggle(string key, bool value)
        {
            if (toggles.ContainsKey(key))
            {
                toggles[key] = value;
            }
            else
            {
                toggles.Add(key, value);
            }
        }

        // Method to get a toggle value
        public bool GetToggle(string key)
        {
            if (toggles.TryGetValue(key, out bool value))
            {
                return value;
            }
            return false; // Default value if key doesn't exist
        }

        // Existing methods like AddTilePlaced and PopLastTilePlaced
        public void AddTilePlaced(string tileRef)
        {
            if (LastTilesPlaced.Count >= 10)
            {
                LastTilesPlaced.Pop(); // Ensure we only keep the last 10
            }
            LastTilesPlaced.Push(tileRef);
        }

        public string PopLastTilePlaced()
        {
            return LastTilesPlaced.Count > 0 ? LastTilesPlaced.Pop() : null;
        }
    }
}