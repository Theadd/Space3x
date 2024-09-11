using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(EditableRichTextAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(EditableRichTextAttribute), true)]
    public class EditableRichTextDrawer : Drawer<EditableRichTextAttribute>
    {
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            var field = new EditableRichText()
            {
                label = property.DisplayName(),
            };
            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            field.BindProperty(property);
            return field;
        }
    }
}
