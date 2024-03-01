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
using VBuild;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
using VBuild.BuildingSystem;
using VBuild.Data;

namespace VBuild.Core
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        internal static Plugin Instance { get; private set; }
        public static ManualLogSource Logger;

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "WorldBuild");
        public static readonly string BuildSettingsJson = Path.Combine(Plugin.ConfigPath, "player_build_settings.json");

        public override void Load()
        {
            Instance = this;
            Logger = Log;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            CommandRegistry.RegisterAll();
            InitConfig();
            ServerEvents.OnGameDataInitialized += GameDataOnInitialize;
            LoadData();
            Plugin.Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void GameDataOnInitialize(World world)
        {
        }

        private void InitConfig()
        {
            // Initialize configuration settings

            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
        }

        public override bool Unload()
        {
            Config.Clear();
            _harmony.UnpatchSelf();
            Databases.SaveBuildSettings();
            return true;
        }

        public void OnGameInitialized()
        {
            CastleTerritoryCache.Initialize();
            Plugin.Logger.LogInfo("TerritoryCache loaded");
        }

        public static void LoadData()
        {
            if (!File.Exists(Plugin.BuildSettingsJson))
            {
                var stream = File.Create(Plugin.BuildSettingsJson);
                stream.Dispose();
            }

            string json = File.ReadAllText(Plugin.BuildSettingsJson);
            Plugin.Logger.LogWarning($"BuildSettings found: {json}");
            try
            {
                Databases.playerBuildSettings = JsonSerializer.Deserialize<Dictionary<ulong, BuildSettings>>(json);
                Plugin.Logger.LogWarning("BuildSettings Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error deserializing data: {ex}");
                Databases.playerBuildSettings = new Dictionary<ulong, BuildSettings>();
                Plugin.Logger.LogWarning("BuildSettings Created");
            }
        }
    }
}