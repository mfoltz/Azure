using HarmonyLib;
using ProjectM;
using ProjectM.Hybrid;
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
using VPlus.Augments;
using Bloodstone.API;
using Unity.Scenes;
using VivoxUnity;
using UnityEngine.SceneManagement;
using UnityEngine;
using VBuild.Core.Toolbox;
using static ProjectM.CustomWorldSpawning;
using ProjectM.UI;
using VRising.GameData.Utils;
using Il2CppSystem.ComponentModel;
using Il2CppSystem;
using WeakReference = Il2CppSystem.WeakReference;
using IntPtr = Il2CppSystem.IntPtr;

namespace VPlus.Hooks
{



    [HarmonyPatch]
    public class ServerBootstrapPatches
    {
        private static readonly string redV = VPlus.Core.Toolbox.FontColors.Red("V");
        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
        [HarmonyPrefix]

        private unsafe static void OnUserConnectedPrefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
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
            if (!VPlus.Data.Databases.playerRanks.ContainsKey(steamId) && VPlus.Core.Plugin.PlayerRankUp)
            {
                RankData rankData = new RankData(0, 0, [], 0, [0, 0], "default", false);
                VPlus.Data.Databases.playerRanks.Add(steamId, rankData);
                ChatCommands.SavePlayerRanks();
            }
            if (!VPlus.Data.Databases.playerPrestige.ContainsKey(steamId) && VPlus.Core.Plugin.PlayerPrestige)
            {
                PrestigeData prestigeData = new PrestigeData(0, 0);
                VPlus.Data.Databases.playerPrestige.Add(steamId, prestigeData);
                ChatCommands.SavePlayerPrestige();
            }
            if (VPlus.Core.Plugin.PlayerAscension)
            {
                if (VPlus.Data.Databases.playerDivinity.ContainsKey(steamId))
                {
                    DivineData currentPlayerDivineData = VPlus.Data.Databases.playerDivinity[steamId];
                    currentPlayerDivineData.OnUserConnected();
                    ChatCommands.SavePlayerDivinity();
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, $"Welcome back! Your {redV}Tokens have been updated, don't forget to redeem them: {VPlus.Core.Toolbox.FontColors.Yellow(currentPlayerDivineData.VTokens.ToString())}");
                }
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
                DivineData divineData = VPlus.Data.Databases.playerDivinity[steamId];

                DivineData currentPlayerDivineData = VPlus.Data.Databases.playerDivinity[steamId];
                currentPlayerDivineData.OnUserDisconnected(user, divineData); // Calculate points and update times
                ChatCommands.SavePlayerDivinity();

            }

        }
    }
}
    

