using System;
using System.Reflection;
using Space3x.InspectorAttributes.Editor.Utilities;
using UnityEditor;
using UnityEditor.UIElements;

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
    }
}
