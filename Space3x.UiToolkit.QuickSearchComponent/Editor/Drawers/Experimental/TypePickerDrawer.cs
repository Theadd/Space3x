using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Types;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TypePickerAttribute), useForChildren: true)]
    public class TypePickerDrawer : Drawer<TypePickerAttribute>
    {
        private enum ElementType
        {
            Instance = 0,
            Named = 1,
            Serializable = 2,
        } 
        
        private TypePickerField m_TypeField;
        private ElementType m_ElementType;
        public override TypePickerAttribute Target => (TypePickerAttribute) attribute;
        public ITypeSearchHandler Handler => (ITypeSearchHandler)(Target.Handler ??= new QuickTypeSearchHandler(Target));
        
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            m_TypeField = new TypePickerField()
            {
                label = property.DisplayName(),
                // Handler = Handler,
                OnShowPopup = OnShowPopup,
                OnSelectionChanged = OnSelectionChanged,
            };
            m_TypeField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            // TODO: Fix this, why binding to a serialized property doesn't synchronize the property value with the UI.
            //       Problem should be around the TypePickerField/BaseField class.
            m_TypeField.BindProperty(property);

            var elementType = Property.GetUnderlyingElementType() ?? Property.GetUnderlyingType();
            m_ElementType = elementType switch
            {
                not null when elementType == typeof(NamedType) => ElementType.Named,
                not null when elementType == typeof(SerializableType) => ElementType.Serializable,
                not null => ElementType.Instance,
                _ => throw new ArgumentException($"Property.GetUnderlyingElementType() is null for {Property.PropertyPath}")
            };
            
            if (Property.HasSerializedProperty())
            {
                m_TypeField.TrackPropertyValue(Property, OnPropertyValueChanged);
                OnPropertyValueChanged(Property);
                
                // EDIT
                var bindableContainer = new BindableElement();
                var inlineManagedObject = new PropertyField(Property.GetSerializedProperty()).WithClasses("ui3x-as-managed-reference-d");
                // inlineManagedObject.SetEnabled(Target.ContentEnabled);
                bindableContainer.Add(inlineManagedObject);
                m_TypeField.Add(bindableContainer);
            }
            
            return m_TypeField;
        }

        private void OnPropertyValueChanged(IPropertyNode propertyNode)
        {
            Debug.Log("2.0");
            var currentValue = Property.GetUnderlyingValue();
            if (Equals(currentValue, m_TypeField.value)) return;
            m_TypeField.value = currentValue;
            Debug.Log("2.1");
        }

        public override void OnUpdate()
        {
            Debug.LogWarning("// TODO: OnUpdate - Just a check to see if this method gets called somehow.");
            // throw new Exception("// TODO: OnUpdate - Just a check to see if this method gets called somehow.");
        }

        public void OnShowPopup(IQuickTypeSearch target, VisualElement selectorField, ShowWindowMode mode) =>
            Handler.OnShowPopup(Property, target, selectorField, mode);

        private object GetValueFromType(Type type)
        {
            switch (m_ElementType)
            {
                case ElementType.Named:
                    return new NamedType(type);
                case ElementType.Serializable:
                    return new SerializableType(type);
                case ElementType.Instance:
                    return type?.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void OnSelectionChanged(IEnumerable<Type> newValues)
        {
            // var newValue = new NamedType(newValues.FirstOrDefault());
            var newValue = GetValueFromType(newValues.FirstOrDefault());
            Debug.Log("1.0 " + Property.PropertyPath);
            // // m_TypeField.value = newValue;
            // if (Property.HasSerializedProperty() &&
            //     Property.GetSerializedProperty() is SerializedProperty serializedProperty)
            // {
            //     Debug.Log("1.1S");
            //     serializedProperty.boxedValue = newValue;
            //     // serializedProperty.serializedObject.ApplyModifiedProperties();
            //     // serializedProperty.serializedObject.UpdateIfRequiredOrScript();
            // }
            // else
            // {
            //     Debug.Log("1.1N");
            //     m_TypeField.value = newValue;
            // }
            // Debug.Log("1.2");

            if (!Property.HasSerializedProperty())
            {
                m_TypeField.value = newValue;
                return;
            }
            
            // Get ready to save modified values on serialized object
            if (Property.GetSerializedObject().hasModifiedProperties)
                Property.GetSerializedObject().ApplyModifiedPropertiesWithoutUndo();
            Property.GetSerializedObject().Update();
            Debug.Log("1.1");
            // Modify values
            // m_TypeField.value = newValue;
            // if (Property.HasSerializedProperty())
                Property.SetUnderlyingValue(newValue);
            Debug.Log("1.2");
            // Save modified values on serialized object 
            if (Property.GetSerializedObject().hasModifiedProperties)
                Property.GetSerializedObject().ApplyModifiedProperties();
            else
                Property.GetSerializedObject().Update();
            Debug.Log("1.3");
        }
    }
}
