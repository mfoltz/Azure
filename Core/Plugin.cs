using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using System.Reflection;
using System.Text.Json;
using Unity.Entities;
using VampireCommandFramework;
using VCreate.Core.Toolbox;
using VCreate.Hooks;
using VCreate.Systems;

namespace VCreate.Core
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    [BepInDependency("gg.deca.VampireCommandFramework")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;
        internal static Plugin Instance { get; private set; }

        private static ManualLogSource Logger;
        public static new ManualLogSource Log => Logger;

        public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
        public static readonly string PlayerSettingsJson = Path.Combine(Plugin.ConfigPath, "player_settings.json");
        public static readonly string PetDataJson = Path.Combine(Plugin.ConfigPath, "pet_data.json");

        public override void Load()
        {
            Instance = this;
            Logger = base.Log;

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
            VCreate.Core.DataStructures.Save();
            return true;
        }

        public void OnGameInitialized()
        {
            CastleTerritoryCache.Initialize();

            Plugin.Logger.LogInfo("TerritoryCache loaded");
        }
        
        public static void LoadData()
        {
            if (!File.Exists(Plugin.PlayerSettingsJson))
            {
                var stream = File.Create(Plugin.PlayerSettingsJson);
                stream.Dispose();
            }

            string json = File.ReadAllText(Plugin.PlayerSettingsJson);
            Plugin.Logger.LogWarning($"PlayerSettings found: {json}");
            try
            {
                var settings = JsonSerializer.Deserialize<Dictionary<ulong, Omnitool>>(json);
                VCreate.Core.DataStructures.PlayerSettings = settings ?? new Dictionary<ulong, Omnitool>();
                Plugin.Logger.LogWarning("PlayerSettings Populated");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo($"No data to deserialize yet: {ex}");
                VCreate.Core.DataStructures.PlayerSettings = new Dictionary<ulong, Omnitool>();
                Plugin.Logger.LogWarning("PlayerSettings Created");
            }
        }
    }
}