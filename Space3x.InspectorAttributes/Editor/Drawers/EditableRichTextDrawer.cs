using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(EditableRichTextAttribute), useForChildren: true)]
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
