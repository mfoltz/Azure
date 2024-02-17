// Decompiled with JetBrains decompiler
// Type: RPGMods.Hooks.BuffSystem_Spawn_Server_Patch
// Assembly: RPGMods, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 7C19C4EB-25BF-44E7-A841-AA7E48DC0C83
// Assembly location: C:\Users\mitch\Downloads\RPGMods_3.dll

using HarmonyLib;
using ProjectM;
using RPGMods.Systems;
using Unity.Collections;
using Unity.Entities;

#nullable disable

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), "OnUpdate")]
    public class BuffSystem_Spawn_Server_Patch
    {
        public static bool buffLogging;

        private static void Prefix(BuffSystem_Spawn_Server __instance)
        {
            if (!PermissionSystem.isVIPSystem)
                return;
            foreach (Entity entity in __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp))
            {
                PrefabGUID componentData = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);
                if (PermissionSystem.isVIPSystem)
                    PermissionSystem.BuffReceiver(entity, componentData);
            }
        }

        private static void Postfix(BuffSystem_Spawn_Server __instance)
        {
            if (!WeaponMasterSystem.isMasteryEnabled)
                return;
            foreach (Entity entity in __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp))
            {
                EntityManager entityManager = __instance.EntityManager;
                if (entityManager.HasComponent<InCombatBuff>(entity))
                {
                    entityManager = __instance.EntityManager;
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    entityManager = __instance.EntityManager;
                    if (entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        entityManager = __instance.EntityManager;
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        if (WeaponMasterSystem.isMasteryEnabled)
                            WeaponMasterSystem.LoopMastery(userEntity, owner);
                    }
                }
            }
        }
    }
}