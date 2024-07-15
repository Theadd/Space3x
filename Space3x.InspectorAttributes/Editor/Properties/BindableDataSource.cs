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

        private IPropertyNode m_Property;

        public BindableDataSource(IPropertyNode property)
        {
            if (property is IPropertyNodeIndex propertyNodeIndex)
                Bind(propertyNodeIndex.Indexer, propertyNodeIndex.Index);
            else
                Bind(property);
        }

        public BindableDataSource(IPropertyNode property, int index) => Bind(property, index);

        private void Bind(IPropertyNode property, int index)
        {
            m_IsNodeIndex = true;
            Index = index;
            Bind(property);
        }
        
        private void Bind(IPropertyNode property)
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
                    try
                    {
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
            set => Value = (T)value;
        }

        internal void NotifyValueChanged() => (m_Property as INonSerializedPropertyNode)?.NotifyValueChanged();
    }
}
