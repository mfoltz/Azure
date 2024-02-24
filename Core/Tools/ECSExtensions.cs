
using System;
using System.Runtime.InteropServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProjectM;
using Unity;
using Unity.Collections;
using Unity.Entities;
using VRising.GameData.Models.Internals;

namespace V.Core.Tools;

public static class ECSExtensions
{
    public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
    {
        ComponentType componentType = new ComponentType(Il2CppType.Of<T>());
        byte[] array = StructureToByteArray(componentData);
        int size = Marshal.SizeOf<T>();
        fixed (byte* data = array)
        {
            VWorld.Server.EntityManager.SetComponentDataRaw(entity, componentType.TypeIndex, data, size);
        }
    }

    public static byte[] StructureToByteArray<T>(T structure) where T : struct
    {
        int num = Marshal.SizeOf(structure);
        byte[] array = new byte[num];
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        Marshal.StructureToPtr(structure, intPtr, fDeleteOld: true);
        Marshal.Copy(intPtr, array, 0, num);
        Marshal.FreeHGlobal(intPtr);
        return array;
    }

    public unsafe static T Read<T>(this Entity entity) where T : struct
    {
        ComponentType componentType = new ComponentType(Il2CppType.Of<T>());
        void* componentDataRawRO = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, componentType.TypeIndex);
        return Marshal.PtrToStructure<T>(new IntPtr(componentDataRawRO));
    }

    public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct
    {
        return VWorld.Server.EntityManager.GetBuffer<T>(entity);
    }

    public static void Add<T>(this Entity entity)
    {
        ComponentType componentType = new ComponentType(Il2CppType.Of<T>());
        VWorld.Server.EntityManager.AddComponent(entity, componentType);
    }

    public static void Remove<T>(this Entity entity)
    {
        ComponentType componentType = new ComponentType(Il2CppType.Of<T>());
        VWorld.Server.EntityManager.RemoveComponent(entity, componentType);
    }

    public static bool Has<T>(this Entity entity)
    {
        ComponentType type = new ComponentType(Il2CppType.Of<T>());
        return VWorld.Server.EntityManager.HasComponent(entity, type);
    }

    public static void LogComponentTypes(this Entity entity)
    {
        NativeArray<ComponentType>.Enumerator enumerator = VWorld.Server.EntityManager.GetComponentTypes(entity).GetEnumerator();
        while (enumerator.MoveNext())
        {
            ComponentType current = enumerator.Current;
            Debug.Log($"{current}");
        }

        Debug.Log("===");
    }

    public static void LogComponentTypes(this EntityQuery entityQuery)
    {
        Il2CppStructArray<ComponentType> queryTypes = entityQuery.GetQueryTypes();
        foreach (ComponentType item in queryTypes)
        {
            Debug.Log($"Query Component Type: {item}");
        }

        Debug.Log("===");
    }

    public static string LookupName(this PrefabGUID prefabGuid)
    {
        PrefabCollectionSystem existingSystem = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>();
        object obj;
        if (!existingSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGuid))
        {
            obj = "GUID Not Found";
        }
        else
        {
            string text = existingSystem.PrefabGuidToNameDictionary[prefabGuid];
            PrefabGUID prefabGUID = prefabGuid;
            obj = text + " " + prefabGUID.ToString();
        }

        return obj.ToString();
    }
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
            value = default;
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
