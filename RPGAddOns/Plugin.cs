using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using System.Reflection;
using Unity.Entities;
using VampireCommandFramework;
using VRising.GameData;

namespace RPGAddOns
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {

        public static Harmony harmony;

        internal static Plugin Instance { get; private set; }

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "RPGAddOns");
        public static readonly string PlayerResetCountsBuffsJson = Path.Combine(ConfigPath, "player_resets.json");
        public static readonly string PlayerPrestigeJson = Path.Combine(ConfigPath, "player_prestige.json");

        

        public static ManualLogSource Logger;
        
        public static ConfigEntry<int> ExtraHealth;
        public static ConfigEntry<int> ExtraPhysicalPower;
        public static ConfigEntry<int> ExtraSpellPower;
        public static ConfigEntry<int> ExtraPhysicalResistance;
        public static ConfigEntry<int> ExtraSpellResistance;
        public static ConfigEntry<int> MaxResets;
        public static ConfigEntry<bool> ItemReward;
        public static ConfigEntry<int> ItemPrefab;
        public static ConfigEntry<int> ItemQuantity;
        public static ConfigEntry<bool> BuffRewardsReset;
        public static ConfigEntry<bool> BuffRewardsPrestige;
        public static ConfigEntry<string> BuffPrefabsReset;
        public static ConfigEntry<string> BuffPrefabsPrestige;

        public override void Load()
        {

            Instance = this;
            Logger = Log;
            CommandRegistry.RegisterAll();
            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            GameData.OnInitialize += GameDataOnInitialize;
            GameData.OnDestroy += GameDataOnDestroy;

            if (!VWorld.IsServer)
            {
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is only for server!");
                return;
            }

            InitConfig();

            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void GameDataOnDestroy()
        {

        }

        private void GameDataOnInitialize(World world)
        {
            Initialize();
            Commands.LoadData();
        }


        public void InitConfig()
        {

            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 0, "Set a waypoint limit per user.");

            //configuration options for BloodyPointTesting
            //ResetLevel options
            //Prestige options
            ExtraHealth = Config.Bind("Config", "ExtraHealth", 50, "Extra health on reset");
            ExtraPhysicalPower = Config.Bind("Config", "ExtraPhysicalPower", 5, "Extra physical power awarded on reset");
            ExtraSpellPower = Config.Bind("Config", "ExtraSpellPower", 5, "Extra spell power awarded on reset");
            ExtraPhysicalResistance = Config.Bind("Config", "ExtraPhysicalResistance", 0, "Extra physical resistance awarded on reset");
            ExtraSpellResistance = Config.Bind("Config", "ExtraSpellResistance", 0, "Extra spell resistance awarded on reset");
            MaxResets = Config.Bind("Config", "MaxResetCount", 5, "Maximum number of times players can reset their level.");
            ItemReward = Config.Bind("Config", "ItemRewards", false, "Gives specified item/quantity to players when resetting if enabled.");
            ItemPrefab = Config.Bind("Config", "ItemPrefab", -651878258, "Item prefab to give players when resetting. Onyx tears default");
            ItemQuantity = Config.Bind("Config", "ItemQuantity", 3, "Item quantity to give players when resetting.");
            BuffRewardsReset = Config.Bind("Config", "BuffRewardsReset", false, "Grants permanent buff to players when resetting if enabled.");
            BuffRewardsPrestige = Config.Bind("Config", "BuffRewardsPrestige", false, "Grants permanent buff to players when prestiging if enabled.");
            BuffPrefabsReset = Config.Bind("Config", "BuffPrefabsReset", "[]", "Buff prefabs to give players when resetting. Granted in order, want # buffs == # levels [Buff1, Buff2, etc] to skip buff for a level set it to be 'placeholder'");
            BuffPrefabsPrestige = Config.Bind("Config", "BuffPrefabsPrestige", "[]", "Buff prefabs to give players when prestiging. Granted in order, want # buffs == # prestige (5) if enabled to skip buff for a level set it to be 'placeholder'");

            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
        }

        public static void Initialize()
        {

        }

        public override bool Unload()
        {
            Commands.SavePlayerResets();
            Commands.SavePlayerPrestige();
            Config.Clear();
            harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {

        }












    }
}
