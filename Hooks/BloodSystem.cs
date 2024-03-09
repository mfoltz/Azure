using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
using ProjectM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VBuild.Core;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;
/*
[HarmonyPatch(typeof(ModifyBloodDrainSystem_Spawn), nameof(ModifyBloodDrainSystem_Spawn.OnUpdate))]
public static class BloodSystem
{
    private static bool flag = false;

    public static void Prefix(ModifyBloodDrainSystem_Spawn __instance)
    {
        if (flag) return;
        Plugin.Logger.LogInfo("ModifyBloodDrainSystem_Spawn Prefix called...");
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        foreach (var entity in entities)
        {
            entity.LogComponentTypes();
            if (Utilities.HasComponent<EntityOwner>(entity))
            {
                EntityOwner entityOwner = Utilities.GetComponentData<EntityOwner>(entity);
                Entity owner = entityOwner.Owner;
                PlayerCharacter playerCharacter = owner.Read<PlayerCharacter>();
                string name = playerCharacter.Name.ToString();
                PlayerService.TryGetCharacterFromName(name, out var character);
                if (Utilities.HasComponent<Blood>(character))
                {
                    //Plugin.Logger.LogInfo("Blood component found...");
                    Blood blood = Utilities.GetComponentData<Blood>(character);
                    blood.MaxBlood._Value = new ModifiableFloat { Value = 10000 };
                    Plugin.Logger.LogInfo(blood.BloodType.GuidHash);
                    Plugin.Logger.LogInfo(blood.Quality);
                    Plugin.Logger.LogInfo(blood.Value);
                    Plugin.Logger.LogInfo(blood.MaxBlood.Value);
                    var buff = character.ReadBuffer<BloodQualityBuff>();
                    Plugin.Logger.LogInfo(buff[0].BloodQualityBuffPrefabGuid.GuidHash);
                    PrefabGUID prefabGUID = new PrefabGUID(20081801);
                    buff.Add(new BloodQualityBuff { BloodQualityBuffPrefabGuid = prefabGUID, BloodQualityBuffEntity = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID] });
                    //flag = true;
                }
            }
        }
        entities.Dispose();
    }
}
*/
[HarmonyPatch(typeof(RepairDoubleVBloodSpawnedSystem), nameof(RepairDoubleVBloodSpawnedSystem.OnUpdate))]
public static class RepairDoubleVBloodSpawnedSystemPatch
{
    public static bool Prefix(RepairDoubleVBloodSpawnedSystem __instance)
    {
        Plugin.Logger.LogInfo("RepairDoubleVBloodSpawnedSystem Prefix called...");
        return false;
    }
}
/*
[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public static class BuffSystem_Spawn_Server_Patch
{
    public static void Prefix(BuffSystem_Spawn_Server __instance)
    {
        Plugin.Logger.LogInfo("BuffSystem_Spawn_Server_Patch Prefix called...");
        NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            //entity.LogComponentTypes();
            Buff buff = entity.Read<Buff>();
            if (buff.Target.Has<Follower>())
            {
                if (buff.Target.Read<Follower>().Followed.Value.Equals(Entity.Null)) continue;


                PrefabGUID prefabGUID = entity.Read<PrefabGUID>();
                Plugin.Logger.LogInfo(prefabGUID.LookupName());
                if (prefabGUID.LookupName().ToLower().Contains("incombat"))
                {
                    //BloodShareBuff_ResetVBlood;
                    //VBloodUnitSpawnSource
                    //float3 charPos = buff.Target.Read<Follower>().Followed.Value.Read<LocalToWorld>().Position;
                }
            }
        }
        entities.Dispose();
    }
}
*/