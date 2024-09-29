// #define SIMULATE_RUNTIME_BUILD
// // TODO: REMOVE NEXT LINE!
// #define UNITY_EDITOR
#if UNITY_EDITOR && !SIMULATE_RUNTIME_BUILD && RUNTIME_UITOOLKIT_DRAWERS
    #define DUAL_MODE
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using Space3x.Properties.Types;
using UnityEngine.Rendering;

namespace Space3x.InspectorAttributes
{
    public static class CachedDrawers
    {
        private static MethodInfo s_GetDrawerTypeForType;
        private static Dictionary<Type, Type> s_Instances;
#if DUAL_MODE
        private static Dictionary<Type, Type> s_RuntimeInstances;
        private static Dictionary<Type, Type> s_RuntimeOverrides;
#endif
        
        private static Func<Type, Type[], bool, Type> s_GetDrawerTypeForTypeDelegate;

        internal static void ReloadAll()
        {
            Initialize();
#if DUAL_MODE
            InitializeDualMode();
#endif
        }

#if UNITY_EDITOR && !SIMULATE_RUNTIME_BUILD
        private static void Initialize()
        {
            s_GetDrawerTypeForType = Type
                .GetType("UnityEditor.ScriptAttributeUtility, UnityEditor")!
                .GetMethod(
                    "GetDrawerTypeForType", 
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new Type[] { typeof(Type), typeof(Type[]), typeof(bool) },
                    null);
            s_Instances = new Dictionary<Type, Type>();
            s_GetDrawerTypeForTypeDelegate = (Func<Type, Type[], bool, Type>)
                s_GetDrawerTypeForType!.CreateDelegate(typeof(Func<Type, Type[], bool, Type>));
        }
#else
        private static void Initialize()
        {
            s_Instances = new Dictionary<Type, Type>();
            foreach (var drawer in TypeUtilityExtensions.GetTypesWithAttributeInCustomAssemblies(typeof(CustomRuntimeDrawer)))
            {
                foreach (var attr in drawer.GetCustomAttributes(typeof(CustomRuntimeDrawer), false))
                {
                    foreach (var type in ((CustomRuntimeDrawer)attr).Types)
                    {
                        s_Instances[type] = (Type)drawer;
                    }
                }
            }
        }
#endif
        
#if DUAL_MODE
        private static void InitializeDualMode()
        {
            s_RuntimeInstances = new Dictionary<Type, Type>();
            s_RuntimeOverrides = new Dictionary<Type, Type>();
            foreach (var drawer in TypeUtilityExtensions.GetTypesWithAttributeInCustomAssemblies(typeof(CustomRuntimeDrawer)))
            {
                foreach (var attr in drawer.GetCustomAttributes(typeof(CustomRuntimeDrawer), false))
                {
                    foreach (var type in ((CustomRuntimeDrawer)attr).Types)
                    {
                        s_RuntimeInstances[type] = (Type)drawer;
                    }
                }
            }
        }
#endif
        
#if DUAL_MODE
        public static Type GetCustomDrawer(Type type)
        {
            if (type == null) return null;
            if (s_Instances.TryGetValue(type, out var value)) return value;
            
            Type drawer = GetCustomDrawerInternal(type);
            Type runtimeDrawer = s_RuntimeInstances.GetValueOrDefault(type, null);
            if (drawer == null)
            {
                s_Instances.Add(type, runtimeDrawer);
                return runtimeDrawer;
            }
            else
            {
                if (drawer != runtimeDrawer)
                    s_RuntimeOverrides[drawer] = runtimeDrawer;
                
                s_Instances.Add(type, drawer);
                return drawer;
            }
        }
        
        private static Type GetCustomDrawerInternal(Type type)
        {
            Type value = null;
#else
        public static Type GetCustomDrawer(Type type)
        {
            if (type == null) return null;
            if (s_Instances.TryGetValue(type, out var value))
                return value;
#endif

#if UNITY_EDITOR && !SIMULATE_RUNTIME_BUILD
            var renderPipelineAssetTypes = GraphicsSettings.isScriptableRenderPipelineEnabled
                ? new[] { GraphicsSettings.currentRenderPipelineAssetType }
                : null;
            
            // SerializedPropertyType.ManagedReference
            //      This type is for fields that have the SerializeReference attribute,
            //      otherwise the object is stored inline (SerializedPropertyType.Generic).
            // So, if that field has a [SerializeReference] it can't be a NonSerialised one.
            // Hence, isPropertyTypeAManagedReference (3rd param) is always false here.
            
            // value = (Type) s_GetDrawerTypeForType.Invoke(null, new object[] { type, renderPipelineAssetTypes, false });
            value = s_GetDrawerTypeForTypeDelegate(type, renderPipelineAssetTypes, false);
            
#if !DUAL_MODE
            s_Instances.Add(type, value);
#endif
            return value;
#else
            return null;
#endif
        }

#if UNITY_EDITOR && RUNTIME_UITOOLKIT_DRAWERS
        public static Type GetRuntimeDrawerOverride(Type originalDrawer) =>
            s_RuntimeOverrides.TryGetValue(originalDrawer, out Type replacementDrawer)
                ? replacementDrawer
                : originalDrawer;
#endif
    }
}
