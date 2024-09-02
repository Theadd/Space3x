using System;
using System.Reflection;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    internal interface IDrawerUtility
    {
        object CopyDecoratorDrawer(object decoratorDrawer);
        
        object CopyPropertyDrawer(object propertyDrawer);
        
        object CreateDecoratorDrawer(Type decoratorType, PropertyAttribute attribute);

        object CreatePropertyDrawer(Type propertyDrawerType, PropertyAttribute attribute, FieldInfo fieldInfo,
            string preferredLabel);
    }
    
    internal class DrawerUtility
    {
        private static IDrawerUtility s_Implementation;
        
        internal static void RegisterImplementationProvider(IDrawerUtility provider) => s_Implementation = provider;

        /// <summary>
        /// Creates an empty copy of the given decorator drawer targeting the same attribute.
        /// </summary>
        /// <returns>A clean copy.</returns>
        public static object CopyDecoratorDrawer(object decoratorDrawer) =>
            s_Implementation?.CopyDecoratorDrawer(decoratorDrawer);

        /// <summary>
        /// Creates an empty copy of the given property drawer sharing the same initial values.
        /// </summary>
        /// <returns>A clean copy.</returns>
        public static object CopyPropertyDrawer(object propertyDrawer) =>
            s_Implementation?.CopyPropertyDrawer(propertyDrawer);

        public static object CreateDecoratorDrawer(Type decoratorType, PropertyAttribute attribute) =>
            s_Implementation?.CreateDecoratorDrawer(decoratorType, attribute);

        public static object CreatePropertyDrawer(Type propertyDrawerType, PropertyAttribute attribute,
            FieldInfo fieldInfo, string preferredLabel) =>
            s_Implementation?.CreatePropertyDrawer(propertyDrawerType, attribute, fieldInfo, preferredLabel);
    }
}
