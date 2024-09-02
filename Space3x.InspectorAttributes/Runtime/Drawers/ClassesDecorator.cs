using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ClassesAttribute), useForChildren: true)]
    public class ClassesDecorator : Decorator<AutoDecorator, ClassesAttribute>
    {
        public override ClassesAttribute Target => (ClassesAttribute) attribute;
        
        public override void OnUpdate()
        {
            VisualTarget?.WithClasses(Target.Enabled, Target.ClassNames.ToArray());
        }
    }
}
