using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(EnableOnAttribute), useForChildren: true)]
    public class EnableOnDecorator : Decorator<AutoDecorator, EnableOnAttribute>, IAttributeExtensionContext<EnableOnAttribute>
    {
        public override EnableOnAttribute Target => (EnableOnAttribute) attribute;
        public IAttributeExtensionContext<EnableOnAttribute> Context => this;

        protected override bool UpdateOnAnyValueChange => true;
        
        public override void OnUpdate()
        {
            Context.WithExtension<EnableOnEx, IEnableOnEx, bool>(
                out var isTrue, 
                defaultValue: true);
        }
    }
}
