using Bloodstone.API;
using Il2CppSystem;
using ProjectM;
using ProjectM.Network;
using ProjectM.Tiles;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using WorldBuild.Core;
using WorldBuild.Core.Services;
using WorldBuild.Core.Toolbox;
using WorldBuild.Data;
using StringComparer = System.StringComparer;

namespace WorldBuild.BuildingSystem
{
    internal class TileSets
    {
        // can activate this by monitoring for ability player gets to use with shift key to place a tile at mouse location
        // use charm/siege interact T02 or something, monitor for abilitycast finishes that match the prefab and run this method
        public static void SpawnTileModel(Entity character)
        {
            Plugin.Logger.LogInfo("SpawnTileModel Triggered");
            if (Utilities.HasComponent<PlayerCharacter>(character))
            {
                PlayerCharacter player = Utilities.GetComponentData<PlayerCharacter>(character);
                string playerName = player.Name.ToString();
                PlayerService.TryGetUserFromName(playerName, out Entity userEntity);
                User user = Utilities.GetComponentData<User>(userEntity);
                ulong SteamId = user.PlatformId;
                if (Databases.playerBuildSettings.TryGetValue(SteamId, out BuildSettings data))
                {
                    PrefabGUID prefabGUID = new(data.TileModel);
                    Entity prefabEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>()._PrefabGuidToEntityMap[prefabGUID];
                    Entity tileEntity = VWorld.Server.EntityManager.Instantiate(prefabEntity);

                    Nullable_Unboxed<float3> aimPosition = new Nullable_Unboxed<float3>(userEntity.Read<EntityInput>().AimPosition);
                    Utilities.SetComponentData(tileEntity, new Translation { Value = aimPosition.Value });

                    int rotation = data.TileRotation;
                    float radians = math.radians(rotation);
                    quaternion rotationQuaternion = quaternion.EulerXYZ(new float3(0, radians, 0));
                    TileModel tileModel = tileEntity.Read<TileModel>();
                    Utilities.SetComponentData(tileEntity, new Rotation { Value = rotationQuaternion });
                    if (data.ImmortalTiles)
                    {
                        Utilities.AddComponentData(tileEntity, new Immortal { IsImmortal = true });
                    }
                    string message = $"Tile spawned at {aimPosition.value.xy} with rotation {data.TileRotation} degrees clockwise.";
                    data.LastTilePlaced = tileEntity.Index.ToString() + ", " + tileEntity.Version.ToString();
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                }
                else
                {
                    string message = "Couldn't find your build preferences, try again after setting them.";
                    ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, message);
                }
            }
        }

        public class TileConstructor(string name, int tilePrefabHash)
        {
            public string Name { get; set; } = name;
            public int TileGUID { get; set; } = tilePrefabHash;
        }

        public class Building
        {
            public static Dictionary<int, TileConstructor> StaticTiles { get; private set; }

            static Building()
            {
                StaticTiles = new Dictionary<int, TileConstructor>
                {
                    { 17, new TileConstructor("Dynamic_Bandit_SmallTent02", WorldBuild.Data.Prefabs.Dynamic_Bandit_SmallTent02.GuidHash) },
                    { 16, new TileConstructor("TM_WorldChest_Epic_01_Full", WorldBuild.Data.Prefabs.TM_WorldChest_Epic_01_Full.GuidHash) },
                    { 15, new TileConstructor("TM_Castle_Floor_Garden_Grass01", WorldBuild.Data.Prefabs.TM_Castle_Floor_Garden_Grass01.GuidHash) },
                    { 14, new TileConstructor("TM_Castle_House_Pillar_Forge01", WorldBuild.Data.Prefabs.TM_Castle_House_Pillar_Forge01.GuidHash) },
                    { 13, new TileConstructor("TM_ForgeMaster_Weaponrack01", WorldBuild.Data.Prefabs.TM_ForgeMaster_Weaponrack01.GuidHash) },
                    { 12, new TileConstructor("TM_Fortressoflight_Brazier01", WorldBuild.Data.Prefabs.TM_Fortressoflight_Brazier01.GuidHash) },
                    { 11, new TileConstructor("TM_Castle_Wall_Tier02_Stone_Entrance", WorldBuild.Data.Prefabs.TM_Castle_Wall_Tier02_Stone_Entrance.GuidHash) },
                    { 10, new TileConstructor("TM_Castle_Floor_Foundation_Stone01", WorldBuild.Data.Prefabs.TM_Castle_Floor_Foundation_Stone01.GuidHash) },
                    { 9, new TileConstructor("TM_Castle_Wall_Tier02_Stone", WorldBuild.Data.Prefabs.TM_Castle_Wall_Tier02_Stone.GuidHash) },
                    { 8, new TileConstructor("TM_CraftingStation_MetalworkStation", WorldBuild.Data.Prefabs.TM_CraftingStation_MetalworkStation.GuidHash) },
                    { 7, new TileConstructor("TM_CraftingStation_BloodBank", WorldBuild.Data.Prefabs.TM_CraftingStation_BloodBank.GuidHash) },
                    { 6, new TileConstructor("TM_CraftingStation_ArtisansCorner", WorldBuild.Data.Prefabs.TM_CraftingStation_ArtisansCorner.GuidHash) },
                    { 5, new TileConstructor("TM_SpecialStation_StablePen", WorldBuild.Data.Prefabs.TM_SpecialStation_StablePen.GuidHash) },
                    { 4, new TileConstructor("TM_CraftingStation_Altar_Frost", WorldBuild.Data.Prefabs.TM_CraftingStation_Altar_Frost.GuidHash) },
                    { 3, new TileConstructor("TM_CraftingStation_Altar_Unholy", WorldBuild.Data.Prefabs.TM_CraftingStation_Altar_Unholy.GuidHash) },
                    { 2, new TileConstructor("TM_CraftingStation_Altar_Spectral", WorldBuild.Data.Prefabs.TM_CraftingStation_Altar_Spectral.GuidHash) },
                    { 1, new TileConstructor("TM_Workstation_Waypoint_World_UnlockedFromStart", WorldBuild.Data.Prefabs.TM_Workstation_Waypoint_World_UnlockedFromStart.GuidHash) }
                };
            }

