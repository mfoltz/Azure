using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using RPGAddOnsEx.Core;
using System;
using Unity.Collections;
using Unity.Entities;

#nullable disable
/*
namespace RPGModsAddOnsEx.Hooks
{
    [HarmonyPatch(typeof(ArmorLevelSystem_Spawn), "OnUpdate")]
    public class ArmorLevelSystem_Spawn_Patch
    {
        private static void Prefix(ArmorLevelSystem_Spawn __instance)
        {
            // going to assume this gets fired whenever armor is equipped/unequipped for a player (not exclusively for any nitpickers reading this :P)
// also no it only fires when equipping armor of course lol
            Plugin.Logger.LogInfo("ArmorLevelSystem_Spawn Prefix called...");
            EntityManager entityManager = __instance.EntityManager;
            NativeArray<Entity> entityArray = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entityArray)
            {
                Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                if (!entityManager.HasComponent<PlayerCharacter>(owner))
                {
                    return;
                }
                else
                {
                    Plugin.Logger.LogInfo("Found player...");

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
                            }
                        }
                    }
                }
            }
            entityArray.Dispose();
        }
    }
}
*/