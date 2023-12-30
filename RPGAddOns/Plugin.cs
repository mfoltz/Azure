using BepInEx;
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
        private Harmony _harmony;

        internal static Plugin Instance { get; private set; }

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "RPGAddOns");
        public static readonly string PlayerResetCountsBuffsJson = Path.Combine(ConfigPath, "player_resets.json");
        public static readonly string PlayerPrestigeJson = Path.Combine(ConfigPath, "player_prestige.json");

        public static ManualLogSource Logger;

        public static int ExtraHealth;
        public static int ExtraPhysicalPower;
        public static int ExtraSpellPower;
        public static int ExtraPhysicalResistance;
        public static int ExtraSpellResistance;
        public static int MaxResets;
        public static bool ItemReward;
        public static int ItemPrefab;
        public static int ItemQuantity;
        public static bool BuffRewardsReset;
        public static bool BuffRewardsPrestige;
        public static string BuffPrefabsReset;
        public static string BuffPrefabsPrestige;

        public override void Load()
        {
            Instance = this;
            Logger = Log;
            CommandRegistry.RegisterAll();
            //_harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
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
            //configuration options for BloodyPointTesting
            //ResetLevel options
            //Prestige options
            ExtraHealth = Config.Bind("Config", "ExtraHealth", 50, "Extra health on reset").Value;
            ExtraPhysicalPower = Config.Bind("Config", "ExtraPhysicalPower", 5, "Extra physical power awarded on reset").Value;
            ExtraSpellPower = Config.Bind("Config", "ExtraSpellPower", 5, "Extra spell power awarded on reset").Value;
            ExtraPhysicalResistance = Config.Bind("Config", "ExtraPhysicalResistance", 0, "Extra physical resistance awarded on reset").Value;
            ExtraSpellResistance = Config.Bind("Config", "ExtraSpellResistance", 0, "Extra spell resistance awarded on reset").Value;
            MaxResets = Config.Bind("Config", "MaxResetCount", 5, "Maximum number of times players can reset their level.").Value;
            ItemReward = Config.Bind("Config", "ItemRewards", false, "Gives specified item/quantity to players when resetting if enabled.").Value;
            ItemPrefab = Config.Bind("Config", "ItemPrefab", -651878258, "Item prefab to give players when resetting. Onyx tears default").Value;
            ItemQuantity = Config.Bind("Config", "ItemQuantity", 3, "Item quantity to give players when resetting.").Value;
            BuffRewardsReset = Config.Bind("Config", "BuffRewardsReset", false, "Grants permanent buff to players when resetting if enabled.").Value;
            BuffRewardsPrestige = Config.Bind("Config", "BuffRewardsPrestige", false, "Grants permanent buff to players when prestiging if enabled.").Value;
            BuffPrefabsReset = Config.Bind("Config", "BuffPrefabsReset", "[]", "Buff prefabs to give players when resetting. Granted in order, want # buffs == # levels [Buff1, Buff2, etc] to skip buff for a level set it to be 'placeholder'").Value;
            BuffPrefabsPrestige = Config.Bind("Config", "BuffPrefabsPrestige", "[]", "Buff prefabs to give players when prestiging. Granted in order, want # buffs == # prestige (5) if enabled to skip buff for a level set it to be 'placeholder'").Value;

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
            _harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
        }
    }
}