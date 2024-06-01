using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    [InitializeOnLoad]
    public static class CachedDrawers
    {
        private static List<Type> s_CustomDrawers;
        private static List<Type> s_DerivedDrawers;
        private static bool s_TypesFetched = false;
        
        static CachedDrawers() => Initialize();

        private static void Initialize()
        {
            s_CustomDrawers = new List<Type>();
            s_DerivedDrawers = new List<Type>();
        }
        
        public static GUIDrawer CreateInstance(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return null; // Activator.CreateInstance<GUIDrawer>();

            GUIDrawer drawer;

            try
            {
                drawer = (GUIDrawer)Activator.CreateInstance(GetCustomDrawer(fieldInfo.FieldType));
            }
            catch (Exception)
            {
                drawer = null;  // Activator.CreateInstance<GUIDrawer>();
            }

            // drawer.track = trackAsset;
            // return drawer;
            // TODO
            return null;
        }

        public static GUIDrawer GetCustomDrawersForType(Type type)
        {
            if (!s_TypesFetched)
                FetchTypesFromCache();

            if (typeof(Attribute).IsAssignableFrom(type))
            {
                if (!typeof(PropertyAttribute).IsAssignableFrom(type))
                {
                    Debug.Log($"  <b>GetCustomDrawersForType({type.Name})</b> => Type derives from Attribute " + 
                              "(But <u><b>NOT</b></u> from PropertyAttribute) <color=#FF0000FF><b>[SKIP]</b></color>");
                    return null;
                }
                else
                {
                    Debug.Log($"  <b>GetCustomDrawersForType({type.Name})</b> => Type derives from PropertyAttribute " +
                              $"<color=#FF0000FF><b>[GOOD]</b></color> (TODO: Check if those can also be of Types other than DecoratorDrawers)");
                    
                }
            }
            else
            {
                
            }
            
            return null;
        }
        
        public static Type GetCustomDrawer(Type trackType)
        {

            // foreach (var drawer in s_DerivedDrawers)
            // {
            //     var attr = Attribute.GetCustomAttribute(drawer, typeof(CustomPropertyDrawer), false) as CustomPropertyDrawer;
            //     if (attr != null && attr.assetType.IsAssignableFrom(trackType))
            //         return drawer;
            // }
            //
            // return typeof(TrackDrawer);
            return null;
        }

        private static void FetchTypesFromCache()
        {
            var customDrawers = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>();
            var derivedDrawers = TypeCache.GetTypesDerivedFrom<GUIDrawer>();

            s_CustomDrawers = customDrawers.Where(drawer => typeof(GUIDrawer).IsAssignableFrom(drawer)).ToList<Type>();
            s_DerivedDrawers = derivedDrawers.Where(drawer => 
                    typeof(PropertyDrawer).IsAssignableFrom(drawer) 
                        ? IsValidUiToolkitPropertyDrawer(drawer)
                        : typeof(DecoratorDrawer).IsAssignableFrom(drawer) && IsValidUiToolkitDecoratorDrawer(drawer))
                .ToList<Type>();
            
            // TODO
            Debug.Log($"<color=#FFFF00FF><b>[FETCHING RESULTS]</b> " +
                      $"CustomPropertyDrawers: {s_CustomDrawers.Count} / {customDrawers.Count}; " +
                      $"DerivedDrawers: {s_DerivedDrawers.Count} / {derivedDrawers.Count};");
            //
            
            s_TypesFetched = true;
        }

        private static string AsString(this GUIDrawer drawer)
        {
            return drawer.GetType().Name;
        }

        private static bool IsValidUiToolkitPropertyDrawer(Type propertyDrawerType) => 
            IsOverride(propertyDrawerType.GetMethod(nameof(PropertyDrawer.CreatePropertyGUI)));
        
        private static bool IsValidUiToolkitDecoratorDrawer(Type decoratorDrawerType) => 
            IsOverride(decoratorDrawerType.GetMethod(nameof(DecoratorDrawer.CreatePropertyGUI)));

        private static bool IsOverride(MethodInfo m) => 
            m.GetBaseDefinition().DeclaringType != m.DeclaringType;
    }
}