using System;
using System.Collections;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.Properties.Types;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [GeneratePropertyBag]
    public abstract class BindableDataSource<T> : IDataSourceViewHashProvider, IBindableDataSource
        where T : class
    {
        public object DeclaringObject;

        public FieldInfo PropertyInfo;

        public int Index = -1;

        private bool m_IsNodeIndex = false;

        private long m_ViewVersion;
        
        private IPropertyNode m_Property;
        
        private IPropertyNode m_PropertyNodeIndex;
        
        public long GetViewHashCode() => m_ViewVersion;

        public IPropertyNode GetPropertyNode() => m_IsNodeIndex ? (m_PropertyNodeIndex ??= AssignTo(m_Property.GetArrayElementAtIndex(Index))) : m_Property;

        public BindableDataSource(IPropertyNode property)
        {
            if (property is IPropertyNodeIndex propertyNodeIndex)
            {
                m_PropertyNodeIndex = AssignTo(property);
                Bind(propertyNodeIndex.Indexer, propertyNodeIndex.Index);
            }
            else
                Bind(property);
        }

        private IPropertyNode AssignTo(IPropertyNode property)
        {
            if (property is IBindablePropertyNode bindableProperty)
                bindableProperty.DataSource = this;
            return property;
        }

        private void Bind(IPropertyNode property, int index)
        {
            m_IsNodeIndex = true;
            Index = index;
            Bind(property);
        }
        
        private void Bind(IPropertyNode property)
        {
            m_Property = property;
            if (!m_IsNodeIndex && m_Property is IBindablePropertyNode bindableProperty)
                AssignTo(bindableProperty);
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

                return PropertyInfo.GetValue(DeclaringObject) as T;
            }
            set
            {
                // DebugLog.Error("SETTER " + (m_PropertyNodeIndex ?? m_Property).PropertyPath);
                if (Equals(Value, value))
                    return;
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
                if (notify)
                {
                    // DebugLog.Error("NOTIFY " + (m_PropertyNodeIndex ?? m_Property).PropertyPath);
                    ++m_ViewVersion;
                    NotifyValueChanged();
                }
            }
        }

        public object BoxedValue
        {
            get => Value;
            set => Value = (T) value;
        }

        public void IncreaseVersionNumber() => ++m_ViewVersion;

        internal void NotifyValueChanged()
        {
            if ((m_PropertyNodeIndex ?? m_Property) is BindablePropertyNode propertyNode && propertyNode.IsUnreliable())
                propertyNode.Controller?.EventHandler.NotifyValueChanged(propertyNode);
            else
            {
                // ++m_ViewVersion;
                (m_Property as INonSerializedPropertyNode)?.NotifyValueChanged(m_PropertyNodeIndex ?? m_Property);
            }
        }
    }
}
