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
        //private static Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>> playerBuffers = new Dictionary<ulong, DynamicBuffer<ModifyUnitStatBuff_DOTS>>();
        public static List<PrefabGUID> deathSet = new List<PrefabGUID>
            {
                new PrefabGUID(1055898174), // Chest
                new PrefabGUID(1400688919), // Boots
                new PrefabGUID(125611165),  // Legs
                new PrefabGUID(-204401621),  // Gloves
            };

        public static List<PrefabGUID> noctumSet = new List<PrefabGUID>
            {
                new PrefabGUID(1076026390), // Chest
                new PrefabGUID(735487676), // Boots
                new PrefabGUID(-810609112),  // Legs
                new PrefabGUID(776192195),  // Gloves
            };

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
                            Plugin.Logger.LogInfo("Player equipping armor...");
                            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                            User user = entityManager.GetComponentData<User>(userEntity);
                            DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                            // do I want to keep a record of each MUSB_DOTS per player? might have to if I want to remove them later
                            //playerBuffers[user.PlatformId] = buffer;
                            ModifyUnitStatBuff_DOTS item = buffer[0];
                            ModifyUnitStatBuff_DOTS itemClone = item;
                            Plugin.Logger.LogInfo("Adding item to buffer...");
                            itemClone.StatType = UnitStatType.PrimaryAttackSpeed;
                            itemClone.Id = ModificationId.NewId(0);
                            buffer.Add(itemClone);
                            Plugin.Logger.LogInfo("Addition complete.");
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
                //this might be firing twice when armor is unequipped, once for the armor and once for the player or something?
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
                            Plugin.Logger.LogInfo("Player unequipping armor...");
                            // first pass appears to be for the armor, second pass is for the player?
                            Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                            User user = entityManager.GetComponentData<User>(userEntity);
                            DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer = entityManager.GetBuffer<ModifyUnitStatBuff_DOTS>(entity);
                            // want to remove what was given above
                            for (int i = 0; i < buffer.Length; i++)
                            {
                                if (buffer[i].StatType == UnitStatType.PrimaryAttackSpeed)
                                {
                                    Plugin.Logger.LogInfo("Removing item from buffer...");
                                    buffer.RemoveAt(i);
                                    Plugin.Logger.LogInfo("Removal complete.");
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