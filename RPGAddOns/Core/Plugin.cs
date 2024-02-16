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

namespace TMPI.Core
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        internal static Plugin Instance { get; private set; }
        public static ManualLogSource Logger;

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "TMPI");
        public static bool shardDrop;
        public static bool setBonus;

        public override void Load()
        {
            Instance = this;
            Logger = Log;
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            TMPI.Core.ServerEvents.OnGameDataInitialized += GameDataOnInitialize;
            GameData.OnInitialize += GameDataOnInitialize;
            InitConfig();
        }

        private void GameDataOnInitialize(World world)
        {
            TMPI.Augments.ArmorModifierSystem.ModifyArmorPrefabEquipmentSet();
        }

        private void InitConfig()
        {
            // Initialize configuration settings
            shardDrop = Config.Bind("Config", "ShardDrop", false, "Enables shard drop from Solarus.").Value;
            setBonus = Config.Bind("Config", "SetBonus", false, "Enables Bloodmoon set bonus for Death gear.").Value;
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
        }

        public override bool Unload()
        {
            Config.Clear();
            _harmony.UnpatchSelf();
            return true;
        }

        public void OnGameInitialized()
        {
        }
    }
}