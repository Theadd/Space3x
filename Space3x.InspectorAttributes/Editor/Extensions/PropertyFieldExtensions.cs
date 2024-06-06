using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

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
            
            // return s_SerializedPropertyInPropertyField?.GetValue(propertyField) as SerializedProperty;
            return s_SerializedPropertyInPropertyField != null ? (SerializedProperty) s_SerializedPropertyInPropertyField.GetValue(propertyField) : null;
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
        
        public static void RebuildChildDecoratorDrawersIfNecessary(this PropertyField parentField, SerializedProperty parentProperty = null)
        {
            parentProperty ??= parentField.GetSerializedProperty();
            var parentPath = parentProperty.propertyPath;
            var property = parentProperty.Copy();
            var visitedNodes = new HashSet<long>();

            SerializedProperty endProperty = property.GetEndProperty();
            var allChildFields = parentField.GetChildren<PropertyField>();
            var childFieldsByPath = new Dictionary<string, PropertyField>();
            foreach (var childField in allChildFields)
            {
                var pName = childField.name ?? "";
                pName = pName.Replace("unity-property-field-", "");
                if (!string.IsNullOrEmpty(pName) && !childFieldsByPath.ContainsKey(pName))
                    childFieldsByPath.Add(pName, childField);
            }
            bool visitChild;
            do
            {
                visitChild = false;
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    long refId = property.managedReferenceId;
                    if (visitedNodes.Add(refId))
                        visitChild = true; // First time seeing node, so visit it
                }
                
                var childProperty = property.Copy();
                if (childFieldsByPath.TryGetValue(childProperty.propertyPath, out var childField))
                {
                    var childAssignedTo = childField.GetSerializedProperty()?.propertyPath ?? "";
                    if (childProperty.propertyPath != childAssignedTo || childField.hierarchy.childCount <= 0)
                    {
                        var prevNestingLevel = childField.GetDrawNestingLevel();
                        childField.SetDrawNestingLevel(0);
                        childProperty.AssignToPropertyField(childField);
                        childField.SetDrawNestingLevel(prevNestingLevel);
                        if (childProperty.propertyPath.StartsWith(parentPath))
                        {
                            var childRelativePath = childProperty.propertyPath.Substring(parentPath.Length + 1);
                            var rebindTo = parentProperty.FindPropertyRelative(childRelativePath);
                            if (rebindTo != null)
                            {
                                childField.BindProperty(rebindTo);
                                childField.MarkDirtyRepaint();
                            }
                            else
                            {
                                Debug.Log($"                      !!!! FAILED TO REBIND WITH RELATIVE PATH: {childRelativePath}");
                            }
                        }
                        else
                        {
                            Debug.Log($"                      !!!! FAILED TO REBIND FIELD AT: {childProperty.propertyPath}");
                        }
                    }
                }
                else
                {
                    if (childProperty.propertyPath != parentPath)
                        Debug.Log($"Not found: {childProperty.propertyPath} ON {parentPath}");
                }
            } while (property.Next(visitChild) && !SerializedProperty.EqualContents(property, endProperty));
            // } while (property.NextVisible(visitChild) && !SerializedProperty.EqualContents(property, endProperty));
            endProperty = (SerializedProperty) null;
        }
    }
}
