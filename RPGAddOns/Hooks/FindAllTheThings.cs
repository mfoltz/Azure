using HarmonyLib;
using ProjectM;
using RPGAddOns;
using Stunlock.Network;
using System.Collections;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static OnUserConnectedManagerOld.EntityWrapper;
using BindingFlags = System.Reflection.BindingFlags;
using FieldInfo = System.Reflection.FieldInfo;
using MethodInfo = System.Reflection.MethodInfo;
using PropertyInfo = System.Reflection.PropertyInfo;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public class OnUserConnectedManagerOld
{
    private static Dictionary<Type, Func<UnityEngine.Object, UnityEngine.Object>> castMethods = new Dictionary<Type, Func<UnityEngine.Object, UnityEngine.Object>>();
    private static GenericObjectProcessor objectProcessor = new GenericObjectProcessor();
    private static bool castMethodsInitialized = false;
    private static HashSet<Type> uniqueTypesCache;

    //private static Dictionary<Type, List<UnityEngine.Object>> objectsByType = new Dictionary<Type, List<UnityEngine.Object>>();
    public static Dictionary<string, List<object>> objectsByType = new Dictionary<string, List<object>>();

    private static Dictionary<string, int> typeDeterminationStats = new Dictionary<string, int>();

    [HarmonyPostfix]
    public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
    {
        /*
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Plugin.Logger.LogInfo("Finding all Objects...");
        var serverObjects = OnUserConnectedManager.ObjectFinder.FindAllObjects(__instance);
        Plugin.Logger.LogInfo("Getting all Types of Objects...");
        uniqueTypesCache = ObjectFinder.GetObjectClasses(serverObjects);
        InitializeCastMethods(uniqueTypesCache);

        foreach (var obj in serverObjects)
        {
            if (obj != null)
            {
                try
                {
                    objectProcessor.Process(obj);
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Error processing object {obj.name}: {ex.Message}");
                }
            }
        }

        ObjectFinder.AnalyzeTypeDeterminationStats();
        stopwatch.Stop();

        Plugin.Logger.LogInfo($"Processing time: {stopwatch.Elapsed} seconds");
        */
    }

    private void ProcessGameObject(GameObject gameObject)
    {
        // Example processing for GameObject
        string components = String.Join(", ", gameObject.GetComponents<Component>().Select(c => c.GetType().Name));
        Plugin.Logger.LogInfo($"GameObject: {gameObject.name}, Active: {gameObject.activeSelf}, Components: {components}");
        // Add more detailed GameObject processing logic here
    }

    private void ProcessEntityWrapper(EntityWrapper entityWrapper)
    {
        // Example processing for EntityWrapper
        // Access and log details about the Entity using the EntityManager
        if (entityWrapper.EntityManager.Exists(entityWrapper.Entity))
        {
            // Example of accessing a component of the entity
            // Replace 'SomeComponentType' with an actual component type
            // if (entityWrapper.EntityManager.HasComponent<SomeComponentType>(entityWrapper.Entity))
            // {
            //     var component = entityWrapper.EntityManager.GetComponentData<SomeComponentType>(entityWrapper.Entity);
            //     // Process and log component data
            // }

            Plugin.Logger.LogInfo($"Entity ID: {entityWrapper.Entity.Index}, Version: {entityWrapper.Entity.Version}");
        }
        else
        {
            Plugin.Logger.LogInfo($"Entity is not valid or does not exist.");
        }
    }

    public static class ReflectionCache
    {
        private static Dictionary<Type, MethodInfo> castMethodCache = new Dictionary<Type, MethodInfo>();

        public static MethodInfo GetCastMethod(Type type)
        {
            if (type == null)
            {
                Plugin.Logger.LogInfo("Type is null, can't retrieve cast method");
                return null;
            }

            if (castMethodCache.TryGetValue(type, out MethodInfo methodInfo))
            {
                Plugin.Logger.LogInfo($"Found cached cast method for type: {type.FullName}");
                return methodInfo; // Return the cached MethodInfo
            }

            try
            {
                //Plugin.Logger.LogInfo($"Attempting to retrieve cast method for type: {type.FullName}");

                methodInfo = typeof(GenericObjectProcessor).GetMethod(nameof(GenericObjectProcessor.CastToType), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type);
                castMethodCache[type] = methodInfo;
                Plugin.Logger.LogInfo($"Successfully retrieved cast method for type: {type.FullName}");
                return methodInfo; // Return the newly retrieved MethodInfo
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in GetCastMethod for type {type.FullName}: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return null;
            }
        }
    }

    private static void InitializeCastMethods(HashSet<Type> uniqueTypes)
    {
        if (!castMethodsInitialized)
        {
            foreach (Type type in uniqueTypes)
            {
                if (type == null)
                {
                    Plugin.Logger.LogInfo("Encountered null type in uniqueTypes, skipping.");
                    continue;
                }

                MethodInfo castMethod = ReflectionCache.GetCastMethod(type);
                if (castMethod != null)
                {
                    Func<UnityEngine.Object, UnityEngine.Object> castFunc = (obj) =>
                    {
                        try
                        {
                            return (UnityEngine.Object)castMethod.Invoke(null, new object[] { obj });
                        }
                        catch (Exception ex)
                        {
                            Plugin.Logger.LogError($"Error invoking cast method for type {type.FullName}: {ex.Message}");
                            return null;
                        }
                    };
                    castMethods[type] = castFunc;
                }
                else
                {
                    Plugin.Logger.LogInfo($"Failed to initialize cast method for type: {type.FullName}");
                }
            }
            castMethodsInitialized = true;
            Plugin.Logger.LogInfo("Cast Methods Initialized.");
        }
    }

    public class GenericObjectProcessor
    {
        private Dictionary<Type, MethodInfo> processMethodCache = new Dictionary<Type, MethodInfo>();

        private MethodInfo GetProcessMethodForType(Type type)
        {
            if (!processMethodCache.TryGetValue(type, out MethodInfo method))
            {
                string methodName = $"Process {type.Name}";
                method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

                // Check if method is found
                if (method == null)
                {
                    //Plugin.Logger.LogInfo($"Method not found for type: {type}");
                    return null;
                }

                processMethodCache[type] = method;
            }

            return method;
        }

        public void Process(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                //Plugin.Logger.LogInfo("Skipped processing a null object.");
                return;
            }
            if (obj is EntityWrapper entityWrapper)
            {
                ProcessEntityWrapper(entityWrapper);
                return;
            }

            Plugin.Logger.LogInfo($"Processing object: {obj.name}");
            Type actualType = ObjectFinder.GetSpecificType(obj); // Updated to use GetSpecificType
            if (actualType == null)
            {
                Plugin.Logger.LogInfo($"Unable to determine specific type for object: {obj.name}");
                LogObjectDetails(obj);
                return;
            }
            try
            {
                MethodInfo castMethod = ReflectionCache.GetCastMethod(actualType);
                if (castMethod == null)
                {
                    Plugin.Logger.LogInfo($"Cast method not found for type: {actualType}");
                    LogObjectDetails(obj);
                }

                object castedObject = castMethod.Invoke(null, new object[] { obj });
                ProcessCastedObject(castedObject);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogInfo($"Exception during processing: {ex.Message}, Stack Trace: {ex.StackTrace}");
            }
        }

        public static T CastToType<T>(UnityEngine.Object obj) where T : UnityEngine.Object
        {
            //Plugin.Logger.LogInfo($"Attempting cast to type: {typeof(T).Name} for object: {obj.name}, runtime type: {obj.GetType().FullName}");
            try
            {
                T casted = (T)Convert.ChangeType(obj, typeof(T));
                //Plugin.Logger.LogInfo($"Successfully cast object '{obj.name}' to type: {typeof(T).Name}");
                return casted;
            }
            catch (InvalidCastException ex)
            {
                Plugin.Logger.LogInfo($"Failed to cast object '{obj.name}' to type: {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        private void ProcessCastedObject(object castedObject)
        {
            if (castedObject == null)
            {
                Plugin.Logger.LogInfo("Casted object is null. Skipping processing.");
                return;
            }
            //Plugin.Logger.LogInfo($"Starting to process casted object of type {castedObject.GetType().Name}");
            Type objType = castedObject.GetType();
            MethodInfo processMethod = GetProcessMethodForType(objType);

            if (processMethod != null)
            {
                processMethod.Invoke(this, new object[] { castedObject });
                //Plugin.Logger.LogInfo($"Successfully processed object of type {castedObject.GetType().Name}");
            }
            else
            {
                //Plugin.Logger.LogInfo($"Object sent to generic processing, no method found {castedObject.GetType().Name}");
                LogObjectDetails(castedObject);
            }
        }

        private void LogObjectDetails(object obj, string indent = "", int depth = 0)
        {
            if (obj == null)
            {
                Plugin.Logger.LogInfo($"{indent}Object is null.");
                return;
            }
            if (depth > 5)
            {
                Plugin.Logger.LogInfo($"{indent}Reached maximum depth of logging.");
                return;
            }

            Type objType = obj.GetType();
            Plugin.Logger.LogInfo($"{indent}Type: {objType.Name}");

            if (typeof(IEnumerable).IsAssignableFrom(objType) && objType != typeof(string))
            {
                int index = 0;
                foreach (var item in (IEnumerable)obj)
                {
                    Plugin.Logger.LogInfo($"{indent}  [{index++}]:");
                    LogObjectDetails(item, indent + "    ", depth + 1);
                }
                return;
            }

            LogPropertiesAndFields(obj, objType, indent, depth);
        }

        private void LogPropertiesAndFields(object obj, Type objType, string indent, int depth)
        {
            foreach (PropertyInfo property in objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    object value = property.GetValue(obj);
                    Plugin.Logger.LogInfo($"{indent}Property {property.Name} ({property.PropertyType.Name}): {value}");

                    if (value != null && !IsSimpleType(value.GetType()))
                    {
                        LogObjectDetails(value, indent + "    ", depth + 1);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogInfo($"{indent}Error accessing property {property.Name}: {ex.Message}");
                }
            }

            foreach (FieldInfo field in objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    object value = field.GetValue(obj);
                    Plugin.Logger.LogInfo($"{indent}Field {field.Name} ({field.FieldType.Name}): {value}");

                    if (value != null && !IsSimpleType(value.GetType()))
                    {
                        LogObjectDetails(value, indent + "    ", depth + 1);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogInfo($"{indent}Error accessing field {field.Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
        }

        private void ProcessEntityWrapper(EntityWrapper entityWrapper)
        {
            // Specific processing logic for EntityWrapper
            // Access and log details about the Entity using the EntityManager
            if (entityWrapper.EntityManager.Exists(entityWrapper.Entity))
            {
                // Access and process entity components
                // Log entity details or perform other operations
            }
            else
            {
                // Handle invalid or non-existent entity
            }
        }

        private void ProcessGameObject(GameObject gameObject)
        {
            string components = String.Join(", ", gameObject.GetComponents<Component>().Select(c => c.GetType().Name));
            Plugin.Logger.LogInfo($"GameObject: {gameObject.name}, Active: {gameObject.activeSelf}, Components: {components}");
        }

        private void ProcessLight(Light light)
        {
            Plugin.Logger.LogInfo($"Light: Type: {light.type}, Intensity: {light.intensity}, Color: {light.color}");
        }

        private void ProcessTransform(Transform transform)
        {
            Plugin.Logger.LogInfo($"Transform: Position: {transform.position}, Rotation: {transform.rotation}, Scale: {transform.localScale}");
        }

        private void ProcessCamera(Camera camera)
        {
            string renderSettings = $"Field of View: {camera.fieldOfView}, Rendering Path: {camera.renderingPath}";
            Plugin.Logger.LogInfo($"Camera: {camera.name}, {renderSettings}");
        }

        private void ProcessDirectionalLight(Light light)
        {
            if (light.type == LightType.Directional)
            {
                Plugin.Logger.LogInfo($"Directional Light: Intensity - {light.intensity}, Color - {light.color}");
                // Add more detailed processing logic specific to Directional Lights here
            }
        }
    }

    public static class ObjectFinder
    {
        public static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> FindAllObjects(ServerBootstrapSystem serverBootstrapSystem)
        {
            Plugin.Logger.LogInfo("Getting UnityEngine.Object as Il2CppType");

            Il2CppSystem.Type objectType = Il2CppSystem.Type.GetType("UnityEngine.Object, UnityEngine.CoreModule");
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> allUnityObjects = Resources.FindObjectsOfTypeAll(objectType);

            // Retrieve all entities as well
            NativeArray<Entity> allEntitiesArray = FindAllEntities(serverBootstrapSystem);
            Plugin.Logger.LogInfo($"Found {allEntitiesArray.Length} entities");
            // Process and add entities to the results
            foreach (var entity in allEntitiesArray)
            {
                // Assuming you have a method to convert an Entity to a UnityEngine.Object or a wrapper class
                UnityEngine.Object entityObject = ConvertEntityToObject(entity, serverBootstrapSystem.EntityManager);
                if (entityObject != null)
                {
                    AddToObjectByTypeDictionary(entityObject.GetType(), entityObject);
                }
            }

            allEntitiesArray.Dispose(); // Important: Dispose of the NativeArray after use

            return allUnityObjects;
        }

        private static UnityEngine.Object ConvertEntityToObject(Entity entity, EntityManager entityManager)
        {
            // Convert Entity to a Unity Object or a wrapper class
            // Implement this method based on how you want to handle entities
            // For example, you can wrap the Entity in a custom class that extends UnityEngine.Object
            return new EntityWrapper(entity, entityManager);
        }

        // ... (other existing methods)

        public static NativeArray<Entity> FindAllEntities(ServerBootstrapSystem __instance)
        {
            EntityManager entityManager = __instance.EntityManager;
            return entityManager.GetAllEntities(Allocator.Temp);
        }

        public static HashSet<Type> GetObjectClasses(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> objects)
        {
            HashSet<Type> objectTypes = new HashSet<Type>();

            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    Type objType = GetSpecificType(obj);
                    if (objType != null)
                    {
                        objectTypes.Add(objType);
                        //Plugin.Logger.LogInfo($"Adding object type to objectTypesCache: {obj.name} | {objType}");
                    }
                    else
                    {
                        //Plugin.Logger.LogInfo($"Failed to determine specific type for object: {obj.name}");
                    }
                }
            }
            Plugin.Logger.LogInfo($"Total Objects: {objects.Count}");
            Plugin.Logger.LogInfo($"Total Unique Object Types: {objectTypes.Count}");
            return objectTypes;
        }

        public static Type GetSpecificType(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                Plugin.Logger.LogInfo("Object is null, can't get type.");
                return null;
            }

            Type objType = obj.GetType();
            string objName = obj.name;

            // Attempt to find a more specific type using reflection
            Type specificType = FindTypeByName(objName);
            if (specificType != null)
            {
                Plugin.Logger.LogInfo($"Found specific type: {specificType.FullName} for object name: {objName}");
                UpdateTypeDeterminationStats(objName, specificType != null);
                AddToObjectByTypeDictionary(specificType, obj);
                return specificType;
            }
            if (specificType == null)
            {
                if (objName.Length == 0)
                {
                    // dont print out names that are blank
                    AddToObjectByTypeDictionary(objType, obj);
                    return objType;
                }
                //Plugin.Logger.LogInfo($"No specific type found for object name: {objName}");
                AddToObjectByTypeDictionary(objType, obj);
                return objType;
            }

            // If no specific type is found, use the direct type of the object
            //Plugin.Logger.LogInfo($"No specific type found for object name: {objName}, defaulting to type: {objType.FullName}");
            AddToObjectByTypeDictionary(objType, obj);
            return objType;
        }

        private static void UpdateTypeDeterminationStats(string typeName, bool success)
        {
            string key = $"{typeName}|{(success ? "Success" : "Fail")}";
            if (!typeDeterminationStats.ContainsKey(key))
            {
                typeDeterminationStats[key] = 0;
            }
            typeDeterminationStats[key]++;
        }

        public static void AnalyzeTypeDeterminationStats()
        {
            var failurePatterns = new Dictionary<string, int>();

            foreach (var kvp in typeDeterminationStats)
            {
                string[] parts = kvp.Key.Split('|');
                string typeName = parts[0];
                bool success = parts[1] == "Success";
                int count = kvp.Value;
                Plugin.Logger.LogInfo($"Type: {typeName}, Result: {success}, Count: {count}");
                if (!success)
                {
                    if (!failurePatterns.ContainsKey(typeName))
                    {
                        failurePatterns[typeName] = 0;
                    }
                    failurePatterns[typeName] += count;
                }
            }

            foreach (var failure in failurePatterns.OrderByDescending(f => f.Value))
            {
                Plugin.Logger.LogInfo($"High failure rate for type {failure.Key}: {failure.Value} failures");
                // Implement logic here to adjust type determination strategy based on failure patterns
            }
        }

        private static Type FindTypeByName(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
            {
                Plugin.Logger.LogInfo("typeName is null or empty, cannot find type.");
                return null;
            }
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Check if the assembly is one of the relevant ones
                if (!relevantAssemblies.Contains(assembly.GetName().Name))
                {
                    continue;
                }

                try
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type; // Match found
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Plugin.Logger.LogInfo($"Error loading types from assembly: {assembly.FullName}");
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        Plugin.Logger.LogInfo($"Loader Exception: {loaderException.Message}");
                        if (loaderException.InnerException != null)
                        {
                            Plugin.Logger.LogInfo($"Inner Exception: {loaderException.InnerException.Message}");
                        }
                    }
                }
            }
            return null;
        }

        private static readonly HashSet<string> relevantAssemblies = new HashSet<string>
{
    "Assembly-CSharp", // Unity's main assembly for game scripts
    "UnityEngine",     // Unity's engine assembly
    "ProjectM",        // Replace with the actual name of your game's main assembly if different
    // Add more assemblies as needed
};

        private static void AddToObjectByTypeDictionary(Type type, UnityEngine.Object obj)
        {
            string typeName = type.FullName; // Use the full name of the type as the dictionary key

            if (!objectsByType.ContainsKey(typeName))
            {
                objectsByType[typeName] = new List<object>();
            }
            objectsByType[typeName].Add(obj);
        }
    }

    public class EntityWrapper : UnityEngine.Object
    {
        public Entity Entity { get; private set; }
        public EntityManager EntityManager { get; private set; }

        public EntityWrapper(Entity entity, EntityManager entityManager)
        {
            this.Entity = entity;
            this.EntityManager = entityManager;
        }

        // Additional methods or properties to interact with the Entity
    }
}