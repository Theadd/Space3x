using System;
using Space3x.UiToolkit.SlicedText.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.InputFields
{
    [UxmlElement]
    public partial class IntegerValueField : TextBaseField<int, IntegerInputField>
    {
        protected override void OnAttachToPanel(AttachToPanelEvent ev)
        {
            base.OnAttachToPanel(ev);
            FieldValueDragger<int, IntegerInputField, IntegerValueField>.Create(this, labelElement);
        }
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class IntegerInputField : TextValueInputField<int>
    {
        public override string ValueToString(int v) => v.ToString();

        public override int StringToValue(string str) => int.TryParse(str, out var result) ? result : 0;

        public override bool Accepts(string text) => int.TryParse(text, out var result);

        public override bool AcceptCharacter(char c) => c == '-' || (c >= '0' && c <= '9');

        protected override void OnFocusOut(FocusOutEvent e) => Text = ValueToString(value);
    }
}
