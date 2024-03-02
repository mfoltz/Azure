using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Scenes;
using VBuild.Core.Toolbox;
using VPlus.Core;

namespace VPlus.Hooks
{

    [HarmonyPatch]
    internal class AbilityGroupSlotSystem
    {

        [HarmonyPatch(typeof(SpawnAbilityGroupSlotsSystem), nameof(SpawnAbilityGroupSlotsSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void Prefix(ProjectM.SpawnAbilityGroupSlotsSystem __instance)
        {
            Plugin.Logger.LogInfo("SpawnAbilityGroupSlotsSystem Prefix called...");
            // want to try adding the inventory background bar
            //WeakAssetReference<PrefabGUID> = __instance._GameBootstrap.AbilityGroupSlotPrefab;
            //NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            

            NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                //entity.LogComponentTypes();
                if (Utilities.HasComponent<AbilityGroupSlot>(entity))
                {
                    Plugin.Logger.LogInfo("AbilityGroupSlot component found in lambda1...");
                    AbilityGroupSlot abilityGroupSlot = Utilities.GetComponentData<AbilityGroupSlot>(entity);
                    //DynamicBuffer<AbilityGroupSlotBuffer> abilityGroupSlotBuffer = VWorld.Server.EntityManager.GetBuffer<AbilityGroupSlotBuffer>(entity);
                    // so maybe I want to replace the 'group slot' so to speak with one that shows active and make sure UI is updated
                    Entity abilityBar = abilityGroupSlot.AbilityBar._Entity;
                    Plugin.Logger.LogInfo($"AbilityBar entity found...");
                    //abilityBar.LogComponentTypes();
                    DynamicBuffer<AbilityGroupSlotBuffer> abilityGroupSlotBuffer = VWorld.Server.EntityManager.GetBuffer<AbilityGroupSlotBuffer>(entity);
                    for (int i = 0; i < abilityGroupSlotBuffer.Length; i++)
                    {
                        
                        AbilityGroupSlotBuffer item = abilityGroupSlotBuffer[i];
                        Plugin.Logger.LogInfo($"AbilityGroupSlotBuffer {i} | Slot showing: {item.ShowOnBar}");
                        item.ShowOnBar = true;
                        abilityGroupSlotBuffer[i] = item;
                    }
                    // buffer should change automatically but that doesn't mean the UI will reinitialize in such a way yet


                }
            }
            entities.Dispose();

            Plugin.Logger.LogInfo("SpawnAbilityGroupSlotsSystem Prefix finished...");
           
           

        }
    }
    
}
