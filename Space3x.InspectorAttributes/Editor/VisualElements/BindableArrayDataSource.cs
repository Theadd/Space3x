// using System.Collections;
// using System.Reflection;
// using Unity.Properties;
// using UnityEngine.UIElements;
//
// namespace Space3x.InspectorAttributes.Editor.VisualElements
// {
//     public class BindableDataSource<T>
//     {
//         public object DeclaringObject;
//
//         public FieldInfo PropertyInfo;
//
//         public int Index = -1;
//
//         private bool m_IsNodeIndex = false;
//
//         private IProperty m_Property;
//
//         public BindableDataSource(IProperty property)
//         {
//             if (property is IPropertyNodeIndex propertyNodeIndex)
//                 Bind(propertyNodeIndex.Indexer, propertyNodeIndex.Index);
//             else
//                 Bind(property);
//         }
//
//         public BindableDataSource(IProperty property, int index) => Bind(property, index);
//
//         private void Bind(IProperty property, int index)
//         {
//             m_IsNodeIndex = true;
//             Index = index;
//             Bind(property);
//         }
//         
//         private void Bind(IProperty property)
//         {
//             m_Property = property;
//             DeclaringObject = property.GetDeclaringObject();
//             PropertyInfo = DeclaringObject.GetType().GetField(
//                 property.Name, 
//                 BindingFlags.Instance 
//                 | BindingFlags.Static
//                 | BindingFlags.NonPublic
//                 | BindingFlags.Public);
//         }
//         
//         [UxmlAttribute, CreateProperty]
//         public T Value
//         {
//             get => (T) (m_IsNodeIndex ? ((IList)PropertyInfo.GetValue(DeclaringObject))[Index] : PropertyInfo.GetValue(DeclaringObject));
//             set
//             {
//                 if (m_IsNodeIndex)
//                 {
//                     IList list = (IList)PropertyInfo.GetValue(DeclaringObject);
//                     list[Index] = value;
//                 }
//                 else
//                     PropertyInfo.SetValue(DeclaringObject, value);
//
//                 if (m_Property is INonSerializedPropertyNode bindableProperty)
//                     bindableProperty.NotifyValueChanged();
//             }
//         }        
//         // [UxmlAttribute, CreateProperty]
//         // public T Value
//         // {
//         //     get => (T) ((IList)PropertyInfo.GetValue(DeclaringObject))[Index];
//         //     set
//         //     {
//         //         IList list = (IList)PropertyInfo.GetValue(DeclaringObject);
//         //         list[Index] = value;
//         //     }
//         // }
//     }
// }