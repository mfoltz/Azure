using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using VCreate.Core;
using VCreate.Core.Toolbox;
using VCreate.Systems;

namespace VCreate.Hooks
{
    /*
    internal class PetExperienceSystem
    {
        [HarmonyPatch]
        public class DeathEventListenerSystem_PetPatch
        {
            [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
            [HarmonyPostfix]
            public static void Postfix(DeathEventListenerSystem __instance)
            {
                NativeArray<Entity> entities = __instance._DeathEventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
                try
                {
                    var enumerator = entities.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.Has<DeathEvent>() || !enumerator.Current.Has<PlayerCharacter>()) continue;
                        DeathEvent deathEvent = enumerator.Current.Read<DeathEvent>();
                        Entity entity = deathEvent.Died;
                        // want to turn entity into experience for pet
                        UnitLevel unitLevel = entity.Read<UnitLevel>();
                        PlayerCharacter playerCharacter = enumerator.Current.Read<PlayerCharacter>();
                        if (DataStructures.PetExperience.TryGetValue(playerCharacter.UserEntity.Read<User>().PlatformId, out PetExperience petExperience))
                        {
                            petExperience.CurrentExperience += unitLevel.Level-petExperience.Level;
                            DataStructures.PetExperience[playerCharacter.UserEntity.Read<User>().PlatformId] = petExperience;
                            
                        }

                        
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
                finally
                {
                    entities.Dispose();
                }
            }
        }
    }
    */
}
