using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Analytics;
using WorldBuild.BuildingSystem;
using WorldBuild.Core;
using WorldBuild.Data;

namespace WorldBuild.Hooks
{

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
                        continue; // Use continue instead of return to proceed with the next entity
                    }

                    Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
                    User user = entityManager.GetComponentData<User>(userEntity);

                    if (Databases.playerBuildSettings.TryGetValue(user.PlatformId, out BuildSettings settings) && settings.BuildMode)
                    {
                        DynamicBuffer<ReplaceAbilityOnSlotBuff> buffer = entityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(entity);
                        if (buffer[0].NewGroupId == WorldBuild.Data.Prefabs.AB_Vampire_Unarmed_Primary_MeleeAttack_AbilityGroup)
                        {
                            // Assuming you want to modify abilities when in build mode without checking the initial ability
                            PrefabGUID spell1 = WorldBuild.Data.Prefabs.AB_Consumable_Tech_Ability_Charm_Level02_AbilityGroup; // Assigning build ability
                            PrefabGUID spell2 = WorldBuild.Data.Prefabs.AB_Debug_NukeAll_Group; // Assigning nuke ability

                            // Replacing or adding abilities directly without checking buffer length
                            ReplaceAbilityOnSlotBuff buildAbility = new ReplaceAbilityOnSlotBuff { Slot = 1, NewGroupId = spell1 };
                            ReplaceAbilityOnSlotBuff nukeAbility = new ReplaceAbilityOnSlotBuff { Slot = 4, NewGroupId = spell2 };

                            //buffer.Clear(); // Clear the buffer if you want to reset abilities
                            buffer.Add(buildAbility);
                            buffer.Add(nukeAbility);
                            Plugin.Logger.LogInfo("Modification complete.");
                        }
                        else
                        {
                            continue;
                        }
                        
                    }
                }
                entityArray.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogInfo($"Error in ReplaceAbilityOnSlotSystem Prefix: {ex.Message}");
            }
        }
    }
}