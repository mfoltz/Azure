using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Math = System.Math;

namespace RPGAddOns
{
    [HarmonyPatch]
    internal class VBloodPointsHook
    {
        [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdate(VBloodSystem __instance)
        {
            // for whatever reason one vblood kill triggers 2 events so make a cooldown or something
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
                                if (Databases.playerRanks != null)
                                {
                                    if (Databases.playerRanks.TryGetValue(SteamID, out RankData data))
                                    {
                                        // this is where max points is derived and checked. level 0 max is 1000, level 1 max is 2000, etc
                                        if (data.Points < ((data.Rank * 1000) + 1000))
                                        {
                                            // calculate points, should probably make this a method
                                            data.Points += GetPoints(playerLevel, unitLevel);
                                            if (data.Points >= ((data.Rank * 1000) + 1000))
                                            {
                                                data.Points = ((data.Rank * 1000) + 1000);
                                            }
                                            Commands.SavePlayerRanks();
                                        }
                                    }
                                    else
                                    {
                                        // create new data then add points
                                        RankData rankData = new(0, GetPoints(playerLevel, unitLevel), []);
                                        Databases.playerRanks.Add(SteamID, rankData);
                                        Commands.SavePlayerRanks();
                                    }
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

        public static int GetPoints(int playerLevel, int unitLevel)
        {
            int delta = playerLevel - unitLevel;
            // base points equals 10 - delta.
            int points = 10 - delta;
            // what if player level 0 vblood level 80? cap points to 10 here
            if (points > 10)
            {
                points = 10;
            }
            // 5% chance per kill to get 10 extra points
            int chance = RandomUtil.GetRandomNumber(0, 100);
            if (chance <= 5)
            {
                points += 10;
            }
            // something like up to 5 extra points per kill based on player level scaled for 0->80
            float scale = (float)playerLevel / 80;
            int extra = (int)Math.Round(scale * 5);
            points += extra;
            // and maybe 1-5 extra points per kill at random weighted towards lower end to round out the mystique
            // 5 5%, 4 10%, 3 15%, 2 30%, 1 40%
            chance = RandomUtil.GetRandomNumber(0, 100);
            if (chance <= 5)
            {
                points += 5;
            }
            else if (chance <= 15)
            {
                points += 4;
            }
            else if (chance <= 30)
            {
                points += 3;
            }
            else if (chance <= 60)
            {
                points += 2;
            }
            else if (chance <= 100)
            {
                points += 1;
            }
            //I could probably make a cooldown timer or something but instead since there are two events happening Im just gonna divide the points by 2 and call it a day
            return (points / 2);
        }

        public class RandomUtil
        {
            private static readonly Random random = new();

            public static int GetRandomNumber(int min, int max)
            {
                return random.Next(min, max);
            }
        }
    }
}