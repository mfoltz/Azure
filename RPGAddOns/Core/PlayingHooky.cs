using Bloodstone.API;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Newtonsoft.Json;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Network;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using Mono;

namespace RPGAddOns.Core
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public class OnUserConnectedPatch
    {
        [HarmonyPostfix]
        public static unsafe void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            Plugin.Logger.LogInfo("Patching...");

            try
            {
                var entityManager = __instance.EntityManager;
                var gameBootstrap = __instance._GameBootstrap;
                var networkManager = __instance._NetServer;
                //var clientCreate = __instance._GameBootstrap.ClientCreateData;
                //var sceneUI = __instance._GameBootstrap.UIAssetsSubScene;
                //var sceneHUD = __instance._GameBootstrap.HUDSubScene;

                var helper = new Helpers();
                // can I get the component from the player client and force it to active or something?
                var charHUDEntryCollection = gameBootstrap.CharacterHUDEntryCollection;
                //var instance = gameBootstrap.CharacterHUDEntryCollection;
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userData = serverClient.UserEntity;
                Plugin.Logger.LogInfo($"{userData}");

                Plugin.Logger.LogInfo($"{serverClient.UserEntity.ToString} connected.");
                CharacterHUDEntryType characterHUDEntryType = CharacterHUDEntryType.Character;
                WeakAssetReference<UnityEngine.GameObject> weakAssetReference = charHUDEntryCollection;
                Plugin.Logger.LogInfo($"{weakAssetReference}");
                if (weakAssetReference.IsReferenceSet && !weakAssetReference.WasCollected)
                {
                    // asset ref still loaded so uh cool, proceed

                    //AssetGuid assetGuid = weakAssetReference.GetAssetGuid();//IL2CPP.il2cpp_runtime_invoke
                    AssetGuid assetGuid = AssetGuid.FromString(weakAssetReference.AssetGuid);
                    Il2CppSystem.Object gameObject = assetGuid.BoxIl2CppObject();
                    Plugin.Logger.LogInfo($"{assetGuid}");
                    //Il2CppSystem.Type targetType = Il2CppSystem.Type.GetType("CharacterHUDEntry");//IL2CPP.il2cpp_runtime_invoke
                    IntPtr intPtr = gameObject.Pointer;
                    CharacterHUDEntryCollection hudEntryCollection = new CharacterHUDEntryCollection(intPtr);
                    //GameObject charHUDEntry = hudEntryCollection.GetCharacterHUD(characterHUDEntryType);//IL2CPP.il2cpp_runtime_invoke
                    //Plugin.Logger.LogInfo($"{charHUDEntry}");

                    /*
                    if (weakAssetReference.IsReferenceSet && !weakAssetReference.WasCollected)
                    {
                        // asset ref still loaded so uh cool, proceed

                        //AssetGuid assetGuid = weakAssetReference.GetAssetGuid();//IL2CPP.il2cpp_runtime_invoke
                        AssetGuid assetGuid = AssetGuid.FromString(weakAssetReference.AssetGuid);
                        Il2CppSystem.Object gameObject = assetGuid.BoxIl2CppObject();
                        Plugin.Logger.LogInfo($"{assetGuid}");
                        //Il2CppSystem.Type targetType = Il2CppSystem.Type.GetType("CharacterHUDEntry");//IL2CPP.il2cpp_runtime_invoke
                        IntPtr intPtr = gameObject.Pointer;
                        CharacterHUDEntryCollection hudEntryCollection = new CharacterHUDEntryCollection(intPtr);
                        GameObject charHUDEntry = hudEntryCollection.GetCharacterHUD(characterHUDEntryType);//IL2CPP.il2cpp_runtime_invoke
                        Plugin.Logger.LogInfo($"{charHUDEntry}");
                        if (charHUDEntry.WasCollected)
                        {
                            Plugin.Logger.LogInfo($"-garbage truck noises-");
                            return;
                        }
                        else
                        {
                            Il2CppSystem.Type targetType = helper.SystemTypeGet(typeof(CharacterHUDEntry));
                            Plugin.Logger.LogInfo($"{targetType}");
                            // spot to add to
                            GameObject HUDCanvas = (GameObject)GameObject.FindWithTag("HUDCanvas");
                            Plugin.Logger.LogInfo($"{HUDCanvas.name}");
                            Plugin.Logger.LogInfo($"{HUDCanvas.Pointer}");
                            Plugin.Logger.LogInfo($"{HUDCanvas.m_CachedPtr}");
                            Plugin.Logger.LogInfo($"{HUDCanvas.activeSelf}");
                            Plugin.Logger.LogInfo($"{HUDCanvas.activeInHierarchy}");
                            Plugin.Logger.LogInfo($"{HUDCanvas.scene}");
                            Plugin.Logger.LogInfo($"{HUDCanvas.tag}");

                            Plugin.Logger.LogInfo($"soooooon");
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogInfo($"Asset ref not set");
                    }*/
                }
                else
                {
                    Plugin.Logger.LogInfo($"Asset ref not set");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }
    }

    public class Helpers
    {
        public Il2CppSystem.Type SystemTypeGet(Type type)
        {
            return Il2CppSystem.Type.GetType(type.AssemblyQualifiedName);
        }

        public ComponentType ComponentTypeGet(string component)
        {
            return ComponentType.ReadOnly(Il2CppSystem.Type.GetType(component));
        }

        public static class AotWorkaroundUtil
        {
            // alternative for Entitymanager.HasComponent
            public static bool HasComponent<T>(Entity entity) where T : struct
            {
                return VWorld.Server.EntityManager.HasComponent(entity, ComponentType<T>());
            }

            // more convenient than Entitymanager.AddComponent
            public static bool AddComponent<T>(Entity entity) where T : struct
            {
                return VWorld.Server.EntityManager.AddComponent(entity, ComponentType<T>());
            }

            // alternative for Entitymanager.AddComponentData
            public static void AddComponentData<T>(Entity entity, T componentData) where T : struct
            {
                AddComponent<T>(entity);
                SetComponentData(entity, componentData);
            }

            // alternative for Entitymanager.RemoveComponent
            public static bool RemoveComponent<T>(Entity entity) where T : struct
            {
                return VWorld.Server.EntityManager.RemoveComponent(entity, ComponentType<T>());
            }

            // alternative for EntityMManager.GetComponentData
            public static unsafe T GetComponentData<T>(Entity entity) where T : struct
            {
                void* rawPointer = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, ComponentTypeIndex<T>());
                return Marshal.PtrToStructure<T>(new System.IntPtr(rawPointer));
            }

            // alternative for EntityManager.SetComponentData
            public static unsafe void SetComponentData<T>(Entity entity, T componentData) where T : struct
            {
                var size = Marshal.SizeOf(componentData);
                //byte[] byteArray = new byte[size];
                var byteArray = StructureToByteArray(componentData);
                fixed (byte* data = byteArray)
                {
                    //UnsafeUtility.CopyStructureToPtr(ref componentData, data);
                    VWorld.Server.EntityManager.SetComponentDataRaw(entity, ComponentTypeIndex<T>(), data, size);
                }
            }

            private static ComponentType ComponentType<T>()
            {
                return new ComponentType(Il2CppType.Of<T>());
            }

            private static int ComponentTypeIndex<T>()
            {
                return ComponentType<T>().TypeIndex;
            }

            private static byte[] StructureToByteArray<T>(T structure) where T : struct
            {
                int size = Marshal.SizeOf(structure);
                byte[] byteArray = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(structure, ptr, true);
                    Marshal.Copy(ptr, byteArray, 0, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
                return byteArray;
            }
        }
    }

    public class UIManager
    {
        private Dictionary<NetConnectionId, GameBootstrap> playerGameBootstrapInstances = new Dictionary<NetConnectionId, GameBootstrap>();

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

        public class ServerScript
        {
        }
    }
}