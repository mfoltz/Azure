using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGAddOnsEx.Core;
using Unity.Collections;
using Unity.Entities;
using VRising.GameData;
using VRising.GameData.Models;

//WIP

namespace RPGAddOnsEx.Hooks_WIP
{
    [HarmonyPatch(typeof(HandleDismantleEventSystem), nameof(HandleDismantleEventSystem.OnUpdate))]
    public class HandleDismantleEventSystem_Patch
    {
        public static void Prefix(HandleDismantleEventSystem __instance)
        {
            try
            {
                Plugin.Logger.LogInfo("HandleDismantleEventSystem Prefix called...");
                EntityManager entityManager = __instance.EntityManager;
                // if you're outside your castle and not an admin you ain't dismantling, moving or editing shit
                NativeArray<Entity> dismantleArray = __instance._QueryDismantleEvent.ToEntityArray(Allocator.Temp);

                foreach (Entity entity in dismantleArray)
                {
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;

                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        // can't really imagine a scenario where something gets dismantled without an owner entity but just to be safe
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Intercepting dismantle event...");
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
                dismantleArray.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    public static class PlaceTileModelSystem_Patch
    {
        public static void Prefix(PlaceTileModelSystem __instance)
        {
            //Plugin.Logger.LogInfo("PlaceTileModelSystem Prefix called...");
            EntityManager entityManager = __instance.EntityManager;
            // if you're outside your castle and not an admin you ain't dismantling, moving or editing shit
            NativeArray<Entity> editArray = __instance._StartEditQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in editArray)
            {
                Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
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
            editArray.Dispose();
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
    }
}