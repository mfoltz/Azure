using HarmonyLib;
using UnityEngine;
using Stunlock.Network;
using TMPro;
using ProjectM;
using Unity.Entities;
using Bloodstone.API;
using System.Reflection;
using RPGAddOns;
using Il2CppInterop.Runtime;

namespace ServerSideBtw
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public class OnUserConnectedManager
    {
        private static readonly ConstructorInfo serverClientConstructor;

        static OnUserConnectedManager()
        {
            Type serverClientType = typeof(ServerBootstrapSystem.ServerClient);
            serverClientConstructor = serverClientType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );
        }

        [HarmonyPostfix]
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            ServerBootstrapSystem serverBootstrapSystem = __instance;
            GameBootstrap gameBootstrap = serverBootstrapSystem.GameBootstrap;
            // WeakAssetReference<GameObject> weakAssetReference = gameBootstrap.UserPrefab;

            if (serverClientConstructor != null)
            {
                object serverClientObj = serverClientConstructor.Invoke(null);
                if (serverClientObj is ServerBootstrapSystem.ServerClient serverClient)
                {
                    // Assuming that the UserEntity is already set correctly in the serverClient instance
                    Entity playerEntity = serverClient.UserEntity;

                    if (entityManager.Exists(playerEntity))
                    {
                        SetupHUDManager(playerEntity);
                    }
                }
                else
                {
                    Plugin.Logger.LogInfo("Could not cast to ServerBootstrapSystem.ServerClient");
                }
            }
            else
            {
                Plugin.Logger.LogInfo("ServerClient constructor is null");
            }

            // Logic for setting up the CustomHUDManager with the player's reference
            // Assuming you have a method to get the player Entity
        }

        private static void SetupHUDManager(Entity player)
        {
            GameObject hudManagerObject = new GameObject("HUDManager");
            HUDManager hudManager = hudManagerObject.AddComponent<HUDManager>();
            hudManager.SetPlayerReference(player);
            // Implement additional logic as needed for other HUD elements
            // ...
        }

        public class HUDManager : MonoBehaviour
        {
            private Entity playerReference;
            public GameObject canvasReference;

            public void SetPlayerReference(Entity player)
            {
                playerReference = player;
                SetupHUD();
            }

            private void SetupHUD()
            {
                // Instantiate and modify player HUD
                GameObject playerHUDClone = InstantiateAndModifyHUD(canvasReference, "PlayerHUDClone");

                // Instantiate and modify boss HUD

                // Additional setup for player-specific HUD elements
                // ...
            }

            private GameObject InstantiateAndModifyHUD(GameObject original, string cloneName)
            {
                GameObject clone = GameObject.Instantiate(original);
                clone.name = cloneName;

                DeactivateAllChildren(clone);
                ActivateAndModifySpecificElements(clone);

                return clone;
            }

            private void DeactivateAllChildren(GameObject obj)
            {
                foreach (Transform child in obj.transform)
                {
                    child.gameObject.SetActive(false);
                    DeactivateAllChildren(child.gameObject);
                }
            }

            private void ActivateAndModifySpecificElements(GameObject hudClone)
            {
                // Activation and modification logic for specific HUD elements
                // Including inventory background, ability bar, and experience bar
                // ...
            }

            private void ModifyExperienceBar(GameObject hudClone)
            {
                // Logic for modifying the experience bar, as previously discussed
                // ...
            }

            // Additional methods for processing and modifying HUD components
            // ...
        }

        public sealed class ServerClient : Il2CppSystem.ValueType
        {
            // Reflection to access the private static field
            private static IntPtr GetNativeFieldInfoPtr(string fieldName)
            {
                FieldInfo fieldInfo = typeof(ServerClient).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
                if (fieldInfo != null)
                {
                    return (IntPtr)fieldInfo.GetValue(null);
                }
                else
                {
                    throw new InvalidOperationException($"Field '{fieldName}' not found in ServerClient.");
                }
            }

            public unsafe Entity UserEntity
            {
                get
                {
                    IntPtr offsetPtr = GetNativeFieldInfoPtr("NativeFieldInfoPtr_UserEntity");
                    nint num = (nint)IL2CPP.Il2CppObjectBaseToPtrNotNull(this) + (int)IL2CPP.il2cpp_field_get_offset(offsetPtr);
                    return *(Entity*)num;
                }
                set
                {
                    IntPtr offsetPtr = GetNativeFieldInfoPtr("NativeFieldInfoPtr_UserEntity");
                    *(Entity*)((nint)IL2CPP.Il2CppObjectBaseToPtrNotNull(this) + (int)IL2CPP.il2cpp_field_get_offset(offsetPtr)) = value;
                }
            }

            public ServerClient Copy()
            {
                ServerClient copiedClient = new ServerClient();
                copiedClient.UserEntity = this.UserEntity;
                return copiedClient;
            }
        }
    }
}