using AdminCommands;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGAddOnsEx.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;
using Plugin = RPGAddOnsEx.Core.Plugin;

#nullable disable

namespace RPGAddOnsEx.Hooks_WIP
{
    [HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Spawn), "OnUpdate")]
    public class ModifyUnitStatBuffSystem_Spawn_Patch
    {
        private static Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>> playerBuffers = new Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>>();

        private static void Prefix(ModifyUnitStatBuffSystem_Spawn __instance)
        {
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                Plugin.Logger.LogInfo("ModifyUnitStatBuffSystem_Spawn Prefix called...");
                foreach (Entity entity in entityArray)
                {
                    //Plugin.Logger.LogInfo($"ArmorLevel: {Utilities.GetComponentData<ArmorLevel>(entity).Level}");
                    // this is the armor level of the item, not the player
                    // so if this entity has that component, that means the player is equipping a piece of armor
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Found player...");
                        // yay, identified when armor is being equipped
                        // now can check for specific types of armor like death gear being equipped, then I might check for death gear being added to player inventory to remove the buff?
                        // how would that work if the player acquired a new set, I don't want that to count as unequipping
                        if (entityManager.TryGetComponentData(entity, out ArmorLevel component))
                        {
                            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                            User user = entityManager.GetComponentData<User>(userEntity);
                            DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                            // do I want to keep a record of each MUSB_DOTS per player? might have to if I want to remove them later
                            playerBuffers[user.PlatformId] = buffer;
                            ModifyUnitStatBuff_DOTS item = buffer[0];
                            ModifyUnitStatBuff_DOTS itemClone = item;
                            Plugin.Logger.LogInfo("Adding item to buffer...");
                            itemClone.StatType = UnitStatType.PhysicalResistance;
                            itemClone.Id = ModificationId.NewId(0);
                            buffer.Add(itemClone);
                            Plugin.Logger.LogInfo("Modification complete.");
                        }
                    }
                }
                entityArray.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Destroy), "OnUpdate")]
    public class ModifyUnitStatBuffSystem_Destroy_Patch
    {
        private static void Prefix(ModifyUnitStatBuffSystem_Destroy __instance)
        {
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                Plugin.Logger.LogInfo("ModifyUnitStatBuffSystem_Destroy Prefix called...");
                foreach (Entity entity in entityArray)
                {
                    entity.LogComponentTypes();
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Found player...");
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                    }
                }
                entityArray.Dispose();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }
        }
    }
}