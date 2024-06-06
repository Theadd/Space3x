using System;
using System.Reflection;
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
    }
}
