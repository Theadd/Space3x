using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(BeginColumnAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(BeginColumnAttribute), true)]
    public class BeginColumnDecorator : GroupMarkerDecorator<BeginColumnGroup, BeginColumnAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override BeginColumnAttribute Target => (BeginColumnAttribute) attribute;
    }
}
