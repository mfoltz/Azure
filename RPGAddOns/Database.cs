using BloodyPoints.Utilities;
using ProjectM;
using System.Collections.Generic;
using System.Text.Json;

namespace BloodyPoints.DB
{
    public class Database
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

        public static List<WaypointData> globalWaypoint { get; set; }
        public static List<WaypointData> waypoints { get; set; }
        public static Dictionary<ulong, int> waypoints_owned { get; set; }



        //BloodyPointsTesting additions

        public static Dictionary<string, ResetData> playerResetCountsBuffs = [];
        public static Dictionary<string, PrestigeData> playerPrestige = [];


        public static class Buff
        {
            public static PrefabGUID InCombat = new PrefabGUID(581443919);
            public static PrefabGUID InCombat_PvP = new PrefabGUID(697095869);
        }
    }
}
