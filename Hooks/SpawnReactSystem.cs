using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Systems;
using VRising.GameData.Models;

[HarmonyPatch(typeof(FollowerSystem), nameof(FollowerSystem.OnUpdate))]
public static class FollowerSystemPatchV2
{
    private static readonly PrefabGUID charm = VCreate.Data.Prefabs.AB_Charm_Active_Human_Buff;

    private static HashSet<Entity> processed = [];

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
                    if (buff.PrefabGuid.GuidHash.Equals(charm.GuidHash) && !processed.Contains(entity))
                    {
                        Plugin.Log.LogInfo("Charm buff detected...");
                        Follower follower = entity.Read<Follower>();
                        Entity followed = follower.Followed._Value;
                        Entity userEntity = followed.Read<PlayerCharacter>().UserEntity;
                        ulong platformId = userEntity.Read<User>().PlatformId;
                        UserModel userModel = VRising.GameData.GameData.Users.GetUserByPlatformId(platformId);
                        var items = userModel.Inventory.Items;
                        foreach (var item in items)
                        {
                            Entity itemEnt = item.Item.Entity;
                            if (!itemEnt.Has<CastAbilityOnConsume>()) continue;

                            ItemData itemData = itemEnt.Read<ItemData>();
                            if (!itemData.ItemCategory.Equals(ItemCategory.Relic)) continue;

                            Plugin.Log.LogInfo("Linking familiar...");
                            //DestroyUtility.CreateDestroyEvent(entityManager, buff.Entity, DestroyReason.Default, DestroyDebugReason.None); // destroy charm buff
                            BuffUtility.TryRemoveBuff(ref buffSpawner, entityCommandBuffer, charm, entity);
                            OnHover.ConvertCharacter(userEntity, entity);
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