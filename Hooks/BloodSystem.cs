using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Shared.Systems;
using ProjectM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using VBuild.Core;
using VBuild.Core.Services;
using VBuild.Core.Toolbox;

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
                    Plugin.Logger.LogInfo(blood.BloodType.GuidHash);
                    Plugin.Logger.LogInfo(blood.Quality);
                    Plugin.Logger.LogInfo(blood.Value);
                    Plugin.Logger.LogInfo(blood.MaxBlood);
                    //var buff = character.ReadBuffer<BloodQualityBuff>();
                    //PrefabGUID prefabGUID = new PrefabGUID(20081801);
                    //buff.Add(new BloodQualityBuff { BloodQualityBuffPrefabGuid = prefabGUID, BloodQualityBuffEntity = Helper.prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID] });
                    //flag = true;
                    
                }
            }
        }
        entities.Dispose();
    }
}
