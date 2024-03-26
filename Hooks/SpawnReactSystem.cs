using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Commands;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using VRising.GameData.Models;

[HarmonyPatch(typeof(FollowerSystem), nameof(FollowerSystem.OnUpdate))]
public static class FollowerSystemPatchV2
{
    private static readonly PrefabGUID charm = VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff;
    private static HashSet<Entity> hashset = new();

    public static void Prefix(FollowerSystem __instance)
    {
        ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
        BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
        
        EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
        EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        try
        {
        outerLoop:
            foreach (Entity entity in entities)
            {
                //Plugin.Log.LogInfo($"{entities.m_Length}");
                if (hashset.Contains(entity)) continue;
                var buffer = entity.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffer)
                {
                    if (buff.PrefabGuid.GuidHash.Equals(charm.GuidHash))
                    {
                        Follower follower = entity.Read<Follower>();
                        Entity followed = follower.Followed._Value;
                        if (!followed.Has<PlayerCharacter>()) continue;
                        if (DataStructures.PlayerSettings.TryGetValue(followed.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var data) && !data.Binding) return;
                        
                        Plugin.Log.LogInfo("Charmed entity following player with binding flag detected in SpawnReactSystem, checking for valid familiar to bind...");
                        
                        Entity userEntity = followed.Read<PlayerCharacter>().UserEntity;

                        int check = entity.Read<PrefabGUID>().GuidHash;
                        if (DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out var dataset))
                        {
                            
                            Plugin.Log.LogInfo($"entityFromQuery: {check}, setFamiliar: {data.Familiar} ");

                            if (!data.Familiar.Equals(check))
                            {
                                Plugin.Log.LogInfo("Failed set familiar check, not the set player familiar.");
                                dataset.Binding = false;
                                hashset.Add(entity);
                                goto outerLoop;
                            }
                            else
                            {
                                Plugin.Log.LogInfo("Found unbound, inactive set familiar, removing charm and binding...");
                                BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, charm, entity);
                                
                                OnHover.ConvertCharacter(userEntity, entity);
                                hashset.Add(entity);
                                goto outerLoop;
                            }
                            
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle the error as needed
            Plugin.Log.LogError(ex);
        }
        finally
        {
            // Ensure entities are disposed of even if an exception occurs
            entities.Dispose();
            hashset.Clear();
        }
    }
}