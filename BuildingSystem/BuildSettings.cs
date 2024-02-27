using ProjectM;
using System.Text.RegularExpressions;
using WorldBuild.Core;
using VampireCommandFramework;

namespace WorldBuild.BuildingSystem
{
    public class BuildSettings
    {
        public bool CanEditTiles = false; // setting to allow moving or dismantling tiles outside of territories
        public bool BuildMode = false; // setting to spawn tiles using charm T02 on shift
        public int TileRotation = 0; // controls orientation of tiles placed
        public int TileModel = 0; // controls model of tiles placed
        public string TileSet = ""; // tileset of tiles to select from
        public string LastTilePlaced = ""; // string representation of entity identifier of last tile model placed for easy undoing

        public BuildSettings(bool canEditTiles, bool buildMode, int tileRotation, int tileModel, string tileSet, string lastTilePlaced)
        {
            CanEditTiles = canEditTiles;
            BuildMode = buildMode;
            TileRotation = tileRotation;
            TileModel = tileModel;
            TileSet = tileSet;
            LastTilePlaced = lastTilePlaced;
        }
    }
}