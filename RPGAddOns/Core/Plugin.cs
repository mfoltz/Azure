using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.UI;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using VRising.GameData;
using WillisCore.Patches;

namespace RPGAddOns.Core
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        public static Keybinding configKeybinding;

        internal static Plugin Instance { get; private set; }
        //private ModifierButtons modButtons = new ModifierButtons();

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "RPGAddOns/player_data");
        public static readonly string PlayerPrestigeJson = Path.Combine(ConfigPath, "player_prestige.json");

        public static readonly string PlayerRanksJson = Path.Combine(ConfigPath, "player_ranks.json");
        public static readonly string PlayerDivinityJson = Path.Combine(ConfigPath, "player_divinity.json");
        public static ManualLogSource Logger;

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

        //public static Keybinding DivineAngelKeybinding;
        public static Keybinding configSpellKeybinding;

        //public static Keybinding ChaosQuakeKeybinding;

        public override void Load()
        {
            Instance = this;
            Logger = Log;
            CommandRegistry.RegisterAll();

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            if (!VWorld.IsServer)
            {
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is only for server!");
                return;
            }

            InitConfig();

            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            ServerEvents.OnGameDataInitialized += GameDataOnInitialize;
            GameData.OnInitialize += GameDataOnInitialize;
            //GameData.OnDestroy += GameDataOnDestroy;
            Commands.LoadData();
        }

        private void GameDataOnDestroy()
        {
        }

        private void GameDataOnInitialize(World world)
        {
            //try creating public class for BuffSpawnerSystem here, instantiating then calling it elsewhere?
        }

        public void InitConfig()
        {
            //configuration options for BloodyPointTesting
            //ResetLevel options
            //Prestige options
            //need more config options
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
            /*
            DivineAngelKeybinding = KeybindManager.Register(new KeybindingDescription()
            {
                Id = "RPGAddOns.divineangel",
                Category = "configKeybinding",
                Name = "Divine Angel Cast",
                DefaultKeybinding = KeyCode.G // Choose an appropriate default key
            });

            ChaosQuakeKeybinding = KeybindManager.Register(new KeybindingDescription()
            {
                Id = "RPGAddOns.chaosquake",
                Category = "configKeybinding",
                Name = "Chaos Quake Cast",
                DefaultKeybinding = KeyCode.G // Choose an appropriate default key
            });

            configSpellKeybinding = KeybindManager.Register(new()
            {
                Id = "blue.rpg.cast",
                Category = "rpg",
                Name = "cast",
                DefaultKeybinding = KeyCode.LeftShift,
            });
            */
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
        }

        public static void Initialize()
        {
        }

        public override bool Unload()
        {
            Commands.SavePlayerPrestige();
            Commands.SavePlayerRanks();
            KeybindManager.Unregister(configKeybinding);

            Config.Clear();
            _harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
        }
    }
}