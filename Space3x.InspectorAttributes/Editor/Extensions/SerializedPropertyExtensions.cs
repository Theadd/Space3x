﻿using System;
using System.Reflection;
using Space3x.Properties.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    public static class SerializedPropertyExtensions
    {
        private static MethodInfo s_PropertyFieldReset = null;

        internal static void AssignToPropertyField(this SerializedProperty property, PropertyField target)
        {
            s_PropertyFieldReset ??= typeof(PropertyField).GetMethod(
                "Reset", 
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(SerializedProperty) },
                null);

            if (s_PropertyFieldReset != null)
                s_PropertyFieldReset.Invoke(target, new object[] { property });
        }

        public static object GetDeclaringObject(this SerializedProperty property)
        {
            var path = property.propertyPath;
            if (path != property.name)
            {
                if (path.EndsWith("." + property.name))
                {
                    var parentPath = path.Substring(0, path.Length - (property.name.Length + 1));
                    var parentProperty = property.serializedObject.FindProperty(parentPath);
                    if (parentProperty != null)
                        return parentProperty.boxedValue;
                    Debug.LogWarning($"Parent property of {property.name} in {path} not found for serializedObject at {parentPath}");
                }
            }
            
            return property.serializedObject.targetObject;
        }
        
        public static string GetParentPath(this SerializedProperty prop)
        {
            if (prop == null || prop.propertyPath == prop.name)
                return "";
            if (prop.propertyPath.EndsWith("." + prop.name))
                return prop.propertyPath[..^(prop.name.Length + 1)];
            else
            {
                if (PropertyExtensions.IsPropertyIndexer(prop.propertyPath, out var fieldName, out var index))
                {
                    var lastDotIndex = fieldName.LastIndexOf('.');
                    if (lastDotIndex < 0)
                        return "";
                    var lastPart = fieldName[(lastDotIndex + 1)..] + ".Array.data[" + index + "]";
                    if (!prop.propertyPath.EndsWith("." + lastPart))
                        throw new Exception(
                            $"Unexpected. PropertyPath \"{prop.propertyPath}\" is not ending with \".{lastPart}\".");
                    return prop.propertyPath[..^(lastPart.Length + 1)];
                }
                else
                {
                    Debug.LogError($"Case not implemented in SerializedProperty.GetParentPath() for: {prop.propertyPath}");
                    return prop.propertyPath;
                }
            }
        }

        internal static int GetParentObjectHash(this SerializedProperty prop)
        {
            var parentPath = prop.GetParentPath();
            if (string.IsNullOrEmpty(parentPath))
                return prop.serializedObject.targetObject.GetInstanceID() * 397;
            else
                return prop.serializedObject.targetObject.GetInstanceID() * 397 ^ parentPath.GetHashCode();
        }

        public static IPropertyNode GetPropertyNode(this SerializedProperty prop)
        {
            var c = PropertyAttributeController.GetInstance(prop);
            IPropertyNode n = null;
            if (PropertyExtensions.IsPropertyIndexer(prop.propertyPath, out var fieldName, out var index))
                n = c?.GetProperty(fieldName, index);
            else
                n = c?.GetProperty(prop.name);

            return n;
        }
    }
}
