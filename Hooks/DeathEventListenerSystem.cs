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
                        Plugin.Log.LogInfo("DeathEventListenerSystem_PetPatch Postfix called...");
                        enumerator.Current.LogComponentTypes();
                        /*
                        if (!enumerator.Current.Has<DeathEvent>() || !enumerator.Current.Has<PlayerCharacter>()) continue;
                        Plugin.Log.LogInfo("DeathEvent involving player detected..."); 
                        if (!enumerator.Current.Has<FollowerBuffer>()) continue; // only want to do this if player has a pet
                        Plugin.Log.LogInfo("FollowerBuffer detected on player..."); // also want to make sure player has a pet and not a charmed human
                        foreach (var follower in enumerator.Current.ReadBuffer<FollowerBuffer>())
                        {
                            DeathEvent
                        }
                        DeathEvent deathEvent = enumerator.Current.Read<DeathEvent>();
                        Entity entity = deathEvent.Died;
                        // want to turn entity into experience for pet
                        UnitLevel unitLevel = entity.Read<UnitLevel>();
                        PlayerCharacter playerCharacter = enumerator.Current.Read<PlayerCharacter>();
                        if (DataStructures.PetExperience.TryGetValue(playerCharacter.UserEntity.Read<User>().PlatformId, out PetExperience petExperience))
                        {
                            petExperience.CurrentExperience += unitLevel.Level-petExperience.Level;
                            DataStructures.PetExperience[playerCharacter.UserEntity.Read<User>().PlatformId] = petExperience;
                            DataStructures.SavePetExperience();
                            
                            //get follower buffer follower and make sure it is not charmed in case player has a charmed human with them


                        }
                        */

                        
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
    
}
