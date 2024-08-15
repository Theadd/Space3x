using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Types;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public interface IQuickTypeSearch : IQuickSearchable
    {
        // ITypePickerAttributeHandler Handler { get; set; }
    }
    
    public partial class TypePickerField : IQuickTypeSearch
    {
        public static readonly BindingId ValueProperty = (BindingId) nameof (value);
        
        public Action<IQuickTypeSearch, VisualElement, ShowWindowMode> OnShowPopup;
        public Action<IEnumerable<Type>> OnSelectionChanged;
        // public ITypePickerAttributeHandler Handler { get; set; }

        public void OnShow(QuickSearchElement element)
        {
            element.RegisterValueChangedCallback(OnSelectionChangeHandler);
            element.SetValueWithoutNotify(new List<Type>() {});
            if (m_ValueType != null)
                element.SetValueWithoutNotify(new List<Type>() { m_ValueType });
        }

        public void OnHide(QuickSearchElement element) => 
            element.UnregisterValueChangedCallback(OnSelectionChangeHandler);

        private void OnSelectionChangeHandler(ChangeEvent<IEnumerable<Type>> e)
        {
            DebugLog.Info("OnSelectionChangeHandler: " + string.Join(", ", e.newValue.Select(t => t.ToString())));
            OnSelectionChanged?.Invoke(e.newValue);
        }

        // [CreateProperty]
        public override object value
        {
            // get => this.m_Value;
            // get => base.value;
            get => rawValue;
            // set
            // {
            //     var prevValue = base.value;
            //     if (Equals(prevValue, value))
            //         return;
            //     base.value = value;
            //     if (!Equals(prevValue, base.value))
            //         SetValueFromObject(base.value);
            // }
            set
            {
                Debug.Log($"<color=#FF0000FF>IN TypePickerField.value SETTER!</color>");
                if (Equals(rawValue, value))
                    return;
                base.value = value;
                // rawValue = value;
                SetValueFromObject(value);
                NotifyPropertyChanged(ValueProperty);
            }
        }
        
        private void SetValueFromObject(object newValue)
        {
            Debug.LogError($"<b><color=#FF0000FF>IN SetValueFromObject({newValue})</color></b>");
            switch (newValue)
            {
                case null:
                    SetValue(null);
                    break;
                case Type type:
                    SetValue(type);
                    break;
                case NamedType namedType:
                    SetValue((Type)namedType);
                    break;
                case SerializableType serializableType:
                    SetValue((Type)serializableType);
                    break;
                default:
                    SetValue(newValue.GetType());
                    break;
            }
        }
    }
}
