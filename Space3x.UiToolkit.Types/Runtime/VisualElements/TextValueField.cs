using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public partial class TextValueField
    {
        // [UxmlElement]
        [HideInInspector]
        public partial class TextInput : TextInputFieldBase<string>.TextInputBase
        {
            private TextValueField parentTextField => (TextValueField)this.parent;

            public bool multiline
            {
                get => Multiline;
                set
                {
                    if ((value || string.IsNullOrEmpty(this.text) || !this.text.Contains("\n")) &&
                        Multiline == value)
                        return;
                    Multiline = value;
                    if (value)
                    {
                        this.text = this.parentTextField.rawValue;
                        this.SetMultiline();
                    }
                    else
                    {
                        this.text = this.text.Replace("\n", "");
                        this.SetSingleLine();
                    }
                }
            }

            [Obsolete("isPasswordField is deprecated. Use textEdition.isPassword instead.")]
            public override bool isPasswordField
            {
                set
                {
                    this.textEdition.isPassword = value;
                    if (!value)
                        return;
                    this.multiline = false;
                }
            }

            protected override string StringToValue(string str) => str;
        }
    }
    
    [UxmlElement]
    public partial class TextValueField : TextInputFieldBase<string>
    {
        public static readonly BindingId multilineProperty = (BindingId)nameof(multiline);

        public new static readonly string ussClassName = "unity-text-field";
        public new static readonly string labelUssClassName = TextValueField.ussClassName + "__label";
        public new static readonly string inputUssClassName = TextValueField.ussClassName + "__input";

        public TextValueField.TextInput textInput => (TextValueField.TextInput)this.textInputBase;

        public VisualElement VisualInput => this.visualInput;

        /// <summary>
        /// Set this to true to allow multiple lines in the textfield and false if otherwise.
        /// </summary>
        [CreateProperty]
        public bool multiline
        {
            get => this.textInput.multiline;
            set
            {
                bool multiline = this.multiline;
                this.textInput.multiline = value;
                if (multiline == this.multiline)
                    return;
                this.NotifyPropertyChanged(in TextValueField.multilineProperty);
            }
        }

        public TextValueField() : this((string)null) { }

        /// <summary>
        /// Creates a new textfield.
        /// </summary>
        /// <param name="maxLength">The maximum number of characters this textfield can hold. If -1, there is no limit.</param>
        /// <param name="multiline">Set this to true to allow multiple lines in the textfield and false if otherwise.</param>
        /// <param name="isPasswordField">Set this to true to mask the characters and false if otherwise.</param>
        /// <param name="maskChar">The character used for masking in a password field.</param>
        public TextValueField(int maxLength, bool multiline, bool isPasswordField, char maskChar)
            : this((string)null, maxLength, multiline, isPasswordField, maskChar) { }
        
        public TextValueField(string label)
            : this(label, -1, false, false, '*') { }

        /// <summary>
        /// Creates a new textfield.
        /// </summary>
        /// <param name="maxLength">The maximum number of characters this textfield can hold. If 0, there is no limit.</param>
        /// <param name="multiline">Set this to true to allow multiple lines in the textfield and false if otherwise.</param>
        /// <param name="isPassword">Set this to true to mask the characters and false if otherwise.</param>
        /// <param name="maskChar">The character used for masking in a password field.</param>
        /// <param name="label"></param>
        /// <param name="isPasswordField"></param>
        public TextValueField(
            string label,
            int maxLength,
            bool multiline,
            bool isPasswordField,
            char maskChar)
            : base(label, maxLength, maskChar, (TextInputFieldBase<string>.TextInputBase)new TextValueField.TextInput())
        {
            this.AddToClassList(TextValueField.ussClassName);
            this.labelElement.AddToClassList(TextValueField.labelUssClassName);
            this.visualInput.AddToClassList(TextValueField.inputUssClassName);
            this.pickingMode = PickingMode.Ignore;
            this.SetValueWithoutNotify("");
            this.multiline = multiline;
            this.textEdition.isPassword = isPasswordField;
        }

        /// <summary>
        /// The string currently being exposed by the field.
        /// </summary>
        public override string value
        {
            get => base.value;
            set
            {
                base.value = value;
                this.textInput.UpdateText(this.rawValue);
            }
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            string newValue1 = this.rawValue;
            if (!this.multiline && this.rawValue != null)
                newValue1 = this.rawValue.Replace("\n", "");
            ((INotifyValueChanged<string>)this.textInput.textElement).SetValueWithoutNotify(newValue1);
        }

        public override void UpdateTextFromValue() => this.SetValueWithoutNotify(this.rawValue);

        [EventInterest(new System.Type[] { typeof(FocusOutEvent) })]
        protected override void HandleEventBubbleUp(EventBase evt)
        {
            base.HandleEventBubbleUp(evt);
            int num1;
            if (this.isDelayed)
            {
                long? eventTypeId = evt?.eventTypeId;
                long num2 = EventBase<FocusOutEvent>.TypeId();
                num1 = eventTypeId.GetValueOrDefault() == num2 & eventTypeId.HasValue ? 1 : 0;
            }
            else
                num1 = 0;

            if (num1 == 0)
                return;
            this.value = this.text;
        }

        // public override void OnViewDataReady()
        // {
        //     base.OnViewDataReady();
        //     this.OverwriteFromViewData((object)this, this.GetFullHierarchicalViewDataKey());
        //     this.text = this.rawValue;
        // }

        protected override string ValueToString(string value) => value;

        protected override string StringToValue(string str) => str;
    }
}
