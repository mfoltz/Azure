using Bloodstone.API;
using ProjectM;
using System.Reflection;
using Unity.Entities;
using RPGAddOnsEx.Core;

namespace RPGAddOnsEx.Augments
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

            List<PrefabGUID> darkMatterSet = new List<PrefabGUID>
            {
                new PrefabGUID(1055898174), // Chest
                new PrefabGUID(1400688919), // Boots
                new PrefabGUID(125611165),  // Legs
                new PrefabGUID(-204401621),  // Gloves
            };

            PrefabGUID setBonus = new(35317589); // Bloodmoon Set Bonus

            foreach (PrefabGUID prefabGUID in darkMatterSet)
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