using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.VisualElements
{
    [UxmlElement(uxmlName: "ui3x.TextLine")]
    public partial class TextLine : TextElement, ILine
    {

        public TextLine()
        {
            ClearClassList();
            enableRichText = true;
            focusable = false;
            displayTooltipWhenElided = false;
            pickingMode = PickingMode.Ignore;
        }
    }
}
