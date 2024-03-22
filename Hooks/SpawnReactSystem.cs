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
        hashset.Clear();
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
                        
                        Entity userEntity = followed.Read<PlayerCharacter>().UserEntity;
                        ulong platformId = userEntity.Read<User>().PlatformId;
                        if (DataStructures.PlayerPetsMap.TryGetValue(platformId, out var data))
                        {
                            var keys = data.Keys;
                            foreach (var key in keys)
                            {
                                if (data.TryGetValue(key, out var pet))
                                {
                                    if (pet.Active)
                                    {
                                        hashset.Add(entity);
                                        goto outerLoop;
                                    }
                                }
                            }
                        }
                        // check for matching prefab on unit and soul gem from player for verification?
                        PrefabGUID prefabGUID = entity.Read<PrefabGUID>();
                        UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(platformId);
                        var items = userModel.Inventory.Items;
                        foreach (var item in items)
                        {
                            if (item.Item.Entity.Equals(Entity.Null)) continue;
                            Plugin.Log.LogInfo(item.Item.Entity.Read<NameableInteractable>().Name.ToString());
                            PrefabGUID other = new(int.Parse(item.Item.Entity.Read<NameableInteractable>().Name.ToString()));
                            Plugin.Log.LogInfo(other);
                            if (other.GuidHash.Equals(prefabGUID.GuidHash))
                            {
                                
                                
                                // verified soul gem, proceed
                                Plugin.Log.LogInfo("Found inactive familiar soulstone, removing charm and binding...");

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
        }
    }
}