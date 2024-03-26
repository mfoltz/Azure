using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Entities;
using VPlus.Augments.Rank;
using VPlus.Core.Commands;
using VPlus.Augments;
using Bloodstone.API;
using VPlus.Core.Toolbox;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using VCreate.Core.Toolbox;
using VRising.GameData.Utils;
using VPlus.Data;
using VRising.GameData.Models;
using RPGMods;
using static VCF.Core.Basics.RoleCommands;
using Steamworks;
using VRising.GameData.Methods;
using User = ProjectM.Network.User;

namespace VPlus.Hooks
{
    [HarmonyPatch]
    public class ServerBootstrapPatches
    {
        private static readonly string redV = FontColors.Red("V");

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
            //string colorName = FontColors.Cyan(user.CharacterName.ToString());
            //string colorStatus = FontColors.Green("online");
            //ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"{colorName} is {colorStatus}.");
            if (!VPlus.Data.Databases.playerDivinity.ContainsKey(steamId) && VPlus.Core.Plugin.PlayerAscension)
            {
                DivineData divineData = new DivineData(0, 0);
                VPlus.Data.Databases.playerDivinity.Add(steamId, divineData);
                ChatCommands.SavePlayerDivinity();
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
            try
            {
                int userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[userIndex];
                Entity userEntity = serverClient.UserEntity;
                User user = __instance.EntityManager.GetComponentData<User>(userEntity);
                Entity playerEntity = user.LocalCharacter.GetEntityOnServer();
                ulong steamId = user.PlatformId;
                //string colorName = FontColors.Cyan(user.CharacterName.ToString());
                //string colorStatus = FontColors.Red("offline");
                //ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"{colorName} is {colorStatus}.");
                if (VPlus.Data.Databases.playerDivinity.ContainsKey(steamId))
                {
                    DivineData divineData = VPlus.Data.Databases.playerDivinity[steamId];

                    DivineData currentPlayerDivineData = VPlus.Data.Databases.playerDivinity[steamId];
                    currentPlayerDivineData.OnUserDisconnected(user, divineData); // Calculate points and update times
                    ChatCommands.SavePlayerDivinity();
                }
            }
            catch (System.Exception ex)
            {
                //VPlus.Core.Logger.Log(VPlus.Core.Logger.Level.Error, ex);
            }
        }
        
        [HarmonyPatch(typeof(SpawnCharacterSystem), nameof(SpawnCharacterSystem.OnUpdate))]
        [HarmonyPostfix]
        private static void SpawnKitPostfix(SpawnCharacterSystem __instance)
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            HashSet<Entity> hash = [];
            
            NativeArray<Entity> entities = __instance._SpawnCharacterQuery.ToEntityArray(Allocator.Temp);
            try
            {
                foreach (Entity entity in entities)
                {
                    if (hash.Contains(entity)) continue;
                    if (!entity.Has<SpawnCharacter>()) continue;
                    SpawnCharacter spawnCharacter = entity.Read<SpawnCharacter>();

                    Entity character = spawnCharacter.PostSpawn_Character;
                    if (!character.Has<Vision>())
                    {
                        continue;
                    }
                    else
                    {
                        Vision vision = character.Read<Vision>();
                        if (vision.Range._Value != 5000f)
                        {
                            vision.Range._Value = 5000f;
                            character.Write(vision);
                            if (Databases.playerDivinity.TryGetValue(spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out DivineData data) && !data.Spawned)
                            {
                                data.Spawned = true;
                                Databases.playerDivinity[spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = data;
                                ChatCommands.SavePlayerDivinity();
                                UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId);
                                foreach (var item in VPlus.Hooks.ReplaceAbilityOnSlotSystem_Patch.keyValuePairs.Keys)
                                {
                                    userModel.TryGiveItem(item, VPlus.Hooks.ReplaceAbilityOnSlotSystem_Patch.keyValuePairs[item], out var _);
                                }
                                ServerChatUtils.SendSystemMessageToClient(entityManager, spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>(), "You've received a starting kit with blood essence, stone, wood, coins, and health potions!");
                            }
                            else
                            {
                                DivineData divineData = new DivineData(0, 0);
                                VPlus.Data.Databases.playerDivinity.Add(spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, divineData);
                                ChatCommands.SavePlayerDivinity();
                                var newdata = Databases.playerDivinity[spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId];
                                newdata.Spawned = true;
                                Databases.playerDivinity[spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId] = data;
                                ChatCommands.SavePlayerDivinity();
                                UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId);
                                foreach (var item in VPlus.Hooks.ReplaceAbilityOnSlotSystem_Patch.keyValuePairs.Keys)
                                {
                                    userModel.TryGiveItem(item, VPlus.Hooks.ReplaceAbilityOnSlotSystem_Patch.keyValuePairs[item], out var _);
                                }
                                ServerChatUtils.SendSystemMessageToClient(entityManager, spawnCharacter.PostSpawn_Character.Read<PlayerCharacter>().UserEntity.Read<User>(), "You've received a starting kit with blood essence, stone, wood, coins, and health potions!");
                            }
                        }
                    }

                }
            }
            finally
            {
                entities.Dispose();
            }
        }
        
    }
}