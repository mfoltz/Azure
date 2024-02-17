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
// almost ready for live maybe

namespace RPGAddOnsEx.Hooks_WIP
{
    [HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Spawn), "OnUpdate")]
    public class ModifyUnitStatBuffSystem_Spawn_Patch
    {
        //private static Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>> playerBuffers = new Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>>();

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
                    // so if this entity has that component, that means the player is equipping a piece of armor probably
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        // yay, identified when armor is being equipped
                        // now can check for specific types of armor like death gear being equipped
                        if (entityManager.TryGetComponentData(entity, out ArmorLevel component))
                        {
                            Plugin.Logger.LogInfo($"ArmorLevel {(int)component.Level} found...");
                            //could also do if armor level == 90

                            if ((int)component.Level == 90)
                            {
                                // this should add 5 sp per piece of death set equipped
                                Plugin.Logger.LogInfo("Player equipping piece of death set...");
                                Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                                User user = entityManager.GetComponentData<User>(userEntity);
                                DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                                Plugin.Logger.LogInfo("Adding stat...");
                                ModifyUnitStatBuff_DOTS item = buffer[0];
                                ModifyUnitStatBuff_DOTS newItem = item;
                                newItem.StatType = UnitStatType.SpellPower;
                                newItem.Id = ModificationId.NewId(0);
                                newItem.Value = 5;
                                buffer.Add(newItem);
                                Plugin.Logger.LogInfo("Addition complete.");
                            }
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
                        if (entityManager.TryGetComponentData(entity, out ArmorLevel component))
                        {
                            Plugin.Logger.LogInfo($"ArmorLevel {(int)component.Level} found...");
                            if ((int)component.Level == 90)
                            {
                                Plugin.Logger.LogInfo("Player unequipping piece of death set...");
                                Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                                User user = entityManager.GetComponentData<User>(userEntity);
                                DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                                Plugin.Logger.LogInfo("Removing stat...");
                                for (int i = 0; i < buffer.Length; i++)
                                {
                                    if (buffer[i].StatType == UnitStatType.SpellPower && buffer[i].Id.Id == 0)
                                    {
                                        buffer.RemoveAt(i);
                                        Plugin.Logger.LogInfo("Removal complete.");
                                    }
                                }
                            }
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
}