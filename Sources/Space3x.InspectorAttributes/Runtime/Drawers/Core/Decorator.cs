using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    /// <summary>
    /// The base class to derive from when implementing your custom <see cref="DecoratorDrawer"/> on a <see cref="UnityEngine.PropertyAttribute"/>.
    /// </summary>
    /// <typeparam name="T">Any <see cref="VisualElement"/> derived <see cref="Type"/> to be instantiated as a container element for the decorator.</typeparam>
    /// <typeparam name="TAttribute">The <see cref="UnityEngine.PropertyAttribute"/> type being decorated.</typeparam>
    public abstract partial class Decorator<T, TAttribute> :
            DecoratorDrawerAdapter, 
            IDrawer<TAttribute>,
            IDecorator
        where T : VisualElement, new()
        where TAttribute : PropertyAttribute
    {
        /// <summary>
        /// A reference to the related <see cref="PropertyField"/> for serialized properties or the
        /// <see cref="BindablePropertyField"/> for non-serialized ones.  
        /// </summary>
        public VisualElement Field { get; set; }
        
        /// <summary>
        /// The <see cref="IPropertyNode"/> being decorated.
        /// </summary>
        public IPropertyNode Property { get; set; }
        
        /// <summary>
        /// The <see cref="PropertyAttribute"/> being decorated.
        /// </summary>
        public virtual TAttribute Target => (TAttribute) attribute;

        /// <summary>
        /// Gets the <see cref="VisualElement"/> that this decorator should affect to.
        /// </summary>
        public VisualElement VisualTarget => Container.GetNextSibling<IAutoElement, IElementBlock>();
        
        public GhostDecorator GhostContainer { get; set; }
        
        VisualElement IDrawer.Container => Container;
        
        /// <summary>
        /// The <see cref="VisualElement"/> container for this decorator.
        /// </summary>
        public T Container { get; private set; }

        public MarkerDecoratorsCache DecoratorsCache =>
            CachedDecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(this);

        protected MarkerDecoratorsCache CachedDecoratorsCache;
        
        /// <summary>
        /// Override this in order to make the decorator call <see cref="OnUpdate">OnUpdate()</see> on any value change
        /// on the property's declaring object. Defaults to false.
        /// </summary>
        protected virtual bool UpdateOnAnyValueChange => false;

        /// <summary>
        /// Override this method to add any initial <c>VisualElement</c>s to the newly created decorator
        /// <see cref="Container"/> (of type <typeparamref name="T"/>) before it is added to the <c>VisualElement</c>'s
        /// hierarchy.
        /// </summary>
        /// <remarks>
        /// At this point, values for <see cref="Field"/>, <see cref="Property"/> and <see cref="Target"/> are already
        /// provided but <see cref="VisualTarget"/> is not since the <see cref="Container"/> is not yet added to the
        /// <c>VisualElement</c>'s hierarchy.
        /// </remarks>
        /// <param name="container">This decorators <c>VisualElement</c> <see cref="Container"/> of type <typeparamref name="T"/>.</param>
        protected virtual void OnCreatePropertyGUI(VisualElement container) {}

        /// <summary>
        /// Override this method to perform any custom logic when the decorator needs to be updated.
        /// </summary>
        public virtual void OnUpdate() {}
        
        /// <summary>
        /// Override this method to perform any custom logic when the decorator is created.
        /// </summary>
        public virtual void OnAttachedAndReady(VisualElement element) { }

        // ReSharper disable once PublicConstructorInAbstractClass VirtualMemberCallInConstructor
        public Decorator() => Initialize();

        [PublicAPI]
        public virtual void Initialize() => LogInternal("[D!] Initialize()!");

        public sealed override VisualElement CreatePropertyGUI()
        {
            LogInternal("[D!] CreatePropertyGUI()! <b>FROM STATE:</b> " + m_State);
            if (!(m_State <= State.None))
            {
                LogInternal("ASSERT FAIL: m_State <= State.None");
                m_DetachingItself = true;
                m_State = State.ResetToInitialState;
                ResetToInitialState();
                m_DetachingItself = false;
            }
            Assert.IsTrue(m_State <= State.None);
            m_State = State.AwaitingGhost;
            GhostContainer = new GhostDecorator() { TargetDecorator = this };
            GhostContainer.WithDevTools(this);
            GhostContainer.RegisterOnAttachToPanelEventOnce(OnAttachGhostToPanel);
            GhostContainer.UnregisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);
            GhostContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);

            return GhostContainer;
        }

        private void OnAttachContainerToPanel()
        {
            Assert.IsTrue(m_State == State.ContextReady);
            if (!Property.IsValid())
            {
                LogInternalError("[D!] OnAttachContainerToPanel()! <b>Property IS NOT VALID!</b>");
                return;
            }
            m_State = State.AttachedAndReady;
            OnAttachedAndReady(Container);
        }
        
        private void OnGeometryChanged(GeometryChangedEvent _)
        {
            LogInternal("[D!] OnGeometryChanged()! <b>FROM STATE:</b> " + m_State + " m_OnUpdateCalled: " + m_OnUpdateCalled);
            Container.UnregisterOnGeometryChangedEventOnce(OnGeometryChanged);
            if (Property == null) return;
            if (m_OnUpdateCalled/* || !(Property?.IsValid() ?? false)*/) return;
                m_OnUpdateCalled = true;
            Assert.IsTrue(m_State == State.AttachedAndReady);
            if (Property?.IsValid() != true)
            {
                LogInternalError("[D!] OnGeometryChanged()! Property IS <b>NOT VALID!</b>");
                return;
            }
            m_State = State.Done;
            OnUpdate();
        }
    }
}
