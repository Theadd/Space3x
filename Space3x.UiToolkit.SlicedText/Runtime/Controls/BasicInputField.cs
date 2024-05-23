using Space3x.UiToolkit.SlicedText.VisualElements;
using Space3x.UiToolkit.SlicedText.Processors;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    [UxmlElement]
    public partial class BasicInputField : SlicedTextEditor
    {
        [UxmlAttribute]
        public bool ReadOnlyDisablesKeyboardFocus { get; set; } = true;
        
        public override bool ReadOnly
        {
            get => base.ReadOnly;
            set
            {
                base.ReadOnly = value;
                StyleAsLocked = ReadOnlyDisablesKeyboardFocus && value;
                focusable = !ReadOnlyDisablesKeyboardFocus || !value;
                
                if (value == true && HasFocus)
                    Blur();
            }
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
            BlockProcessor = BlockProcessor ?? new SingleLineProcessor();
            EditorEventHandler = EditorEventHandler ?? new TextEditorKeyboardEventHandler();
            
            base.Initialize();
        }

        public BasicInputField()
        {
            AddToClassList("sliced-editor--compact");

            Multiline = false;
            GrowableValues = (-1, -1);
            UpdateAncestorScrollView = false;
        }

        protected override void StylesResolvedHandler(bool isLastCall)
        {
            if (ClassListContains("sliced-editor--compact"))
            {
                if (parent.resolvedStyle.flexDirection == FlexDirection.Column ||
                    parent.resolvedStyle.flexDirection == FlexDirection.ColumnReverse)
                {
                    if (!ClassListContains("sliced-editor--vertical"))
                        AddToClassList("sliced-editor--vertical");
                }
                else
                {
                    if (ClassListContains("sliced-editor--vertical"))
                        RemoveFromClassList("sliced-editor--vertical");
                }
            }

            base.StylesResolvedHandler(isLastCall);
        }
    }
}
