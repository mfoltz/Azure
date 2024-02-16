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

namespace RPGAddOnsEx.Hooks
{
    [HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Spawn), "OnUpdate")]
    public class ModifyUnitStatBuffSystem_Spawn_Patch
    {
        private static void Prefix(ModifyUnitStatBuffSystem_Spawn __instance)
        {
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                // is this an array of all entities associated with the 'event'? if so can grab armor entity probably
                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                Plugin.Logger.LogInfo("ModifyUnitStatBuffSystem_Spawn Prefix called...");
                foreach (Entity entity in entityArray)
                {
                    // ah, so the event is probably it's own entity
                    entity.LogComponentTypes();
                    Plugin.Logger.LogInfo($"ArmorLevel: {entityManager.GetComponentData<ArmorLevel>(entity).Level}");
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    //ForEachLambdaJobDescription entities = __instance.Entities;

                    // maybe prefix will have state of equipment before armor is equipped in postfix or something?
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Found player...");
                        /*
                        if (entityManager.TryGetComponentData<Equipment>(entity, out Equipment component))
                        {
                            List<NetworkedEntity> slotEntities = new List<NetworkedEntity>
                            {
                                component.ArmorChestSlotEntity,
                                component.ArmorGlovesSlotEntity,
                                component.ArmorLegsSlotEntity,
                                component.ArmorFootgearSlotEntity
                            };
                            foreach (NetworkedEntity networkedEntity in slotEntities)
                            {
                                Entity armorEntity = networkedEntity._Entity;
                                if (armorEntity != Entity.Null)
                                {
                                    armorEntity.LogComponentTypes();
                                }
                            }
                        }
                        */
                        // need to make sure this is when they are equipping armor or it will apply every time the player is updated
                        // also, need to take away what was given when the armor is unequipped or else it will apply forever
                        // check their inventory?

                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);

                        ModifyUnitStatBuff_DOTS item = buffer[0];
                        ModifyUnitStatBuff_DOTS itemClone = item;
                        Plugin.Logger.LogInfo("Adding item to buffer...");
                        itemClone.StatType = UnitStatType.PhysicalResistance;
                        itemClone.Id = ModificationId.NewId(0);
                        buffer.Add(itemClone);
                        Plugin.Logger.LogInfo("Modification complete.");
                    }
                }
                entityArray.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }
        }

        private static void Postfix(ModifyUnitStatBuffSystem_Spawn __instance)
        {
            try
            {
                EntityManager entityManager = __instance.EntityManager;

                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                Plugin.Logger.LogInfo("ModifyUnitStatBuffSystem_Spawn Postfix called...");
                foreach (Entity entity in entityArray)
                {
                    entity.LogComponentTypes();
                    Plugin.Logger.LogInfo($"ArmorLevel: {entityManager.GetComponentData<ArmorLevel>(entity).Level}");
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    //ForEachLambdaJobDescription entities = __instance.Entities;

                    // maybe prefix will have state of equipment before armor is equipped in postfix or something?
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Plugin.Logger.LogInfo("Found player...");
                        /*
                        if (entityManager.TryGetComponentData<Equipment>(entity, out Equipment component))
                        {
                            List<NetworkedEntity> slotEntities = new List<NetworkedEntity>
                            {
                                component.ArmorChestSlotEntity,
                                component.ArmorGlovesSlotEntity,
                                component.ArmorLegsSlotEntity,
                                component.ArmorFootgearSlotEntity
                            };
                            foreach (NetworkedEntity networkedEntity in slotEntities)
                            {
                                Entity armorEntity = networkedEntity._Entity;
                                if (armorEntity != Entity.Null)
                                {
                                    armorEntity.LogComponentTypes();
                                }
                            }
                        }
                        */
                        //Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        //Equipment componentData = entityManager.GetComponentData<Equipment>(userEntity);
                        DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);

                        ModifyUnitStatBuff_DOTS item = buffer[0];
                        ModifyUnitStatBuff_DOTS itemClone = item;
                        Plugin.Logger.LogInfo("Adding item to buffer...");
                        itemClone.StatType = UnitStatType.PhysicalResistance;
                        itemClone.Id = ModificationId.NewId(0);
                        buffer.Add(itemClone);
                        Plugin.Logger.LogInfo("Modification complete.");
                    }
                }
                entityArray.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }
        }

        private static void ModifyArmorStats()
        {
        }
    }
}