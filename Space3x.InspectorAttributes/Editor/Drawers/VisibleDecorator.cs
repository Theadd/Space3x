using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(VisibleAttribute))]
    public class VisibleDecorator : Decorator<AutoDecorator, VisibleAttribute>, IAttributeExtensionContext<VisibleAttribute>
    {
        public override VisibleAttribute Target => (VisibleAttribute) attribute;
        public IAttributeExtensionContext<VisibleAttribute> Context => this;

        protected override bool RedrawOnAnyValueChange => false;
        
        public override void OnUpdate()
        {
            Context.WithExtension<VisibleEx, IVisibleEx, bool>(
                out var isTrue, 
                defaultValue: true);
        }
    }
}
