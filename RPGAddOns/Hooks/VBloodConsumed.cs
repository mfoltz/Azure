﻿using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGAddOns.Core;
using RPGAddOns.VeinModules;
using Unity.Entities;
using Unity.Mathematics;
using VRising.GameData;
using VRising.GameData.Methods;
using VRising.GameData.Models;
using WillisCore;
using Math = System.Math;
using Random = System.Random;

namespace RPGAddOns.Hooks
{
    [HarmonyPatch]
    internal class VBloodConsumed
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
                    if (!VWorld.Server.EntityManager.TryGetComponentData(_event.Target, out PlayerCharacter playerData)) continue;

                    // there were 2 events from 1 kill, what does this imply?
                    Plugin.Logger.LogInfo($"Processing event: {_event}"); // Log details about each event

                    //EntityQuery query = __instance.__ConsumeBloodJob_entityQuery; //this seems to be the player entity as it did not have a unit level component
                    //NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);

                    Entity _vblood = __instance._PrefabCollectionSystem._PrefabGuidToEntityMap[_event.Source];
                    string vBloodName = __instance._PrefabCollectionSystem._PrefabDataLookup[_event.Source].AssetName.ToString();
                    Plugin.Logger.LogInfo($"VBlood name format: {vBloodName}"); // Log details about each event

                    string playerName = playerData.Name.ToString();
                    Entity user = playerData.UserEntity;
                    //var testWar = user.Read<ProjectM.UI.GoToHUDMenu>();

                    try
                    {
                        // need to check for hypothetical players in the ascension locations as well as if they have the mats required, should be kinda easy in this context? famous last words...
                        // need to define zones elsewhere or here, who cares really
                        // need to check for mats per level
                        // need to check for appropriate vblood kill
                        // then ascend player?
                        // should get coordinates and extrapolate to a map if possible
                        //1365358996 prefab for li9ghtning strike on ascension, looks neat
                        UserModel usermodel = GameData.Users.GetUserByCharacterName(playerName);
                        Entity characterEntity = usermodel.FromCharacter.Character;
                        float3 playerPosition = usermodel.Position;
                        // ascension location number 1

                        // check if player is inside these bounds with LOGIC and SCIENCE
                        if (vBloodName == "CHAR_Manticore_VBlood")
                        {
                            //check player positions

                            float3 divineLocation1NWCorner = new(-1397.987f, 20f, -1221.586f);
                            float3 divineLocation1SWCorner = new(-1386.987f, 20.48779f, -1221.781f);
                            float3 divineLocation1NECorner = new(-1398.22f, 20.56775f, -1214.962f);
                            float3 divineLocation1SECorner = new(-1386.954f, 20.0773f, -1214.544f);
                            var usersEnum = GameData.Users.Online;
                            var usersList = GameData.Users.Online.ToList();
                            for (int i = 0; i < usersList.Count; i++)
                            {
                                var online = usersList[i];
                                var playerPosition1 = online.Position;
                                if (PositionChecker.IsWithinArea(playerPosition1, divineLocation1NWCorner, divineLocation1SWCorner, divineLocation1NECorner, divineLocation1SECorner))
                                {
                                    //check for mats
                                    //check for ascension level
                                    //ascend player
                                    //add buff
                                    //add points
                                    //save data
                                    //send message
                                    //return
                                }
                            }
                        }
                        if (vBloodName == "CHAR_Cursed_MountainBeast_VBlood")
                        {
                            //check player positions
                        }
                        if (vBloodName == "CHAR_Gloomrot_Monster_VBlood")
                        {
                            //check player positions
                        }

                        //
                        // so what all do I need to define a zone... wonder if it's easier to make a circle around a point with a radius or 4 points for a square
                        if (vBloodName == "CHAR_ChurchOfLight_Paladin_VBlood")
                        {
                            //add solarus shard to player inventory
                            Plugin.Logger.LogInfo($"Attempting to add shard to player inventory"); // Log details about each event

                            PrefabGUID shard = AdminCommands.Data.Prefabs.Item_Building_Relic_Paladin;
                            // sure I should properly fix the vblood 2 for 1 kill event thing orrrr I coould just keep doing simple bandaid fixes like this
                            if (InventoryUtilities.TryGetInventoryEntity(entityManager, characterEntity, out Entity inventoryEntity))
                            {
                                InventoryUtilitiesServer.TryRemoveItem(entityManager, inventoryEntity, shard, 1);
                            }
                            AddItemToInventory(shard, 1, usermodel);
                        }
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
                                if (DataStructures.playerRanks != null)
                                {
                                    if (DataStructures.playerRanks.TryGetValue(SteamID, out RankData data))
                                    {
                                        // this is where max points is derived and checked. level 0 max is 1000, level 1 max is 2000, etc
                                        if (data.Points < data.Rank * 1000 + 1000)
                                        {
                                            // calculate points, should probably make this a method
                                            data.Points += GetPoints(playerLevel, unitLevel);
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
                                        RankData rankData = new(0, GetPoints(playerLevel, unitLevel), []);
                                        DataStructures.playerRanks.Add(SteamID, rankData);
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
            return points / 2;
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

        // Usage
    }
}