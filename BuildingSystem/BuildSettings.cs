using ProjectM;
using System.Text.RegularExpressions;
using WorldBuild.Core;
using VampireCommandFramework;

namespace WorldBuild.BuildingSystem
{
    public class BuildSettings
    {
        public bool CanEditTiles { get; set; } // setting to allow moving or dismantling tiles outside of territories
        public bool BuildMode { get; set; }  // setting to spawn tiles using siege interact T02 on shift

        public bool DismantleMode { get; set; } // setting to dismantle tiles using siege interact T02 on shift
        public int TileRotation { get; set; } // controls orientation of tiles placed
        public int TileModel { get; set; } // controls model of tiles placed
        public string TileSet { get; set; } // tileset of tiles to select from
        public List<string> LastTilePlaced { get; set; } // string representation of entity identifier of last tile model placed for easy undoing
        public bool ImmortalTiles {  get; set; } // setting to make tiles indestructible

        public BuildSettings(bool canEditTiles, bool buildMode, int tileRotation, int tileModel, string tileSet, List<string> lastTilePlaced, bool immortalTiles)
        {
            CanEditTiles = canEditTiles;
            BuildMode = buildMode;
            TileRotation = tileRotation;
            TileModel = tileModel;
            TileSet = tileSet;
            LastTilePlaced = lastTilePlaced;
            ImmortalTiles = immortalTiles;
        }
    }
}