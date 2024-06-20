using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ColorAttribute), useForChildren: true)]
    public class ColorDecorator : Decorator<AutoDecorator, ColorAttribute>, IAttributeExtensionContext<ColorAttribute>
    {
        public override ColorAttribute Target => (ColorAttribute) attribute;
        
        public IAttributeExtensionContext<ColorAttribute> Context => this;
        
        protected override bool UpdateOnAnyValueChange => Target is IConditionEx;

        public override void OnUpdate()
        {
            // If this decorator targets an attribute that implements IConditionEx interface, this applies the
            // ConditionEx extension on it and returns the result as an out parameter, which defaults to the
            // provided defaultValue (true) when the attribute does not implement the IConditionEx interface.
            // This way, the same decorator implementation can be used on any of the ColorAttribute derived
            // classes (YellowAttribute, YellowOnAttribute, RedAttribute, RedOnAttribute, etc.).
            Context.WithExtension<ConditionEx, IConditionEx, bool>(out var isTrue, defaultValue: true);
            VisualTarget?.WithClasses(isTrue, "ui3x-color--" + Target.Color);
        }
    }
}
