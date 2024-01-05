using AdminCommands;
using Bloodstone.API;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using VRising.GameData;
using VRising.GameData.Models;
using Exception = System.Exception;
using Input = BepInEx.Unity.IL2CPP.UnityEngine.Input;
using KeyCode = BepInEx.Unity.IL2CPP.UnityEngine.KeyCode;

namespace RPGAddOns.Core
{
    [HarmonyPatch]
    public static class GameplayInputSystem_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameplayInputSystem), nameof(GameplayInputSystem.HandleInput))]
        public static void HandleInput(GameplayInputSystem __instance)
        {
            Plugin.Logger.LogInfo($"Input detected"); // Log details about each event

            ModifierKeyHandler.HandleInput(__instance);
        }
    }

    public static class ModifierKeyHandler
    {
        public static void HandleInput(GameplayInputSystem __instance)
        {
            Plugin.Logger.LogInfo($"Handling input"); // Log details about each event
            
            if (Input.GetKeyInt(KeyCode.LeftShift))
            {
                Plugin.Logger.LogInfo($"Shift key pressed"); // Log details about each event
                HandleShiftAction(__instance);
                Plugin.Logger.LogInfo($"Shift key action complete"); // Log details about each event
            }
            else
            {
                return;
            }
        }

        private static void HandleShiftAction(GameplayInputSystem __instance)
        {
            Plugin.Logger.LogInfo($"Retrieving entity from input");
            // want this to be something players can choose a skill for
            // hmmm how do I have this work for all players? make data file with ability prefab to pass to this method
            // the instance should be the one from the player pushing the key? I think?
            DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            EntityQuery query = __instance._SingletonEntityQuery_LocalCharacter_29;
            NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
            try
            {
                Plugin.Logger.LogInfo($"Number of entities: {entities.Length}"); // Log details about each event

                Plugin.Logger.LogInfo($"Attempting to cast ability"); // Log details about each event
                // Iterate through the entities of which there should only be 1
                foreach (var entity in entities)
                {
                    //need to get User entity and Character entity from here
                    if (!VWorld.Server.EntityManager.TryGetComponentData(entity, out PlayerCharacter playerData))
                    {
                        string playerName = playerData.Name.ToString();
                        Entity user = playerData.UserEntity;
                        UserModel usermodel = GameData.Users.GetUserByCharacterName(playerName);
                        Entity character = usermodel.FromCharacter.Character;
                        CastAbilityServerDebugEvent serverDebugEvent = new CastAbilityServerDebugEvent();
                        FromCharacter fromCharacter = new FromCharacter();
                        fromCharacter.Character = character;
                        fromCharacter.User = user;
                        serverDebugEvent.AbilityGroup = AdminCommands.Data.Prefabs.AB_ChurchOfLight_Paladin_AngelicAscent_AbilityGroup;
                        serverDebugEvent.AimPosition = new Nullable_Unboxed<float3>(user.Read<EntityInput>().AimPosition);
                        serverDebugEvent.Who = character.Read<NetworkId>();
                        debugEventsSystem.CastAbilityServerDebugEvent(user.Read<User>().Index, ref serverDebugEvent, ref fromCharacter);
                    }
                    else
                    {
                        Plugin.Logger.LogInfo($"Unable to get player data"); // Log details about each event
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
            finally
            {
                // Ensure the array is disposed even if an exception occurs
                entities.Dispose();
            }
        }
    }
}