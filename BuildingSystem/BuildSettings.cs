using ProjectM;
using System.Text.RegularExpressions;
using VBuild.Core;
using VampireCommandFramework;
using VBuild.BuildingSystem;

namespace VBuild.BuildingSystem
{
    public class BuildSettings
    {
        public bool CanEditTiles { get; set; } // setting to allow moving or dismantling tiles outside of territories
        public bool BuildMode { get; set; }  // setting to spawn tiles using charm T02 on Q
        public bool InspectToggle { get; set; } // setting to disable prefab spawning in favor of inspecting entity at mouse hover
        public int TileRotation { get; set; } // controls orientation of tiles placed
        public int TileModel { get; set; } // controls model of tiles placed
        public bool SnappingToggle { get; set; } // setting to allow snapping of tiles to grid
        public int MapIcon { get; set; } // controls model of map icons placed
        public string TileSet { get; set; } // tileset of tiles to select from
        public Stack<string> LastTilesPlaced { get; set; } = new Stack<string>(); // string representation of entity identifier of last tile models placed for easy undoing
        public bool ImmortalTiles { get; set; } // setting to make tiles indestructible
        public bool MapIconToggle { get; set; } // setting to allow map icons to be placed on tiles
        public BuildSettings(bool canEditTiles, bool buildMode,bool inspectToggle, int tileRotation, int tileModel, bool snappingToggle ,int mapIcon, string tileSet, Stack<string> lastTilesPlaced, bool immortalTiles, bool mapIconToggle)
        {
            CanEditTiles = canEditTiles;
            BuildMode = buildMode;
            InspectToggle = inspectToggle;
            TileRotation = tileRotation;
            TileModel = tileModel;
            SnappingToggle = snappingToggle;
            MapIcon = mapIcon;
            TileSet = tileSet;
            LastTilesPlaced = lastTilesPlaced;
            ImmortalTiles = immortalTiles;
            MapIconToggle = mapIconToggle;
        }

        
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