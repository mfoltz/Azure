using DismantleDenied.Core;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.TextCore;
using VRising.GameData;
using VRising.GameData.Models;

//WIP

namespace DismantleDenied.Hooks
{
    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.HandleDismantleTileModelEvents))]
    public static class PlaceTileModelSystem_Patch
    {
        private static HashSet<Entity> processedEntities = new HashSet<Entity>();

        public static bool Prefix(PlaceTileModelSystem __instance)
        {
            // Assume dismantling is disallowed by default.
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
                // On exception, we maintain the default stance of not allowing dismantling.
            }

            // Return the result of processing. If true, dismantling is allowed; if false, it's disallowed.
            return allowDismantling;
        }

        private static bool ProcessDismantlingEvents(EntityManager entityManager, NativeArray<Entity> dismantleArray)
        {
            foreach (Entity entity in dismantleArray)
            {
                if (processedEntities.Contains(entity))
                {
                    continue; // Skip already processed entities
                }

                processedEntities.Add(entity);

                // Only proceed if the entity has a FromCharacter component
                if (Utilities.HasComponent<FromCharacter>(entity))
                {
                    Plugin.Logger.LogInfo("Intercepting dismantle event...");
                    Entity userEntity = entityManager.GetComponentData<FromCharacter>(entity).User;
                    User user = entityManager.GetComponentData<User>(userEntity);
                    string name = user.CharacterName.ToString();
                    UserModel userModel = GameData.Users.GetUserByCharacterName(name);

                    // Check if the user is an admin or in their castle to allow dismantling
                    if (user.IsAdmin || VRising.GameData.Methods.UserModelMethods.IsInCastle(userModel))
                    {
                        Plugin.Logger.LogInfo("Dismantling allowed.");
                        return true; // Allow dismantling for admins or users in their castle
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Player is not in their castle and not an admin, dismantling not allowed.");
                        // If not an admin and not in their castle, do not allow dismantling.
                        // Since we want to disallow unless specifically allowed, we don't immediately return false here,
                        // as other entities in the array might meet the criteria.
                    }
                }
            }

            // Default to disallowing dismantling if no entities specifically allow it.
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