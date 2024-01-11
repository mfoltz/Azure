using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using ProjectM;
using RPGAddOns.Core;
using Stunlock.Network;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGAddOns.Rank
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public class OnUserConnectedManager
    {
        private static ScenePoolManager _scenePoolManager;
        private static CoroutineHelper _coroutineHelper;

        public static void InitializeWithScenePoolManagerAndCoroutineHelper(ScenePoolManager scenePoolManager, CoroutineHelper coroutineHelperComponent)
        {
            Plugin.Logger.LogInfo($"Initializing... ScenePoolManager ID: {scenePoolManager?.GetHashCode()}, CoroutineHelper ID: {coroutineHelperComponent?.GetInstanceID()}");

            _scenePoolManager = scenePoolManager;
            _coroutineHelper = coroutineHelperComponent;

            if (_scenePoolManager == null || _coroutineHelper == null)
            {
                Plugin.Logger.LogError("Initialization failed: ScenePoolManager or CoroutineHelper is null.");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            Plugin.Logger.LogInfo("Patching...");

            try
            {
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];

                Plugin.Logger.LogInfo($"{serverClient.PlatformId} connected.");

                BootstrapManager.Instance.Bootstraps(netConnectionId, __instance._GameBootstrap);

                if (_coroutineHelper == null)
                {
                    Plugin.Logger.LogError("_coroutineHelper is null in Postfix");
                    return;
                }

                // Delaying execution until the scene is loaded
                _coroutineHelper.StartCoroutine(_scenePoolManager.LoadPlayerSceneOnConnect(serverClient.NetConnectionId.ToString()));
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        public class CoroutineHelper : MonoBehaviour
        {
        }
    }

    public class BootstrapManager
    {
        public static BootstrapManager Instance { get; } = new BootstrapManager();
        private ConcurrentDictionary<NetConnectionId, GameBootstrap> playerGameBootstrapInstances = new ConcurrentDictionary<NetConnectionId, GameBootstrap>();

        public void Bootstraps(NetConnectionId connectionId, GameBootstrap gameBootstrap)
        {
            playerGameBootstrapInstances[connectionId] = gameBootstrap;
        }

        public GameBootstrap GetGameBootstrap(NetConnectionId connectionId)
        {
            if (playerGameBootstrapInstances.TryGetValue(connectionId, out GameBootstrap gameBootstrap))
            {
                return gameBootstrap;
            }
            return null;
        }
    }

    public class ScenePoolManager
    {
        private readonly MonoBehaviour coroutineContext;

        public event Action<Scene> OnSceneLoaded;

        public ScenePoolManager(MonoBehaviour coroutineContext)
        {
            this.coroutineContext = coroutineContext;
            scenePool = [];
        }

        private static Dictionary<string, Scene> scenePool = [];

        //for loading scene instances on demand for mem mangement whenever i get to that
        public void AddSceneToPool(string identifier, Scene scene)
        {
            // Add the loaded scene to the pool
            scenePool[identifier] = scene;
        }

        //so for later when I want the player scene instance again I need to make sure I can get it or else would need a new connection to make changes to the scene

        public void LoadSceneFromPool(string identifier)
        {
            //loading scene instance specific to each player from the pool if it exists and this method is called
            if (scenePool.ContainsKey(identifier))
            {
                coroutineContext.StartCoroutine(LoadSceneFromPoolCoroutine(identifier));
            }
            else
            {
                Plugin.Logger.LogInfo("Couldn't retrieve scene instance from pool, please reconnect to add a new instance to the pool.");
            }
        }

        public IEnumerator LoadPlayerSceneOnConnect(string identifier)
        {
            string sceneName = "UIEntryPoint";
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return new CustomWaitUntil(() => asyncLoad.isDone);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                var inventoryBackground = FindGameObjectInScene(loadedScene, "InventoryBackground");
                if (inventoryBackground != null)
                {
                    inventoryBackground.SetActive(true);
                }
                else
                {
                    Plugin.Logger.LogError("InventoryBackground not found in the loaded scene.");
                }
                scenePool[identifier] = loadedScene; // Add to pool for future reference
            }
            else
            {
                Plugin.Logger.LogError("Failed to load the scene or scene is not valid");
            }
        }

        private static GameObject FindGameObjectInScene(Scene scene, string gameObjectName)
        {
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                Transform foundTransform = rootGameObject.transform.Find(gameObjectName);
                if (foundTransform != null)
                    return foundTransform.gameObject;
            }
            return null;
        }

        //dont think I need to use this yet, only for loading scenes if already present in the pool but it's neat that it can run at the same time as new players joining
        public IEnumerator LoadSceneFromPoolCoroutine(string identifier)
        {
            string sceneName = "UIEntryPoint";
            Plugin.Logger.LogInfo($"Loading scene for {identifier}");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return asyncLoad;
            }
        }

        // Activate a scene from the pool when needed eventually
        public void ActivateSceneFromPool(string identifier)
        {
            if (scenePool.TryGetValue(identifier, out Scene scene))
            {
                SceneManager.SetActiveScene(scene);
            }
            else
            {
                Plugin.Logger.LogInfo("Couldn't retrieve scene instance from pool, please reconnect to add a new instance to the pool.");
            }
        }

        // Unload a scene to free up memory when player logs off or when need to for emory I guess
        public void UnloadSceneFromPool(string identifier)
        {
            if (scenePool.TryGetValue(identifier, out Scene scene))
            {
                SceneManager.UnloadSceneAsync(scene);
                scenePool.Remove(identifier);
            }
        }

        public class CustomWaitUntil : CustomYieldInstruction
        {
            private readonly Func<bool> _predicate;

            public override bool keepWaiting
            {
                get
                {
                    // Continue waiting while the predicate is true
                    return _predicate();
                }
            }

            public CustomWaitUntil(Func<bool> predicate)
            {
                if (predicate == null)
                    throw new ArgumentNullException(nameof(predicate));

                _predicate = predicate;
            }
        }
    }
}