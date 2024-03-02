using Bloodstone.API;
using ProjectM;
using Unity.Entities;
using VPlus.Core;
using VPlus.Core.Tools;

namespace VPlus.Augments
{
    public static class ArmorModifierSystem
    {
        public static void ModifyArmorPrefabEquipmentSet()
        {
            if (!Plugin.modifyDeathSetBonus)
            {
                return;
            }
            EntityManager entityManager = VWorld.Server.EntityManager;
            PrefabGUID setBonus = new(35317589); // Bloodmoon Set Bonus

            foreach (PrefabGUID prefabGUID in VPlus.Data.Kit.deathSet)
            {
                Entity armorEntity = GetPrefabEntityByPrefabGUID(prefabGUID, entityManager);

                if (armorEntity != Entity.Null)
                {
                    var equippableData = Utilities.GetComponentData<EquippableData>(armorEntity);
                    equippableData.EquipmentSet = setBonus;
                    Utilities.SetComponentData(armorEntity, equippableData);
                }
                else
                {
                    Plugin.Logger.LogInfo($"Could not find prefab entity for GUID: {prefabGUID}");
                }
            }
        }

        public static Entity GetPrefabEntityByPrefabGUID(PrefabGUID prefabGUID, EntityManager entityManager)
        {
            try
            {
                PrefabCollectionSystem prefabCollectionSystem = entityManager.World.GetExistingSystem<PrefabCollectionSystem>();
                if (prefabCollectionSystem._SpawnableNameToPrefabGuidDictionary.TryGetValue("HUDCanvas", out PrefabGUID canvas))
                {
                    Plugin.Logger.LogInfo($"Found HUDCanvas PrefabGUID: {canvas.GuidHash.ToString()}");
                }

                return prefabCollectionSystem._PrefabGuidToEntityMap[prefabGUID];
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error: {ex}");
                return Entity.Null;
            }
        }
    }
}