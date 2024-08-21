using System;
using System.Collections.Generic;
using Space3x.InspectorAttributes.Types;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public interface IQuickTypeSearch : IQuickSearchable { }
    
    public partial class TypePickerField : IQuickTypeSearch
    {
        public static readonly BindingId ValueProperty = (BindingId) nameof (value);
        
        public Action<IQuickTypeSearch, VisualElement, ShowWindowMode> OnShowPopup;
        public Action<IEnumerable<Type>> OnSelectionChanged;

        public void OnShow(QuickSearchElement element)
        {
            element.RegisterValueChangedCallback(OnSelectionChangeHandler);
            element.SetValueWithoutNotify(new List<Type>() {});
            if (m_ValueType != null)
                element.SetValueWithoutNotify(new List<Type>() { m_ValueType });
        }

        public void OnHide(QuickSearchElement element) => 
            element.UnregisterValueChangedCallback(OnSelectionChangeHandler);

        private void OnSelectionChangeHandler(ChangeEvent<IEnumerable<Type>> e) => 
            OnSelectionChanged?.Invoke(e.newValue);

        public override object value
        {
            get => rawValue;
            set
            {
                if (Equals(rawValue, value))
                    return;
                base.value = value;
                SetValueFromObject(value);
                NotifyPropertyChanged(ValueProperty);
            }
        }
        
        private void SetValueFromObject(object newValue)
        {
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
