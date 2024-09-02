using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering;

namespace Space3x.InspectorAttributes
{
    public static class CachedDrawers
    {
        private static MethodInfo s_GetDrawerTypeForType;
        private static Dictionary<Type, Type> s_Instances;

        static CachedDrawers() => Initialize();

// #if UNITY_EDITOR
//         private static void Initialize()
//         {
//             s_GetDrawerTypeForType = Type
//                 .GetType("UnityEditor.ScriptAttributeUtility, UnityEditor")!
//                 .GetMethod(
//                     "GetDrawerTypeForType", 
//                     BindingFlags.NonPublic | BindingFlags.Static,
//                     null,
//                     new Type[] { typeof(Type), typeof(Type[]), typeof(bool) },
//                     null);
//             s_Instances = new Dictionary<Type, Type>();
//         }
// #else
        private static void Initialize()
        {
            
        }
        
// #endif
        
        public static Type GetCustomDrawer(Type type)
        {
            if (type == null) return null;
            if (s_Instances.TryGetValue(type, out var value))
                return value;

#if UNITY_EDITOR
            var renderPipelineAssetTypes = GraphicsSettings.isScriptableRenderPipelineEnabled
                ? new[] { GraphicsSettings.currentRenderPipelineAssetType }
                : null;
            
            // SerializedPropertyType.ManagedReference
            //      This type is for fields that have the SerializeReference attribute,
            //      otherwise the object is stored inline (SerializedPropertyType.Generic).
            // So, if that field has a [SerializeReference] it can't be a NonSerialised one.
            // Hence, isPropertyTypeAManagedReference is always false here.
            value = (Type) s_GetDrawerTypeForType.Invoke(null, new object[] { type, renderPipelineAssetTypes, false });
            s_Instances.Add(type, value);
            return value;
#else
            return null;
#endif
        }
    }
}
