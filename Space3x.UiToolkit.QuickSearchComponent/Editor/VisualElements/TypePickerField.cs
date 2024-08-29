using System;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    [UxmlElement]
    [HideInInspector]
    public partial class TypePickerField : BaseField<object>
    {
        private VisualElement m_VisualInput;
        private TextField m_TextField;
        private Button m_Button;
        private Type m_ValueType;
        
        public new string label
        {
            get => ((TextField)m_VisualInput).label;
            set
            {
                if (((TextField)m_VisualInput).label == value)
                    return;
                ((TextField)m_VisualInput).label = value;
            }
        }
        
        public TypePickerField() : this(label: "") { }
        
        public TypePickerField(string label) : this(label, visualInput: new TextField()
        {
            multiline = false,
            isReadOnly = true,
            selectAllOnFocus = false,
            selectAllOnMouseUp = false,
            textEdition = { hidePlaceholderOnFocus = false, placeholder = "None" },
            textSelection = { doubleClickSelectsWord = true, tripleClickSelectsLine = true },
            label = null,
            style =
            {
                marginRight = 0f,
                flexDirection = FlexDirection.Row,
            }
        }) { }

        protected TypePickerField(string label, VisualElement visualInput) : base(label: null, visualInput)
        {
            m_VisualInput = visualInput;
            m_VisualInput.focusable = false;
            this.label = label;
            m_VisualInput.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            Add(m_VisualInput);
            // this.labelElement.focusable = false;
            EnableInClassList(UssConstants.UssTypePicker, true);
            // this.labelElement.AddToClassList(ObjectField.labelUssClassName);
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
            // this.RegisterValueChangedCallback(OnValueChangedCallback);
        }

        // private void OnValueChangedCallback(ChangeEvent<object> ev) => SetValueFromObject(ev.newValue);

        protected virtual void OnAttachToPanel(AttachToPanelEvent ev)
        {
            if (m_TextField is not null) return;
            if (m_VisualInput is not TextField textField)
                throw new Exception($"{nameof(TypePickerField)} expects a {nameof(TextField)} as BaseField<>'s visualInput.");
            m_TextField = textField;
            // Add(m_TextField);
            if (m_ValueType != null)
                SetValue(m_ValueType);
            ApplyFieldStyles();
            // EDIT: MarkDirtyRepaint();
        }
        
        protected void SetValue(Type newValue)
        {
            // DebugLog.Info($"<color=#AF4AFFFF>{nameof(TypePickerField)}.SetValue {this.GetHashCode()} := {newValue}</color>");
            m_ValueType = newValue;
            // EDIT
            m_TextField?.SetValueWithoutNotify(TypeRewriter.AsDisplayName(newValue, DisplayStyle));
            // if (m_TextField != null) 
            //     m_TextField.value = TypeRewriter.AsDisplayName(newValue, TypeRewriter.NoStyle);
        }
    }
}
