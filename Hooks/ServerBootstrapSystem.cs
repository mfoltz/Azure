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
        }
    }
}