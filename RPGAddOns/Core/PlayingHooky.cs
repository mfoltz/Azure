using HarmonyLib;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Network;
using Unity.Entities;
using UnityEngine;

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
                Plugin.Logger.LogInfo($"check1");

                var charHUDEntryCollection = gameBootstrap.CharacterHUDEntryCollection;
                //var instance = gameBootstrap.CharacterHUDEntryCollection;
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userData = entityManager.GetComponentData<User>(serverClient.UserEntity);
                Plugin.Logger.LogInfo($"{userData}");

                Plugin.Logger.LogInfo($"{userData.CharacterName} connected.");
                CharacterHUDEntryType characterHUDEntryType = CharacterHUDEntryType.Character;

                WeakAssetReference<UnityEngine.GameObject> weakAssetReference = charHUDEntryCollection;
                Plugin.Logger.LogInfo($"{weakAssetReference}");

                if (weakAssetReference.IsReferenceSet && !weakAssetReference.WasCollected)
                {
                    Plugin.Logger.LogInfo($"check4");

                    // asset ref still loaded so uh cool, proceed
                    AssetGuid assetGuid = weakAssetReference.GetAssetGuid();
                    Il2CppSystem.Object gameObject = assetGuid.BoxIl2CppObject();
                    Plugin.Logger.LogInfo($"{assetGuid}");

                    IntPtr intPtr = gameObject.Pointer;
                    CharacterHUDEntryCollection hudEntryCollection = new CharacterHUDEntryCollection(intPtr);
                    GameObject charHUDEntry = hudEntryCollection.GetCharacterHUD(characterHUDEntryType);
                    Plugin.Logger.LogInfo($"{charHUDEntry}");
                    if (charHUDEntry == null)
                    {
                        Plugin.Logger.LogInfo($"{charHUDEntry}");
                    }
                    else
                    {
                        CharacterHUDEntry charHUD = charHUDEntry.GetComponentInChildren<CharacterHUDEntry>();
                        if (charHUD != null)
                        {
                            charHUD.gameObject.SetActive(true);
                            Plugin.Logger.LogInfo($"{charHUD}");
                        }
                        else
                        {
                            Plugin.Logger.LogInfo($"check8");
                        }
                    }
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
}