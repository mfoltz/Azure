using Bloodstone.API;
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
using static Il2CppSystem.Linq.Expressions.Interpreter.NullableMethodCallInstruction;
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
            if (counter < 20) return;
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

        //private static Entity infinite = Entity.Null;
        private static List<Entity> zones = [];

        private static int otherTimer = 0;

        public static void RunMethods()
        {
            Tokens.UpdateTokens(); //
            timer += 1; // want to run event every 2 hours and save happens every 2 minutes
            EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            EntityCommandBuffer ecb = entityCommandBufferSystem.CreateCommandBuffer();
            if (timer > 1)
            {
                timer = 0;
                isRunning = true;

                Plugin.Logger.LogInfo("Running events");
                try
                {
                    
                    string red = VPlus.Core.Toolbox.FontColors.Red("Warning");
                    string message = $"{red}: The Sacred Nodes will be active soon... ";
                    ServerChatUtils.SendSystemMessageToAllClients(ecb, message);
                }
                catch (Exception e)
                {
                    Plugin.Logger.LogInfo($"Error running events: {e.Message}");
                }
            }
            else if (isRunning)
            {
                EntityManager entityManager = VWorld.Server.EntityManager;
                if (timer == 1)
                {
                    timer = 0; // reset while event is running
                    otherTimer += 1; // want to do stuff with this until it reaches 4 then reset


                    switch (otherTimer)
                    {
                        case 1:
                            HandleCase1();
                            break;

                        case 2:
                            HandleCase2();
                            break;

                        case 3:
                            HandleCase3();
                            break;

                        case 4:
                            CleanUp();
                            break;
                    }

                    void HandleCase1()
                    {
                        float3 center = new(-1549, 0, -56);
                        string message1 = $"The Sacred Node at the Transcendum Mine is now active.";
                        Entity zone = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                        Entity holyZone = VWorld.Server.EntityManager.Instantiate(zone);
                        Entity node1 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity1 = entityManager.Instantiate(node1);
                        holyZone.Write<Translation>(new Translation { Value = center });
                        SetupMapIcon(nodeEntity1, VCreate.Data.Prefabs.MapIcon_POI_Resource_QuartzMine);
                        zones.Add(holyZone);
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message1);
                    }

                    void HandleCase2()
                    {
                        string message2 = $"The Sacred Node at the Quartz Quarry is now active.";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message2);
                        float3 otherfloat = new(-1743, 0, -438); //quartzmines
                        Entity zone3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone3 = VWorld.Server.EntityManager.Instantiate(zone3);
                        holyZone3.Write<Translation>(new Translation { Value = otherfloat });
                        Entity node2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity2 = entityManager.Instantiate(node2);
                        nodeEntity2.Write<Translation>(new Translation { Value = otherfloat });
                        SetupMapIcon(nodeEntity2, VCreate.Data.Prefabs.MapIcon_POI_Resource_QuartzMine);
                        zones.Add(holyZone3);
                        zones.Add(nodeEntity2);
                    }

                    void HandleCase3()
                    {
                        string message3 = $"The Sacred Node at the Silver Mine is now active.";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message3);
                        float3 float3 = new(-2326, 15, -390); //silvermines
                        Entity zone2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone2 = VWorld.Server.EntityManager.Instantiate(zone2);
                        holyZone2.Write<Translation>(new Translation { Value = float3 });
                        Entity node3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity3 = entityManager.Instantiate(node3);
                        nodeEntity3.Write<Translation>(new Translation { Value = float3 });
                        SetupMapIcon(nodeEntity3, VCreate.Data.Prefabs.MapIcon_POI_Resource_QuartzMine);
                        zones.Add(holyZone2);
                        zones.Add(nodeEntity3);
                    }

                    void SetupMapIcon(Entity entity, PrefabGUID prefabGUID)
                    {
                        if (!entity.Has<AttachMapIconsToEntity>())
                        {
                            var buffer = entityManager.AddBuffer<AttachMapIconsToEntity>(entity);
                            buffer.Add(new AttachMapIconsToEntity { Prefab = prefabGUID });
                        }
                        else
                        {
                            var found = entity.ReadBuffer<AttachMapIconsToEntity>();
                            found.Add(new AttachMapIconsToEntity { Prefab = prefabGUID });
                        }
                    }
                }
            }
        }

        public static void CleanUp()
        {
            EntityManager entityManager = VWorld.Server.EntityManager;

            foreach (var zone in zones)
            {
                if (entityManager.Exists(zone))
                {
                    SystemPatchUtil.Destroy(zone);
                }
                else
                {
                    Plugin.Logger.LogInfo("Failed to destroy zone.");
                }
            }
        }
    }
}