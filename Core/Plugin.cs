using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using VRising.GameData;
using UnityEngine.SceneManagement;
using System.Text.Json;

namespace RPGAddOnsEx.Core
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
        public static readonly string PlayerPrestigeJson = Path.Combine(Plugin.ConfigPath, "player_prestige.json");
        public static readonly string PlayerRanksJson = Path.Combine(Plugin.ConfigPath, "player_ranks.json");
        public static readonly string PlayerDivinityJson = Path.Combine(Plugin.ConfigPath, "player_divinity.json");

        public static int ExtraHealth;
        public static int ExtraPhysicalPower;
        public static int ExtraSpellPower;
        public static int ExtraPhysicalResistance;
        public static int ExtraSpellResistance;

        public static int MaxPrestiges;
        public static int MaxRanks;

        public static bool PlayerAscension;
        public static bool PlayerPrestige;
        public static bool PlayerRankUp;

        public static bool ItemReward;
        public static int ItemPrefab;
        public static int ItemQuantity;
        public static bool ItemMultiplier;

        public static bool BuffRewardsPrestige;
        public static bool BuffRewardsRankUp;
        public static string BuffPrefabsPrestige;
        public static string BuffPrefabsRankUp;

        public static bool modifyDeathSetBonus;
        public static bool modifyDeathSetStats;

        public static int deathSetBonus;
        public static string extraStatType;
        public static int extraStatValue;

        public static bool shardDrop;

        public static int rankCommandsCooldown;
        public static bool rankPointsModifier; //true for multiply points gained and false for divide points gained
        public static int rankPointsFactor; // int to divide or muliply by

        public override void Load()
        {
            Instance = this;
            Logger = Log;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            CommandRegistry.RegisterAll();
            InitConfig();
            RPGAddOnsEx.Core.ServerEvents.OnGameDataInitialized += GameDataOnInitialize;
            GameData.OnInitialize += GameDataOnInitialize;

            ChatCommands.LoadData();
            Plugin.Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void GameDataOnInitialize(World world)
        {
            RPGAddOnsEx.Augments.ArmorModifierSystem.ModifyArmorPrefabEquipmentSet();
        }

        private void InitConfig()
        {
            // Initialize configuration settings
            /*
            ExtraHealth = Config.Bind("Config", "ExtraHealth", 0, "Extra health on reset").Value;
            ExtraPhysicalPower = Config.Bind("Config", "ExtraPhysicalPower", 0, "Extra physical power awarded on reset").Value;
            ExtraSpellPower = Config.Bind("Config", "ExtraSpellPower", 0, "Extra spell power awarded on reset").Value;
            ExtraPhysicalResistance = Config.Bind("Config", "ExtraPhysicalResistance", 0, "Extra physical resistance awarded on reset").Value;
            ExtraSpellResistance = Config.Bind("Config", "ExtraSpellResistance", 0, "Extra spell resistance awarded on reset").Value;
            */

            MaxPrestiges = Config.Bind("Config", "MaxPrestiges", -1, "Maximum number of times players can prestige their level. -1 is infinite").Value;
            MaxRanks = Config.Bind("Config", "MaxRanks", 5, "Maximum number of times players can rank up.").Value;

            ItemReward = Config.Bind("Config", "ItemRewards", true, "Gives specified item/quantity to players when prestiging if enabled.").Value;
            ItemPrefab = Config.Bind("Config", "ItemPrefab", -77477508, "Item prefab to give players when resetting. Demon fragments default").Value;
            ItemQuantity = Config.Bind("Config", "ItemQuantity", 1, "Item quantity to give players when resetting.").Value;
            ItemMultiplier = Config.Bind("Config", "ItemMultiplier", true, "Multiplies the item quantity by the player's prestige if enabled.").Value;

            BuffRewardsPrestige = Config.Bind("Config", "BuffRewardsReset", true, "Grants permanent buff to players when prestiging if enabled.").Value;
            BuffRewardsRankUp = Config.Bind("Config", "BuffRewardsPrestige", true, "Grants permanent buff to players when ranking up if enabled.").Value;
            BuffPrefabsPrestige = Config.Bind("Config", "BuffPrefabsPrestige", "[-1124645803,1163490655,1520432556,-1559874083,1425734039]", "Buff prefabs to give players when prestiging.").Value;
            BuffPrefabsRankUp = Config.Bind("Config", "BuffPrefabsRank", "[476186894,-1703886455,-238197495,1068709119,-1161197991]", "Buff prefabs to give players when ranking up.").Value;

            modifyDeathSetStats = Config.Bind("Config", "ModifyDeathSetStats", true, "Modify the stats of the death set").Value;
            modifyDeathSetBonus = Config.Bind("Config", "ModifyDeathSetBonus", true, "Modify the set bonus of the death set").Value;
            deathSetBonus = Config.Bind("Config", "DeathSetBonus", 35317589, "Set bonus to apply to the death set, bloodmoon by default if enabled").Value;
            extraStatType = Config.Bind("Config", "ExtraStatType", "SpellPower", "Stat type to add to the death set").Value;
            extraStatValue = Config.Bind("Config", "ExtraStatValue", 5, "Stat value to add to the death set").Value;

            PlayerAscension = Config.Bind("Config", "PlayerAscension", false, "Enable player ascension").Value;
            PlayerPrestige = Config.Bind("Config", "PlayerPrestige", true, "Enable player prestige").Value;
            PlayerRankUp = Config.Bind("Config", "PlayerRankUp", true, "Enable player rank up").Value;

            rankCommandsCooldown = Config.Bind("Config", "RankCommandsCooldown", 0, "Cooldown for rank commands in minutes").Value;

            shardDrop = Config.Bind("Config", "ShardDrop", true, "Enable shard drop from Solarus").Value;

            rankPointsModifier = Config.Bind("Config", "RankPointsModifier", true, "True for multiply, false for divide").Value;
            rankPointsFactor = Config.Bind("Config", "RankPointsFactor", 1, "Factor to multiply or divide rank points by").Value;

            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
        }

        public override bool Unload()
        {
            ChatCommands.SavePlayerRanks();
            ChatCommands.SavePlayerPrestige();
            //ChatCommands.SavePlayerDivinity();
            Config.Clear();
            _harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
        }
    }
}