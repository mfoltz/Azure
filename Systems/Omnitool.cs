namespace VCreate.Systems
{
    public class Omnitool
    {
        public static readonly Dictionary<string, bool> modes = [];

        public static readonly Dictionary<string, int> data = [];

        public static bool Permissions { get; set; }
        public static bool Emotes { get; set; }
        public static Stack<string> LastPlaced { get; set; } = new Stack<string>();
        public static string OriginalBody { get; set; }

        // Constructor
        public Omnitool()
        {
            // Initialize default values for settings
            SetMode("InspectToggle", false); // lists buffs, name and prefab of hovered unit. also outputs components to server log
            SetMode("SnappingToggle", false); // toggles snapping to grid for spawned structures
            SetMode("ImmortalToggle", false); // toggles immortality for spawned structures 
            SetMode("MapIconToggle", false); // toggles map icon for spawned structures
            SetMode("DestroyToggle", false); // toggles DestroyMode (destroy unit, won't work on vampires)
            SetMode("CopyToggle", false); // toggles CopyMode, spawns last unit inspected/set as charmed (need to add check for vampire horses as those will crash server)
            SetMode("ControlToggle", false); // toggles ControlMode (control unit, swap back with same emote)
            SetMode("DebuffToggle", false); // toggles DebuffMode (debuff unit, clear all buffs)
            SetMode("ConvertToggle", false); // toggles ConvertMode (convert unit, follows and fights)
            SetMode("BuffToggle", false); // toggles BuffMode (buff unit, uses last buff set)
            SetMode("LinkToggle", false); // toggles LinkMode (WIP, don't use... yet)
            SetMode("TileToggle", false); // toggles TileMode (spawn tile, uses last tile set)


            SetData("Rotation", 0); // rotation for spawned structures
            SetData("Unit", 0); // unit prefab for CopyMode
            SetData("Tile", 0); // tile prefab for TileMode
            SetData("GridSize", 0); // grid size for snapping spawned structures
            SetData("MapIcon", 0); // map icon prefab for spawned structures
            SetData("Buff", 0); // buff prefab for BuffMode
        }

        // Methods for mode dictionary
        public static void SetMode(string key, bool value)
        {
            modes[key] = value; // This automatically handles add or update
        }

        public static bool GetMode(string key)
        {
            if (modes.TryGetValue(key, out bool value))
            {
                return value;
            }
            return false; // Consider handling this case more explicitly
        }

        public static void SetData(string key, int value)
        {
            data[key] = value; // This automatically handles add or update
        }

        public static int GetData(string key, int defaultValue = 0)
        {
            if (data.TryGetValue(key, out int value))
            {
                return value;
            }
            return defaultValue; // Consider handling this case more explicitly
        }

        // Methods for undo functionality
        public static void AddEntity(string tileRef)
        {
            if (LastPlaced.Count >= 10)
            {
                LastPlaced.Pop(); // Ensure we only keep the last 10
            }
            LastPlaced.Push(tileRef);
        }

        public static string PopEntity()
        {
            return LastPlaced.Count > 0 ? LastPlaced.Pop() : null;
        }
    }
}