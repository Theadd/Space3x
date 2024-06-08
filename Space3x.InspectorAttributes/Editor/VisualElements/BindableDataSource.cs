using System.Reflection;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    public class BindableDataSource<T>
    {
        public object DeclaringObject;

        public FieldInfo PropertyInfo;

        public BindableDataSource(object declaringObject, string propertyName)
        {
            DeclaringObject = declaringObject;
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
            get => (T) PropertyInfo.GetValue(DeclaringObject);
            set
            {
                // if (((T) PropertyInfo.GetValue(DeclaringObject)) == value)
                //     return;
 
                PropertyInfo.SetValue(DeclaringObject, value);
                // UpdateText();
                // NotifyPropertyChanged(ValueProperty);
            }
        }
    }
}