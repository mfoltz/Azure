using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VRising.GameData.Models;
using VRising.GameData;
using RPGAddOnsEx.Core;
using ProjectM.Network;
using ProjectM.Shared;

//WIP

/*
public static class PlaceTileModelSystem_Patch
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public class TileCheck
    {
        public static void Prefix(PlaceTileModelSystem __instance)
        {
            try
            {
                //Plugin.Logger.LogInfo("PlaceTileModelSystem Prefix called...");
                EntityManager entityManager = __instance.EntityManager;
                // if you're outside your castle and not an admin you ain't dismantling, moving or editing shit
                NativeArray<Entity> dismantleArray = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);

                foreach (Entity entity in dismantleArray)
                {
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    // no owner because freebuild, right, hmmm
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Intercepting tileDismantle event...");
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        string name = user.CharacterName.ToString();
                        UserModel userModel = GameData.Users.GetUserByCharacterName(name);
                        if (VRising.GameData.Methods.UserModelMethods.IsInCastle(userModel))
                        {
                            Plugin.Logger.LogInfo("Player is in their castle, dismantling tile allowed.");
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Player is not in their castle, checking if admin...");
                            if (!user.IsAdmin)
                            {
                                Plugin.Logger.LogInfo("Player is not an admin, dismantling tile not allowed.");
                                return;
                            }
                        }
                    }
                }
                dismantleArray.Dispose();
                NativeArray<Entity> moveArray = __instance._MoveTileQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in moveArray)
                {
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Intercepting tileMove event...");
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        string name = user.CharacterName.ToString();
                        UserModel userModel = GameData.Users.GetUserByCharacterName(name);
                        if (VRising.GameData.Methods.UserModelMethods.IsInCastle(userModel))
                        {
                            Plugin.Logger.LogInfo("Player is in their castle, moving tile allowed.");
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("Player is not in their castle, checking if admin...");
                            if (!user.IsAdmin)
                            {
                                Plugin.Logger.LogInfo("Player is not an admin, moving tile not allowed.");
                                return;
                            }
                        }
                    }
                }
                moveArray.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
                return;
            }
        }
    }
}
*/