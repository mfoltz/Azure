using AdminCommands;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGAddOnsEx.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.TextCore;
using VRising.GameData;
using VRising.GameData.Models;
using Plugin = RPGAddOnsEx.Core.Plugin;

//WIP

namespace DismantleDenier.Hooks
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.HandleDismantleTileModelEvents))]
    public static class PlaceTileModelSystem_Patch
    {
        private static HashSet<Entity> processedEntities = new HashSet<Entity>();

        public static bool Prefix(PlaceTileModelSystem __instance)
        {
            bool allowDismantling = false;

            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> dismantleArray = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);

                // Process dismantling events
                allowDismantling = ProcessDismantlingEvents(entityManager, dismantleArray);

                dismantleArray.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }

            return allowDismantling;
        }

        private static bool ProcessDismantlingEvents(EntityManager entityManager, NativeArray<Entity> dismantleArray)
        {
            foreach (Entity entity in dismantleArray)
            {
                if (processedEntities.Contains(entity))
                {
                    continue; // Skip to the next iteration if entity has already been processed
                }
                processedEntities.Add(entity);
                if (!Utilities.HasComponent<FromCharacter>(entity))
                {
                    continue;
                }
                else
                {
                    Plugin.Logger.LogInfo("Intercepting dismantle event...");
                    Entity userEntity = entityManager.GetComponentData<FromCharacter>(entity).User;
                    User user = entityManager.GetComponentData<User>(userEntity);
                    string name = user.CharacterName.ToString();
                    UserModel userModel = GameData.Users.GetUserByCharacterName(name);
                    if (!VRising.GameData.Methods.UserModelMethods.IsInCastle(userModel) && !user.IsAdmin)
                    {
                        Plugin.Logger.LogInfo("Player is not in their castle and not an admin, dismantling not allowed.");
                        return false; // Dismantling not allowed
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Dismantling allowed.");
                        return true; // Dismantling allowed
                    }
                }
            }
            // Disallow dismantling by default?
            return false;
        }
    }
}

/*
                NativeArray<Entity> moveArray = __instance._MoveTileQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in moveArray)
                {
                    entity.LogComponentTypes();
                    if (!Utilities.HasComponent<FromCharacter>(entity))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Intercepting tileMove event...");
                        Entity userEntity = entityManager.GetComponentData<FromCharacter>(entity).User;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        string name = user.CharacterName.ToString();b
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
                            else
                            {
                                Plugin.Logger.LogInfo("Player is an admin, moving tile allowed.");
                            }
                        }
                    }
                }
                moveArray.Dispose();
                NativeArray<Entity> editArray = __instance._StartEditQuery.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in editArray)
                {
                    entity.LogComponentTypes();
                    if (!Utilities.HasComponent<FromCharacter>(entity))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Intercepting tileDismantle event...");
                        Entity userEntity = entityManager.GetComponentData<FromCharacter>(entity).User;
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
public static void Postfix(PlaceTileModelSystem __instance)
        {
            NativeArray<Entity> dismantleArray = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (Entity entity in dismantleArray)
                {
                    entity.LogComponentTypes();
                    if (!Utilities.HasComponent<FromCharacter>(entity))
                    {
                        continue;
                    }
                    if (shouldAllowDismantle)
                    {
                        Plugin.Logger.LogInfo("Dismantling allowed.");
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Dismantling not allowed.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }
        }
                */