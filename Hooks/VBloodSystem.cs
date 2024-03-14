using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using VPlus.Augments.Rank;
using VPlus.Core;
using VPlus.Core.Commands;
using VPlus.Core.Toolbox;
using VPlus.Data;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using Math = System.Math;
using Random = System.Random;

namespace VPlus.Hooks
{
    [HarmonyPatch]
    internal class VBloodSystemPatch
    {
        private static int counter = 0;

        [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdate(ProjectM.VBloodSystem __instance)
        {
            if (!__instance.EventList.IsEmpty)
            {
                var check = __instance.EventList.Length.ToString();

                EntityManager entityManager = VWorld.Server.EntityManager;

                foreach (var _event in __instance.EventList)
                {
                    if (!VWorld.Server.EntityManager.TryGetComponentData(_event.Target, out PlayerCharacter playerData)) continue;

                    Entity _vblood = __instance._PrefabCollectionSystem._PrefabGuidToEntityMap[_event.Source];
                    string vBloodName = __instance._PrefabCollectionSystem._PrefabDataLookup[_event.Source].AssetName.ToString();

                    string playerName = playerData.Name.ToString();
                    Entity user = playerData.UserEntity;

                    try
                    {
                        UserModel usermodel = GameData.Users.GetUserByCharacterName(playerName);
                        Entity characterEntity = usermodel.FromCharacter.Character;
                        float3 playerPosition = usermodel.Position;

                        if (vBloodName == "CHAR_ChurchOfLight_Paladin_VBlood")
                        {
                            if (Plugin.shardDrop)
                            {
                                Plugin.Logger.LogInfo($"Attempting to add shard to player inventory"); // Log details about each event

                                PrefabGUID shard = VCreate.Data.Prefabs.Item_Building_Relic_Paladin;
                                if (InventoryUtilities.TryGetInventoryEntity(entityManager, characterEntity, out Entity inventoryEntity))
                                {
                                    InventoryUtilitiesServer.TryRemoveItem(entityManager, inventoryEntity, shard, 1);
                                }
                                AddItemToInventory(shard, 1, usermodel);
                            }
                        }
                        if (entityManager.TryGetComponentData(user, out User component))
                        {
                            ulong SteamID = component.PlatformId;
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
                                        if (data.Points < data.Rank * 1000 + 1000)
                                        {
                                            // calculate points, should probably make this a method
                                            data.Points += GetPoints(playerLevel, unitLevel, component);
                                            if (data.Points >= data.Rank * 1000 + 1000)
                                            {
                                                data.Points = data.Rank * 1000 + 1000;
                                            }
                                            ChatCommands.SavePlayerRanks();
                                        }
                                    }
                                    else
                                    {
                                        // create new data then add points
                                        RankData rankData = new(0, GetPoints(playerLevel, unitLevel, component), [], 0, [0, 0], "none", false);
                                        Databases.playerRanks.Add(SteamID, rankData);
                                        ChatCommands.SavePlayerRanks();
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

        public static int GetPoints(int playerLevel, int unitLevel, User user)
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
            if (Plugin.rankPointsModifier)
            {
                // multiply points gained
                points *= Plugin.rankPointsFactor;
            }
            else
            {
                // divide points gained
                points /= Plugin.rankPointsFactor;
            }
            // message player points earned
            EntityManager entityManager = VWorld.Server.EntityManager;
            if (counter == 0)
            {
                counter += points;
                return points;
            }
            else
            {
                counter += points;
                string counterString = counter.ToString();
                var colorString = FontColors.White(counterString);
                string toSend = "You've earned " + colorString + " rank points!";
                ServerChatUtils.SendSystemMessageToClient(entityManager, user, toSend);
                counter = 0;
                return points;
            }
        }

        public static void AddItemToInventory(PrefabGUID guid, int amount, UserModel user)
        {
            unsafe
            {
                user.TryGiveItem(guid, 1, out Entity itemEntity);
                return;
            }
        }

        public class RandomUtil
        {
            private static readonly Random random = new();

            public static int GetRandomNumber(int min, int max)
            {
                return random.Next(min, max);
            }
        }

        public class PositionChecker
        {
            public static bool IsWithinArea(float3 position, float3 corner1, float3 corner2, float3 corner3, float3 corner4)
            {
                float minX = math.min(math.min(corner1.x, corner2.x), math.min(corner3.x, corner4.x));
                float maxX = math.max(math.max(corner1.x, corner2.x), math.max(corner3.x, corner4.x));
                float minZ = math.min(math.min(corner1.z, corner2.z), math.min(corner3.z, corner4.z));
                float maxZ = math.max(math.max(corner1.z, corner2.z), math.max(corner3.z, corner4.z));

                return position.x >= minX && position.x <= maxX && position.z >= minZ && position.z <= maxZ;
            }
        }

    }
}