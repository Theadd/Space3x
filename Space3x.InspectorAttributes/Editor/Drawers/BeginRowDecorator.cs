using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(BeginRowAttribute), true)]
    public class BeginRowDecorator : GroupMarkerDecorator<BeginRowGroup, BeginRowAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override BeginRowAttribute Target => (BeginRowAttribute) attribute;
    }
}