            public Dictionary<int, TileConstructor> Tiles { get; private set; }

            public Building()
            {
                Tiles = StaticTiles;
            }
        }

        public static Dictionary<int, TileConstructor> GetTilesBySet(string setName)
        {
            if (ModelRegistry.tilesBySet.TryGetValue(setName, out var setTiles))
            {
                return setTiles.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            return null;
        }

        public static readonly HashSet<string> adminSets = new(StringComparer.OrdinalIgnoreCase)
        {
            "building",
        };

        public static class ModelRegistry
        {
            public static readonly Dictionary<string, Dictionary<int, TileConstructor>> tilesBySet = new(StringComparer.OrdinalIgnoreCase);

            static ModelRegistry()
            {
                RegisterTiles("Building", new Dictionary<int, TileConstructor>
                {
                    { 15, new TileConstructor("TM_Castle_Floor_Garden_Grass01", WorldBuild.Data.Prefabs.TM_Castle_Floor_Garden_Grass01.GuidHash) },
                    { 14, new TileConstructor("TM_Castle_House_Pillar_Forge01", WorldBuild.Data.Prefabs.TM_Castle_House_Pillar_Forge01.GuidHash) },
                    { 13, new TileConstructor("TM_ForgeMaster_Weaponrack01", WorldBuild.Data.Prefabs.TM_ForgeMaster_Weaponrack01.GuidHash) },
                    { 12, new TileConstructor("TM_Fortressoflight_Brazier01", WorldBuild.Data.Prefabs.TM_Fortressoflight_Brazier01.GuidHash) },
                    { 11, new TileConstructor("TM_Castle_Wall_Tier02_Stone_Entrance", WorldBuild.Data.Prefabs.TM_Castle_Wall_Tier02_Stone_Entrance.GuidHash) },
                    { 10, new TileConstructor("TM_Castle_Floor_Foundation_Stone01", WorldBuild.Data.Prefabs.TM_Castle_Floor_Foundation_Stone01.GuidHash) },
                    { 9, new TileConstructor("TM_Castle_Wall_Tier02_Stone", WorldBuild.Data.Prefabs.TM_Castle_Wall_Tier02_Stone.GuidHash) },
                    { 8, new TileConstructor("TM_CraftingStation_MetalworkStation", WorldBuild.Data.Prefabs.TM_CraftingStation_MetalworkStation.GuidHash) },
                    { 7, new TileConstructor("TM_CraftingStation_BloodBank", WorldBuild.Data.Prefabs.TM_CraftingStation_BloodBank.GuidHash) },
                    { 6, new TileConstructor("TM_CraftingStation_ArtisansCorner", WorldBuild.Data.Prefabs.TM_CraftingStation_ArtisansCorner.GuidHash) },
                    { 5, new TileConstructor("TM_SpecialStation_StablePen", WorldBuild.Data.Prefabs.TM_SpecialStation_StablePen.GuidHash) },
                    { 4, new TileConstructor("TM_CraftingStation_Altar_Frost", WorldBuild.Data.Prefabs.TM_CraftingStation_Altar_Frost.GuidHash) },
                    { 3, new TileConstructor("TM_CraftingStation_Altar_Unholy", WorldBuild.Data.Prefabs.TM_CraftingStation_Altar_Unholy.GuidHash) },
                    { 2, new TileConstructor("TM_CraftingStation_Altar_Spectral", WorldBuild.Data.Prefabs.TM_CraftingStation_Altar_Spectral.GuidHash) },
                    { 1, new TileConstructor("TM_Workstation_Waypoint_World_UnlockedFromStart", WorldBuild.Data.Prefabs.TM_Workstation_Waypoint_World_UnlockedFromStart.GuidHash) }
                });
            }

            public static void RegisterTiles(string setName, Dictionary<int, TileConstructor> tiles)
            {
                tilesBySet[setName] = tiles;
            }

            private static Dictionary<int, TileConstructor> GetTilesFromSetChoice(string setChoice)
            {
                // This method should return the appropriate tiles dictionary based on the set choice
                // Example:
                return setChoice switch
                {
                    "building" => Building.StaticTiles,
                    _ => null,
                };
            }
        }
    }
}