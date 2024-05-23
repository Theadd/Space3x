using Space3x.UiToolkit.SlicedText.Iterators;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.InputFields
{
    [UxmlElement]
    public partial class TextValueField : TextBaseField<string, TextInputField> { }
    
    [UxmlElement]
    [HideInInspector]
    public partial class TextInputField : TextValueInputField<string>
    {
        public override string ValueToString(string v) => v;

        public override string StringToValue(string str) => str;

        public override bool Accepts(string text) => true;

        public override bool AcceptCharacter(char c) => !c.IsControl();

        protected override void OnFocusOut(FocusOutEvent e)
        {
            var valueAsText = ValueToString(value) ?? string.Empty;
            if (Text != valueAsText)
                Text = valueAsText;
        }
    }
}
