// Decompiled with JetBrains decompiler
// Type: RPGMods.Hooks.HandleGameplayEventsBase_Patch
// Assembly: RPGMods, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 7C19C4EB-25BF-44E7-A841-AA7E48DC0C83
// Assembly location: C:\Users\mitch\Downloads\RPGMods_3.dll

using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using RPGMods.Systems;
using RPGMods.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;

#nullable disable
/*
namespace RPGAddOnsEx.Hooks
{
    [HarmonyPatch(typeof(HandleGameplayEventsBase.GameplayEventInput), "OnUpdate")]
    public class HandleGameplayEventsBase_Patch
    {
        private static void Postfix(HandleGameplayEventsBase __instance)
        {
            if (ExperienceSystem.isEXPActive)
                ProximityLoop.UpdateCache();
            if (Cache.spawnNPC_Listen.Count <= 0)
                return;
            foreach (KeyValuePair<float, SpawnNPCListen> keyValuePair in (ConcurrentDictionary<float, SpawnNPCListen>)Cache.spawnNPC_Listen)
            {
                SpawnNPCListen spawnNpcListen = keyValuePair.Value;
                if (spawnNpcListen.Process)
                {
                    spawnNpcListen = keyValuePair.Value;
                    Entity entity1 = spawnNpcListen.getEntity();
                    spawnNpcListen = keyValuePair.Value;
                    SpawnOptions options = spawnNpcListen.Options;
                    EntityManager entityManager;
                    if (options.ModifyBlood)
                    {
                        entityManager = __instance.EntityManager;
                        if (entityManager.HasComponent<BloodConsumeSource>(entity1))
                        {
                            entityManager = __instance.EntityManager;
                            BloodConsumeSource componentData = entityManager.GetComponentData<BloodConsumeSource>(entity1) with
                            {
                                UnitBloodType = options.BloodType,
                                BloodQuality = options.BloodQuality,
                                CanBeConsumed = options.BloodConsumeable
                            };
                            entityManager = __instance.EntityManager;
                            entityManager.SetComponentData<BloodConsumeSource>(entity1, componentData);
                        }
                    }
                    if (options.ModifyStats)
                    {
                        entityManager = __instance.EntityManager;
                        entityManager.SetComponentData<UnitStats>(entity1, options.UnitStats);
                    }
                    spawnNpcListen = keyValuePair.Value;
                    LifeTime lifeTime;
                    if ((double)spawnNpcListen.Duration < 0.0)
                    {
                        entityManager = __instance.EntityManager;
                        ref EntityManager local = ref entityManager;
                        Entity entity2 = entity1;
                        lifeTime = new LifeTime();
                        lifeTime.Duration = 0.0f;
                        lifeTime.EndAction = LifeTimeEndAction.None;
                        LifeTime componentData = lifeTime;
                        local.SetComponentData<LifeTime>(entity2, componentData);
                    }
                    else
                    {
                        entityManager = __instance.EntityManager;
                        ref EntityManager local1 = ref entityManager;
                        Entity entity3 = entity1;
                        lifeTime = new LifeTime();
                        ref LifeTime local2 = ref lifeTime;
                        spawnNpcListen = keyValuePair.Value;
                        double duration = (double)spawnNpcListen.Duration;
                        local2.Duration = (float)duration;
                        lifeTime.EndAction = LifeTimeEndAction.Destroy;
                        LifeTime componentData = lifeTime;
                        local1.SetComponentData<LifeTime>(entity3, componentData);
                    }
                    Cache.spawnNPC_Listen.Remove(keyValuePair.Key);
                }
            }
        }
    }
}
*/