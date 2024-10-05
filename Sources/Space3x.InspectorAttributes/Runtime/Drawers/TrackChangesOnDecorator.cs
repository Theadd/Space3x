using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(TrackChangesOnAttribute), false)]
#endif
    [CustomRuntimeDrawer(typeof(TrackChangesOnAttribute), false)]
    public class TrackChangesOnDecorator : Decorator<AutoDecorator, TrackChangesOnAttribute>, IAttributeExtensionContext<TrackChangesOnAttribute>
    {
        public override TrackChangesOnAttribute Target => (TrackChangesOnAttribute) attribute;
        
        public IAttributeExtensionContext<TrackChangesOnAttribute> Context => this;

        private bool m_IsReady;

        public override void OnAttachedAndReady(VisualElement element)
        {
// #if UNITY_EDITOR
//             UnityEditor.UIElements.BindingExtensions.Unbind(GhostContainer);
// #endif
            if (m_IsReady) return;
            Context.WithExtension<TrackChangesOnEx, ITrackChangesOnEx, GhostDecorator>(GhostContainer);
            m_IsReady = true;
        }

        public override void OnUpdate()
        {
            if (!Property.TryCreateInvokable<object, object>(Property.Name, out var invokable, Target.PropertyName, drawer: this))
                return;
            var result = invokable.Parameters == null ? invokable.Invoke() : invokable.InvokeWith(invokable.Parameters);
            if (Property is not IInvokablePropertyNode invokablePropertyNode)
                return;
            var returnType = ((PropertyAttributeController)invokablePropertyNode.GetController()).AnnotatedType
                .GetValue(invokablePropertyNode.Name)?.RuntimeMethod.ReturnType;
            if (returnType != typeof(void))
                invokablePropertyNode.SetValue(result);
        }

        public override void OnReset(bool disposing = false)
        {
            m_IsReady = false;
            base.OnReset(disposing);
        }
    }
}
