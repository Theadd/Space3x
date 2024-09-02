using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(EndRowAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(EndRowAttribute), true)]
    public class EndRowDecorator : GroupMarkerDecorator<EndRowGroup, EndRowAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override EndRowAttribute Target => (EndRowAttribute) attribute;
    }
}
