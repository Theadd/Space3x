using System;
using System.Collections;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class DataSourceObjectBinding : BindableDataSource<UnityEngine.Object>
    {
        public DataSourceObjectBinding(IPropertyNode property) : base(property) { }

        public DataSourceObjectBinding(IPropertyNode property, int index) : base(property, index) { }
    }
    
    // public class DataSourceObjectBinding : IBindableDataSource
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
    //     public DataSourceObjectBinding(IPropertyNode property)
    //     {
    //         if (property is IPropertyNodeIndex propertyNodeIndex)
    //             Bind(propertyNodeIndex.Indexer, propertyNodeIndex.Index);
    //         else
    //             Bind(property);
    //     }
    //
    //     public DataSourceObjectBinding(IPropertyNode property, int index) => Bind(property, index);
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
    //     public UnityEngine.Object Value
    //     {
    //         get
    //         {
    //             if (m_IsNodeIndex)
    //             {
    //                 try
    //                 {
    //                     return ((IList)PropertyInfo.GetValue(DeclaringObject))[Index] as UnityEngine.Object;
    //                 }
    //                 catch (ArgumentOutOfRangeException)
    //                 {
    //                     return (UnityEngine.Object) null;
    //                 }
    //             }
    //             else
    //             {
    //                 return PropertyInfo.GetValue(DeclaringObject) as UnityEngine.Object;
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
    //         set => Value = (UnityEngine.Object) value;
    //     }
    //
    //     internal void NotifyValueChanged() => (m_Property as INonSerializedPropertyNode)?.NotifyValueChanged();
    // }
}
