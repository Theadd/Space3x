using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.InputFields
{
    [UxmlElement(uxmlName: "ui3x.ToolbarSearchField")]
    public partial class ToolbarSearchField : SearchValueFieldBase<TextValueField, string, TextInputField>
    {
        public new static readonly string ussClassName = "unity-toolbar-search-field";
        
        public ToolbarSearchField() => AddToClassList(ussClassName);

        protected override bool FieldIsEmpty(string fieldValue) => string.IsNullOrEmpty(fieldValue);

        protected override void ClearTextField() => value = string.Empty;
    }
}
