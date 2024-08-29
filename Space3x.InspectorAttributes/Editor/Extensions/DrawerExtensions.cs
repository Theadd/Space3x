using System;
using System.Reflection;
using Space3x.InspectorAttributes.Editor.Drawers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    [ExcludeFromDocs]
    public static class DrawerExtensions
    {
        #region PIVATE REFLECTION STUFF
        private static FieldInfo s_PropertyDrawerAttribute = null;
        private static FieldInfo s_PropertyDrawerFieldInfo = null;
        private static FieldInfo s_PropertyDrawerPreferredLabel = null;
        private static FieldInfo s_DecoratorDrawerAttribute = null;

        private static void SetAttribute(this PropertyDrawer propertyDrawer, PropertyAttribute attribute)
        {
            s_PropertyDrawerAttribute ??= typeof(PropertyDrawer).GetField(
                "m_Attribute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (s_PropertyDrawerAttribute != null)
                s_PropertyDrawerAttribute.SetValue(propertyDrawer, attribute);
        }
        
        private static void SetFieldInfo(this PropertyDrawer propertyDrawer, FieldInfo fieldInfo)
        {
            s_PropertyDrawerFieldInfo ??= typeof(PropertyDrawer).GetField(
                "m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (s_PropertyDrawerFieldInfo != null)
                s_PropertyDrawerFieldInfo.SetValue(propertyDrawer, fieldInfo);
        }
        
        private static void SetPreferredLabel(this PropertyDrawer propertyDrawer, string preferredLabel)
        {
            s_PropertyDrawerPreferredLabel ??= typeof(PropertyDrawer).GetField(
                "m_PreferredLabel", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (s_PropertyDrawerPreferredLabel != null)
                s_PropertyDrawerPreferredLabel.SetValue(propertyDrawer, preferredLabel);
        }
        
        private static void SetAttribute(this DecoratorDrawer decoratorDrawer, PropertyAttribute attribute)
        {
            s_DecoratorDrawerAttribute ??= typeof(DecoratorDrawer).GetField(
                "m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (s_DecoratorDrawerAttribute != null)
                s_DecoratorDrawerAttribute.SetValue(decoratorDrawer, attribute);
        }
        #endregion

        /// <summary>
        /// Creates an empty copy of the given decorator drawer targeting the same attribute.
        /// </summary>
        /// <returns>A clean copy.</returns>
        public static DecoratorDrawer CreateCopy(this DecoratorDrawer decoratorDrawer)
        {
            DecoratorDrawer drawer = (DecoratorDrawer) Activator.CreateInstance(decoratorDrawer.GetType());
            drawer.SetAttribute(decoratorDrawer.attribute);
            return drawer;
        }
        
        /// <summary>
        /// Creates an empty copy of the given property drawer sharing the same initial values.
        /// </summary>
        /// <returns>A clean copy.</returns>
        public static PropertyDrawer CreateCopy(this PropertyDrawer propertyDrawer)
        {
            PropertyDrawer drawer = (PropertyDrawer) Activator.CreateInstance(propertyDrawer.GetType());
            drawer.SetAttribute(propertyDrawer.attribute);
            drawer.SetFieldInfo(propertyDrawer.fieldInfo);
            drawer.SetPreferredLabel(propertyDrawer.preferredLabel);
            return drawer;
        }

        public static DecoratorDrawer CreateDecoratorDrawer(Type decoratorType, PropertyAttribute attribute)
        {
            var drawer = (DecoratorDrawer)Activator.CreateInstance(decoratorType);
            drawer.SetAttribute(attribute);
            return drawer;
        }
        
        public static PropertyDrawer CreatePropertyDrawer(Type propertyDrawerType, PropertyAttribute attribute,
            FieldInfo fieldInfo, string preferredLabel)
        {
            var drawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            drawer.SetAttribute(attribute);
            drawer.SetFieldInfo(fieldInfo);
            drawer.SetPreferredLabel(preferredLabel);
            return drawer;
        }
        
        public static IPanel GetPanel(this IDrawer drawer) => drawer is IDecorator decorator
            ? decorator.GhostContainer.panel
            : drawer.Container.panel;
    }
}
