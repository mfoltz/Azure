﻿using Bloodstone.API;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Network;
using ProjectM.Terrain;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using V.Augments;
using VCreate.Core.Services;
using VCreate.Core.Toolbox;
using VPlus.Core;
using VPlus.Core.Commands;
using VPlus.Data;
using VRising.GameData.Models;
using Exception = System.Exception;

namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), "TriggerSave")]
    public class TriggerPersistenceSaveSystem_Patch
    {
        public static void Postfix() => Events.RunMethods();
    }

    public class Tokens
    {
        private static int counter = 0;

        public static void UpdateTokens()
        {
            counter += 1;
            if (counter < 10) return;
            Plugin.Logger.LogInfo("Updating tokens");
            var playerDivinities = Databases.playerDivinity;
            if (playerDivinities == null) return;
            foreach (var entry in playerDivinities)
            {
                // filter for people online and offline here

                try
                {
                    UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(entry.Key);
                    if (!userModel.IsConnected) continue;
                    ulong steamId = entry.Key;
                    DivineData currentPlayerDivineData = entry.Value;

                    string name = ChatCommands.GetPlayerNameById(steamId);
                    PlayerService.TryGetUserFromName(name, out var userEntity);
                    User user = Utilities.GetComponentData<User>(userEntity);
                    // Safely execute the intended actions outside of the main game loop to avoid conflicts.
                    // Consider adding locks or other concurrency control mechanisms if needed.
                    currentPlayerDivineData.OnUserDisconnected(user, currentPlayerDivineData); // Simulate user disconnection
                    currentPlayerDivineData.OnUserConnected();    // Simulate user reconnection
                    ChatCommands.SavePlayerDivinity();            // Save changes if necessary
                                                                  //Plugin.Logger.LogInfo($"Updated token data for player {steamId}");
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogInfo($"Error updating token data for player {entry.Key}: {e.Message}");
                }
            }
            counter = 0;
        }
    }

    public static class Events
    {
        private static int timer = 0; //in minutes
        private static bool isRunning = false;
        private static Entity infinite = Entity.Null;
        private static int otherTimer = 0;

        public static void RunMethods()
        {
            Tokens.UpdateTokens(); //
            timer += 2; // want to run event every 2 hours and save happens every 2 minutes
            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            EntityCommandBuffer ecb = entityCommandBufferSystem.CreateCommandBuffer();
            if (timer > 2)
            {
                timer = 0;
                isRunning = true;

                Plugin.Logger.LogInfo("Running events");
                try
                {
                    EntityManager entityManager = VWorld.Server.EntityManager;
                    float3 center = new(-1000, 0, -514);
                    Entity node = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                    Entity nodeEntity = entityManager.Instantiate(node);
                    //node.LogComponentTypes();
                    //Entity zone = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                    //zone.LogComponentTypes();
                    //SystemPatchUtil.Destroy(zone);
                    //Utilities.AddComponentData(node, new Immortal { IsImmortal = true });
                    Health health = nodeEntity.Read<Health>();
                    health.MaxHealth._Value = 50000f;
                    health.Value = 50000f;
                    health.MaxRecoveryHealth = 50000f;

                    nodeEntity.Write(health);
                    nodeEntity.Write<Translation>(new Translation { Value = center });
                    RadialDamageDebuff debuff = new RadialDamageDebuff
                    {
                        InnerDamagePerSecond = 15f,
                        OuterDamagePerSecond = 15f,
                        InnerRadius = 5f,
                        OuterRadius = 20f,
                        DamageIncrements = 15f,
                        DamageSum = 15f,
                        DamageType = MainDamageType.RadialHoly
                    };
                    Utilities.AddComponentData(nodeEntity, debuff);
                    
                    if (!nodeEntity.Has<AttachMapIconsToEntity>())
                    {
                        var buffer = entityManager.AddBuffer<AttachMapIconsToEntity>(nodeEntity);
                        buffer.Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_Siege_Summon_T02_Complete });
                    }
                    else
                    {
                        var found = nodeEntity.ReadBuffer<AttachMapIconsToEntity>();
                        found.Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_Siege_Summon_T02_Complete });
                    }
                    
                    Plugin.Logger.LogInfo("Created and set node...");
                    infinite = nodeEntity;
                    string red = VPlus.Core.Toolbox.FontColors.Red("Warning");
                    string message = $"{red}: holy radiation detected at the colosseum. The Church must be hiding something!";
                    ServerChatUtils.SendSystemMessageToAllClients(ecb, message);
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogInfo($"Error running events: {e.Message}");
                }
            }
            else if (isRunning)
            {
                int proxy = timer;
                if (proxy == 2)
                {
                    timer = 0; // reset while event is running
                    otherTimer += 1; // want to do stuff with this until it reaches 5 then nuke

                    try
                    {
                        if (infinite.Has<RadialDamageDebuff>())
                        {
                            RadialDamageDebuff debuff = infinite.Read<RadialDamageDebuff>();
                            debuff.InnerDamagePerSecond += 15f;
                            debuff.OuterDamagePerSecond += 15f;
                            debuff.InnerRadius += 5f;
                            debuff.OuterRadius += 5f;


                            infinite.Write(debuff);
                        }
                        else
                        {
                            Plugin.Logger.LogInfo("No radial damage debuff...");
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogInfo($"Error setting radials: {e.Message}");
                    }
                    switch (otherTimer)
                    {
                        case 1:
                            string message1 = $"The sacred node is becoming unstable. This won't go unnoticed... ";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message1);
                            break;
                        case 2:
                            string message2 = $"Dunley's militia has sent a dispatch requesting aid from Brighthaven.";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message2);
                            break;
                        case 3:
                            string message3 = $"The Church of Luminance is amplifying the holy radiation to purge the area!";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message3);
                            break;
                        case 4:
                            CleanUp();
                            string message4 = $"The area has been completely purged by the light. No anomalies remain.";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message4);
                            timer = 0;
                            isRunning = false;
                            break; 
                    }
                    
                }
            }
        }

        public static void CleanUp()
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            
            
            if (entityManager.Exists(infinite))
            {
                SystemPatchUtil.Destroy(infinite);
            }
            else
            {
                Plugin.Logger.LogInfo("Failed to destroy node.");
            }
            
        }
    }
}