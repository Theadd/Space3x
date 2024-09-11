using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes;
using Space3x.InspectorAttributes.Editor;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.InspectorAttributes.Types;
using Space3x.Properties.Types;
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
        private BindableElement m_InstanceContainer;
        private ElementType m_ElementType;
        public override TypePickerAttribute Target => (TypePickerAttribute) attribute;
        public ITypeSearchHandler Handler => (ITypeSearchHandler)(((ITypePickerAttribute)Target).Handler ??= 
            new QuickTypeSearchHandler(Target) { IncludeAbstractTypes = m_ElementType != ElementType.Instance });
        
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            DebugLog.Info($"[USK3] [TypePickerDrawer] OnCreatePropertyGUI: {property.PropertyPath}");
            m_TypeField = new TypePickerField()
            {
                label = property.DisplayName(),
                // Handler = Handler,
                OnShowPopup = OnShowPopup,
                OnSelectionChanged = OnSelectionChanged,
                style =
                {
                    flexDirection = FlexDirection.Column,
                    overflow = Overflow.Visible
                }
            };
            // m_TypeField.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            // TODO: Fix this, why binding to a serialized property doesn't synchronize the property value with the UI.
            //       Problem should be around the TypePickerField/BaseField class.
            
            // Binding is not supported for managed references
            if (!property.HasSerializedProperty())
                m_TypeField.BindProperty(property);

            m_ElementType = GetPropertyElementType(property);
            
            // if (property.HasSerializedProperty() || !(property.IsArrayOrList() || property.IsArrayOrListElement())) 
            m_TypeField.TrackPropertyValue(Property, OnPropertyValueChanged);
            if (property.HasSerializedProperty() || !property.IsArrayOrList())
                OnPropertyValueChanged(property);
            if (m_ElementType == ElementType.Instance)
            {
                m_InstanceContainer = new BindableElement();
                m_InstanceContainer.WithClasses(UssConstants.UssTypePickerInstanceContainer);
                property.SetExpanded(true);
                m_InstanceContainer.SetVisible(m_TypeField.value != null);
                m_InstanceContainer.Add(CreatePropertyField(property));
                m_TypeField.Add(m_InstanceContainer);
            }
            
            return m_TypeField;
        }

        private void OnPropertyValueChanged(IPropertyNode propertyNode)
        {
            // TrackPropertyValue on collection elements tracks the collection property itself instead of their elements.
            // TODO: Uncomment.
            if (!Equals(Property, propertyNode))
                return;
            // {
            //     Debug.Log($"[NOTIC3] [TypePickerDrawer] <b>NOT EQUALS!</b> OnPropertyValueChanged: {Property.PropertyPath} != {propertyNode.PropertyPath}");
            //     // return;
            //     if (Property.PropertyPath == propertyNode.PropertyPath)
            //     {
            //         Debug.Log("STOP HERE");
            //         var areEqual = Equals(Property, propertyNode);
            //         Debug.Log(areEqual);
            //     }
            // }
            // else
            // {
            //     Debug.Log($"[NOTIC3] [TypePickerDrawer] <color=#00FF00FF><b>EQUALS!</b></color> OnPropertyValueChanged: {Property.PropertyPath} != {propertyNode.PropertyPath}");
            // }
            var currentValue = Property.GetValue();
            if (Equals(currentValue, m_TypeField.value)) return;
            m_TypeField.value = currentValue;
            if (m_ElementType == ElementType.Instance)
                m_InstanceContainer?.SetVisible(currentValue != null);
        }

        public override void OnUpdate()
        {
            if (m_ElementType == ElementType.Instance)
            {
                m_InstanceContainer?.Clear();
                Property.SetExpanded(true);
                m_InstanceContainer?.Add(CreatePropertyField(Property));
                m_InstanceContainer?.SetVisible(Property.GetUnderlyingValue() != null);
            }
        }

        public void OnShowPopup(IQuickTypeSearch target, VisualElement selectorField, ShowWindowMode mode) =>
            Handler.OnShowPopup(this, target, selectorField, mode);

        private object GetValueFromType(Type type)
        {
            switch (m_ElementType)
            {
                case ElementType.Named:
                    return new NamedType(type);
                case ElementType.Serializable:
                    return new SerializableType(type);
                case ElementType.Instance:
                    return type == null 
                        ? null 
                        : typeof(ScriptableObject).IsAssignableFrom(type) 
                            ? ScriptableObject.CreateInstance(type) 
                            : type.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnSelectionChanged(IEnumerable<Type> newValues) => 
            Property.SetValue(GetValueFromType(newValues.FirstOrDefault()));

        private static ElementType GetPropertyElementType(IPropertyNode property)
        {
            var elementType = (property.GetUnderlyingElementType() ?? property.GetUnderlyingType()) 
                              ?? (property.IsArrayOrListElement() 
                                  ? property.GetParentProperty().GetUnderlyingElementType()
                                  : null);
            return elementType switch
            {
                not null when elementType == typeof(NamedType) => ElementType.Named,
                not null when elementType == typeof(SerializableType) => ElementType.Serializable,
                not null => ElementType.Instance,
                _ => throw new ArgumentException($"GetPropertyElementType() is null for {property.PropertyPath}")
            };
        }
        
        private static VisualElement CreatePropertyField(IPropertyNode property)
        {
            DebugLog.Info($"[USK3] [TypePickerDrawer] CreatePropertyField: {property.PropertyPath}");
            return property.HasSerializedProperty()
                ? new PropertyField(property.GetSerializedProperty())
                : new BindablePropertyField(property, property.IsArrayOrList()).WithClasses(UssConstants
                    .UssShowInInspector);
        }
    }
}
