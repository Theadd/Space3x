using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    [UxmlElement]
    [HideInInspector]
    public abstract partial class TextInputFieldBase<TValueType> : BaseField<TValueType>
    {
        public static readonly BindingId autoCorrectionProperty = (BindingId)nameof(autoCorrection);
        public static readonly BindingId hideMobileInputProperty = (BindingId)nameof(hideMobileInput);
        public static readonly BindingId hidePlaceholderOnFocusProperty = (BindingId)nameof(hidePlaceholderOnFocus);
        public static readonly BindingId keyboardTypeProperty = (BindingId)nameof(keyboardType);
        public static readonly BindingId isReadOnlyProperty = (BindingId)nameof(isReadOnly);
        public static readonly BindingId isPasswordFieldProperty = (BindingId)nameof(isPasswordField);
        public static readonly BindingId textSelectionProperty = (BindingId)nameof(textSelection);
        public static readonly BindingId textEditionProperty = (BindingId)nameof(textEdition);
        public static readonly BindingId placeholderTextProperty = (BindingId)nameof(placeholderText);
        public static readonly BindingId selectionColorProperty = (BindingId)nameof(selectionColor);
        public static readonly BindingId cursorColorProperty = (BindingId)nameof(cursorColor);
        public static readonly BindingId cursorIndexProperty = (BindingId)nameof(cursorIndex);
        public static readonly BindingId cursorPositionProperty = (BindingId)nameof(cursorPosition);
        public static readonly BindingId selectIndexProperty = (BindingId)nameof(selectIndex);
        public static readonly BindingId selectAllOnFocusProperty = (BindingId)nameof(selectAllOnFocus);
        public static readonly BindingId selectAllOnMouseUpProperty = (BindingId)nameof(selectAllOnMouseUp);
        public static readonly BindingId maxLengthProperty = (BindingId)nameof(maxLength);
        public static readonly BindingId doubleClickSelectsWordProperty = (BindingId)nameof(doubleClickSelectsWord);
        public static readonly BindingId tripleClickSelectsLineProperty = (BindingId)nameof(tripleClickSelectsLine);
        public static readonly BindingId emojiFallbackSupportProperty = (BindingId)nameof(emojiFallbackSupport);
        public static readonly BindingId isDelayedProperty = (BindingId)nameof(isDelayed);
        public static readonly BindingId maskCharProperty = (BindingId)nameof(maskChar);
        public static readonly BindingId verticalScrollerVisibilityProperty = (BindingId)nameof(verticalScrollerVisibility);

        private static CustomStyleProperty<Color> s_SelectionColorProperty = new CustomStyleProperty<Color>("--unity-selection-color");
        private static CustomStyleProperty<Color> s_CursorColorProperty = new CustomStyleProperty<Color>("--unity-cursor-color");

        public const int kMaxLengthNone = -1;
        public const char kMaskCharDefault = '*';
        public static new readonly string ussClassName = "unity-base-text-field";
        public static new readonly string labelUssClassName = ussClassName + "__label";
        public static new readonly string inputUssClassName = ussClassName + "__input";
        public static readonly string multilineContainerClassName = ussClassName + "__multiline-container";
        public static readonly string singleLineInputUssClassName = inputUssClassName + "--single-line";
        public static readonly string multilineInputUssClassName = inputUssClassName + "--multiline";
        public static readonly string placeholderUssClassName = inputUssClassName + "--placeholder";
        public static readonly string multilineInputWithScrollViewUssClassName = multilineInputUssClassName + "--scroll-view";
        public static readonly string textInputUssName = "unity-text-input";
        
        private TextInputFieldBase<TValueType>.TextInputBase m_TextInputBase;

        protected VisualElement visualInput { get; private set; }

        public bool password
        {
            get => this.textEdition.isPassword;
            set => this.textEdition.isPassword = value;
        }

        public bool selectWordByDoubleClick
        {
            get => this.textSelection.doubleClickSelectsWord;
            set => this.textSelection.doubleClickSelectsWord = value;
        }

        public bool selectLineByTripleClick
        {
            get => this.textSelection.tripleClickSelectsLine;
            set => this.textSelection.tripleClickSelectsLine = value;
        }

        public bool readOnly
        {
            get => this.isReadOnly;
            set => this.isReadOnly = value;
        }

        [CreateProperty]
        public string placeholderText
        {
            get => this.textEdition.placeholder;
            // [VisibleToOtherModules(new string[] { "UnityEditor.UIBuilderModule" })]
            set
            {
                if (this.textEdition.placeholder == value)
                    return;
                this.textEdition.placeholder = value;
                OnPlaceholderChanged();
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.placeholderTextProperty);
            }
        }

        [CreateProperty]
        public bool hidePlaceholderOnFocus
        {
            get => this.textEdition.hidePlaceholderOnFocus;
            set
            {
                if (this.textEdition.hidePlaceholderOnFocus == value)
                    return;
                this.textEdition.hidePlaceholderOnFocus = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.hidePlaceholderOnFocusProperty);
            }
        }

        protected TextInputFieldBase(
            int maxLength,
            char maskChar,
            TextInputFieldBase<TValueType>.TextInputBase textInputBase)
            : this((string)null, maxLength, maskChar, textInputBase)
        {
        }

        protected TextInputFieldBase(
            string label,
            int maxLength,
            char maskChar,
            TextInputFieldBase<TValueType>.TextInputBase textInputBase)
            : base(label, (VisualElement)textInputBase)
        {
            visualInput = textInputBase;
            this.tabIndex = 0;
            this.delegatesFocus = true;
            this.labelElement.tabIndex = -1;
            this.AddToClassList(TextInputFieldBase<TValueType>.ussClassName);
            this.labelElement.AddToClassList(TextInputFieldBase<TValueType>.labelUssClassName);
            this.visualInput.AddToClassList(TextInputFieldBase<TValueType>.inputUssClassName);
            this.visualInput.AddToClassList(TextInputFieldBase<TValueType>.singleLineInputUssClassName);
            this.m_TextInputBase = textInputBase;
            this.m_TextInputBase.textEdition.maxLength = maxLength;
            this.m_TextInputBase.textEdition.maskChar = maskChar;
            this.RegisterCallback<CustomStyleResolvedEvent>(
                new EventCallback<CustomStyleResolvedEvent>(this.OnFieldCustomStyleResolved));
            // textInputBase.textElement.OnPlaceholderChanged += new Action(this.OnPlaceholderChanged);
        }

        protected TextInputFieldBase<TValueType>.TextInputBase textInputBase => this.m_TextInputBase;

        [CreateProperty(ReadOnly = true)]
        public ITextSelection textSelection => this.m_TextInputBase.textElement.selection;

        [CreateProperty(ReadOnly = true)]
        public ITextEdition textEdition => (ITextEdition)this.m_TextInputBase.textElement;

        // protected Action<bool> onIsReadOnlyChanged
        // {
        //     get => this.m_TextInputBase.textElement.onIsReadOnlyChanged;
        //     set => this.m_TextInputBase.textElement.onIsReadOnlyChanged = value;
        // }

        [CreateProperty]
        public bool isReadOnly
        {
            get => this.textEdition.isReadOnly;
            set
            {
                if (this.textEdition.isReadOnly == value)
                    return;
                this.textEdition.isReadOnly = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.isReadOnlyProperty);
            }
        }

        [CreateProperty]
        public bool isPasswordField
        {
            get => this.textEdition.isPassword;
            set
            {
                if (this.textEdition.isPassword == value)
                    return;
                this.textEdition.isPassword = value;
                // this.m_TextInputBase.IncrementVersion(VersionChangeType.Repaint);
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.isPasswordFieldProperty);
            }
        }

        [CreateProperty]
        public bool autoCorrection
        {
            get => this.textEdition.autoCorrection;
            set
            {
                if (this.textEdition.autoCorrection == value)
                    return;
                this.textEdition.autoCorrection = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.autoCorrectionProperty);
            }
        }

        [CreateProperty]
        public bool hideMobileInput
        {
            get => this.textEdition.hideMobileInput;
            set
            {
                if (this.textEdition.hideMobileInput == value)
                    return;
                this.textEdition.hideMobileInput = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.hideMobileInputProperty);
            }
        }

        [CreateProperty]
        public TouchScreenKeyboardType keyboardType
        {
            get => this.textEdition.keyboardType;
            set
            {
                if (this.textEdition.keyboardType == value)
                    return;
                this.textEdition.keyboardType = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.keyboardTypeProperty);
            }
        }

        public TouchScreenKeyboard touchScreenKeyboard => this.textEdition.touchScreenKeyboard;

        [CreateProperty]
        public int maxLength
        {
            get => this.textEdition.maxLength;
            set
            {
                if (this.textEdition.maxLength == value)
                    return;
                this.textEdition.maxLength = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.maxLengthProperty);
            }
        }

        [CreateProperty]
        public bool isDelayed
        {
            get => this.textEdition.isDelayed;
            set
            {
                if (this.textEdition.isDelayed == value)
                    return;
                this.textEdition.isDelayed = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.isDelayedProperty);
            }
        }

        [CreateProperty]
        public char maskChar
        {
            get => this.textEdition.maskChar;
            set
            {
                if ((int)this.textEdition.maskChar == (int)value)
                    return;
                this.textEdition.maskChar = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.maskCharProperty);
            }
        }

        [CreateProperty(ReadOnly = true)] public Color selectionColor => this.textSelection.selectionColor;

        [CreateProperty(ReadOnly = true)] public Color cursorColor => this.textSelection.cursorColor;

        [CreateProperty]
        public int cursorIndex
        {
            get => this.textSelection.cursorIndex;
            set
            {
                if (this.textSelection.cursorIndex == value)
                    return;
                this.textSelection.cursorIndex = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.cursorIndexProperty);
            }
        }

        [CreateProperty(ReadOnly = true)] public Vector2 cursorPosition => this.textSelection.cursorPosition;

        [CreateProperty]
        public int selectIndex
        {
            get => this.textSelection.selectIndex;
            set
            {
                if (this.textSelection.selectIndex == value)
                    return;
                this.textSelection.selectIndex = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.selectIndexProperty);
            }
        }

        public void SelectAll() => this.textSelection.SelectAll();

        public void SelectNone() => this.textSelection.SelectNone();

        public void SelectRange(int cursorIndex, int selectionIndex)
        {
            this.textSelection.SelectRange(cursorIndex, selectionIndex);
        }

        [CreateProperty]
        public bool selectAllOnFocus
        {
            get => this.textSelection.selectAllOnFocus;
            set
            {
                if (this.textSelection.selectAllOnFocus == value)
                    return;
                this.textSelection.selectAllOnFocus = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.selectAllOnFocusProperty);
            }
        }

        [CreateProperty]
        public bool selectAllOnMouseUp
        {
            get => this.textSelection.selectAllOnMouseUp;
            set
            {
                if (this.textSelection.selectAllOnMouseUp == value)
                    return;
                this.textSelection.selectAllOnMouseUp = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.selectAllOnMouseUpProperty);
            }
        }

        [CreateProperty]
        public bool doubleClickSelectsWord
        {
            get => this.textSelection.doubleClickSelectsWord;
            set
            {
                if (this.textSelection.doubleClickSelectsWord == value)
                    return;
                this.textSelection.doubleClickSelectsWord = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.doubleClickSelectsWordProperty);
            }
        }

        [CreateProperty]
        public bool tripleClickSelectsLine
        {
            get => this.textSelection.tripleClickSelectsLine;
            set
            {
                if (this.textSelection.tripleClickSelectsLine == value)
                    return;
                this.textSelection.tripleClickSelectsLine = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.tripleClickSelectsLineProperty);
            }
        }

        public string text
        {
            get => this.m_TextInputBase.text;
            set => m_TextInputBase.text = value;
        }

        [CreateProperty]
        public bool emojiFallbackSupport
        {
            get => this.m_TextInputBase.textElement.emojiFallbackSupport;
            set
            {
                if (this.m_TextInputBase.textElement.emojiFallbackSupport == value)
                    return;
                this.labelElement.emojiFallbackSupport = value;
                this.m_TextInputBase.textElement.emojiFallbackSupport = value;
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.emojiFallbackSupportProperty);
            }
        }

        [CreateProperty]
        public ScrollerVisibility verticalScrollerVisibility
        {
            get => this.textInputBase.verticalScrollerVisibility;
            set
            {
                if (this.textInputBase.verticalScrollerVisibility == value)
                    return;
                this.textInputBase.SetVerticalScrollerVisibility(value);
                this.NotifyPropertyChanged(in TextInputFieldBase<TValueType>.verticalScrollerVisibilityProperty);
            }
        }

        public Vector2 MeasureTextSize(string textToMeasure, float width, MeasureMode widthMode, float height, MeasureMode heightMode) => 
            m_TextInputBase.textElement.MeasureTextSize(textToMeasure, width, widthMode, height, heightMode);

        [EventInterest(new System.Type[]
        {
            typeof(NavigationSubmitEvent), typeof(FocusInEvent), typeof(FocusEvent), typeof(FocusOutEvent),
            typeof(BlurEvent)
        })]
        protected override void HandleEventBubbleUp(EventBase evt)
        {
            base.HandleEventBubbleUp(evt);
            if (this.textEdition.isReadOnly)
                return;
            if (evt.eventTypeId == EventBase<NavigationSubmitEvent>.TypeId() &&
                evt.target != this.textInputBase.textElement)
                this.textInputBase.textElement.Focus();
            else if (evt.eventTypeId == EventBase<NavigationMoveEvent>.TypeId() &&
                     evt.target != this.textInputBase.textElement)
                CallToSwitchFocusOnEvent(this.focusController, (Focusable)this.textInputBase.textElement, evt);
            else if (evt.eventTypeId == EventBase<FocusInEvent>.TypeId())
            {
                if (!this.showMixedValue)
                    return;
                ((INotifyValueChanged<string>)this.textInputBase.textElement).SetValueWithoutNotify((string)null);
            }
            else if (evt.eventTypeId == EventBase<FocusEvent>.TypeId())
            {
                this.UpdatePlaceholderClassList();
            }
            else
            {
                if (evt.eventTypeId != EventBase<BlurEvent>.TypeId())
                    return;
                if (this.showMixedValue)
                    this.UpdateMixedValueContent();
                this.UpdatePlaceholderClassList();
            }
        }

        private static MethodInfo s_SwitchFocusOnEventMethod;
        
        private static void CallToSwitchFocusOnEvent(FocusController focusController, Focusable currentFocusable,
            EventBase e)
        {
            s_SwitchFocusOnEventMethod ??= typeof(FocusController).GetMethod(
                "SwitchFocusOnEvent",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            s_SwitchFocusOnEventMethod?.Invoke(focusController, new object[] { currentFocusable, e });
        }

        protected abstract string ValueToString(TValueType value);

        protected abstract TValueType StringToValue(string str);

        protected override void UpdateMixedValueContent()
        {
            if (this.showMixedValue)
            {
                ((INotifyValueChanged<string>)this.textInputBase.textElement).SetValueWithoutNotify(
                    BaseField<TValueType>
                        .mixedValueString);
                this.AddToClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
                this.visualInput?.AddToClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
            }
            else
            {
                this.UpdateTextFromValue();
                this.visualInput?.RemoveFromClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
                this.RemoveFromClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
            }
        }

        public bool hasFocus =>
            // [VisibleToOtherModules(new string[] { "UnityEditor.UIBuilderModule" })]
            this.textInputBase.textElement.IsFocused();


        public virtual void UpdateValueFromText() => this.value = this.StringToValue(this.text);

        public virtual void UpdateTextFromValue()
        {
        }

        private void OnFieldCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            this.m_TextInputBase.OnInputCustomStyleResolved(e);
        }
    }
}