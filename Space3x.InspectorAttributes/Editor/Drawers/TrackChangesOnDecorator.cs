using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TrackChangesOnAttribute), useForChildren: false)]
    public class TrackChangesOnDecorator : Decorator<AutoDecorator, TrackChangesOnAttribute>, IAttributeExtensionContext<TrackChangesOnAttribute>
    {
        public override TrackChangesOnAttribute Target => (TrackChangesOnAttribute) attribute;
        
        public IAttributeExtensionContext<TrackChangesOnAttribute> Context => this;

        private bool m_IsReady = false;
        
        public override void OnUpdate()
        {
            if (!m_IsReady)
            {
                Context.WithExtension<TrackChangesOnEx, ITrackChangesOnEx, GhostDecorator>(GhostContainer);
                m_IsReady = true;
                return;
            }

            if (Property.TryCreateInvokable<object, object>(Property.Name, out var invokable)) 
                invokable.Invoke();
        }
    }
}
