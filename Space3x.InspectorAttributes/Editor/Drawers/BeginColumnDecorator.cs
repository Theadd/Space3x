using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BeginColumnAttribute), true)]
    public class BeginColumnDecorator : GroupMarkerDecorator<BeginColumnGroup, BeginColumnAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override BeginColumnAttribute Target => (BeginColumnAttribute) attribute;
    }
}
