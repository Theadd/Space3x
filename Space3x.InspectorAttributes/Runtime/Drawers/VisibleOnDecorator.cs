using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(VisibleOnAttribute), useForChildren: true)]
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
