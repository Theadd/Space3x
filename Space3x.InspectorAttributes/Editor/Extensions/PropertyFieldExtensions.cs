using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    public static class PropertyFieldExtensions
    {
        private static FieldInfo s_SerializedPropertyInPropertyField = null;
        
        private static FieldInfo s_PropertyFieldDrawNestingLevel = null;
        
        private static FieldInfo s_PropertyFieldParentPropertyField = null;

        public static SerializedProperty GetSerializedProperty(this PropertyField propertyField)
        {
            s_SerializedPropertyInPropertyField ??= typeof(PropertyField).GetField(
                "m_SerializedProperty",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            return s_SerializedPropertyInPropertyField?.GetValue(propertyField) as SerializedProperty;
        }

        public static int GetDrawNestingLevel(this PropertyField propertyField)
        {
            s_PropertyFieldDrawNestingLevel ??= typeof(PropertyField).GetField(
                "m_DrawNestingLevel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            return s_PropertyFieldDrawNestingLevel != null ? (int) s_PropertyFieldDrawNestingLevel.GetValue(propertyField) : 0;
        }
        
        public static void SetDrawNestingLevel(this PropertyField propertyField, int value)
        {
            s_PropertyFieldDrawNestingLevel ??= typeof(PropertyField).GetField(
                "m_DrawNestingLevel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (s_PropertyFieldDrawNestingLevel != null)
                s_PropertyFieldDrawNestingLevel.SetValue(propertyField, value);
        }

        public static PropertyField GetParentPropertyField(this PropertyField propertyField)
        {
            s_PropertyFieldParentPropertyField ??= typeof(PropertyField).GetField(
                "m_ParentPropertyField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            return s_PropertyFieldParentPropertyField != null ? (PropertyField) s_PropertyFieldParentPropertyField.GetValue(propertyField) : null;
        }
    }
}
