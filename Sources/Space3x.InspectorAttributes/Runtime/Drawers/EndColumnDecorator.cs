using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(EndColumnAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(EndColumnAttribute), true)]
    public class EndColumnDecorator : GroupMarkerDecorator<EndColumnGroup, EndColumnAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override EndColumnAttribute Target => (EndColumnAttribute) attribute;
    }
}
