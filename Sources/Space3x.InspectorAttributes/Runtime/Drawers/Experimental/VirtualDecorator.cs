using System;
using JetBrains.Annotations;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    /// <summary>
    /// VirtualDecorator is a quick and dirty way to implement a decorator in-place. Which also provides access to the
    /// VisualElement's hierarchy tree for that panel.
    /// </summary>
    /// <remarks>
    /// From Wikipedia: In object-oriented programming, a virtual function or virtual method is an inheritable and
    /// overridable function or method that is dispatched dynamically.
    /// It's a terrible name anyway, though. I haven't come up with anything better.
    /// </remarks>
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(VirtualDecoratorAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(VirtualDecoratorAttribute), true)]
    [PublicAPI]
    public class VirtualDecorator : Decorator<AutoDecorator, VirtualDecoratorAttribute>
    {
        private Invokable<object, object> m_OnUpdate;

        public override VirtualDecoratorAttribute Target => (VirtualDecoratorAttribute) attribute;
        
        protected override bool UpdateOnAnyValueChange => Target.UpdateOnAnyValueChange;
        
        protected override void OnCreatePropertyGUI(VisualElement container)
        {
            if (!string.IsNullOrEmpty(Target.OnCreate))
                if (Property.TryCreateInvokable<VisualElement, object>(Target.OnCreate, out var invokable, drawer: this))
                    if (invokable.Parameters == null)
                        invokable.Invoke(container);
                    else
                        invokable.InvokeWith(invokable.Parameters);
        }

        public override void OnAttachedAndReady(VisualElement element)
        {
            if (!string.IsNullOrEmpty(Target.OnAttached))
                if (Property.TryCreateInvokable<VisualElement, object>(Target.OnAttached, out var invokable, drawer: this))
                    if (invokable.Parameters == null)
                        invokable.Invoke(element);
                    else
                        invokable.InvokeWith(invokable.Parameters);
            // if (!string.IsNullOrEmpty(Target.OnUpdate))
            //     (Container ?? element).RegisterOnGeometryChangedEventOnce(OnFirstUpdate);
        }

        // private void OnFirstUpdate(GeometryChangedEvent _) => OnUpdate();
        
        public override void OnUpdate()
        {
            if (!string.IsNullOrEmpty(Target.OnUpdate))
            {
                if (m_OnUpdate == null)
                    if (Property.TryCreateInvokable<object, object>(Target.OnUpdate, out var invokable, drawer: this))
                        m_OnUpdate = invokable;
                if (m_OnUpdate == null)
                    throw new ArgumentException($"Invalid value supplied for {nameof(VirtualDecoratorAttribute)}.{nameof(Target.OnUpdate)}. ({Target.OnUpdate})");
                if (m_OnUpdate.Parameters == null)
                    m_OnUpdate.Invoke();
                else
                    m_OnUpdate.InvokeWith(m_OnUpdate.Parameters);
            }
        }
    }
}
