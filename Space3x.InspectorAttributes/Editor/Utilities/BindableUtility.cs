using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Utilities
{
    [ExcludeFromDocs]
    public static class BindableUtility
    {
        public static void AutoNotifyValueChangedOnNonSerialized(VisualElement element, IPropertyNode property)
        {
            if (property is not INonSerializedPropertyNode bindableProperty) return;
            object gValue = (
                    element.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance)
                    ?? element.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
                )
                ?.GetValue(element);
            if (gValue == null) return;
            dynamic dynValue = gValue;
            AutoNotifyValueChangedOnNonSerializedWrapper(dynValue, element, bindableProperty);
        }
        
        private static void AutoNotifyValueChangedOnNonSerializedWrapper<T>(T dynamicValue, VisualElement element, INonSerializedPropertyNode property)
        {
            if (element is INotifyValueChanged<T> control)
            {
                control.RegisterValueChangedCallback<T>(ev =>
                {
                    if (element.dataSource is DataSourceBinding sourceBinding)
                    {
                        if (RuntimeHelpers.Equals((T) sourceBinding.Value, ev.newValue) && !RuntimeHelpers.Equals(ev.previousValue, ev.newValue))
                            sourceBinding.NotifyValueChanged();
                    }
                    else
                    {
                        property.NotifyValueChanged(property);
                    }
                });
            }
        }
        
        public static void RegisterValueChangedCallback(VisualElement element, Action callback)
        {
            object gValue = (
                    element.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance)
                    ?? element.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
                    )
                ?.GetValue(element);
            if (gValue == null) return;
            dynamic dynValue = gValue;
            RegisterValueChangeWrapper(dynValue, element, callback);
        }
        
        private static void RegisterValueChangeWrapper<T>(T dynamicValue, VisualElement element, Action callback)
        {
            if (element is INotifyValueChanged<T> control) 
                control.RegisterValueChangedCallback((_) => callback());
        }
        
        public static void SetValueWithoutNotify(VisualElement element, object value)
        {
            object gValue = (
                    element.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance)
                    ?? element.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
                )
                ?.GetValue(element);
            if (gValue == null) return;
            dynamic dynValue = gValue;
            SetValueWithoutNotifyWrapper(dynValue, element, value);
        }
        
        private static void SetValueWithoutNotifyWrapper<T>(T dynamicValue, VisualElement element, object value)
        {
            if (element is INotifyValueChanged<T> control) 
                control.SetValueWithoutNotify((T) value);
        }
    }
}
