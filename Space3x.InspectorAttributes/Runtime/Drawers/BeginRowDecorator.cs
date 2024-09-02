using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(BeginRowAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(BeginRowAttribute), true)]
    public class BeginRowDecorator : GroupMarkerDecorator<BeginRowGroup, BeginRowAttribute>
    {
        public override IGroupMarkerDecorator LinkedMarkerDecorator { get; set; } = null;
        
        public override BeginRowAttribute Target => (BeginRowAttribute) attribute;
    }
}
