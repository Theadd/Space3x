using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ObjectPickerAttribute), useForChildren: true)]
    public class ObjectPickerDrawer : Drawer<ObjectPickerAttribute>
    {
        public override ObjectPickerAttribute Target => (ObjectPickerAttribute) attribute;
        
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            var field = new UnityEditor.Search.ObjectField()
            {
                label = property.DisplayName(),
                objectType = Target.ObjectType
            };
            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            field.BindProperty(property);
            return field;
        }
    }
}
