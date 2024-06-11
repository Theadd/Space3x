using System;
using System.Reflection;
using Space3x.InspectorAttributes.Editor.Drawers.NonSerialized;
using Space3x.InspectorAttributes.Editor.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    public static class SerializedPropertyExtensions
    {
        public static bool TryCreateInvokable<TIn, TOut>(
            this SerializedProperty self,
            string memberName, 
            out Invokable<TIn, TOut> invokableMember)
        {
            invokableMember = ReflectionUtility.CreateInvokable<TIn, TOut>(memberName, self); 
            return invokableMember != null;
        }

        private static MethodInfo s_PropertyFieldReset = null;

        public static void AssignToPropertyField(this SerializedProperty property, PropertyField target)
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

        public static int GetDecoratorsContainerHash(this SerializedProperty property) => 
            property.serializedObject.GetHashCode();

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
                    // return fieldName;
                }
                else
                {
                    Debug.LogError($"Case not implemented in SerializedProperty.GetParentPath() for: {prop.propertyPath}");
                    return prop.propertyPath;
                }
            }
        }

        public static int GetParentObjectHash(this SerializedProperty prop)
        {
            var parentPath = prop.GetParentPath();
            if (string.IsNullOrEmpty(parentPath))
                return prop.serializedObject.targetObject.GetInstanceID();
            else
                return prop.serializedObject.targetObject.GetInstanceID() ^ parentPath.GetHashCode();
        }

        public static IProperty GetPropertyNode(this SerializedProperty prop)
        {
            // return PropertyAttributeController.GetInstance(prop)?.GetProperty(prop.name);
            var c = PropertyAttributeController.GetInstance(prop);
            IProperty n = null;
            if (PropertyExtensions.IsPropertyIndexer(prop.propertyPath, out var fieldName, out var index))
            {
                n = c?.GetProperty(fieldName, index);
                Debug.Log($"fieldName: {fieldName}, index: {index}, n.PropertyPath: {n?.PropertyPath}");
            }
            else
            {
                n = c?.GetProperty(prop.name);
            }
            
            Debug.Log($"@GetPropertyNode: c is null? {(c == null)}; n is null? {(n == null)}; prop.name = {prop.name}; prop.propertyPath = {prop.propertyPath}");
            return n;
        }
    }
}
