// using Unity.Properties;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;
// using Object = UnityEngine.Object;
//
// namespace Space3x.InspectorAttributes.Editor.VisualElements
// {
//     public class MyDataSource
//     {
//         [CreateProperty]
//         public string Name { get; set; }
//         [CreateProperty]
//         public int Level { get; set; }
//     }
//     
//     [UxmlElement]
//     public partial class NonSerializedPropertyField : VisualElement
//     {
//         public static readonly string ussClassName = "ui3x-non-serialized-property-field";
//         public static readonly string decoratorDrawersContainerClassName = "unity-decorator-drawers-container";
//         
//         private VisualElement m_DecoratorDrawersContainer;
//
//         public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer;
//         
//         [UxmlAttribute, CreateProperty]
//         public string Title
//         {
//             get => label.text;
//             set
//             {
//                 if (label.text == value)
//                     return;
//  
//                 label.text = value;
//                 UpdateText();
//                 NotifyPropertyChanged(titleProperty);
//             }
//         }
//
//         public NonSerializedPropertyField()
//         {
//             AddToClassList(ussClassName);
//
//             // // Create a preview image.
//             // m_Preview = new Image();
//             // Add(m_Preview);
//             //
//             // // Create an ObjectField, set its object type, and register a callback when its value changes.
//             // m_ObjectField = new ObjectField();
//             // m_ObjectField.objectType = typeof(Texture2D);
//             // m_ObjectField.RegisterValueChangedCallback(OnObjectFieldValueChanged);
//             // Add(m_ObjectField);
//             //
//             // styleSheets.Add(Resources.Load<StyleSheet>("texture_preview_element"));
//
//             PropertyField a;
//         }
//
//         private void OnCreateCustomGUI()
//         {
//             var element = new VisualElement();
//             
//  
// // Sets a data source that will be available to binding instances on this element and its children.
//             element.dataSource = new MyDataSource
//             {
//                 Name = "Peter",
//                 Level = 9001
//             };
//  
//             var nameLabel = new Label();
//             element.Add(nameLabel);
//  
// // Create a one-way binding from the source to the "text" property of the label.
//             nameLabel.SetBinding(nameof(Label.text), new DataBinding
//             {
//                 dataSourcePath = new PropertyPath(nameof(MyDataSource.Name)),
//                 bindingMode = BindingMode.ToTarget
//             });
//  
//             var levelField = new IntegerField();
//             element.Add(levelField);
//  
// // Create a two-way binding from the source to the "value" property of the integer field. Changes in the UI will be propagated back to the source.
//             levelField.SetBinding(nameof(IntegerField.value), new DataBinding
//             {
//                 dataSourcePath = new PropertyPath(nameof(MyDataSource.Level)),
//                 bindingMode = BindingMode.TwoWay
//             });
//  
//             rootVisualElement.Add(element);
//         }
//     }
// }