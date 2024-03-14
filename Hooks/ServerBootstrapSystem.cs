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
            Entity playerEntity = user.LocalCharacter.GetEntityOnServer();
            ulong steamId = user.PlatformId;

            if (!VCreate.Core.DataStructures.PlayerSettings.TryGetValue(steamId, out Omnitool data))
            {
                Omnitool newdata = new();
                VCreate.Core.DataStructures.PlayerSettings.Add(steamId, data);
                DataStructures.Save();
            }
            //SetFollowers(__instance, netConnectionId);
            // will need to update entity reference here to return to body
            
            if (data.OriginalBody != null)
            {
                var bodyQuery = VWorld.Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new[]
                {
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<PrefabGUID>(),
                ComponentType.ReadOnly<VampireTag>()
            },
                    //None = new[] { ComponentType.ReadOnly<Dead>(), ComponentType.ReadOnly<DestroyTag>() }
                });
                NativeArray<Entity> bodies = bodyQuery.ToEntityArray(Allocator.Temp);
                foreach (var body in bodies)
                {
                    // looking for original character entity
                    if (Utilities.HasComponent<PlayerCharacter>(body))
                    {
                        if (!body.Read<PlayerCharacter>().Name.Equals(user.CharacterName)) continue;
                        else
                        {
                            data.OriginalBody = body.Index + ", " + body.Version;
                            DataStructures.Save();
                            ServerEvents.ReturnSoul(data, userEntity); break;
                        }
                    }
                }
                bodies.Dispose();

            }
            
        }

        // This created an infinite loop since the server wasn't expecting a debug event from a disconnected user -_-
        /*
        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.))]
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
        

        //This might not be needed but leaving here for now, doesn't get called currently
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
                if (!userData.LocalCharacter._Entity.Has<FollowerBuffer>()) return;
                var buffer = userData.LocalCharacter._Entity.ReadBuffer<FollowerBuffer>();
                for (int i = 0; i < buffer.Length; i++)
                {
                    //
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e.Message);
            }
        }
        */
    }
}