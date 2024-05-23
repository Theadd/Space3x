using Space3x.UiToolkit.SlicedText.Processors;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    [UxmlElement]
    public partial class BasicTextField : SlicedTextEditor
    {
        [UxmlAttribute]
        public bool ReadOnlyDisablesKeyboardFocus { get; set; } = true;

        // [UxmlAttribute]
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

        public override void Initialize()
        {
            BlockProcessor = BlockProcessor ?? new SingleLineProcessor();
            EditorEventHandler = EditorEventHandler ?? new TextEditorKeyboardEventHandler();
            
            base.Initialize();
        }

        public BasicTextField()
        {
            AddToClassList("sliced-editor--compact");

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
