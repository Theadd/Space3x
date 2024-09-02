using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    [ExcludeFromDocs]
    internal static class DrawerExtensions
    {
        private static FieldInfo s_PropertyDrawerAttribute = null;
        private static FieldInfo s_PropertyDrawerFieldInfo = null;
        private static FieldInfo s_PropertyDrawerPreferredLabel = null;
        private static FieldInfo s_DecoratorDrawerAttribute = null;

        internal static void SetAttribute(this PropertyDrawer propertyDrawer, PropertyAttribute attribute)
        {
            s_PropertyDrawerAttribute ??= typeof(PropertyDrawer).GetField(
                "m_Attribute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (s_PropertyDrawerAttribute != null)
                s_PropertyDrawerAttribute.SetValue(propertyDrawer, attribute);
        }
        
        internal static void SetFieldInfo(this PropertyDrawer propertyDrawer, FieldInfo fieldInfo)
        {
            s_PropertyDrawerFieldInfo ??= typeof(PropertyDrawer).GetField(
                "m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (s_PropertyDrawerFieldInfo != null)
                s_PropertyDrawerFieldInfo.SetValue(propertyDrawer, fieldInfo);
        }
        
        internal static void SetPreferredLabel(this PropertyDrawer propertyDrawer, string preferredLabel)
        {
            s_PropertyDrawerPreferredLabel ??= typeof(PropertyDrawer).GetField(
                "m_PreferredLabel", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (s_PropertyDrawerPreferredLabel != null)
                s_PropertyDrawerPreferredLabel.SetValue(propertyDrawer, preferredLabel);
        }
        
        internal static void SetAttribute(this DecoratorDrawer decoratorDrawer, PropertyAttribute attribute)
        {
            s_DecoratorDrawerAttribute ??= typeof(DecoratorDrawer).GetField(
                "m_Attribute", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (s_DecoratorDrawerAttribute != null)
                s_DecoratorDrawerAttribute.SetValue(decoratorDrawer, attribute);
        }
    }
}
