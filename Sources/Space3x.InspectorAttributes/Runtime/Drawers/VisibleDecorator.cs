using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(VisibleAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(VisibleAttribute), true)]
    public class VisibleDecorator : Decorator<AutoDecorator, VisibleAttribute>, IAttributeExtensionContext<VisibleAttribute>
    {
        public override VisibleAttribute Target => (VisibleAttribute) attribute;
        public IAttributeExtensionContext<VisibleAttribute> Context => this;

        protected override bool UpdateOnAnyValueChange => false;
        
        public override void OnUpdate()
        {
            Context.WithExtension<VisibleEx, IVisibleEx, bool>(
                out var isTrue, 
                defaultValue: true);
        }
    }
}
