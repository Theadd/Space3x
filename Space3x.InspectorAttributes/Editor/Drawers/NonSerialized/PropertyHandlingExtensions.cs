using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    public static class PropertyHandlingExtensions
    {
        public static PropertyAttributeController PropertyHandlers(this IDrawer drawer)
        {
            drawer.LogActiveEditors();
            Debug.Log("------------------------------------------");
            var serializedObject = drawer.Property?.serializedObject;
            var targetObject = serializedObject?.targetObject;  // for example, a Component instance of SimpleQuickShowAsPopup Type. (MonoBehaviour/ScriptableObject/etc.)
            Type targetType = targetObject != null ? targetObject.GetType() : null;
            var isEditingMultipleObjects = serializedObject?.isEditingMultipleObjects ?? false;
            var isValid = !(isEditingMultipleObjects && !AllObjectTypesAreTheSame(serializedObject?.targetObjects));

            if (drawer is IDecorator decorator)
            {
                
            }
            else if (drawer is PropertyDrawer propertyDrawer)
            {
                // propertyDrawer.fieldInfo.DeclaringType
            }

            return null;
        }

        internal static bool AllObjectTypesAreTheSame(IReadOnlyList<Object> targetObjects)
        {
            if (targetObjects == null || targetObjects.Count == 0)
                return false;

            var type = targetObjects[0].GetType();
            for (var i = 1; i < targetObjects.Count; i++)
                if (targetObjects[i].GetType() != type)
                    return false;

            return true;
        }
    }
}