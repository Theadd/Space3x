using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.UiToolkit.SlicedText.Iterators;
using Space3x.UiToolkit.SlicedText.InputFields;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    public interface ITextValueField
    {
        bool Accepts(string text);
        bool AcceptCharacter(char c);
    }
    
    public abstract class TextValueInputField<TValueType> : BasicInputField, ITextValueField, INotifyValueChanged<TValueType>
    {
        public abstract string ValueToString(TValueType value);

        public abstract TValueType StringToValue(string str);

        public virtual bool Accepts(string text) => true;

        public virtual bool AcceptCharacter(char c) => true;

        public bool IsValid { get; protected set; }

        /// <summary>
        /// If you're creating a new field component by subclassing from <see cref="TextBaseField"/> or
        /// any other type that doesn't directly derive from <see cref="TextValueInputField{TValueType}"/> (this class),
        /// don't overwrite this, override <see cref="TextBaseField.OnValueChange"/> instead.
        /// </summary>
        public Action<TValueType> OnValueChange { get; set; }

        public bool PropagateChangeEvent { get; set; } = true;

        private string m_Text;
        
        [CreateProperty]
        [UxmlAttribute]
        public override string Text
        {
            get => m_Text;
            set => SetTextValue(value, true);
        }
        
        private TValueType m_Value;
        
        [CreateProperty]
        public virtual TValueType value
        {
            get { return m_Value; }
            set
            {
                // if (m_MValue.Equals(value)) 
                if (EqualityComparer<TValueType>.Default.Equals(m_Value, value)) 
                    return;

                if (panel != null && PropagateChangeEvent)
                {
                    SetValueWithoutNotify(value);
                    NotifyPropertyChanged(in valueProperty);
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
                
                OnValueChange?.Invoke(value);
            }
        }
        
        private static readonly BindingId valueProperty = (BindingId) nameof (value);
        
        public void SetValueWithoutNotify(TValueType newValue)
        {
            m_Value = newValue;

            if (!StringToValue(m_Text).Equals(m_Value))
            {
                Text = ValueToString(m_Value);
            }
        }

        protected override void SendChangeEvent(ChangeRecord record)
        {
            SetTextValue(Multiline ? base.Text : Editor.SliceAt(0).ToString());
        }

        protected void SetTextValue(string text, bool propagateTextToEditor = false)
        {
            m_Text = text;

            if (Accepts(text))
            {
                IsValid = true;
                
                if (propagateTextToEditor) 
                    base.Text = text;

                TValueType aux = StringToValue(text);
//                if (EqualityComparer<TValueType>.Default.Equals(aux, m_MValue)) return;
                value = aux;
            }
            else
            {
                IsValid = false;
                
                if (propagateTextToEditor) 
                    base.Text = text;
            }
        }
        
        public override void Initialize()
        {
            ClearHistoryWhenTextChangedFromCode = false;

            Editor = Editor ?? new InputSliceEditor(this);
            
            base.Initialize();
        }

        public class InputSliceEditor : StringSliceGroup
        {
            protected ITextValueField ValueField;
            
            public InputSliceEditor(ITextValueField valueField) : base()
            {
                ValueField = valueField;
            }
            
            protected override void Normalize(ref string text)
            {
                text = string.Concat(text.Where(c => ValueField.AcceptCharacter(c)));
                
                base.Normalize(ref text);
            }
        }
    }
}
