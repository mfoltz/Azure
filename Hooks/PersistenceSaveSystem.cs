using Bloodstone.API;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
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
        private static Entity zone = Entity.Null;
        private static Entity storm = Entity.Null;
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
                    float3 center = new(-1001, 0, -514);
                    Entity nodePrefab = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Crystal_01_Stage0_Resource];
                    Entity node = entityManager.Instantiate(nodePrefab);
                    node.Write<Translation>(new Translation { Value = center });
                    if (!node.Has<Immortal>())
                    {
                        Immortal immortal = new() { IsImmortal = true };
                        Utilities.AddComponentData(node, immortal);
                    }
                    else
                    {
                        Immortal immortal = node.Read<Immortal>();
                        immortal.IsImmortal = true;
                        node.Write(immortal);
                    }
                    Health health = node.Read<Health>();
                    health.MaxHealth._Value = 50000f;
                    health.Value = 50000f;
                    node.Write(health);
                    Plugin.Logger.LogInfo("Created node, set health...");
                    if (!node.Has<AttachMapIconsToEntity>())
                    {
                        var buffer = entityManager.AddBuffer<AttachMapIconsToEntity>(node);
                        buffer.Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_Siege_Summon_T02_Complete });
                    }
                    else
                    {
                        var found = node.ReadBuffer<AttachMapIconsToEntity>();
                        found.Add(new AttachMapIconsToEntity { Prefab = VCreate.Data.Prefabs.MapIcon_Siege_Summon_T02_Complete });
                    }

                    infinite = node;
                    Entity stormPrefab = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.LightningStormType_TeslaHills];
                    Entity zonePrefab = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.TM_Holy_Zone_Area_T02];
                    Entity holy = entityManager.Instantiate(zonePrefab);
                    Entity lightning = entityManager.Instantiate(stormPrefab);
                    zone = holy;
                    storm = lightning;
                    holy.Write(new Translation { Value = center });
                    lightning.Write(new Translation { Value = center });
                    string red = VPlus.Core.Toolbox.FontColors.Red("Warning:");
                    string message = $"{red} intense radiation detected at the colosseum. You have a bad feeling about this...";
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
                        if (zone.Has<RadialDamageDebuff>())
                        {
                            RadialDamageDebuff debuff = zone.Read<RadialDamageDebuff>();
                            debuff.InnerDamagePerSecond *= 1.2f;
                            if (otherTimer > 3)
                            {
                                debuff.OuterDamagePerSecond *= 1.2f;
                            }
                        }
                        else if (storm.Has<RadialDamageDebuff>())
                        {
                            RadialDamageDebuff debuff = storm.Read<RadialDamageDebuff>();
                            debuff.DamageIncrements *= 1.1f;

                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogInfo($"Error setting radials: {e.Message}");
                    }
                    switch (otherTimer)
                    {
                        case 1:
                            string message1 = $"The infinite node is becoming dangerously unstable...";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message1);
                            break;
                        case 2:
                            string message2 = $"Dunley's milita has sent a dispatch to Brighthaven requesting aid.";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message2);
                            break;
                        case 3:
                            string message3 = $"The Church of Luminance has alerted Solarus, holy nuke inbound...";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message3);
                            break;
                        case 4:
                            CastNuke();
                            string message4 = $"The area has been purged by the light. No anomalies remain.";
                            ServerChatUtils.SendSystemMessageToAllClients(ecb, message4);
                            timer = 0;
                            isRunning = false;
                            break; 
                    }
                    
                }
            }
        }

        public static void CastNuke()
        {
            EntityManager entityManager = VWorld.Server.EntityManager;
            Entity paladin = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[VCreate.Data.Prefabs.CHAR_ChurchOfLight_Paladin_VBlood];
            NetworkId networkId;
            if (paladin.Has<NetworkId>())
            {
                networkId = paladin.Read<NetworkId>();
            }
            else
            {
                networkId = new NetworkId { Generation = 0, Index = 0 };
            }
            
            Nullable_Unboxed<float3> position = new Nullable_Unboxed<float3>(new float3(-1001, 0, -514));
            CastAbilityServerDebugEvent castAbilityServerDebugEvent = new CastAbilityServerDebugEvent
            {
                AbilityGroup = VCreate.Data.Prefabs.AB_Debug_NukeAll_Group,
                AimPosition = position,
                Who = networkId,
            };
            IEnumerable<UserModel> online = VRising.GameData.GameData.Users.Online;
            Entity user;
            if (!online.Any())
            {
                user = new Entity { Index = networkId.Index, Version = networkId.Generation };
            }
            else
            {
                user = online.First().Entity;
            }
            FromCharacter FromCharacter = new() { Character = paladin, User = user };
            DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
            debugEventsSystem.CastAbilityServerDebugEvent(networkId.Index, ref castAbilityServerDebugEvent, ref FromCharacter);
            // try to destroy the entities here incase nuke doesnt work or something
            if (entityManager.Exists(zone))
            {
                SystemPatchUtil.Destroy(zone);
            }
            else if (entityManager.Exists(storm))
            {
                SystemPatchUtil.Destroy(storm);
            }
            else if (entityManager.Exists(infinite))
            {
                SystemPatchUtil.Destroy(infinite);
            }
            Plugin.Logger.LogInfo("CastNuke complete.");
        }
    }
}