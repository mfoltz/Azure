using ProjectM;
using System.Text.RegularExpressions;
using VBuild.Core;
using VampireCommandFramework;
using VBuild.BuildingSystem;

namespace VBuild.BuildingSystem
{
    public class Omnisettings
    {
        public static Dictionary<string, bool> modes = new Dictionary<string, bool>();

        public static Dictionary<string, int> build = new Dictionary<string, int>();
        public int TileRotation { get; set; }
        public int TileModel { get; set; }
        public int TileSnap { get; set; }
        public Stack<string> LastTilesPlaced { get; set; } = new Stack<string>();
        public int MapIcon { get; set; }
        public string OriginalBody { get; set; }

        // Constructor
        public Tools()
        {
            // Initialize default values for toggles
            SetMode("CanEditTiles", false);
            SetMode("BuildMode", false);
            SetMode("InspectToggle", false);
            SetMode("SnappingToggle", false);
            SetMode("ImmortalTiles", false);
            SetMode("MapIconToggle", false);
            SetMode("KillToggle", false);
            SetMode("CopyToggle", false);
            SetMode("ControlToggle", false);
            SetMode("DebuffToggle", false);
            SetMode("ConvertToggle", false);
            SetMode("BuffToggle", false);
            SetMode("EquipToggle", false);
            SetMode("LinkToggle", false);
        }
        

        // Methods for mode dictionary
        public void SetMode(string key, bool value)
        {
            if (modes.ContainsKey(key))
            {
                modes[key] = value;
            }
            else
            {
                modes.Add(key, value);
            }
        }

        public bool GetMode(string key)
        {
            if (modeToggles.TryGetValue(key, out bool value))
            {
                return value;
            }
            return false; // Default value if key doesn't exist
        }

        // Methods for building dictionary
        public void SetBuild(string key, int value)
        {
            if (build.ContainsKey(key))
            {
                build[key] = value;
            }
            else
            {
                build.Add(key, value);
            }
        }

        public int GetBuild(string key)
        {
            if (build.TryGetValue(key, out int value))
            {
                return value;
            }
            return 0; // Default value if key doesn't exist
        }

        // Methods for undo functionality
        public void AddEntity(string tileRef)
        {
            if (LastTilesPlaced.Count >= 10)
            {
                LastTilesPlaced.Pop(); // Ensure we only keep the last 10
            }
            LastTilesPlaced.Push(tileRef);
        }

        public string PopEntity()
        {
            return LastTilesPlaced.Count > 0 ? LastTilesPlaced.Pop() : null;
        }
    }
}