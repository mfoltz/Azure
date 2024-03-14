using Bloodstone.API;
using Il2CppInterop.Runtime;
using System.Runtime.InteropServices;
using Unity.Entities;
namespace VPlus.Core.Toolbox
{
    public static class Utilities
    {
        public static Il2CppSystem.Type Il2CppTypeGet(Type type)
        {
            return Il2CppSystem.Type.GetType(type.ToString());
        }

        public static ComponentType ComponentTypeGet(string component)
        {
            return ComponentType.ReadOnly(Il2CppSystem.Type.GetType(component));
        }

        // alternative for Entitymanager.HasComponent
        public static bool HasComponent<T>(Entity entity) where T : struct
        {
            return VWorld.Server.EntityManager.HasComponent(entity, ComponentTypeOther<T>());
        }

        // more convenient than Entitymanager.AddComponent
        public static bool AddComponent<T>(Entity entity) where T : struct
        {
            return VWorld.Server.EntityManager.AddComponent(entity, ComponentTypeOther<T>());
        }

        // alternative for Entitymanager.AddComponentData
        public static void AddComponentData<T>(Entity entity, T componentData) where T : struct
        {
            AddComponent<T>(entity);
            SetComponentData(entity, componentData);
        }

        // alternative for Entitymanager.RemoveComponent
        public static bool RemoveComponent<T>(Entity entity) where T : struct
        {
            return VWorld.Server.EntityManager.RemoveComponent(entity, ComponentTypeOther<T>());
        }

        // alternative for EntityMManager.GetComponentData
        public static unsafe T GetComponentData<T>(Entity entity) where T : struct
        {
            void* rawPointer = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, ComponentTypeIndex<T>());
            return Marshal.PtrToStructure<T>(new IntPtr(rawPointer));
        }

        // alternative for EntityManager.SetComponentData
        public static unsafe void SetComponentData<T>(Entity entity, T componentData) where T : struct
        {
            var size = Marshal.SizeOf(componentData);
            //byte[] byteArray = new byte[size];
            var byteArray = StructureToByteArray(componentData);
            fixed (byte* data = byteArray)
            {
                //UnsafeUtility.CopyStructureToPtr(ref componentData, data);
                VWorld.Server.EntityManager.SetComponentDataRaw(entity, ComponentTypeIndex<T>(), data, size);
            }
        }

        private static ComponentType ComponentTypeOther<T>()
        {
            return new ComponentType(Il2CppType.Of<T>());
        }

        private static int ComponentTypeIndex<T>()
        {
            return ComponentTypeOther<T>().TypeIndex;
        }

        private static byte[] StructureToByteArray<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            byte[] byteArray = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, true);
                Marshal.Copy(ptr, byteArray, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return byteArray;
        }
    }



  
}