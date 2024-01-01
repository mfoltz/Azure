using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using Bloodstone.Hooks;
using HarmonyLib;
using LibCpp2IL.BinaryStructures;
using ProjectM.Network;
using RPGAddOns.Patch;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using static RootMotion.FinalIK.InteractionObject;
using static RPGAddOns.CastCommands;

namespace RPGAddOns
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        public static Keybinding configKeybinding;
        internal static Plugin Instance { get; private set; }

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "RPGAddOns");
        public static readonly string PlayerRanksJson = Path.Combine(ConfigPath, "player_ranks.json");
        public static readonly string PlayerPrestigesJson = Path.Combine(ConfigPath, "player_prestiges.json");

        public static ManualLogSource Logger;

        public static int ExtraHealth;
        public static int ExtraPhysicalPower;
        public static int ExtraSpellPower;
        public static int ExtraPhysicalResistance;
        public static int ExtraSpellResistance;
        public static int MaxPrestiges;
        public static int MaxRanks;

        public static bool ItemReward;
        public static int ItemPrefab;
        public static int ItemQuantity;
        public static bool BuffRewardsPrestige;
        public static bool BuffRewardsRankUp;
        public static string BuffPrefabsPrestige;
        public static string BuffPrefabsRankUp;

        public static Keybinding DivineAngelKeybinding;
        public static Keybinding ChaosQuakeKeybinding;

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
            MaxPrestiges = Config.Bind("Config", "MaxPrestiges", 5, "Maximum number of times players can prestige their level.").Value;

            MaxRanks = Config.Bind("Config", "MaxRanks", 5, "Maximum number of times players can rank up.").Value;
            ItemReward = Config.Bind("Config", "ItemRewards", false, "Gives specified item/quantity to players when resetting if enabled.").Value;
            ItemPrefab = Config.Bind("Config", "ItemPrefab", -651878258, "Item prefab to give players when resetting. Onyx tears default").Value;
            ItemQuantity = Config.Bind("Config", "ItemQuantity", 3, "Item quantity to give players when resetting.").Value;
            BuffRewardsPrestige = Config.Bind("Config", "BuffRewardsReset", false, "Grants permanent buff to players when resetting if enabled.").Value;
            BuffRewardsRankUp = Config.Bind("Config", "BuffRewardsPrestige", false, "Grants permanent buff to players when prestiging if enabled.").Value;
            BuffPrefabsPrestige = Config.Bind("Config", "BuffPrefabsReset", "[]", "Buff prefabs to give players when resetting. Granted in order, want # buffs == # levels [Buff1, Buff2, etc] to skip buff for a level set it to be 'placeholder'").Value;
            BuffPrefabsRankUp = Config.Bind("Config", "BuffPrefabsPrestige", "[]", "Buff prefabs to give players when prestiging. Granted in order, want # buffs == # prestige (5) if enabled to skip buff for a level set it to be 'placeholder'").Value;

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
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
        }

        public static void Initialize()
        {
        }

        public override bool Unload()
        {
            Commands.SavePlayerPrestiges();
            Commands.SavePlayerRanks();
            KeybindManager.Unregister(Plugin.configKeybinding);

            Config.Clear();
            _harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
        }
    }
}