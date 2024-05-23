using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(EndColumnAttribute), true)]
    public class EndColumnDecorator : GroupMarkerDecorator<EndColumnGroup, EndColumnAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override EndColumnAttribute Target => (EndColumnAttribute) attribute;
    }
}
