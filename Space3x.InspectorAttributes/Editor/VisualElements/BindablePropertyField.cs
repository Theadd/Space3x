using System;
using System.Reflection;
using Space3x.UiToolkit.Types;
using Unity.Properties;
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
        public object DeclaringObject;
        public string PropertyName;
        public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer ??= CreateDecoratorDrawersContainer();
        private VisualElement m_DecoratorDrawersContainer;

        public BindablePropertyField()
        {
            if (DecoratorDrawersContainer != null)
            {
                this.WithClasses(ussClassName);
            }
        }

        private VisualElement CreateDecoratorDrawersContainer()
        {
            var element = new VisualElement();
            element.WithClasses(decoratorDrawersContainerClassName);
            this.Insert(0, element);
            return element;
        }

        public void BindTo(object declaringObject, string propertyName)
        {
            DeclaringObject = declaringObject;
            PropertyName = propertyName;
            var propertyInfo = declaringObject.GetType().GetRuntimeField(propertyName);
            VisualElement field = propertyInfo.GetValue(declaringObject) switch
            {
                long _ => ConfigureField<LongField, long>(Field as LongField, (Func<LongField>)(() => new LongField())),
                ulong _ => ConfigureField<UnsignedLongField, ulong>(Field as UnsignedLongField,
                    (Func<UnsignedLongField>)(() => new UnsignedLongField())),
                uint _ => ConfigureField<UnsignedIntegerField, uint>(Field as UnsignedIntegerField,
                    (Func<UnsignedIntegerField>)(() => new UnsignedIntegerField())),
                int _ => ConfigureField<IntegerField, int>(Field as IntegerField, () => new IntegerField()),
                double _ => ConfigureField<DoubleField, double>(Field as DoubleField, () => new DoubleField()),
                float _ => ConfigureField<FloatField, float>(Field as FloatField, () => new FloatField()),
                bool _ => ConfigureField<Toggle, bool>(Field as Toggle, () => new Toggle()),
                string _ => ConfigureField<TextField, string>(Field as TextField, () => new TextField()),
                _ => throw new NotImplementedException($"{propertyInfo.FieldType} not yet implemented in {nameof(BindablePropertyField)}.{nameof(BindTo)}().")
            };
            Field = field;
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
                this.dataSource = new BindableDataSource<TValue>(DeclaringObject, PropertyName);
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
