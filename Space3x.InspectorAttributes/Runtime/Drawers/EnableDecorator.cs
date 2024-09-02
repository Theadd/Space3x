using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(EnableAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(EnableAttribute), true)]
    public class EnableDecorator : Decorator<AutoDecorator, EnableAttribute>, IAttributeExtensionContext<EnableAttribute>
    {
        public override EnableAttribute Target => (EnableAttribute) attribute;
        public IAttributeExtensionContext<EnableAttribute> Context => this;

        protected override bool UpdateOnAnyValueChange => false;
        
        public override void OnUpdate()
        {
            Context.WithExtension<EnableEx, IEnableEx, bool>(
                out var isTrue, 
                defaultValue: true);
        }
    }
}
