using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using UnityEngine.SceneManagement;
using System.Text.Json;
using VRising.GameData;

namespace DismantleDenier.Core
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        internal static Plugin Instance { get; private set; }
        public static ManualLogSource Logger;

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "DismantleDenier");
        public static bool buildingPlacementRestrictions;

        public override void Load()
        {
            Instance = this;
            Logger = Log;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            CommandRegistry.RegisterAll();
            InitConfig();
            DismantleDenier.Core.ServerEvents.OnGameDataInitialized += GameDataOnInitialize;
            GameData.OnInitialize += GameDataOnInitialize;
            Plugin.Logger.LogInfo("Plugin DismantleDenier is loaded!");
        }

        private void GameDataOnInitialize(World world)
        {
        }

        private void InitConfig()
        {
            // Initialize configuration settings

            buildingPlacementRestrictions = Config.Bind("Config", "buildingPlacementRestrictions", false, "Enable or disable placement restrictions. I suspect this might have something to do with the overgrowth.").Value;
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