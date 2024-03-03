using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGMods;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using V.Augments;
using VBuild.Data;
using VPlus.Augments.Rank;
using VPlus.Core.Commands;
using VPlusV.Augments;

namespace VPlus.Hooks
{
    [HarmonyPatch]
    public class ServerBootstrapPatches
    {
        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
        [HarmonyPrefix]
    
        private static void OnUserConnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            int userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[userIndex];
            Entity userEntity = serverClient.UserEntity;
            User user = __instance.EntityManager.GetComponentData<User>(userEntity);
            Entity playerEntity = user.LocalCharacter.GetEntityOnServer();
            ulong steamId = user.PlatformId;

            if (!VPlus.Data.Databases.playerDivinity.ContainsKey(steamId) && VPlus.Core.Plugin.PlayerAscension)
            {
                DivineData divineData = new DivineData(0, 0);
                VPlus.Data.Databases.playerDivinity.Add(steamId, divineData);
                ChatCommands.SavePlayerDivinity();
                // start tracking VPoints, every hour get a crystal unless inventory is full then you get VPoints you can redeem for crystals at the same ratio

            }
            if (!VPlus.Data.Databases.playerRanks.ContainsKey(steamId)  && VPlus.Core.Plugin.PlayerRankUp)
            {
                RankData rankData = new RankData(0, 0, [], 0, [],"default",false);
                VPlus.Data.Databases.playerRanks.Add(steamId, rankData);
                ChatCommands.SavePlayerRanks();
            }
            if (!VPlus.Data.Databases.playerPrestige.ContainsKey(steamId) && VPlus.Core.Plugin.PlayerPrestige)
            {
                PrestigeData prestigeData = new PrestigeData(0, 0);
                VPlus.Data.Databases.playerPrestige.Add(steamId, prestigeData);
                ChatCommands.SavePlayerPrestige();
            }
            if (VPlus.Data.Databases.playerDivinity.ContainsKey(steamId) && VPlus.Core.Plugin.PlayerAscension)
            {
                DivineData currentPlayerDivineData = VPlus.Data.Databases.playerDivinity[steamId];
                currentPlayerDivineData.OnUserConnected(); // Mark the connection time
                ChatCommands.SavePlayerDivinity();
            }

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

            if (VPlus.Data.Databases.playerDivinity.ContainsKey(steamId))
            {
                DivineData currentPlayerDivineData = VPlus.Data.Databases.playerDivinity[steamId];
                currentPlayerDivineData.OnUserDisconnected(); // Calculate points and update times
                ChatCommands.SavePlayerDivinity();
            }

        }
    }

    
    
   
}
