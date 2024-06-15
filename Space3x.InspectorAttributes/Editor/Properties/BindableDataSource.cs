using System;
using System.Collections;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public interface IBindableDataSource
    {
        public object BoxedValue { get; set; }
    }
    
    public class BindableDataSource<T> : IBindableDataSource
    {
        public object DeclaringObject;

        public FieldInfo PropertyInfo;

        public int Index = -1;

        private bool m_IsNodeIndex = false;

        private IProperty m_Property;

        public BindableDataSource(IProperty property)
        {
            if (property is IPropertyNodeIndex propertyNodeIndex)
                Bind(propertyNodeIndex.Indexer, propertyNodeIndex.Index);
            else
                Bind(property);
        }

        public BindableDataSource(IProperty property, int index) => Bind(property, index);

        private void Bind(IProperty property, int index)
        {
            m_IsNodeIndex = true;
            Index = index;
            Bind(property);
        }
        
        private void Bind(IProperty property)
        {
            m_Property = property;
            DeclaringObject = property.GetDeclaringObject();
            PropertyInfo = DeclaringObject.GetType().GetField(
                property.Name, 
                BindingFlags.Instance 
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public);
        }
        
        [UxmlAttribute, CreateProperty]
        public T Value
        {
            get => (T) (m_IsNodeIndex ? ((IList)PropertyInfo.GetValue(DeclaringObject))[Index] : PropertyInfo.GetValue(DeclaringObject));
            set
            {
                var notify = true;
                if (m_IsNodeIndex)
                {
                    IList list = (IList)PropertyInfo.GetValue(DeclaringObject);
                    list[Index] = value;
                }
                else
                {
                    Debug.Log($"typeof(t)={typeof(T).FullName}; typeof(value)={value?.GetType().Name}; value={value}; FieldType.Name={PropertyInfo.FieldType.Name}");
                    try
                    {
                        // RuntimeHelpers.Equals() // TODO
                        PropertyInfo.SetValue(DeclaringObject, (T)value);
                    }
                    catch (Exception e)
                    {
                        notify = false;
                        Debug.LogException(e);
                    }
                }
                if (notify && m_Property is INonSerializedPropertyNode bindableProperty)
                    NotifyValueChanged();
            }
        }

        public object BoxedValue
        {
            get => (object) (T) Value;
            set
            {
                Debug.Log($"<b><color=#000000FF>// TODO: Uncomment BoxedValue setter of BindableDataSource.</color></b>");
                // Value = (T)value;
            }
        }

        internal void NotifyValueChanged() => (m_Property as INonSerializedPropertyNode)?.NotifyValueChanged();
    }
}
