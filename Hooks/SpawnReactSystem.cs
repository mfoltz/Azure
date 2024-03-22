using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
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

    public static void Prefix(FollowerSystem __instance)
    {
        ServerGameManager serverGameManager = VWorld.Server.GetExistingSystem<ServerScriptMapper>()._ServerGameManager;
        BuffUtility.BuffSpawner buffSpawner = BuffUtility.BuffSpawner.Create(serverGameManager);
        EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
        EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();

        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        try
        {
            foreach (Entity entity in entities)
            {
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
                        foreach (var b in buffs)
                        {
                            if (b.PrefabGuid.GuidHash.Equals(VCreate.Data.Prefabs.AB_Charm_Owner_HasCharmedTarget_Buff.GuidHash))
                            {
                                return;
                            }
                            else
                            {
                                // check for familiar not in combat mode, skip if so
                                if (DataStructures.PlayerPetsMap.TryGetValue(followed.Read<PlayerCharacter>().UserEntity.Read<User>().PlatformId, out var data))
                                {
                                    if (!data[entity.Read<PrefabGUID>().LookupName().ToString()].Combat)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                        Entity userEntity = followed.Read<PlayerCharacter>().UserEntity;
                        ulong platformId = userEntity.Read<User>().PlatformId;
                        // check for matching prefab on unit and soul gem from player for verification?
                        PrefabGUID prefabGUID = entity.Read<PrefabGUID>();
                        UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(platformId);
                        var items = userModel.Inventory.Items;
                        foreach (var item in items)
                        {
                            if (item.Item.Entity.Equals(Entity.Null)) continue;
                            //ItemData itemData = item.Item.Entity.Read<ItemData>();
                            PrefabGUID soulPrefab = item.Item.Entity.Read<CastAbilityOnConsume>().AbilityGuid;
                            if (soulPrefab.GuidHash.Equals(prefabGUID.GuidHash))
                            {
                                DataStructures.PlayerPetsMap.TryGetValue(platformId, out var data);
                                if (data.TryGetValue(prefabGUID.LookupName().ToString(), out var familiarData) && familiarData.Active) return;
                                
                                // verified soul gem, proceed
                                Plugin.Log.LogInfo("Found inactive familiar soulstone, removing charm and binding...");

                                BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, charm, entity);
                                OnHover.ConvertCharacter(userEntity, entity);
                                return;
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