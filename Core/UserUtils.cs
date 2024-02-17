using System;
using System.Collections.Generic;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VRising.GameData.Models;
using VRising.GameData.Models.Internals;

namespace RPGAddOnsEx.Core
{
    public static class Extensions
    {
        public static List<PrefabGUID> deathSet = new List<PrefabGUID>
            {
                new PrefabGUID(1055898174), // Chest
                new PrefabGUID(1400688919), // Boots
                new PrefabGUID(125611165),  // Legs
                new PrefabGUID(-204401621),  // Gloves
            };

        public static List<PrefabGUID> noctumSet = new List<PrefabGUID>
            {
                new PrefabGUID(1076026390), // Chest
                new PrefabGUID(735487676), // Boots
                new PrefabGUID(-810609112),  // Legs
                new PrefabGUID(776192195),  // Gloves
            };

        public static List<T> GetBufferInternal<T>(this EntityManager entityManager, Entity entity) where T : new()
        {
            try
            {
                DynamicBuffer<T> buffer = entityManager.GetBuffer<T>(entity);
                List<T> list = new List<T>();
                NativeArray<T>.Enumerator enumerator = buffer.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    list.Add(current);
                }

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool HasComponentInternal<T>(this EntityManager entityManager, Entity entity)
        {
            try
            {
                return entityManager.HasComponent<T>(entity);
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGetComponentDataInternal<T>(this EntityManager entityManager, Entity entity, out T value) where T : new()
        {
            try
            {
                value = entityManager.GetComponentData<T>(entity);
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }

        public static T GetManagedComponentDataInternal<T>(this World world, BaseEntityModel entity) where T : class
        {
            PrefabGUID? prefabGUID = entity.PrefabGUID;
            if (!prefabGUID.HasValue)
            {
                return null;
            }

            ManagedDataRegistry managedDataRegistry = world.GetExistingSystem<GameDataSystem>().ManagedDataRegistry;
            return managedDataRegistry.GetOrDefault<T>(prefabGUID.Value);
        }
    }
}