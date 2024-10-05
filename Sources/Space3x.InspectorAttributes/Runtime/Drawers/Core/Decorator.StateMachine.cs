using System;
using Space3x.Properties.Types;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public abstract partial class Decorator<T, TAttribute>
    {
        private State m_State = State.None;
        
        private class Context
        {
            public IPropertyNode Property { get; set; }
            public VisualElement Field { get; set; }
            public IPanel Panel { get; set; }
        }

        private enum State
        {
            /// <summary>
            /// The IDisposable.Dispose() method has been called. Resetting all values to their initial state and freeing
            /// registered events and bindings. That's right, disposed decorators on serialized properties may be reused
            /// by a PropertyField when selecting another GameObject with the same setup of components.
            /// </summary>
            Disposed = -2,
            /// <summary>
            /// All values are reset to their initial state, callbacks unregistered and all bindings cleared. Making the
            /// decorator ready to be used again. This state is triggered by the Decorator itself if it is found to be
            /// in an invalid state.
            /// </summary>
            ResetToInitialState = -1,
            /// <summary>
            /// No state has been previously assigned, it's a new decorator instance.
            /// </summary>
            None = 0,
            /// <summary>
            /// GhostContainer created but not yet attached to a panel.
            /// </summary>
            AwaitingGhost = 1,
            /// <summary>
            /// GhostContainer is attached and the active context is populated from its parent field.
            /// But Container hasn't been attached yet.
            /// </summary>
            ContextReady = 2,
            /// <summary>
            /// Container is attached to a panel and OnAttachedAndReady() has been called.
            /// </summary>
            AttachedAndReady = 3,
            /// <summary>
            /// Received and unregistered a GeometryChangedEvent which triggered the first and only OnUpdate() call
            /// directly managed by the decorator.
            /// </summary>
            Done = 4,
        }

        private void HandleDetachFromPanelEventOnInvalidState(Context context)
        {
            m_DetachingItself = true;
            m_State = State.ResetToInitialState;
            ResetToInitialState();
            Assert.IsNull(GhostContainer?.panel);
            m_DetachingItself = false;
        }

        private Context CreateContext()
        {
            var field = GhostContainer == null ? null : GetClosestParentField(GhostContainer);
            return new Context()
            {
                Field = field,
                Property = field?.GetPropertyNode(),
                Panel = field?.panel
            };
        }

        private static VisualElement GetClosestParentField(VisualElement element)
        {
#if UNITY_EDITOR
            return element.GetClosestParentOfAnyType<UnityEditor.UIElements.PropertyField, BindablePropertyField, UnityEditor.UIElements.InspectorElement>();
#else
            return element.GetClosestParentOfType<BindablePropertyField, TemplateContainer>();
#endif
        }
    }
}
