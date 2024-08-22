using Space3x.Attributes.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LayerPickerAttribute), useForChildren: true)]
    public class LayerDrawer : Drawer<LayerPickerAttribute>
    {
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            var field = new LayerField() { label = property.DisplayName() };
            field.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            field.BindProperty(property);
            return field;
        }
    }
}
