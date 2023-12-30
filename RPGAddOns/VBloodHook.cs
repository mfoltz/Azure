using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace RPGAddOns
{
    internal class VBloodHook
    {
        [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdate(VBloodSystem __instance)
        {
            EntityManager entityManager = __instance.EntityManager;
            if (!__instance.EventList.IsEmpty)
            {
                foreach (var _event in __instance.EventList)
                {
                    Il2CppStructArray<ComponentType> componentTypes = new(1L);
                    componentTypes[0] = ComponentType.ReadOnly<PrefabGuidComponent>();
                    EntityQuery query = entityManager.CreateEntityQuery(componentTypes);

                    NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
                    try
                    {
                        foreach (var entity in entities)
                        {
                            PrefabGuidComponent prefabComponent = entityManager.GetComponentData<PrefabGuidComponent>(entity);
                            if (prefabComponent.GetPrefabGUID() == _event.Source)
                            {
                                // Found an entity with the matching PrefabGUID
                                // Perform points calculation here
                                Entity targetEntity = _event.Target;
                                Entity sourceEntity = entity;

                                int min = 1;
                                int max = 10;

                                ulong SteamID = entityManager.GetComponentData<User>(targetEntity).PlatformId;

                                int playerLevel = RPGMods.Systems.ExperienceSystem.getLevel(SteamID);
                                int unitLevel = entityManager.GetComponentData<UnitLevel>(targetEntity).Level;
                                int delta = playerLevel - unitLevel;

                                // if delta < 0 player is lower level than unit, if abs(delta) > 5 player should not get any points
                            }
                        }
                    }
                    finally
                    {
                        if (entities.IsCreated)
                        {
                            entities.Dispose();
                            query.Dispose();
                        }
                    }
                }
            }
        }
    }
}