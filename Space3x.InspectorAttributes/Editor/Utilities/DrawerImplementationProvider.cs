using System;
using System.Reflection;
using UnityEngine;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    [InitializeOnLoad]
    internal class DrawerImplementationProvider : IDrawerUtility
    {
        static DrawerImplementationProvider() =>
            DrawerUtility.RegisterImplementationProvider(new DrawerImplementationProvider());
        
        public object CopyDecoratorDrawer(object decoratorDrawer)
        {
            DecoratorDrawer drawer = (DecoratorDrawer) Activator.CreateInstance(decoratorDrawer.GetType());
            drawer.SetAttribute(((DecoratorDrawer)decoratorDrawer).attribute);
            return drawer;
        }

        public object CopyPropertyDrawer(object propertyDrawer)
        {
            PropertyDrawer drawer = (PropertyDrawer) Activator.CreateInstance(propertyDrawer.GetType());
            drawer.SetAttribute(((PropertyDrawer)propertyDrawer).attribute);
            drawer.SetFieldInfo(((PropertyDrawer)propertyDrawer).fieldInfo);
            drawer.SetPreferredLabel(((PropertyDrawer)propertyDrawer).preferredLabel);
            return drawer;
        }

        public object CreateDecoratorDrawer(Type decoratorType, PropertyAttribute attribute)
        {
            var drawer = (DecoratorDrawer)Activator.CreateInstance(decoratorType);
            drawer.SetAttribute(attribute);
            return drawer;
        }

        public object CreatePropertyDrawer(Type propertyDrawerType, PropertyAttribute attribute, FieldInfo fieldInfo,
            string preferredLabel)
        {
            var drawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            drawer.SetAttribute(attribute);
            drawer.SetFieldInfo(fieldInfo);
            drawer.SetPreferredLabel(preferredLabel);
            return drawer;
        }
    }
}
