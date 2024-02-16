using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Entities;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using TMPI.Core;

namespace TMPI.Hooks
{
    [HarmonyPatch]
    internal class VBloodConsumed
    {
        [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdate(VBloodSystem __instance)
        {
            if (!__instance.EventList.IsEmpty)
            {
                EntityManager entityManager = VWorld.Server.EntityManager;

                foreach (var _event in __instance.EventList)
                {
                    if (!entityManager.TryGetComponentData(_event.Target, out PlayerCharacter playerData)) continue;

                    Entity _vblood = __instance._PrefabCollectionSystem._PrefabGuidToEntityMap[_event.Source];
                    Entity user = playerData.UserEntity;

                    string vBloodName = __instance._PrefabCollectionSystem._PrefabDataLookup[_event.Source].AssetName.ToString();
                    string playerName = playerData.Name.ToString();

                    UserModel usermodel = GameData.Users.GetUserByCharacterName(playerName);
                    Entity characterEntity = usermodel.FromCharacter.Character;

                    try
                    {
                        if (vBloodName == "CHAR_ChurchOfLight_Paladin_VBlood")
                        {
                            PrefabGUID prefabGUID = new(2019195024);
                            if (InventoryUtilities.TryGetInventoryEntity(entityManager, characterEntity, out Entity inventoryEntity))
                            {
                                InventoryUtilitiesServer.TryRemoveItem(entityManager, inventoryEntity, prefabGUID, 1);
                            }
                            AddItemToInventory(prefabGUID, 1, usermodel);
                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogError($"Error: {e}");
                    }
                }
            }
        }

        public static void AddItemToInventory(PrefabGUID guid, int amount, UserModel user)
        {
            unsafe
            {
                user.TryGiveItem(guid, 1, out Entity itemEntity);
                return;
            }
        }
    }
}