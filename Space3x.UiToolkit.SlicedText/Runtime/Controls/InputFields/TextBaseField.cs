using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;
using Space3x.UiToolkit.Types;
using UnityEngine;

namespace Space3x.UiToolkit.SlicedText.InputFields
{
    public partial class TextBaseField<TValueType, TInputField>
    {
        private static readonly MethodInfo StartEditingMethod =
            R.GetMethod<BaseField<TValueType>>("StartEditing", R.NonPublicInstance, new[] {typeof(EventBase)});
        private static readonly MethodInfo EndEditingMethod =
            R.GetMethod<BaseField<TValueType>>("EndEditing", R.NonPublicInstance, new[] {typeof(EventBase)});
        
        protected virtual void OnAttachToPanel(AttachToPanelEvent ev)
        {
            labelElement.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            labelElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected virtual void OnDetachFromPanel(DetachFromPanelEvent ev)
        {
            labelElement.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            labelElement.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }
        
        private void OnPointerDown(PointerDownEvent ev) => StartEditingMethod.Invoke(((BaseField<TValueType>) this), new object[] { ev as EventBase });

        private void OnPointerUp(PointerUpEvent ev) => EndEditingMethod.Invoke(((BaseField<TValueType>) this), new object[] { ev as EventBase });
    }
    
    /// <summary>
    /// Main base class to derive Text base input fields from. It derives from Unity's <see cref="BaseField{TValueType}"/> class
    /// itself, providing many existing common functionality on top of it. Encapsulating an accessible (public) instance of
    /// our own text processor implementation instead of burying it behind inaccessible (internal) types.
    /// </summary>
    /// <typeparam name="TValueType">The type of value this field holds.</typeparam>
    /// <typeparam name="TInputField">The type of text processor that will be used to process the input. e.g. <see cref="IntegerInputField"/> as basic sample.</typeparam>
    [UxmlElement]
    public partial class TextBaseField<TValueType, TInputField> : BaseField<TValueType> where TInputField : TextValueInputField<TValueType>, new()
    {
        public TInputField VisualInput { get; private set; }
        
        public virtual bool ReadOnly
        {
            get => VisualInput.ReadOnly;
            set => VisualInput.ReadOnly = value;
        }
        
        public override TValueType value
        {
            get => base.value;
            set
            {
                if (VisualInput.value.Equals(value))
                {
                    // if (!base.value.Equals(value))
                    if (!EqualityComparer<TValueType>.Default.Equals(base.value, value))
                        base.value = value;
                    return;
                }

                VisualInput.value = value;
            }
        }

        public override void SetValueWithoutNotify(TValueType newValue)
        {
            base.SetValueWithoutNotify(newValue);
            
            if (!VisualInput.value.Equals(newValue))
                VisualInput.value = newValue;
        }

        private bool m_DefaultUnityStyles = true;
        private string m_LastColorSchemeClass = string.Empty;

        [UxmlAttribute]
        public bool DefaultUnityStyles
        {
            get => m_DefaultUnityStyles;
            set
            {
                if (m_DefaultUnityStyles == value)
                    return;

                m_DefaultUnityStyles = !m_DefaultUnityStyles;
                (VisualInput.ColorSchemeClass, m_LastColorSchemeClass) = (m_LastColorSchemeClass, VisualInput.ColorSchemeClass);
                ApplyStyleClassNames();
            }
        }
        
        [UxmlAttribute]
        public bool ApplyDefaultMonospaceFont { get; set; } = true;
        
        public static Font DefaultFont { get; set; } = Resources.Load<Font>("SourceCodePro-Regular");

        public TextBaseField() : this(null) {}
        
        public TextBaseField(string label)
            : this(label, new TInputField()
            {
                focusable = true, 
                tabIndex = 0,
                PropagateChangeEvent = false
            }) {}

        
        protected TextBaseField(string label, VisualElement visualInput) : base(label, visualInput)
        {
            VisualInput = (TInputField) visualInput;
            if (ApplyDefaultMonospaceFont)
            {
                VisualInput.style.unityFontDefinition = new StyleFontDefinition(StyleKeyword.None);
                VisualInput.style.unityFont = new StyleFont(DefaultFont);
                VisualInput.style.fontSize = new StyleLength(new Length(12f, LengthUnit.Pixel));
            }
            VisualInput.OnValueChange = OnValueChange;
            ApplyStyleClassNames();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }
        
        protected virtual void OnValueChange(TValueType newValue) => value = newValue;

        private void ApplyStyleClassNames()
        {
            VisualInput.WithClasses(m_DefaultUnityStyles, 
                "unity-base-text-field__input", 
                "unity-base-field__input", 
                "unity-base-text-field__input--single-line", 
                "unity-base-text-field");
            this.WithClasses(m_DefaultUnityStyles, "unity-base-text-field", "unity-base-field");
        }
    }
}
