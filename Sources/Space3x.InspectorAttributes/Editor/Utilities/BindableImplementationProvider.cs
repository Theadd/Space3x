using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Space3x.Properties.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    [InitializeOnLoad]
    internal class BindableImplementationProvider : IBindableUtility
    {
        static BindableImplementationProvider() =>
            BindableUtility.RegisterImplementationProvider(new BindableImplementationProvider());
        
        public void AutoNotifyValueChangedOnNonSerialized(VisualElement element, IPropertyNode property)
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
                        Debug.Log($"<color=#00FF00FF>AutoNotifyValueChangedOnNonSerializedWrapper ON dataSource</color>");
                        if (RuntimeHelpers.Equals((T) sourceBinding.Value, ev.newValue) && !RuntimeHelpers.Equals(ev.previousValue, ev.newValue))
                            sourceBinding.NotifyValueChanged();
                    }
                    else
                    {
                        Debug.Log($"<color=#00FF00FF>AutoNotifyValueChangedOnNonSerializedWrapper ON property</color>");
                        property.NotifyValueChanged(property);
                    }
                });
            }
        }
        
        public void RegisterValueChangedCallback(VisualElement element, Action callback)
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
        
        public void SetValueWithoutNotify(VisualElement element, object value)
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
            else if (dynamicValue is Enum && element is INotifyValueChanged<Enum> dropdown)
                dropdown.SetValueWithoutNotify((Enum)value);
        }
    }
}
