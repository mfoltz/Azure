using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VPlus.Core.Toolbox;
using Plugin = VPlus.Core.Plugin;

#nullable disable
// almost ready for live maybe

namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(ModifyUnitStatBuffSystem_Spawn), "OnUpdate")]
    public class ModifyUnitStatBuffSystem_Spawn_Patch
    {
        //private static Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>> playerBuffers = new Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>>();

        private static void Prefix(ModifyUnitStatBuffSystem_Spawn __instance)
        {
            if (!Plugin.modifyDeathSetStats)
            {
                return;
            }
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                //Plugin.Logger.LogInfo("ModifyUnitStatBuffSystem_Spawn Prefix called...");
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
                            //Plugin.Logger.LogInfo($"ArmorLevel {(int)component.Level} found...");
                            //could also do if armor level == 90

                            if ((int)component.Level == 90)
                            {
                                // this should add 5 sp per piece of death set equipped
                                //Plugin.Logger.LogInfo("Player equipping piece of death set...");
                                Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                                User user = entityManager.GetComponentData<User>(userEntity);
                                DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                                //Plugin.Logger.LogInfo("Adding stat...");
                                //ModifyUnitStatBuff_DOTS item = buffer[0];
                                //ModifyUnitStatBuff_DOTS newItem = item;
                                ModifyUnitStatBuff_DOTS newItem = MUSB_Functions.GetStatType(Plugin.extraStatType);
                                // will be spell power by default if no match from config
                                newItem.Value = Plugin.extraStatValue;
                                buffer.Add(newItem);
                                //Plugin.Logger.LogInfo("Addition complete.");
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
            if (!Plugin.modifyDeathSetStats)
            {
                return;
            }
            try
            {
                EntityManager entityManager = __instance.EntityManager;
                NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                //Plugin.Logger.LogInfo("ModifyUnitStatBuffSystem_Destroy Prefix called...");
                foreach (Entity entity in entityArray)
                {
                    //entity.LogComponentTypes();
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        if (entityManager.TryGetComponentData(entity, out ArmorLevel component))
                        {
                            //Plugin.Logger.LogInfo($"ArmorLevel {(int)component.Level} found...");
                            if ((int)component.Level == 90)
                            {
                                //Plugin.Logger.LogInfo("Player unequipping piece of death set...");
                                Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                                User user = entityManager.GetComponentData<User>(userEntity);
                                DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                                //Plugin.Logger.LogInfo("Removing stat...");
                                for (int i = 0; i < buffer.Length; i++)
                                {
                                    ModifyUnitStatBuff_DOTS newItem = MUSB_Functions.GetStatType(Plugin.extraStatType);
                                    UnitStatType type = buffer[i].StatType;
                                    if (buffer[i].StatType == type && buffer[i].Id.Id == 0)
                                    {
                                        buffer.RemoveAt(i);
                                        //Plugin.Logger.LogInfo("Removal complete.");
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