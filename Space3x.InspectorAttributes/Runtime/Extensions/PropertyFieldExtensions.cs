#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;

namespace Space3x.InspectorAttributes
{
    public static class PropertyFieldExtensions
    {
        private static FieldInfo s_SerializedPropertyInPropertyField = null;
        
        public static SerializedProperty GetSerializedProperty(this PropertyField propertyField)
        {
            s_SerializedPropertyInPropertyField ??= typeof(PropertyField).GetField(
                "m_SerializedProperty",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            return s_SerializedPropertyInPropertyField != null ? (SerializedProperty) s_SerializedPropertyInPropertyField.GetValue(propertyField) : null;
        }
    }
}
#endif