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

        public IPropertyNode GetPropertyNode();
    }
    
    public abstract class BindableDataSource<T> : IBindableDataSource 
        where T : class
    {
        public object DeclaringObject;

        public FieldInfo PropertyInfo;

        public int Index = -1;

        private bool m_IsNodeIndex = false;

        private IPropertyNode m_Property;

        public IPropertyNode GetPropertyNode() => m_IsNodeIndex ? m_Property.GetArrayElementAtIndex(Index) : m_Property;

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
            get
            {
                if (m_IsNodeIndex)
                {
                    try
                    {
                        return ((IList)PropertyInfo.GetValue(DeclaringObject))[Index] as T;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return (T) null;
                    }
                }
                else
                {
                    return PropertyInfo.GetValue(DeclaringObject) as T;
                }
            }
            set
            {
                Debug.Log("@BindableDataSource.Value SETTER! " + m_Property.Name);
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
                        PropertyInfo.SetValue(DeclaringObject, value);
                    }
                    catch (Exception e)
                    {
                        notify = false;
                        Debug.LogException(e);
                    }
                }
                if (notify && m_Property is INonSerializedPropertyNode)
                    NotifyValueChanged();
            }
        }

        public object BoxedValue
        {
            get => Value;
            set => Value = (T) value;
        }

        internal void NotifyValueChanged() => (m_Property as INonSerializedPropertyNode)?.NotifyValueChanged();
    }
}
