using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    [UxmlElement]
    [HideInInspector]
    public abstract partial class SearchValueFieldBase<TTextInputType, T> : VisualElement, INotifyValueChanged<T>
        where TTextInputType : TextInputFieldBase<T>, new()
    {
        public static readonly BindingId valueProperty = (BindingId)nameof(value);
        public static readonly BindingId placeholderTextProperty = (BindingId)nameof(placeholderText);
        private readonly Button m_SearchButton;
        private readonly Button m_CancelButton;
        private readonly TTextInputType m_TextField;
        public static readonly string ussClassName = "unity-search-field-base";

        public static readonly string textUssClassName =
            SearchValueFieldBase<TTextInputType, T>.ussClassName + "__text-field";

        public static readonly string textInputUssClassName =
            SearchValueFieldBase<TTextInputType, T>.textUssClassName + "__input";

        public static readonly string searchButtonUssClassName =
            SearchValueFieldBase<TTextInputType, T>.ussClassName + "__search-button";

        public static readonly string cancelButtonUssClassName =
            SearchValueFieldBase<TTextInputType, T>.ussClassName + "__cancel-button";

        public static readonly string cancelButtonOffVariantUssClassName =
            SearchValueFieldBase<TTextInputType, T>.cancelButtonUssClassName + "--off";

        public static readonly string popupVariantUssClassName =
            SearchValueFieldBase<TTextInputType, T>.ussClassName + "--popup";

        protected TTextInputType textInputField => this.m_TextField;

        protected Button searchButton => this.m_SearchButton;

        [CreateProperty]
        public T value
        {
            get => this.m_TextField.value;
            set
            {
                T y = this.m_TextField.value;
                this.m_TextField.value = value;
                if (EqualityComparer<T>.Default.Equals(this.m_TextField.value, y))
                    return;
                textInputField.UpdateTextFromValue();
                this.NotifyPropertyChanged(in SearchValueFieldBase<TTextInputType, T>.valueProperty);
            }
        }

        [CreateProperty]
        public string placeholderText
        {
            get => this.m_TextField.textEdition.placeholder;
            set
            {
                if (value == this.m_TextField.textEdition.placeholder)
                    return;
                this.m_TextField.textEdition.placeholder = value;
                this.NotifyPropertyChanged(in SearchValueFieldBase<TTextInputType, T>.placeholderTextProperty);
            }
        }

        protected SearchValueFieldBase()
        {
            // FIXME: this.isCompositeRoot = true;
            this.focusable = true;
            this.tabIndex = 0;
            // FIXME: this.excludeFromFocusRing = true;
            this.delegatesFocus = true;
            this.AddToClassList(SearchValueFieldBase<TTextInputType, T>.ussClassName);
            Button button1 = new Button((Action)(() => { }));
            button1.name = "unity-search";
            this.m_SearchButton = button1;
            this.m_SearchButton.AddToClassList(SearchValueFieldBase<TTextInputType, T>.searchButtonUssClassName);
            this.m_SearchButton.focusable = false;
            VisualElement.Hierarchy hierarchy = this.hierarchy;
            hierarchy.Add((VisualElement)this.m_SearchButton);
            this.m_TextField = new TTextInputType();
            this.m_TextField.AddToClassList(SearchValueFieldBase<TTextInputType, T>.textUssClassName);
            hierarchy = this.hierarchy;
            hierarchy.Add((VisualElement)this.m_TextField);
            this.m_TextField.RegisterValueChangedCallback<T>(new EventCallback<ChangeEvent<T>>(this.OnValueChanged));
            VisualElement visualElement = this.m_TextField.Q(TextInputFieldBase<string>.textInputUssName, (string)null);
            visualElement.RegisterCallback<KeyDownEvent>(new EventCallback<KeyDownEvent>(this.OnTextFieldKeyDown),
                TrickleDown.TrickleDown);
            visualElement.AddToClassList(SearchValueFieldBase<TTextInputType, T>.textInputUssClassName);
            Button button2 = new Button((Action)(() => { }));
            button2.name = "unity-cancel";
            this.m_CancelButton = button2;
            this.m_CancelButton.AddToClassList(SearchValueFieldBase<TTextInputType, T>.cancelButtonUssClassName);
            this.m_CancelButton.AddToClassList(
                SearchValueFieldBase<TTextInputType, T>.cancelButtonOffVariantUssClassName);
            hierarchy = this.hierarchy;
            hierarchy.Add((VisualElement)this.m_CancelButton);
            this.RegisterCallback<AttachToPanelEvent>(new EventCallback<AttachToPanelEvent>(this.OnAttachToPanelEvent));
            this.m_CancelButton.clickable.clicked += new Action(this.OnCancelButtonClick);
        }

        private void OnAttachToPanelEvent(AttachToPanelEvent evt) => this.UpdateCancelButton();

        private void OnValueChanged(ChangeEvent<T> change) => this.UpdateCancelButton();

        protected abstract void ClearTextField();

        private void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
                return;
            this.ClearTextField();
        }

        private void OnCancelButtonClick() => this.ClearTextField();

        public virtual void SetValueWithoutNotify(T newValue)
        {
            this.m_TextField.SetValueWithoutNotify(newValue);
            this.UpdateCancelButton();
        }

        protected abstract bool FieldIsEmpty(T fieldValue);

        private void UpdateCancelButton()
        {
            this.m_CancelButton.EnableInClassList(
                SearchValueFieldBase<TTextInputType, T>.cancelButtonOffVariantUssClassName,
                !FieldIsEmpty(this.m_TextField.value));
                // string.IsNullOrEmpty(this.m_TextField.text));
        }
    }
}
