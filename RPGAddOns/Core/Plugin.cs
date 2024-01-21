using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using RPGAddOns.Hooks;
using RPGAddOns.Core;
using RPGAddOns;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using VRising.GameData;
using UnityEngine.SceneManagement;
using static RPGAddOns.Core.ChatCommands;

namespace RPGAddOns
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        internal static Plugin Instance { get; private set; }
        public static ManualLogSource Logger;

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "RPGAddOns/player_data");
        public static readonly string PlayerPrestigeJson = Path.Combine(ConfigPath, "player_prestige.json");
        public static readonly string PlayerRanksJson = Path.Combine(ConfigPath, "player_ranks.json");
        public static readonly string PlayerDivinityJson = Path.Combine(ConfigPath, "player_divinity.json");

        // Configuration settings
        public static int ExtraHealth;

        public static int ExtraPhysicalPower;
        public static int ExtraSpellPower;
        public static int ExtraPhysicalResistance;
        public static int ExtraSpellResistance;
        public static int MaxPrestiges;
        public static int MaxRanks;
        public static int MaxAscensions;

        public static bool AscensionStats;
        public static bool ItemReward;
        public static int ItemPrefab;
        public static int ItemQuantity;
        public static bool BuffRewardsPrestige;
        public static bool BuffRewardsRankUp;
        public static string BuffPrefabsPrestige;
        public static string BuffPrefabsRankUp;

        public override void Load()
        {
            Instance = this;
            Logger = Log;
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            CommandRegistry.RegisterAll();

            InitConfig();
            ServerEvents.OnGameDataInitialized += GameDataOnInitialize;
            GameData.OnInitialize += GameDataOnInitialize;
            ChatCommands.LoadData();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void GameDataOnInitialize(World world)
        {
        }

        private void InitConfig()
        {
            // Initialize configuration settings
            AscensionStats = Config.Bind("Config", "AscensionStats", true, "Enables stat bonuses on ascension").Value;
            ExtraHealth = Config.Bind("Config", "ExtraHealth", 0, "Extra health on reset").Value;
            ExtraPhysicalPower = Config.Bind("Config", "ExtraPhysicalPower", 0, "Extra physical power awarded on reset").Value;
            ExtraSpellPower = Config.Bind("Config", "ExtraSpellPower", 0, "Extra spell power awarded on reset").Value;
            ExtraPhysicalResistance = Config.Bind("Config", "ExtraPhysicalResistance", 0, "Extra physical resistance awarded on reset").Value;
            ExtraSpellResistance = Config.Bind("Config", "ExtraSpellResistance", 0, "Extra spell resistance awarded on reset").Value;
            MaxPrestiges = Config.Bind("Config", "MaxPrestiges", -1, "Maximum number of times players can prestige their level. -1 is infinite").Value;
            MaxRanks = Config.Bind("Config", "MaxRanks", 9, "Maximum number of times players can rank up.").Value;
            ItemReward = Config.Bind("Config", "ItemRewards", true, "Gives specified item/quantity to players when prestiging if enabled.").Value;
            ItemPrefab = Config.Bind("Config", "ItemPrefab", -77477508, "Item prefab to give players when resetting. Onyx tears default").Value;
            ItemQuantity = Config.Bind("Config", "ItemQuantity", 1, "Item quantity to give players when resetting.").Value;
            BuffRewardsPrestige = Config.Bind("Config", "BuffRewardsReset", true, "Grants permanent buff to players when resetting if enabled.").Value;
            BuffRewardsRankUp = Config.Bind("Config", "BuffRewardsPrestige", true, "Grants permanent buff to players when prestiging if enabled.").Value;
            BuffPrefabsPrestige = Config.Bind("Config", "BuffPrefabsPrestige", "[1425734039,-91451769,1491093272,1163490655,-1572696947,-1559874083,-1124645803]", "Buff prefabs to give players when resetting. Granted in order, want # buffs == # levels [Buff1, Buff2, etc] to skip buff for a level set it to be 0").Value;
            BuffPrefabsRankUp = Config.Bind("Config", "BuffPrefabsRank", "[-1100642493,1637213963,476186894,1195333673,-2008977590,546136204,-2054010438,1883675026,-2064810948]", "Buff prefabs to give players when prestiging. Granted in order, want # buffs == # prestige [Buff1, Buff2, etc] if enabled to skip buff for a level set it to be 0").Value;

            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
        }

        public override bool Unload()
        {
            ChatCommands.SavePlayerPrestige();
            ChatCommands.SavePlayerRanks();
            Config.Clear();
            _harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
            // Implementation for when the game is fully initialized
        }
    }
}