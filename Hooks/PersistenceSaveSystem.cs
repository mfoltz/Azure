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
            if (timer > 180)
            {
                timer = 0;
                isRunning = true;

                Plugin.Logger.LogInfo("Running events");
                try
                {
                    
                    string red = VPlus.Core.Toolbox.FontColors.Red("Warning");
                    string cyancrystal = VPlus.Core.Toolbox.FontColors.Cyan("Crystal");
                    string message = $"{red}: the {cyancrystal} Nodes will be active soon... ";
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
                            HandleCase4();
                            break;
                        case 5:
                            CleanUp();
                            otherTimer = 0;
                            timer = 0;
                            isRunning = false;
                            break;
                    }

                    void HandleCase1()
                    {
                        float3 center = new(-1549, -5, -56);
                        string greencursed = VPlus.Core.Toolbox.FontColors.Green("Cursed");
                        string message1 = $"The {greencursed} Node at the Transcendum Mine is now active.";
                        Entity zone = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                        Entity holyZone = VWorld.Server.EntityManager.Instantiate(zone);
                        Entity node1 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity1 = entityManager.Instantiate(node1);
                        nodeEntity1.Write<Translation>(new Translation { Value = center });
                        holyZone.Write<Translation>(new Translation { Value = center });
                        SetupMapIcon(nodeEntity1, VCreate.Data.Prefabs.MapIcon_POI_Resource_CoalMine);
                        zones.Add(nodeEntity1);
                        zones.Add(holyZone);
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message1);
                    }

                    void HandleCase2()
                    {
                        if (!entityManager.Exists(zones.First()))
                        {
                            var second = zones[1];
                            if (entityManager.Exists(second))
                            {
                                SystemPatchUtil.Destroy(second);
                            }
                        }
                        string yellowholy = VPlus.Core.Toolbox.FontColors.Yellow("Blessed");
                        string message2 = $"The {yellowholy} Node at the Quartz Quarry is now active.";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message2);
                        float3 otherfloat = new(-1743, -5, -438); //quartzmines
                        Entity zone3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone3 = VWorld.Server.EntityManager.Instantiate(zone3);
                        holyZone3.Write<Translation>(new Translation { Value = otherfloat });
                        Entity node2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity2 = entityManager.Instantiate(node2);
                        nodeEntity2.Write<Translation>(new Translation { Value = otherfloat });
                        SetupMapIcon(nodeEntity2, VCreate.Data.Prefabs.MapIcon_POI_Resource_CoalMine);
                        zones.Add(nodeEntity2);
                        zones.Add(holyZone3);
                        
                    }

                    void HandleCase3()
                    {
                        if (!entityManager.Exists(zones[2]))
                        {
                            var third = zones[3];
                            if (entityManager.Exists(third))
                            {
                                SystemPatchUtil.Destroy(third);
                            }
                        }
                        string purpleblursed = VPlus.Core.Toolbox.FontColors.Purple("Blursed");
                        string message3 = $"The {purpleblursed} Node at the Silver Mine is now active.";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message3);
                        float3 float3 = new(-2326, 15, -390); //silvermines
                        Entity zone2 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                        Entity holyZone2 = VWorld.Server.EntityManager.Instantiate(zone2);
                        Entity zone4 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone4 = VWorld.Server.EntityManager.Instantiate(zone4);

                        holyZone2.Write<Translation>(new Translation { Value = float3 });
                        Entity node3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity3 = entityManager.Instantiate(node3);
                        nodeEntity3.Write<Translation>(new Translation { Value = float3 });
                        holyZone4.Write<Translation>(new Translation { Value = float3 });
                        SetupMapIcon(nodeEntity3, VCreate.Data.Prefabs.MapIcon_POI_Resource_CoalMine);
                        zones.Add(nodeEntity3);
                        zones.Add(holyZone2);
                        zones.Add(holyZone4);
                        
                    }
                    void HandleCase4()
                    {
                        if (!entityManager.Exists(zones[4]))
                        {
                            var fourth = zones[5];
                            if (entityManager.Exists(fourth))
                            {
                                SystemPatchUtil.Destroy(fourth);
                            }
                            var fifth = zones[6];
                            if (entityManager.Exists(fifth))
                            {
                                SystemPatchUtil.Destroy(fifth);
                            }
                        }
                        string redcondemned = VPlus.Core.Toolbox.FontColors.Red("Condemned");
                        string message4 = $"The {redcondemned} Node at the Spider Cave is now active.";
                        ServerChatUtils.SendSystemMessageToAllClients(ecb, message4);
                        float3 float3 = new(-1087, 0, 47); //crystal 01 position
                        Entity zone3 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Cursed_Zone_Area01];
                        Entity holyZone3 = VWorld.Server.EntityManager.Instantiate(zone3);
                        Entity zone5 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                        Entity holyZone5 = VWorld.Server.EntityManager.Instantiate(zone5);
                        Entity zone6 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Garlic_Zone_Area01];
                        Entity holyZone6 = VWorld.Server.EntityManager.Instantiate(zone6);
                        holyZone3.Write<Translation>(new Translation { Value = float3 });

                        Entity node4 = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage1_Resource];
                        Entity nodeEntity4 = entityManager.Instantiate(node4);
                        nodeEntity4.Write<Translation>(new Translation { Value = float3 });
                        holyZone5.Write<Translation>(new Translation { Value = float3 });
                        holyZone6.Write<Translation>(new Translation { Value = float3 });
                        SetupMapIcon(nodeEntity4, VCreate.Data.Prefabs.MapIcon_POI_Resource_CoalMine);
                        zones.Add(holyZone3);
                        zones.Add(holyZone5);
                        zones.Add(nodeEntity4);
                        zones.Add(holyZone6);
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
                    Plugin.Logger.LogInfo("Zone already destroyed.");
                }
            }
            zones.Clear();
        }
    }
}