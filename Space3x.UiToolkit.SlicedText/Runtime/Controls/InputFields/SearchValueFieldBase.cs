using System.Collections.Generic;
using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.InputFields
{
    [UxmlElement]
    public abstract partial class SearchValueFieldBase<TBaseField, TValue, TInputField> : VisualElement, INotifyValueChanged<TValue>
        where TInputField : TextValueInputField<TValue>, new()
        where TBaseField : TextBaseField<TValue, TInputField>, new()
    {
        private static readonly BindingId valueProperty = (BindingId) nameof (value);
        public static readonly string ussClassName = "unity-search-field-base";
        public static readonly string textUssClassName = ussClassName + "__text-field";
        public static readonly string textInputUssClassName = textUssClassName + "__input";
        public static readonly string searchButtonUssClassName = ussClassName + "__search-button";
        public static readonly string cancelButtonUssClassName = ussClassName + "__cancel-button";
        public static readonly string cancelButtonOffVariantUssClassName = cancelButtonUssClassName + "--off";
        
        public TBaseField TextField;
        public Button SearchButton;
        public Button CancelButton;

        [CreateProperty]
        public TValue value
        {
            get => this.TextField.value;
            set
            {
                TValue y = this.TextField.value;
                this.TextField.value = value;
                if (EqualityComparer<TValue>.Default.Equals(this.TextField.value, y))
                    return;
                this.NotifyPropertyChanged(in valueProperty);
            }
        }

        public SearchValueFieldBase()
        {
            focusable = true;
            tabIndex = 0;
            delegatesFocus = true;
            AddToClassList(ussClassName);
            SearchButton = (Button)(new Button(() => { }) { name = "unity-search", focusable = false }).WithClasses(searchButtonUssClassName);
            CancelButton = (Button)(new Button(OnCancelButtonClick) { name = "unity-cancel" }).WithClasses(cancelButtonUssClassName); //, cancelButtonOffVariantUssClassName);
            TextField = (TBaseField)(new TBaseField() { DefaultUnityStyles = true }).WithClasses(textUssClassName);
            TextField.RegisterValueChangedCallback<TValue>(OnValueChanged);
            TextField.VisualInput.RegisterCallback<KeyDownEvent>(OnTextFieldKeyDown, TrickleDown.TrickleDown);
            TextField.VisualInput.AddToClassList(textInputUssClassName);
            this.AlsoAdd(SearchButton).AlsoAdd(TextField).AlsoAdd(CancelButton);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        }
        
        public virtual void SetValueWithoutNotify(TValue newValue)
        {
            TextField.SetValueWithoutNotify(newValue);
            UpdateCancelButton();
        }
        
        private void OnAttachToPanelEvent(AttachToPanelEvent evt) => UpdateCancelButton();

        private void OnValueChanged(ChangeEvent<TValue> change) => UpdateCancelButton();

        protected abstract bool FieldIsEmpty(TValue fieldValue);
        
        protected abstract void ClearTextField();
        
        private void UpdateCancelButton() => CancelButton.EnableInClassList(cancelButtonOffVariantUssClassName, FieldIsEmpty(TextField.value));

        private void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
                return;
            ClearTextField();
        }
        
        private void OnCancelButtonClick() => ClearTextField();
    }
}
