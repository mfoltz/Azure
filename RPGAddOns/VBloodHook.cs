using Bloodstone.API;
using HarmonyLib;
using MS.Internal.Xml.XPath;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace RPGAddOns
{
    [HarmonyPatch]
    internal class VBloodHook
    {
        [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdate(VBloodSystem __instance)
        {
            if (!__instance.EventList.IsEmpty)
            {
                var check = __instance.EventList.Length.ToString();
                //Plugin.Logger.LogInfo($"EventList events: {check}"); // Log details about each event

                //EntityManager entityManager = __instance.EntityManager;
                EntityManager entityManager = VWorld.Server.EntityManager;
                foreach (var _event in __instance.EventList)
                {
                    if (!VWorld.Server.EntityManager.TryGetComponentData<PlayerCharacter>(_event.Target, out PlayerCharacter playerData)) continue;

                    // there were 2 events from 1 kill, what does this imply?
                    Plugin.Logger.LogInfo($"Processing event: {_event}"); // Log details about each event

                    //EntityQuery query = __instance.__ConsumeBloodJob_entityQuery; //this seems to be the player entity as it did not have a unit level component
                    //NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);

                    Entity _vblood = __instance._PrefabCollectionSystem._PrefabGuidToEntityMap[_event.Source];
                    string playerName = playerData.Name.ToString();
                    Entity user = playerData.UserEntity;

                    try
                    {
                        if (entityManager.TryGetComponentData(user, out User component))
                        {
                            ulong SteamID = component.PlatformId;
                            //Plugin.Logger.LogInfo($"SteamID: {SteamID}"); // Log details about each event
                            //Plugin.Logger.LogInfo($"Player Level: {RPGMods.Systems.ExperienceSystem.getLevel(SteamID)}"); // Log details about each event
                            //Plugin.Logger.LogInfo($"Unit Level: {entityManager.GetComponentData<UnitLevel>(_vblood).Level}"); // Log details about each event
                            int playerLevel = RPGMods.Systems.ExperienceSystem.getLevel(SteamID);
                            int unitLevel = entityManager.GetComponentData<UnitLevel>(_vblood).Level;
                            int delta = playerLevel - unitLevel;
                            if (delta > 10)
                            {
                                return;
                            }
                            else
                            {
                                // check for database existence just in case. if it exists, and the player key can be found, check for points < max points before adding points. if not, create new database and add points
                                if (Databases.playerRank != null)
                                {
                                    if (Databases.playerRank.TryGetValue(SteamID, out ResetData data))
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogError($"Error: {e}");
                    }
                }
            }
        }
    }
}