using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(VisibleOnAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(VisibleOnAttribute), true)]
    public class VisibleOnDecorator : Decorator<AutoDecorator, VisibleOnAttribute>, IAttributeExtensionContext<VisibleOnAttribute>
    {
        public override VisibleOnAttribute Target => (VisibleOnAttribute) attribute;
        public IAttributeExtensionContext<VisibleOnAttribute> Context => this;

        protected override bool UpdateOnAnyValueChange => true;
        
        public override void OnUpdate()
        {
            Context.WithExtension<VisibleOnEx, IVisibleOnEx, bool>(
                out var isTrue, 
                defaultValue: true);
        }
    }
}
