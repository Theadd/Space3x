using System;
using System.Collections;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class DataSourceBinding : BindableDataSource<object>
    {
        public DataSourceBinding(IPropertyNode property) : base(property) { }

        public DataSourceBinding(IPropertyNode property, int index) : base(property, index) { }
    }

    // public class DataSourceBinding : IBindableDataSource
    // {
    //     public object DeclaringObject;
    //
    //     public FieldInfo PropertyInfo;
    //
    //     public int Index = -1;
    //
    //     private bool m_IsNodeIndex = false;
    //
    //     private IPropertyNode m_Property;
    //
    //     public DataSourceBinding(IPropertyNode property)
    //     {
    //         if (property is IPropertyNodeIndex propertyNodeIndex)
    //             Bind(propertyNodeIndex.Indexer, propertyNodeIndex.Index);
    //         else
    //             Bind(property);
    //     }
    //
    //     public DataSourceBinding(IPropertyNode property, int index) => Bind(property, index);
    //
    //     private void Bind(IPropertyNode property, int index)
    //     {
    //         m_IsNodeIndex = true;
    //         Index = index;
    //         Bind(property);
    //     }
    //     
    //     private void Bind(IPropertyNode property)
    //     {
    //         m_Property = property;
    //         DeclaringObject = property.GetDeclaringObject();
    //         PropertyInfo = DeclaringObject.GetType().GetField(
    //             property.Name, 
    //             BindingFlags.Instance 
    //             | BindingFlags.Static
    //             | BindingFlags.NonPublic
    //             | BindingFlags.Public);
    //     }
    //     
    //     public T GetValue<T>() => (T) (m_IsNodeIndex ? ((IList)PropertyInfo.GetValue(DeclaringObject))[Index] : PropertyInfo.GetValue(DeclaringObject));
    //     
    //     public void SetValue<T>(T value)
    //     {
    //         var notify = true;
    //         if (m_IsNodeIndex)
    //         {
    //             IList list = (IList)PropertyInfo.GetValue(DeclaringObject);
    //             list[Index] = value;
    //         }
    //         else
    //         {
    //             try
    //             {
    //                 PropertyInfo.SetValue(DeclaringObject, (T)value);
    //             }
    //             catch (Exception e)
    //             {
    //                 notify = false;
    //                 Debug.LogException(e);
    //             }
    //         }
    //         if (notify && m_Property is INonSerializedPropertyNode)
    //             NotifyValueChanged();
    //     }
    //
    //     [UxmlAttribute, CreateProperty]
    //     public object Value
    //     {
    //         get
    //         {
    //             if (m_IsNodeIndex)
    //             {
    //                 try
    //                 {
    //                     return ((IList)PropertyInfo.GetValue(DeclaringObject))[Index];
    //                 }
    //                 catch (ArgumentOutOfRangeException)
    //                 {
    //                     return null;
    //                 }
    //             }
    //             else
    //             {
    //                 return PropertyInfo.GetValue(DeclaringObject);
    //             }
    //         }
    //         set
    //         {
    //             var notify = true;
    //             if (m_IsNodeIndex)
    //             {
    //                 IList list = (IList)PropertyInfo.GetValue(DeclaringObject);
    //                 list[Index] = value;
    //             }
    //             else
    //             {
    //                 try
    //                 {
    //                     PropertyInfo.SetValue(DeclaringObject, value);
    //                 }
    //                 catch (Exception e)
    //                 {
    //                     notify = false;
    //                     Debug.LogException(e);
    //                 }
    //             }
    //             if (notify && m_Property is INonSerializedPropertyNode)
    //                 NotifyValueChanged();
    //         }
    //     }
    //
    //     public object BoxedValue
    //     {
    //         get => Value;
    //         set => Value = value;
    //     }
    //
    //     internal void NotifyValueChanged() => (m_Property as INonSerializedPropertyNode)?.NotifyValueChanged();
    // }
}
