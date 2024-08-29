using System;
using UnityEditor;
using UnityEngine;

namespace Space3x.Properties.Types.Editor
{
    public static class PropertyExtensions
    {
        public static SerializedProperty GetSerializedProperty(this IPropertyNode self)
        {
            if (self is not ISerializedPropertyNode property)
                return null;

            try
            {
                return property.SerializedObject.FindProperty(property.PropertyPath);
            }
            catch (NullReferenceException)
            {
                // Can happen when switching from Debug mode to Normal mode in the inspector after editing some properties.
                Debug.LogWarning("NullReferenceException when trying to get property: " + property.PropertyPath + Environment.NewLine
                                 + "Can happen when switching from Debug mode to Normal mode in the inspector after editing some properties.");
            }

            return null;
        }
    }
}
