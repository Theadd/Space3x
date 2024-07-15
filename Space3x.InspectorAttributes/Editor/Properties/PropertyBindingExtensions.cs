using System;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public static class PropertyBindingExtensions
    {
        public static void BindProperty<TValue>(this BaseField<TValue> field, IPropertyNode property)
        {
            if (property.HasSerializedProperty() && property.GetSerializedProperty() is SerializedProperty serializedProperty) 
                field.BindProperty(serializedProperty);
            else
            {
                field.dataSource = new BindableDataSource<TValue>(property);
                field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
                    bindingMode = BindingMode.TwoWay
                });
            }
        }

        public static void BindProperty<TValue>(this BindableElement element, IPropertyNode property, BindingId bindingId)
        {
            if (property.HasSerializedProperty() && property.GetSerializedProperty() is SerializedProperty serializedProperty) 
                element.BindProperty(serializedProperty);
            else
            {
                element.dataSource = new BindableDataSource<TValue>(property);
                element.SetBinding(bindingId, new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
                    bindingMode = BindingMode.TwoWay
                });
            }
        }

        public static void Unbind<TValue>(this BaseField<TValue> field)
        {
            if (field.HasBinding(nameof(BaseField<TValue>.value)))
                field.ClearBinding(nameof(BaseField<TValue>.value));
            BindingExtensions.Unbind((VisualElement)field);
        }

        public static void TrackPropertyValue(this VisualElement element, IPropertyNode property, Action<IPropertyNode> callback = null)
        {
            if (property.HasSerializedProperty())
                element.TrackPropertyValue(property.GetSerializedProperty(), callback == null ? null : _ => callback(property));
            else
            {
                if (callback != null && property is INonSerializedPropertyNode bindableProperty)
                {
                    bindableProperty.ValueChanged -= callback;
                    bindableProperty.ValueChanged += callback;
                }
            }
        }

        public static void TrackSerializedObjectValue(this VisualElement element, IPropertyNode property, Action callback = null)
        {
            element.TrackSerializedObjectValue(property.GetSerializedObject(), callback == null ? null : _ => callback.Invoke());
        }
    }
}
