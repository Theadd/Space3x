using System;
using System.Reflection;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Drawers.NonSerialized;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [UxmlElement]
    public partial class BindablePropertyField : VisualElement
    {
        public static readonly string ussClassName = "ui3x-bindable-property-field";
        public static readonly string decoratorDrawersContainerClassName = "unity-decorator-drawers-container";
        public VisualElement Field;
        public IProperty Property;
        public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer ??= CreateDecoratorDrawersContainer();
        private VisualElement m_DecoratorDrawersContainer;
        private PropertyAttributeController m_Controller;

        public BindablePropertyField()
        {
            if (DecoratorDrawersContainer != null)
            {
                this.WithClasses(ussClassName, "unity-property-field");
            }
        }

        private VisualElement CreateDecoratorDrawersContainer()
        {
            var element = new VisualElement();
            element.WithClasses(decoratorDrawersContainerClassName);
            this.Insert(0, element);
            return element;
        }

        public void BindProperty(IProperty property)
        {
            if (!(property is NonSerializedPropertyNode nonSerializedPropertyNode))
                throw new ArgumentException("Invalid IProperty.");
            nonSerializedPropertyNode.Field = this;
            Property = nonSerializedPropertyNode;
            m_Controller = PropertyAttributeController.GetInstance(Property);
            if (m_Controller == null)
                throw new ArgumentException("Unexpected, value for PropertyAttributeController is null.");
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null)
                throw new ArgumentException("Unexpected value.");

            PropertyDrawer drawer = null;
            var propertyDrawer = vType.PropertyDrawer;
            if (propertyDrawer != null && typeof(IDrawer).IsAssignableFrom(propertyDrawer))
            {
                try
                {
                    drawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawer);
                    drawer.SetAttribute(vType.PropertyDrawerAttribute);
                    drawer.SetFieldInfo(vType.RuntimeField);
                    drawer.SetPreferredLabel(Property.DisplayName());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    drawer = null;
                }
            }
            
            if (drawer == null)
            {
                BindToBuiltInField();
            }
            else
            {
                var customDrawer = drawer as ICreateDrawerOnPropertyNode;
                var field = customDrawer.CreatePropertyNodeGUI(Property);
                if (Field != null && Field != field)
                    Field.RemoveFromHierarchy();
                Field = field;
                Add(Field);
            }
        }

        public void AttachDecoratorDrawers()
        {
            DecoratorDrawersContainer.Clear();
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null)
                throw new ArgumentException("Unexpected value.");
            for (var i = 0; i < vType.DecoratorDrawers.Count; i++)
            {
                DecoratorDrawer drawer = null;
                var decorator = vType.DecoratorDrawers[i];
                if (decorator != null)
                {
                    try
                    {
                        drawer = (DecoratorDrawer)Activator.CreateInstance(decorator);
                        drawer.SetAttribute(vType.PropertyAttributes[i]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        drawer = null;
                    }
                    if (drawer != null)
                    {
                        var decoratorElement = drawer.CreatePropertyGUI();
                        if (decoratorElement == null)
                        {
                            Debug.LogError("Custom DecoratorDrawer is not compatible with UI Toolkit. " + drawer.GetType().Name);
                            continue;
                        }

                        DecoratorDrawersContainer.Add(decoratorElement);
                    }
                }
            }
        }
        
        private void BindToBuiltInField()
        {
            var propertyInfo = m_Controller.DeclaringObject.GetType().GetField(Property.Name, 
                BindingFlags.Instance 
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public);
            var isAdded = Field != null;
            VisualElement field = propertyInfo.GetValue(m_Controller.DeclaringObject) switch
            {
                long _ => ConfigureField<LongField, long>(Field as LongField, () => new LongField(label: Property.DisplayName())),
                ulong _ => ConfigureField<UnsignedLongField, ulong>(Field as UnsignedLongField, () => new UnsignedLongField(label: Property.DisplayName())),
                uint _ => ConfigureField<UnsignedIntegerField, uint>(Field as UnsignedIntegerField, () => new UnsignedIntegerField(label: Property.DisplayName())),
                int _ => ConfigureField<IntegerField, int>(Field as IntegerField, () => new IntegerField(label: Property.DisplayName())),
                double _ => ConfigureField<DoubleField, double>(Field as DoubleField, () => new DoubleField(label: Property.DisplayName())),
                float _ => ConfigureField<FloatField, float>(Field as FloatField, () => new FloatField(label: Property.DisplayName())),
                bool _ => ConfigureField<Toggle, bool>(Field as Toggle, () => new Toggle(label: Property.DisplayName())),
                string _ => ConfigureField<TextField, string>(Field as TextField, () => new TextField(label: Property.DisplayName())),
                _ => throw new NotImplementedException($"{propertyInfo.FieldType} not yet implemented in {nameof(BindablePropertyField)}.{nameof(BindToBuiltInField)}().")
            };
            Field = field;
            if (!isAdded)
                this.Add(Field);
        }
        
        private TField ConfigureField<TField, TValue>(
            TField field,
            // SerializedProperty property,
            Func<TField> factory)
            where TField : BaseField<TValue>
        {
            if ((object) field == null)
            {
                field = factory();
                field.RegisterValueChangedCallback<TValue>((evt => 
                    this.OnFieldValueChanged((EventBase) evt)));
                this.dataSource = new BindableDataSource<TValue>(m_Controller.DeclaringObject, Property.Name);
            }
            // string str = this.label ?? property.localizedDisplayName;
            // field.bindingPath = property.propertyPath;
            // field.SetProperty(BaseField<TValue>.serializedPropertyCopyName, (object) property.Copy());
            // field.name = "unity-input-" + property.propertyPath;
            // field.label = str;
            field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
                bindingMode = BindingMode.TwoWay
            });
            // PropertyField.ConfigureFieldStyles<TField, TValue>(field);
            return field;
        }

        private void OnFieldValueChanged(EventBase evt)
        {
            Debug.Log("  IN OnFieldValueChanged(ev);");
        }
    }
}

// [UxmlElement]
// public partial class LocalizedToggle : VisualElement
// {
//     public static readonly BindingId titleProperty = nameof(Title);
//  
//     [UxmlAttribute, CreateProperty]
//     public string Title
//     {
//         get => title;
//         set
//         {
//             if (title == value)
//                 return;
//  
//             title = value;
//             UpdateText();
//             NotifyPropertyChanged(titleProperty);
//         }
//     }
//     private string title = "<KEY>";
//  
//     private Label label;
//     private Label off;
//     private Label on;
//  
//     public LocalizedToggle()
//     {
//         CreateElement();
//         UpdateText();
//     }
//  
//     private void CreateElement()
//     {
//         VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("Components/LocalizedToggle");
//         Add(uxml.CloneTree());
//     }
//  
//     private void UpdateText()
//     {
//         label = this.Q<Label>("title");
//         off = this.Q<Label>("off");
//         on = this.Q<Label>("on");
//  
//         label.text = Title;
//     }
// }
