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

//        public static SerializedProperty GetSerializedProperty(this PropertyField self)
//        {
//            return GetSerializedPropertyIn(self);
//        }
//
//        // TODO: FIXME: [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_SerializedProperty")]
//        private static extern ref SerializedProperty GetSerializedPropertyIn(PropertyField propertyField);

//        public static T GetSerializedValue<T>(this SerializedProperty property)
//        {
//            object @object = property.serializedObject.targetObject;
//            string[] propertyNames = property.propertyPath.Split('.');
// 
//            // Clear the property path from "Array" and "data[i]".
//            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
//                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();
// 
//            // Get the last object of the property path.
//            foreach (string path in propertyNames)
//            {
//                @object = @object.GetType()
//                    .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
//                    .GetValue(@object);
//            }
// 
//            if (@object.GetType().GetInterfaces().Contains(typeof(IList<T>)))
//            {
//                int propertyIndex = int.Parse(property.propertyPath[property.propertyPath.Length - 2].ToString());
// 
//                return ((IList<T>) @object)[propertyIndex];
//            }
//            else return (T) @object;
//        }
    }
}
