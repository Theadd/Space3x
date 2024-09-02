using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ClassesAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(ClassesAttribute), true)]
    public class ClassesDecorator : Decorator<AutoDecorator, ClassesAttribute>
    {
        public override ClassesAttribute Target => (ClassesAttribute) attribute;
        
        public override void OnUpdate()
        {
            VisualTarget?.WithClasses(Target.Enabled, Target.ClassNames.ToArray());
        }
    }
}
