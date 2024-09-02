using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(EndRowAttribute), true)]
    public class EndRowDecorator : GroupMarkerDecorator<EndRowGroup, EndRowAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override EndRowAttribute Target => (EndRowAttribute) attribute;
    }
}
