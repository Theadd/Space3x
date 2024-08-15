using System;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
using UnityEditor.UIElements;
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
            }
        }) { }

        protected TypePickerField(string label, VisualElement visualInput) : base(label, visualInput)
        {
            m_VisualInput = visualInput;
            m_VisualInput.focusable = false;
            Add(m_VisualInput);
            this.labelElement.focusable = false;
            this.AddToClassList(ObjectField.ussClassName);
            this.labelElement.AddToClassList(ObjectField.labelUssClassName);
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
            this.RegisterValueChangedCallback(OnValueChangedCallback);
        }

        private void OnValueChangedCallback(ChangeEvent<object> ev)
        {
            DebugLog.Info($"<b><color=#00FF00FF>WOF!</color> <color=#00FF7FFF>WOF!</color> <color=#7FFF00FF>WOF!</color> " +
                          $"<color=#7F00FFFF>{nameof(TypePickerField)}.OnValueChangedCallback := {ev.newValue}</color></b>");
            // SetValueFromObject(ev.newValue);
        }

        protected virtual void OnAttachToPanel(AttachToPanelEvent ev)
        {
            if (m_TextField is not null) return;
            if (m_VisualInput is not TextField textField)
                throw new Exception($"{nameof(TypePickerField)} expects a {nameof(TextField)} as BaseField<>'s visualInput.");
            m_TextField = textField;
            m_Button = new Button(() => OnShowPopup?.Invoke(this, m_TextField, ShowWindowMode.NormalWindow)) { text = " " };
            m_TextField.Add(m_Button);
            // Add(m_TextField);
            if (m_ValueType != null)
                SetValue(m_ValueType);
            ApplyFieldStyles();
            // EDIT: MarkDirtyRepaint();
        }
        
        protected void SetValue(Type newValue)
        {
            DebugLog.Info($"<color=#7F00FFFF>{nameof(TypePickerField)}.SetValue := {newValue}</color>");
            m_ValueType = newValue;
            // EDIT
            m_TextField?.SetValueWithoutNotify(TypeRewriter.AsDisplayName(newValue, TypeRewriter.NoStyle));
            // if (m_TextField != null) 
            //     m_TextField.value = TypeRewriter.AsDisplayName(newValue, TypeRewriter.NoStyle);
        }
    }
}
