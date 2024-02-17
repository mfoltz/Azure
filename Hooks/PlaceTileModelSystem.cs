using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VRising.GameData.Models;
using VRising.GameData;
using RPGAddOnsEx.Core;
using ProjectM.Network;

public static class PlaceTileModelSystem_Patch
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyCanDismantle))]
    public static class VerifyCanDismantle
    {
        public static void Destroying(PlaceTileModelSystem __instance)
        {
            Plugin.Logger.LogInfo("PlaceTileModelSystem Destroying called...");
            // if you're outside your castle and not an admin you ain't dismantling shit
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> entityArray = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in entityArray)
                {
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;

                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Intercepting attempted dismantle...");
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        string name = user.CharacterName.ToString();
                        UserModel userModel = GameData.Users.GetUserByCharacterName(name);
                        if (VRising.GameData.Methods.UserModelMethods.IsInCastle(userModel))
                        {
                            Plugin.Logger.LogInfo("Player is in their castle, dismantling allowed.");
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Player is not in their castle, checking if admin...");
                            if (!user.IsAdmin)
                            {
                                Plugin.Logger.LogInfo("Player is not an admin, dismantling not allowed.");
                                return;
                            }
                        }
                    }
                }
                entityArray.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }
        }
    }
}