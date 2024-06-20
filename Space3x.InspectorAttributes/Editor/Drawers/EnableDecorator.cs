using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(EnableAttribute))]
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
