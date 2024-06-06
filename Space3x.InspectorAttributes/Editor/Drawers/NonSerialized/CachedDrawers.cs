using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    public static class CachedDrawers
    {
        private static MethodInfo s_GetDrawerTypeForType;
        private static Dictionary<Type, Type> s_Instances;

        static CachedDrawers() => Initialize();

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
        }
        
        public static Type GetCustomDrawer(Type type)
        {
            if (s_Instances.TryGetValue(type, out var value))
                return value;

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
        }
    }

    // [InitializeOnLoad]
    // public static class CachedDrawersDraft
    // {
    //     private static List<Type> s_CustomDrawers;
    //     private static List<Type> s_DerivedDrawers;
    //     private static bool s_TypesFetched = false;
    //     
    //     static CachedDrawersDraft() => Initialize();
    //
    //     private static void Initialize()
    //     {
    //         s_CustomDrawers = new List<Type>();
    //         s_DerivedDrawers = new List<Type>();
    //     }
    //     
    //     public static GUIDrawer CreateInstance(FieldInfo fieldInfo)
    //     {
    //         if (fieldInfo == null)
    //             return null; // Activator.CreateInstance<GUIDrawer>();
    //
    //         // HAVING:
    //         //      public bool hasPropertyDrawer => this.propertyDrawer != null;
    //         //
    //         // First, iterate all PropertyAttributes on that field to
    //         // get the drawer Type for each custom attribute, create
    //         // a drawer instance for each Type and, using reflection,
    //         // assign corresponding values to:
    //         //
    //         //  CachedDrawers.GetCustomDrawer(propertyAttribute.GetType())
    //         //  * if (typeof (DecoratorDrawer).IsAssignableFrom(drawerFromCachedDrawers)
    //         //      && (!(field != (FieldInfo) null)
    //         //          || !field.FieldType.IsArrayOrList()
    //         //          || propertyType.IsArrayOrList()))   <-- Ignore last one
    //         //
    //         //      DecoratorDrawer instance = (DecoratorDrawer) Activator.CreateInstance(forPropertyAndType);
    //         //      instance.m_Attribute = (propertyAttribute) attribute;
    //         //      // And store it in some collection
    //         //
    //         // Then, if hasPropertyDrawer is false, 
    //         GUIDrawer drawer;
    //
    //         try
    //         {
    //             drawer = (GUIDrawer)Activator.CreateInstance(GetCustomDrawer(fieldInfo.FieldType));
    //         }
    //         catch (Exception)
    //         {
    //             drawer = null;  // Activator.CreateInstance<GUIDrawer>();
    //         }
    //
    //         // drawer.track = trackAsset;
    //         // return drawer;
    //         // TODO
    //         return null;
    //     }
    //
    //     public static GUIDrawer GetCustomDrawersForType(Type type)
    //     {
    //         if (!s_TypesFetched)
    //             FetchTypesFromCache();
    //
    //         if (typeof(Attribute).IsAssignableFrom(type))
    //         {
    //             if (!typeof(PropertyAttribute).IsAssignableFrom(type))
    //             {
    //                 Debug.Log($"  <b>GetCustomDrawersForType({type.Name})</b> => Type derives from Attribute " + 
    //                           "(But <u><b>NOT</b></u> from PropertyAttribute) <color=#FF0000FF><b>[SKIP]</b></color>");
    //                 return null;
    //             }
    //             else
    //             {
    //                 Debug.Log($"  <b>GetCustomDrawersForType({type.Name})</b> => Type derives from PropertyAttribute " +
    //                           $"<color=#FF0000FF><b>[GOOD]</b></color> (TODO: Check if those can also be of Types other than DecoratorDrawers)");
    //                 
    //             }
    //         }
    //         else
    //         {
    //             
    //         }
    //         
    //         return null;
    //     }
    //
    //     private static MethodInfo s_GetDrawerTypeForType;
    //     
    //     public static Type GetCustomDrawer(Type type)
    //     {
    //         // foreach (var drawer in s_DerivedDrawers)
    //         // {
    //         //     var attr = Attribute.GetCustomAttribute(drawer, typeof(CustomPropertyDrawer), false) as CustomPropertyDrawer;
    //         //     if (attr != null && attr.assetType.IsAssignableFrom(trackType))
    //         //         return drawer;
    //         // }
    //         //
    //         // return typeof(TrackDrawer);
    //         var renderPipelineAssetTypes = GraphicsSettings.isScriptableRenderPipelineEnabled
    //             ? new[] { GraphicsSettings.currentRenderPipelineAssetType }
    //             : null;
    //         // SerializedPropertyType.ManagedReference
    //         //      This type is for fields that have the SerializeReference attribute,
    //         //      otherwise the object is stored inline (SerializedPropertyType.Generic).
    //         // So, if that field has a [SerializeReference] it can't be a NonSerialised one.
    //         // Hence, isPropertyTypeAManagedReference is always false here.
    //         return ScriptAttributeUtility.GetDrawerTypeForType(type, renderPipelineAssetTypes, false);
    //     }
    //
    //     private static void FetchTypesFromCache()
    //     {
    //         var customDrawers = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>();
    //         var derivedDrawers = TypeCache.GetTypesDerivedFrom<GUIDrawer>();
    //
    //         s_CustomDrawers = customDrawers.Where(drawer => typeof(GUIDrawer).IsAssignableFrom(drawer)).ToList<Type>();
    //         s_DerivedDrawers = derivedDrawers.Where(drawer => 
    //                 typeof(PropertyDrawer).IsAssignableFrom(drawer) 
    //                     ? IsValidUiToolkitPropertyDrawer(drawer)
    //                     : typeof(DecoratorDrawer).IsAssignableFrom(drawer) && IsValidUiToolkitDecoratorDrawer(drawer))
    //             .ToList<Type>();
    //         
    //         // TODO
    //         Debug.Log($"<color=#FFFF00FF><b>[FETCHING RESULTS]</b> " +
    //                   $"CustomPropertyDrawers: {s_CustomDrawers.Count} / {customDrawers.Count}; " +
    //                   $"DerivedDrawers: {s_DerivedDrawers.Count} / {derivedDrawers.Count};");
    //         //
    //         
    //         s_TypesFetched = true;
    //     }
    //
    //     private static string AsString(this GUIDrawer drawer)
    //     {
    //         return drawer.GetType().Name;
    //     }
    //
    //     private static bool IsValidUiToolkitPropertyDrawer(Type propertyDrawerType) => 
    //         IsOverride(propertyDrawerType.GetMethod(nameof(PropertyDrawer.CreatePropertyGUI)));
    //     
    //     private static bool IsValidUiToolkitDecoratorDrawer(Type decoratorDrawerType) => 
    //         IsOverride(decoratorDrawerType.GetMethod(nameof(DecoratorDrawer.CreatePropertyGUI)));
    //
    //     private static bool IsOverride(MethodInfo m) => 
    //         m.GetBaseDefinition().DeclaringType != m.DeclaringType;
    // }
}
