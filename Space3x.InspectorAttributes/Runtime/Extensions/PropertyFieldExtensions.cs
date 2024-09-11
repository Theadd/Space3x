#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
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
        
        /// <summary>
        /// Removes the <see cref="PropertyField"/> from the VisualElement's hierarchy, properly removing
        /// all its decorator drawers.
        /// </summary>
        public static void ProperlyRemoveFromHierarchy(this PropertyField propertyField)
        {
            var decoratorDrawersContainer = propertyField.Children()
                .FirstOrDefault(c => c.ClassListContains(UssConstants.UssDecoratorDrawersContainer));
            if (decoratorDrawersContainer != null)
            {
                for (var i = decoratorDrawersContainer.hierarchy.childCount - 1; i >= 0; i--)
                    if (decoratorDrawersContainer.hierarchy[i] is GhostDecorator ghostDecorator)
                        ghostDecorator.TargetDecorator.ProperlyRemoveFromHierarchy();
                decoratorDrawersContainer.Clear();
            }
            try
            {
                propertyField.RemoveFromHierarchy();
            }
            catch (Exception e)
            {
                DebugLog.Error(e.ToString());
            }
        }
    }
}
#endif