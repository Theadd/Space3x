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
            LogInternal("[D!] CreatePropertyGUI()!");
            // var numGhosts = GhostContainer?.hierarchy.parent?.hierarchy.childCount ?? -1;
            Assert.IsTrue(m_State <= State.None);
            m_Disposed = false;
            // if (GhostContainer != null || m_TotallyRemoved)
            // {
            //     m_DetachingItself = true;
            //     ResetToInitialState();
            //     m_DetachingItself = false;
            //     // DebugLog.Info($"  <b><color=#6666FFFF>[CREATE COPY!]</color> ThisHash: {this.GetHashCode()}</b>");
            //     // return ((DecoratorDrawerAdapter)DrawerUtility.CopyDecoratorDrawer(this)).CreatePropertyGUI();
            //
            // }
            // DebugLog.Info($"<color=#FFFF00FF><b>[CREATE]</b> {this.GetType().Name}, NºGh: {numGhosts}, NullGh: {GhostContainer == null}, " +
            //               $"NullCT: {Container == null}, NullP: {Property == null}, THash: {this.GetHashCode()}, Rem: {m_Removed}, " +
            //               $"Add: {m_Added}, TotallyRemoved: {m_TotallyRemoved}</color>");
            m_State = State.AwaitingGhost;
            GhostContainer = new GhostDecorator() { TargetDecorator = this };
            GhostContainer.WithDevTools(this);
            // m_Ready = false;
            GhostContainer.RegisterOnAttachToPanelEventOnce(OnAttachGhostToPanel);
            // GhostContainer.RegisterCallback<AttachToPanelEvent>(OnAttachGhostToPanel);
            GhostContainer.UnregisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);
            GhostContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);

            return GhostContainer;
        }

        private void OnAttachContainerToPanel()
        {
            if (m_TotallyRemoved) return;
            // if (GhostContainer.panel == null) return;
            if (GhostContainer?.IsPhysicallyAndLogicallyDetachedFromPanel() ?? true)
            {
                // TODO: remove
                Assert.IsFalse(true);
                return;
            }
            
            if (m_Ready || !Property.IsValid()) return;
            m_Ready = true;

            OnAttachedAndReady(Container);
        }
        
        private void OnGeometryChanged(GeometryChangedEvent _)
        {
            if (m_TotallyRemoved) return;
            // EDIT: current implementation should avoid the commented block below, placing assertion to make sure of it.
            // Assert.IsFalse(GhostContainer.IsPhysicallyAndLogicallyDetachedFromPanel());

            if (Property == null) return;
            if (m_OnUpdateCalled || !(Property?.IsValid() ?? false)) return;
            m_OnUpdateCalled = true;
            OnUpdate();
        }
        
        private void BindToClosestParentPropertyFieldOf(VisualElement target)
        {
#if UNITY_EDITOR
            Field = target.GetClosestParentOfAnyType<UnityEditor.UIElements.PropertyField, BindablePropertyField, UnityEditor.UIElements.InspectorElement>();
#else
            Field = target.GetClosestParentOfType<BindablePropertyField, TemplateContainer>();
#endif
            if (Field == null)
            {
                Debug.LogWarning($"<color=#FF0000CC>Could not find parent PropertyField of {target.name} for {attribute.GetType().Name}</color>");
                return;
            }

            try
            {
                Property = Field.GetPropertyNode();
            }
            catch (ObjectDisposedException ex)
            {
#if UNITY_EDITOR
                LogInternal(ex.ToString());
                DebugInternal("##@@##@@##@@##@@##@@##@@ FORCING FULL REBIND ON PROPERTYFIELD # ");
                UnityEditor.UIElements.BindingExtensions.Unbind((UnityEditor.UIElements.PropertyField)Field);
                UnityEditor.UIElements.BindingExtensions.Bind((UnityEditor.UIElements.PropertyField)Field, Property.GetSerializedObject());
#endif
                Property = Field.GetPropertyNode();
            }
            if (UpdateOnAnyValueChange)
                GhostContainer.TrackSerializedObjectValue(Property, OnUpdate);
        }
    }
}
