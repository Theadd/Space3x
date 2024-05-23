using System;
using Space3x.UiToolkit.SlicedText.VisualElements;
using Space3x.UiToolkit.SlicedText.Processors;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    [Obsolete]
    [UxmlElement]
    public partial class BindableTextField : BasicTextField, IBindable, INotifyValueChanged<string>, INotifyValueChanged<ChangeRecord>
    {
        private ChangeRecord _value;

        /// <summary>
        /// Binding object that will be updated.
        /// </summary>
        public IBinding binding { get; set; }
        
        /// <summary>
        /// Path of the target property to be bound.
        /// </summary>
        [UxmlAttribute]
        public string bindingPath { get; set; }

        protected override void SendChangeEvent(ChangeRecord record)
        {
            record.Target = this;

            ((INotifyValueChanged<ChangeRecord>) this).value = record;
        }

        /// <summary>
        /// The value associated with the field.
        /// </summary>
        [CreateProperty]
        public virtual string value
        {
            get => Text;
            set
            {
                if (panel != null)
                {
                    using (ChangeEvent<string> evt = ChangeEvent<string>
                        .GetPooled(null, value))
                    {
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                    }
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
            }
        }

        public void SetValueWithoutNotify(ChangeRecord newValue)
        {
            _value = newValue;
        }

        public void SetValueWithoutNotify(string newValue)
        {
            Text = newValue;
        }
        
        public override ILineBlockProcessor<TextLine> BlockProcessor
        {
            get => base.BlockProcessor;
            set
            {
                base.BlockProcessor = value;

                if (base.BlockProcessor != null)
                {
                    base.BlockProcessor.OnReadyChangeRecord = SendChangeEvent;
                    base.BlockProcessor.Editor = Editor;
                    base.BlockProcessor.Colorizer = SyntaxHighlighter;
                }
            }
        }
        
        public override void Initialize()
        {
            BlockProcessor ??= new LineBlockProcessor() { MinBlockSize = 0 };
            // BlockProcessor = BlockProcessor ?? new MarkdownBlockProcessor() {};
            // BlockProcessor = BlockProcessor ?? new SingleLineProcessor() {};
            SyntaxHighlighter ??= IColorize.Default; /* STRIPPED CODE: ?? new Colorize() { Language = "mdx" }; */

            base.Initialize();
        }
        
//        public new class UxmlFactory : UxmlFactory<BindableTextField, UxmlTraits> {}
//
//        /// <summary>
//        /// Defines <see cref="UxmlTraits"/> for the <see cref="BindableElement"/>.
//        /// </summary>
//        public new class UxmlTraits : BasicTextField.UxmlTraits
//        {
//            UxmlStringAttributeDescription m_PropertyPath;
//            UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription
//            {
//                name = "value", 
//                defaultValue = string.Empty
//            };
//
//
//            /// <summary>
//            /// Constructor.
//            /// </summary>
//            public UxmlTraits()
//            {
//                m_PropertyPath = new UxmlStringAttributeDescription { name = "binding-path" };
//            }

            /// <summary>
            /// Initialize <see cref="EnumField"/> properties using values from the attribute bag.
            /// </summary>
//            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
//            {
//                base.Init(ve, bag, cc);
//                string propPath = m_PropertyPath.GetValueFromBag(bag, cc);
//
//                if (!string.IsNullOrEmpty(propPath))
//                {
//                    var field = ve as IBindable;
//                    if (field != null)
//                    {
//                        field.bindingPath = propPath;
//                    }
//                }
//            }
//        }
        
        ChangeRecord INotifyValueChanged<ChangeRecord>.value
        {
            get => _value;
            set
            {
                if (panel != null)
                {
                    using (ChangeEvent<ChangeRecord> evt = ChangeEvent<ChangeRecord>
                        .GetPooled(_value, value))
                    {
                        /*var logText = "";
                        if (value.InsertCount == 1 && BlockProcessor.TryGetValueFromCache(value.Index, out string someText))
                        {
                            logText = someText;
                        }*/
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                    }
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
            }
        }
    }
}
