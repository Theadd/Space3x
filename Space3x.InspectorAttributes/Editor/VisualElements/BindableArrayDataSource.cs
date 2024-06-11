using System.Collections;
using System.Reflection;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    public class BindableArrayDataSource<TArray, T>
        where TArray : IList
    {
        public object DeclaringObject;

        public FieldInfo PropertyInfo;

        public int Index;

        public BindableArrayDataSource(object declaringObject, string propertyName, int arrayIndex)
        {
            DeclaringObject = declaringObject;
            Index = arrayIndex;
            PropertyInfo = declaringObject.GetType().GetField(
                propertyName, 
                BindingFlags.Instance 
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public);
        }
        
        [UxmlAttribute, CreateProperty]
        public T Value
        {
            get => (T) ((TArray)PropertyInfo.GetValue(DeclaringObject))[Index];
            set
            {
                IList list = (TArray)PropertyInfo.GetValue(DeclaringObject);
                list[Index] = value;
            }
        }
    }
}