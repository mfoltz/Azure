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
            Entity playerEntity = user.LocalCharacter.GetEntityOnServer();
            ulong steamId = user.PlatformId;

            if (!VCreate.Core.DataStructures.PlayerSettings.ContainsKey(steamId))
            {
                Omnitool data = new();
                VCreate.Core.DataStructures.PlayerSettings.Add(steamId, data);
                DataStructures.Save();
            }
            SetFollowers(__instance, netConnectionId);
        }

        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
        [HarmonyPrefix]
        private static void OnUserDisconnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            int userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[userIndex];
            Entity userEntity = serverClient.UserEntity;
            User user = __instance.EntityManager.GetComponentData<User>(userEntity);
            Entity playerEntity = user.LocalCharacter.GetEntityOnServer();
            ulong steamId = user.PlatformId;
            // return player to original body on disconnect if able
            if (!DataStructures.PlayerSettings.TryGetValue(steamId, out Omnitool data)) return;
            if (data.OriginalBody != null)
            {
                ServerEvents.ReturnSoul(data);
            }
        }

        private static void SetFollowers(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            Plugin.Log.LogInfo("ServerBootstrapSystem Prefix called...");
            var em = __instance.EntityManager;
            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;
            var userData = __instance.EntityManager.GetComponentData<User>(userEntity);

            try
            {
                var buffer = userData.LocalCharacter._Entity.ReadBuffer<FollowerBuffer>();
                for (int i = 0; i < buffer.Length; i++)
                {
                    LocalToWorld localToWorld = userEntity.Read<LocalToWorld>();
                    var follower = buffer[i];
                    if (follower.Entity._Entity.Has<LastTranslation>())
                    {
                        follower.Entity._Entity.Write<LastTranslation>(new LastTranslation { Value = localToWorld.Position });
                    }
                    if (follower.Entity._Entity.Has<Translation>())
                    {
                        follower.Entity._Entity.Write<Translation>(new Translation { Value = localToWorld.Position });
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e.Message);
            }
        }
    }
}