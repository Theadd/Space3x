using Space3x.Attributes.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TagPickerAttribute), useForChildren: true)]
    public class TagPickerDrawer : Drawer<TagPickerAttribute>
    {
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            var field = new TagField() { label = preferredLabel };
            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            field.BindProperty(property);
            return field;
        }
    }
}
