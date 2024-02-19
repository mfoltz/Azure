using HarmonyLib;
using UnityEngine;
using Stunlock.Network;
using ProjectM;
using Unity.Entities;
using Bloodstone.API;
using RPGAddOnsEx;
using ProjectM.UI;
using Unity.Scenes;
using System.Reflection;
using VRising.GameData.Models.Internals;
using System.Xml.Linq;
using RPGAddOnsEx.Core;
using Extensions = System.Xml.Linq.Extensions;
using ProjectM.Network;
using VRising.GameData.Models;
using VRising.GameData;

// WIP
/*
namespace ServerSideBtw
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public class OnUserConnectedManager
    {
        private static Dictionary<ulong, PlayerHUDInfo> playerHUDs = new Dictionary<ulong, PlayerHUDInfo>();

        [HarmonyPostfix]
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            try
            {
                ServerBootstrapSystem serverBootstrapSystem = __instance;
                Plugin.Logger.LogInfo("OnUserConnected Postfix called...");

                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];

                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                Plugin.Logger.LogInfo("Server client found...");
                var serverWorld = serverBootstrapSystem.World;
                var userEntity = serverClient.UserEntity;
                var steamID = serverClient.PlatformId;
                Plugin.Logger.LogInfo($"User entity retrieved: {userEntity}");
                Plugin.Logger.LogInfo("SetupHUDManager called...");
                SetupHUDManager(userEntity, steamID, serverBootstrapSystem);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in OnUserConnected_Patch: {ex}");
            }
        }

        private static void SetupHUDManager(Entity player, ulong steamID, ServerBootstrapSystem serverBootstrapSystem)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            if (player == Entity.Null)
            {
                Plugin.Logger.LogError("Player entity is null.");
                return;
            }

            string playerName = entityManager.GetComponentData<User>(player).CharacterName.ToString();
            UserModel userModel = GameData.Users.GetUserByCharacterName(playerName);
            BaseEntityModel baseEntityModel = userModel.Internals;
            //var baseEntityModel = entityManager.GetComponentData<BaseEntityModel>(player);
            CustomManagedDataModel customModel = new CustomManagedDataModel(serverBootstrapSystem.World, baseEntityModel);
            // Access the GameDataSystem to retrieve the ManagedDataRegistry
            ManagedDataRegistry managedDataRegistry = serverBootstrapSystem.World.GetExistingSystem<GameDataSystem>().ManagedDataRegistry;

            // Retrieve the ManagedCharacterHUD using the GetManagedComponentDataInternal extension method
            ManagedCharacterHUD managedCharacterHUD = customModel.GetManagedCharacterHUD(serverBootstrapSystem.World, baseEntityModel);

            if (managedCharacterHUD == null)
            {
                Plugin.Logger.LogError("ManagedCharacterHUD is null.");
                return;
            }

            // Assuming ManagedCharacterHUD contains the HUD Canvas or related data
            // Process the retrieved data to setup HUD Manager
            // This part of the code needs to be adapted based on the actual structure of ManagedCharacterHUD

            // Example of setting up the HUD Manager
            GameObject hudManagerObject = new GameObject("HUDManager");
            HUDManager hudManager = hudManagerObject.AddComponent<HUDManager>();
            if (hudManager == null)
            {
                Plugin.Logger.LogError("Failed to add HUDManager to GameObject.");
                return;
            }
            GameObject playerCanvas = FindHUDCanvas(player, steamID, serverBootstrapSystem);
            // Set the player reference and HUD Canvas in HUDManager
            hudManager.SetPlayerReference(player, steamID, playerCanvas); // assuming managedCharacterHUD can be directly used as HUD canvas

            Plugin.Logger.LogInfo("HUD Manager setup complete.");
        }

        // Custom class mimicking BaseManagedDataModel
        public class CustomManagedDataModel
        {
            private readonly World _world;
            private readonly BaseEntityModel _entityModel;

            public CustomManagedDataModel(World world, BaseEntityModel entityModel)
            {
                _world = world;
                _entityModel = entityModel;
            }

            public ManagedCharacterHUD GetManagedCharacterHUD(World world, BaseEntityModel entityModel)
            {
                // Access the GameDataSystem to retrieve the ManagedDataRegistry
                ManagedDataRegistry managedDataRegistry = world.GetExistingSystem<GameDataSystem>().ManagedDataRegistry;

                // Use reflection to call the internal method GetManagedComponentDataInternal
                MethodInfo getManagedComponentDataInternal = typeof(Extensions)
                    .GetMethod("GetManagedComponentDataInternal", BindingFlags.Static | BindingFlags.NonPublic);

                ManagedCharacterHUD managedCharacterHUD = null;

                if (getManagedComponentDataInternal != null)
                {
                    managedCharacterHUD = getManagedComponentDataInternal
                        .MakeGenericMethod(typeof(ManagedCharacterHUD))
                        .Invoke(null, new object[] { world, entityModel }) as ManagedCharacterHUD;
                }

                if (managedCharacterHUD == null)
                {
                    Plugin.Logger.LogError("ManagedCharacterHUD is null.");
                }

                return managedCharacterHUD;
            }
        }

        private static GameObject FindHUDCanvas(Entity playerEntity, ulong steamID, ServerBootstrapSystem serverBootstrapsystem)
        {
            Plugin.Logger.LogInfo("Finding HUD Canvas for player: " + steamID);

            // Using reflection to access the static field "PrefabName" from the type ManagedCharacterHUD
            var type = typeof(ProjectM.ManagedCharacterHUD);
            FieldInfo prefabNameField = type.GetField("PrefabName", BindingFlags.NonPublic | BindingFlags.Static);

            if (prefabNameField == null)
            {
                Plugin.Logger.LogError("PrefabName field not found in ManagedCharacterHUD");
                return null;
            }

            string prefabName = (string)prefabNameField.GetValue(null);
            if (string.IsNullOrEmpty(prefabName))
            {
                Plugin.Logger.LogError("PrefabName is null or empty");
                return null;
            }

            // Now use the prefab name to load the prefab
            GameObject hudCanvasPrefab = Resources.Load<GameObject>(prefabName);
            if (hudCanvasPrefab == null)
            {
                Plugin.Logger.LogError("HUD Canvas prefab not found: " + prefabName);
                return null;
            }

            Plugin.Logger.LogInfo("HUD Canvas prefab found: " + prefabName);
            return hudCanvasPrefab;
        }

        public class PlayerHUDInfo
        {
            public GameObject HUDCanvas { get; set; }
            public Entity PlayerEntity { get; set; }

            public PlayerHUDInfo(GameObject hudCanvas, Entity playerEntity)
            {
                HUDCanvas = hudCanvas;
                PlayerEntity = playerEntity;
            }
        }

        public static GameObject InstantiateAndModifyHUD(GameObject original, ulong steamID)
        {
            GameObject clone = GameObject.Instantiate(original);
            clone.name = "HUDCanvas(Clone) - " + steamID.ToString(); // Changed the name to be more descriptive
            // Modify clone as needed ...
            // Assume we have a method to activate and modify specific elements, which should be static if not part of the instance
            HUDManager.ActivateAndModifySpecificElements(clone); // If this method needs to be non-static, then adjust accordingly
            return clone;
        }

        public class HUDManager : MonoBehaviour
        {
            private Entity playerReference;
            private ulong steamID;
            private GameObject originalHUDCanvas;
            // This needs to be set externally

            private void Awake()
            {
                // Initialization code here

                Plugin.Logger.LogInfo("HUDManager has awoken...");
            }

            public void Initialize(Entity player, ulong id, GameObject hudCanvas)
            {
                playerReference = player;
                steamID = id;
                originalHUDCanvas = hudCanvas;

                // Further initialization if necessary...
            }

            public void SetPlayerReference(Entity player, ulong id, GameObject hudCanvas)
            {
                playerReference = player;
                steamID = id;
                originalHUDCanvas = hudCanvas;

                SetupHUD();
            }

            private void SetupHUD()
            {
                // Instantiate and modify HUD logic here...
                GameObject clonedHUDCanvas = InstantiateAndModifyHUD(originalHUDCanvas, steamID);
                ReparentModifiedElements(clonedHUDCanvas, originalHUDCanvas);
                Destroy(clonedHUDCanvas); // Destroy is valid in MonoBehaviour
            }

            public static GameObject CloneAndSetupAbilityBarEntry(GameObject abilitiesLayout)
            {
                GameObject spellsLayout = abilitiesLayout.transform.GetChild(0).gameObject;
                if (spellsLayout != null)
                {
                    // Clone the spells layout
                    GameObject clonedAbilityBarEntry = Instantiate(spellsLayout);

                    // Deactivate all children of the cloned entry
                    DeactivateAllChildren(clonedAbilityBarEntry);

                    // Set the parent of the cloned entry to the Abilities GameObject
                    clonedAbilityBarEntry.transform.SetParent(abilitiesLayout.transform, false);

                    // Return the cloned entry for further use
                    Plugin.Logger.LogInfo("Cloned Ability Bar Entry setup complete.");
                    return clonedAbilityBarEntry;
                }
                else
                {
                    Plugin.Logger.LogError("AbilityBarEntry not found in the abilities layout");
                    return null;
                }
            }

            private void ReparentModifiedElements(GameObject source, GameObject destination)
            {
                while (source.transform.childCount > 0)
                {
                    Transform child = source.transform.GetChild(0);
                    child.SetParent(destination.transform, false);
                }
                Plugin.Logger.LogInfo("Elements reparented successfully.");
            }

            private static void DeactivateAllChildren(GameObject obj)
            {
                foreach (Transform child in obj.transform)
                    child.gameObject.SetActive(false);
            }

            public static void ActivateAndModifySpecificElements(GameObject hudClone)
            {
                // Activation and modification logic for specific HUD elements
                // Including inventory background, ability bar, and experience bar
                // want to start with BottomBarCanvas
                // activate, go down level, activate, repeat
                hudClone.SetActive(true);
                GameObject bottomBarCanvas = hudClone.transform.GetChild(0).gameObject;
                bottomBarCanvas.SetActive(true);
                GameObject bottomBar = bottomBarCanvas.transform.GetChild(0).gameObject;
                bottomBar.SetActive(true);
                // this should get the last object in the tree which is what we are after, also get abilitydummy while we're here
                GameObject backgroundParent = bottomBar.transform.GetChild(2).gameObject;
                backgroundParent.SetActive(true);
                GameObject backgroundChild = backgroundParent.transform.GetChild(0).gameObject;
                backgroundChild.SetActive(true);
                GameObject inventoryBackground = bottomBar.transform.GetChild(bottomBar.transform.childCount - 1).gameObject;
                inventoryBackground.SetActive(true);
                GameObject abilityBar = bottomBar.transform.GetChild(1).gameObject;
                abilityBar.SetActive(true);
                GameObject abilitiesLayout = abilityBar.transform.GetChild(0).gameObject;
                GameObject abilityBarEntry = CloneAndSetupAbilityBarEntry(abilitiesLayout);
                if (abilityBarEntry != null)
                {
                    abilityBarEntry.SetActive(true);
                }
                else
                {
                    Plugin.Logger.LogError("AbilityBarEntry not found in the abilities layout");
                }
            }

            // Additional methods for processing and modifying HUD components...
        }
    }
}
*/