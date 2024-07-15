using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(FileDialogAttribute), useForChildren: true)]
    public class FileDialogDrawer : Drawer<FileDialogAttribute>
    {
        public override FileDialogAttribute Target => (FileDialogAttribute) attribute;
        protected override VisualElement OnCreatePropertyGUI(IProperty property)
        {
            var field = new FileDialogField()
            {
                label = Target.Text ?? property.DisplayName(),
                DialogTitle = Target.Title,
                DefaultValue = Target.DefaultValue,
                FileExtension = Target.FileExtension,
                IsSaveFileDialog = Target.IsSave,
                DefaultName = Target.DefaultName,
                RelativeToProject = Target.RelativeToProject
            };
            field.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            field.BindProperty(property);
            return field;
        }
    }
}
