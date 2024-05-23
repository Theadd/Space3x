using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public class TextLine : TextElement
    {
        /*protected static ILanguage mLanguage = Languages.Markdown;
        protected static CodeFormatter SourceHandler = new CodeFormatter();

        public override string text
        {
            get => base.text;
            set => base.text = value;  // SourceHandler.GetRichTextString(value, mLanguage, false);
        }*/

        public TextLine()
        {
            ClearClassList();
            enableRichText = true;
            focusable = false;
            displayTooltipWhenElided = false;
            // TODO: PickingMode.Ignore --> No pointer events (also does focusable = false)
            pickingMode = PickingMode.Ignore;
        }
    }
}
