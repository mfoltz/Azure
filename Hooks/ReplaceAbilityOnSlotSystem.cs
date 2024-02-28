using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using WorldBuild.BuildingSystem;
using WorldBuild.Core;
using WorldBuild.Data;

namespace WorldBuild.Hooks
{
    // want to redo this to put charm T02 on shift

    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), "OnUpdate")]
    public class ReplaceAbilityOnSlotSystem_Patch
    {
        private static void Prefix(ReplaceAbilityOnSlotSystem __instance)
        {
            try
            {
                EntityManager entityManager = VWorld.Server.EntityManager;
                NativeArray<Entity> entityArray = __instance.__Spawn_entityQuery.ToEntityArray(Allocator.Temp);
                Plugin.Logger.LogInfo("ReplaceAbilityOnSlotSystem Prefix called...");
                foreach (Entity entity in entityArray)
                {
                    Entity owner = entityManager.GetComponentData<EntityOwner>(entity).Owner;
                    if (!entityManager.HasComponent<PlayerCharacter>(owner))
                    {
                        return;
                    }
                    else
                    {
                        Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                        User user = entityManager.GetComponentData<User>(userEntity);
                        // only continue if buildMode is enabled
                        if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings))
                        {
                            if (settings.BuildMode)
                            {
                                // allow
                                if (entityManager.HasComponent<WeaponLevel>(entity))
                                {
                                    DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);

                                    Plugin.Logger.LogInfo($"Player equipping/unequipping weapon, adding build trigger spell to shift...");
                                    ReplaceAbilityOnSlotBuff item = buffer[2];
                                    ReplaceAbilityOnSlotBuff newItem = item;

                                    PrefabGUID prefabGUID = WorldBuild.Data.Prefabs.AB_Interact_Siege_Structure_T02_AbilityGroup;

                                    newItem.Slot = 3;
                                    newItem.NewGroupId = prefabGUID;
                                    var newNewItem = newItem;
                                    newNewItem.NewGroupId = WorldBuild.Data.Prefabs.AllowJumpFromCliffsBuff;
                                    // apparently both can be on the slot at once, neat
                                    buffer.Add(newNewItem);

                                    // cliff jump
                                    buffer.Add(newItem);
                                    Plugin.Logger.LogInfo("Modification complete.");
                                    return;
                                }
                            }
                            else
                            {
                                // want to return it to just cliffjump here if not in build mode
                                if (entityManager.HasComponent<WeaponLevel>(entity))
                                {
                                    DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);

                                    Plugin.Logger.LogInfo($"Player equipping/unequipping weapon, removing build trigger spell from shift...");
                                    ReplaceAbilityOnSlotBuff item = buffer[2];
                                    ReplaceAbilityOnSlotBuff newItem = item;
                                    newItem.Slot = 3;
                                    newItem.NewGroupId = WorldBuild.Data.Prefabs.AllowJumpFromCliffsBuff;
                                    buffer.Add(newItem);

                                    // cliff jump
                                    Plugin.Logger.LogInfo("Modification complete.");
                                }
                                return;
                            }
                        }
                    }
                }
                entityArray.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogInfo(ex.Message);
            }
        }
    }
}