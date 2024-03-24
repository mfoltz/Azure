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
                if (hashset.Contains(entity)) continue;
                var buffer = entity.ReadBuffer<BuffBuffer>();
                foreach (var buff in buffer)
                {
                    if (buff.PrefabGuid.GuidHash.Equals(charm.GuidHash))
                    {
                        Follower follower = entity.Read<Follower>();
                        Entity followed = follower.Followed._Value;
                        if (!followed.Has<PlayerCharacter>()) continue;
                        //Plugin.Log.LogInfo("Charmed entity detected in SpawnReactSystem...");
                        var buffs = followed.ReadBuffer<BuffBuffer>();
                        if (DataStructures.PlayerPetsMap.TryGetValue(followed.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var pet))
                        {
                            var keys = pet.Keys;
                            foreach (var key in keys)
                            {
                                if (pet.TryGetValue(key, out var value))
                                {
                                    if (!value.Combat)
                                    {
                                        hashset.Add(entity);
                                        goto outerLoop;
                                    }
                                    else if (value.Active)
                                    {
                                        hashset.Add(entity);
                                        goto outerLoop;
                                    }
                                }
                            }
                        }
                        
                        Entity userEntity = followed.Read<PlayerCharacter>().UserEntity;

                        int check = entity.Read<PrefabGUID>().GuidHash;
                        if (DataStructures.PlayerSettings.TryGetValue(userEntity.Read<User>().PlatformId, out var data))
                        {
                            if (DataStructures.UnlockedPets.TryGetValue(userEntity.Read<User>().PlatformId, out var unlockedPets))
                            {
                                Plugin.Log.LogInfo($"entityFromQuery: {check}, setFamiliar: {data.Familiar} ");
                                if (!unlockedPets.Contains(check))
                                {
                                    hashset.Add(entity);
                                    goto outerLoop;
                                }
                                else if (!data.Familiar.Equals(check))
                                {
                                    hashset.Add(entity);
                                    goto outerLoop;
                                }
                                else
                                {
                                    Plugin.Log.LogInfo("Found inactive familiar, removing charm and binding...");
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