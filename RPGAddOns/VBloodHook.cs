using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RPGAddOns
{
    [HarmonyPatch]
    internal class VBloodHook
    {
        [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdate(VBloodSystem __instance)
        {
            //Plugin.Logger.LogInfo("VBloodSystem OnUpdate called"); // Log when method is called
            // the OnUpdate method seems to happen quite a bit so no need for checks up here
            EntityManager entityManager = __instance.EntityManager;
            //ServerChatUtils.SendSystemMessageToAllClients(entityManager, "VBLOOD KILL DETECTED");

            if (!__instance.EventList.IsEmpty)
            {
                foreach (var _event in __instance.EventList)
                {
                    Plugin.Logger.LogInfo($"Processing event: {_event}"); // Log details about each event

                    ServerChatUtils.SendSystemMessageToClient(entityManager, entityManager.GetComponentData<User>(_event.Target), "VBLOOD KILL DETECTED");

                    Il2CppStructArray<ComponentType> componentTypes = new(1L);
                    componentTypes[0] = ComponentType.ReadOnly<PrefabGuidComponent>();
                    EntityQuery query = entityManager.CreateEntityQuery(componentTypes);
                    NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
                    //should return all entities that have a PrefabGuidComponent in this context
                    try
                    {
                        foreach (var entity in entities)
                        {
                            PrefabGuidComponent prefabComponent = entityManager.GetComponentData<PrefabGuidComponent>(entity);
                            if (prefabComponent.GetPrefabGUID() == _event.Source)
                            {
                                // Found an entity with the matching PrefabGUID
                                // Perform your logic here
                                Entity targetEntity = _event.Target;
                            }
                        }
                    }
                    finally
                    {
                        if (entities.IsCreated)
                        {
                            entities.Dispose();
                        }
                    }
                }
            }
            else
            {
                //Plugin.Logger.LogInfo("EventList is empty"); // Log if EventList is empty
            }
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}