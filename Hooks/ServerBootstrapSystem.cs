using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Entities;
using VCreate.Systems;
using VCreate.Core;
using Unity.Transforms;
using VCreate.Core.Toolbox;
using System.Reflection.Metadata.Ecma335;
using Bloodstone.API;
using ProjectM.Tiles;
using Unity.Collections;
using VCreate.Core.Commands;
using static Il2CppSystem.Data.Common.ObjectStorage;
using static VCreate.Core.Commands.PetCommands;

namespace VCreate.Hooks
{
    [HarmonyPatch]
    public class ServerBootstrapPatches
    {
        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
        [HarmonyPrefix]
        private static unsafe void OnUserConnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            int userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[userIndex];
            Entity userEntity = serverClient.UserEntity;
            User user = __instance.EntityManager.GetComponentData<User>(userEntity);
            ulong steamId = user.PlatformId;

            if (!VCreate.Core.DataStructures.PlayerSettings.ContainsKey(steamId))
            {
                Omnitool newdata = new();
                VCreate.Core.DataStructures.PlayerSettings.Add(steamId, newdata);
                DataStructures.SavePlayerSettings();
            }
            if (!VCreate.Core.DataStructures.PlayerPetsMap.ContainsKey(steamId))
            {
                VCreate.Core.DataStructures.PlayerPetsMap.Add(steamId, []);
                DataStructures.SavePetExperience();
            }
            if (PetCommands.PlayerFamiliarStasisMap.TryGetValue(steamId, out var familiarStasis))
            {
                Entity pet = familiarStasis.FamiliarEntity;
                if (pet != Entity.Null && familiarStasis.IsInStasis)
                {
                    SystemPatchUtil.Enable(pet);
                    familiarStasis.IsInStasis = false;
                    pet = Entity.Null;
                    PetCommands.PlayerFamiliarStasisMap[steamId] = new FamiliarStasisState(pet, false);
                    Plugin.Log.LogInfo("Player familiar has been reactivated on reconnecting.");
                }

            }
            
        }
        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
        [HarmonyPrefix]
        private static unsafe void OnUserDisconnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            int userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[userIndex];
            Entity userEntity = serverClient.UserEntity;
            User user = __instance.EntityManager.GetComponentData<User>(userEntity);
            ulong steamId = user.PlatformId;
            if (DataStructures.PlayerPetsMap.TryGetValue(steamId, out var data))
            {
                var keys = data.Keys;
                foreach (var key in keys)
                {
                    if (data.TryGetValue(key, out var value))
                    {
                        if (value.Active)
                        {
                            // put in stasis
                            PetCommands.PlayerFamiliarStasisMap.TryGetValue(steamId, out var stasis);
                            Entity pet = PetCommands.FindPlayerFamiliar(user.LocalCharacter._Entity);
                            if (pet != Entity.Null)
                            {
                                SystemPatchUtil.Disable(pet);
                                PlayerFamiliarStasisMap[steamId] = new FamiliarStasisState(pet, true);
                                Plugin.Log.LogInfo("Player familiar has been put in stasis on disconnecting.");
                            }
                        }
                    }
                }
            }
        }

    }

}