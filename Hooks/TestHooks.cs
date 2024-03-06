using HarmonyLib;
using Microsoft.VisualBasic;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using VBuild.Core.Toolbox;
using VPlus.Core;

namespace VPlus.Hooks
{
    [HarmonyPatch(typeof(SpawnAbilityGroupSlotsSystem), nameof(SpawnAbilityGroupSlotsSystem.OnUpdate))]
    public class SpawnAbilityGroupSlotsSystem_Patch
    {
        [HarmonyPrefix]
        public static void OnUpdatePrefix(SpawnAbilityGroupSlotsSystem __instance)
        {
            EntityManager entityManager = __instance.EntityManager;
            

            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                //entity.LogComponentTypes();
                //Plugin.Logger.LogInfo("Entity in LambdaJob1");
                if (Utilities.HasComponent<AbilityGroupSlot>(entity))
                {
                    // intercept 4th one, activate slot?
                    AbilityGroupSlot abilityGroupSlot = Utilities.GetComponentData<AbilityGroupSlot>(entity); // this is the ability slot
                    Entity abilityBar = abilityGroupSlot.AbilityBar._Entity;
                    if (Utilities.HasComponent<AbilityGroupSlotBuffer>(abilityBar))
                    {
                        DynamicBuffer<AbilityGroupSlotBuffer> abilityGroupSlotBuffers = entityManager.GetBuffer<AbilityGroupSlotBuffer>(abilityBar);
                        foreach (var buffer in abilityGroupSlotBuffers)
                        {

                            
                            

                            Plugin.Logger.LogInfo($"{abilityGroupSlot.GroupGuid.Value} | {abilityGroupSlot.CopyCooldown._Value} | {abilityGroupSlot.SlotId} | {buffer.ShowOnBar}");
                            //groupSlot.LogComponentTypes();
                            NetworkedEntity groupSlot = buffer.GroupSlotEntity;

                            groupSlot.TryGetSyncedEntity(out Entity syncedEntity);
                            if (syncedEntity != Entity.Null)
                            {
                                //syncedEntity.LogComponentTypes();
                                if (Utilities.HasComponent<AbilityGroupSlot>(syncedEntity))
                                {
                                    AbilityGroupSlot syncedSlot = Utilities.GetComponentData<AbilityGroupSlot>(syncedEntity);
                                    Plugin.Logger.LogInfo($"{syncedSlot.GroupGuid.Value} | {syncedSlot.CopyCooldown._Value} | {syncedSlot.SlotId} | {buffer.ShowOnBar}");
                                    Entity abilityBarEntity = syncedSlot.AbilityBar._Entity;
                                    abilityBarEntity.LogComponentTypes();
                                }
                            }
                            else
                            {
                                Plugin.Logger.LogInfo("Synced entity is null");
                            }


                        }
                    }
                    
                    

                
                    
                    
                }
            }
            entities.Dispose();
        }
    }

    [HarmonyPatch(typeof(InteractSystemServer), nameof(InteractSystemServer.OnUpdate))]
    public class InteractSystemServer_Patch
    {
        [HarmonyPrefix]
        public static void OnUpdatePrefix(InteractSystemServer __instance)
        {
            //Plugin.Logger.LogInfo("InteractSystemerverPostifx called...");
            
        }
    }

    
}