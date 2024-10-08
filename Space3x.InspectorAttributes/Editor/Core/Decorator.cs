﻿using System;
using Space3x.Attributes.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    /// <summary>
    /// The base class to derive from when implementing your custom <see cref="DecoratorDrawer"/> on a <see cref="PropertyAttribute"/>.
    /// </summary>
    /// <typeparam name="T">Any <see cref="VisualElement"/> derived <see cref="Type"/> to be instantiated as a container element for the decorator.</typeparam>
    /// <typeparam name="TAttribute">The <see cref="PropertyAttribute"/> type being decorated.</typeparam>
    public abstract class Decorator<T, TAttribute> : 
            DecoratorDrawer, 
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
        /// A reference to the <see cref="VisualElement"/> that this decorator should affect to.
        /// </summary>
        public VisualElement VisualTarget => Container.GetNextSibling<AutoDecorator, IElementBlock>();
        
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
        /// on the serialized object being inspected. Defaults to false.
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
        /// <param name="container">This decorator's <c>VisualElement</c> <see cref="Container"/> of type <typeparamref name="T"/>.</param>
        protected virtual void OnCreatePropertyGUI(VisualElement container) {}

        /// <summary>
        /// Override this method to perform any custom logic when the decorator needs to be updated.
        /// </summary>
        public virtual void OnUpdate() {}
        
        /// <summary>
        /// Override this method to perform any custom logic when the decorator is created.
        /// </summary>
        public virtual void OnAttachedAndReady(VisualElement element) { }
        
        private bool m_Ready;
        private bool m_Disposed = false;
        private bool m_Removed = false;
        private bool m_TotallyRemoved = false;
        private bool m_Added = false;
        private bool m_OnUpdateCalled = false;
        
        public sealed override VisualElement CreatePropertyGUI()
        {
            var numGhosts = GhostContainer?.hierarchy.parent?.hierarchy.childCount ?? -1;
            // DebugLog.Info("IN CreatePropertyGUI(); numGhosts: " + numGhosts + $"; ThisHash: {this.GetHashCode()}");
            if (GhostContainer != null || m_TotallyRemoved)
            {
                // DebugLog.Info($"  <b><color=#6666FFFF>[CREATE COPY!]</color> ThisHash: {this.GetHashCode()}</b>");
                return this.CreateCopy().CreatePropertyGUI();
            }
            // DebugLog.Info($"<color=#FFFF00FF><b>[CREATE]</b> {this.GetType().Name}, NºGh: {numGhosts}, NullGh: {GhostContainer == null}, " +
            //               $"NullCT: {Container == null}, NullP: {Property == null}, THash: {this.GetHashCode()}, Rem: {m_Removed}, " +
            //               $"Add: {m_Added}, TotallyRemoved: {m_TotallyRemoved}</color>");
            GhostContainer = new GhostDecorator() { TargetDecorator = this };
            GhostContainer.WithDevTools(this);
            m_Ready = false;
            GhostContainer.RegisterCallback<AttachToPanelEvent>(OnAttachGhostToPanel);
            GhostContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);

            return GhostContainer;
        }

        private void OnDetachGhostFromPanel(DetachFromPanelEvent evt)
        {
            var hasPanel = GhostContainer.hierarchy.parent?.hierarchy.parent != null;
            var originPanelHash = GetPanelContentHash(evt.originPanel);
            // DebugLog.Info($"<color=#FFFF00FF>OnDetachGhostFromPanel: <b>[{(hasPanel ? "IGNORE, HAS PANEL" : "REMOVE, NO PANEL")}]</b> " +
            //               $"{this.GetType().Name}, Removed: {m_Removed}, Added: {m_Added}, ThisHash: {this.GetHashCode()}</color>");

            if (hasPanel)
                return; // TODO
            m_Removed = true;
            if (originPanelHash != 0)
            {
                // GhostContainer?.LogThis($"REMOVING GHOST... HAS PANEL: {hasPanel}, THash: {this.GetHashCode()} => " +
                //                         $"{this.GetType().Name}, OriginPanel: {originPanelHash}, DestPanel: " +
                //                         $"{(evt.destinationPanel == null ? -1 : GetPanelContentHash(evt.destinationPanel))}");
                ProperlyRemoveFromHierarchy();
            }
            else
            {
                GhostContainer?.LogThis($"INVALIDATING GHOST... HAS PANEL: {hasPanel}, THash: {this.GetHashCode()} => {this.GetType().Name}, " +
                                        $"OriginPanel: {originPanelHash}, DestPanel: {(evt.destinationPanel == null ? -1 : GetPanelContentHash(evt.destinationPanel))}");
            }
        }

        public void ProperlyRemoveFromHierarchy()
        {
            m_Removed = true;
            m_TotallyRemoved = true;
            GhostContainer?.UnregisterCallback<AttachToPanelEvent>(OnAttachGhostToPanel);
            GhostContainer?.UnregisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);
            GhostContainer?.Unbind();
            // EDIT: 
            // OnReset(disposing: false);
            Dispose(disposing: false);
            //
            GhostContainer = null;
        }
        
        private static int GetPanelContentHash(IPanel panel) =>
            panel?.visualTree is VisualElement { childCount: >= 2 } vPanel
                ? vPanel[1].GetHashCode()
                : 0;
        
        private void OnAttachGhostToPanel(AttachToPanelEvent ev)
        {
            if (m_TotallyRemoved)
            {
                DebugLog.Error($"<b>IN OnAttachGhostToPanel (1) FOR A TOTALLY REMOVED {this.GetType().Name} ThisHash: {this.GetHashCode()}</b>");
                return;
            }
            
            if (m_Added)
            {
                // GhostContainer.LogThis($"  <color=#FFFF00FF>OnAttachGhostToPanel: <b>[SKIP ADD!!]</b> {this.GetType().Name}, " +
                //                        $"THash: {this.GetHashCode()}, Rem: {m_Removed}, Add: {m_Added}, " +
                //                        $"PanelHash: {GetPanelContentHash(ev.destinationPanel)}</color>");
                BindToClosestParentPropertyFieldOf(GhostContainer);
                return;
            }

            // GhostContainer.LogThis($"  <color=#FFFF00FF>OnAttachGhostToPanel: <b>[ADD]</b> {this.GetType().Name}, " +
            //                        $"THash: {this.GetHashCode()}, Rem: {m_Removed}, Add: {m_Added}, " +
            //                        $"PanelHash: {GetPanelContentHash(ev.destinationPanel)}</color>");
            BindToClosestParentPropertyFieldOf(GhostContainer);
            m_Added = true;
            if (!m_Removed)
            {
                if (Field != null)
                {
                    if (Container == null)
                    {
                        ((IDrawer) this).AddDefaultStyles();
                        Container = new T();
                        Container.WithDevTools(this);
                        OnCreatePropertyGUI(Container);
                    }
                    EnsureContainerIsProperlyAttached(() =>
                    {
                        if (m_TotallyRemoved)
                        {
                            DebugLog.Error($"<b>IN OnAttachGhostToPanel (3) FOR A TOTALLY REMOVED {this.GetType().Name} ThisHash: {this.GetHashCode()}</b>");
                            return;
                        }
                        OnAttachContainerToPanel();
                        EditorApplication.delayCall += OnGeometryChanged;
                    });
                    
                    ((IDrawer) this).InspectorElement?.RegisterCallbackOnce<GeometryChangedEvent>(_ => OnGeometryChanged());
                }
                else
                    Debug.LogWarning($"<color=#FF0000CC>Could not find parent PropertyField of {((VisualElement)ev.target).name} for {attribute.GetType().Name}</color>");
            }
        }

        /// <inheritdoc/>
        public bool EnsureContainerIsProperlyAttached(Action onProperlyAttachedCallback = null)
        {
            if (!HasValidContainer())
            {
                m_Ready = false;
                Container.RemoveFromHierarchy();
                Container.WithClasses("ui3x-detached");
                if (onProperlyAttachedCallback != null)
                    Container.RegisterCallbackOnce<AttachToPanelEvent>((_) => 
                    {
                        onProperlyAttachedCallback.Invoke();
                    });
                ProperlyAddContainerBeforeField();
                return false;
            }
            else
                onProperlyAttachedCallback?.Invoke();

            return true;
        }

        private int GetPositionOfGhostDecoratorFromTheEnd()
        {
            var parent = GhostContainer.hierarchy.parent;
            if (parent == null)
                return -1;
            var index = parent.hierarchy.IndexOf(GhostContainer);
            var maxIndex = parent.hierarchy.childCount - 1;
            var sum = 0;
            for (var i = index + 1; i <= maxIndex; i++)
            {
                if (parent.hierarchy.ElementAt(i) is GhostDecorator)
                    sum++;
            }
            return sum;
        }

        private VisualElement GetContainerOfPreviousDecorator()
        {
            var parent = GhostContainer.hierarchy.parent;
            if (parent == null)
                return null;
            var index = parent.hierarchy.IndexOf(GhostContainer) - 1;
            if (index < 0)
                return null;

            var previous = parent.hierarchy.ElementAt(index) as VisualElement;
            if (previous is not GhostDecorator previousGhostDecorator)
                return null;
            
            return previousGhostDecorator.DecoratorContainer;
        }

        private void ProperlyAddContainerBeforeField()
        {
            var previousContainer = GetContainerOfPreviousDecorator();
            if (previousContainer != null)
            {
                previousContainer.AddAfter(Container);
                return;
            }
            var posFromEnd = GetPositionOfGhostDecoratorFromTheEnd();
            var element = (VisualElement) Field;
            if (posFromEnd > 0)
            {
                for (var i = 0; i < posFromEnd; i++)
                {
                    var previous = element.GetPreviousSibling();
                    if (previous == null)
                        break;
                    if (previous is AutoDecorator)
                    {
                        if (previous.GetPreviousSibling() is GroupMarker aux) 
                            previous = aux;
                        element = previous;
                    }
                }
            }
            
            element.AddBefore(Container);
        }

        private void OnAttachContainerToPanel()
        {
            // Debug.Log("AAAAA " + Property.PropertyPath + " IsValid: " + Property.IsValid() + $", m_Ready = {m_Ready}, m_OnUpdateCalled = {m_OnUpdateCalled}, m_TotallyRemoved = {m_TotallyRemoved}" + $"; ThisHash: {this.GetHashCode()}");
            // EDIT: Added "|| !Property.IsValid()"
            if (m_Ready || !Property.IsValid()) return;
            m_Ready = true;
            if (m_TotallyRemoved) return;

            OnAttachedAndReady(Container);
        }

        private void OnGeometryChanged()
        {
            // Debug.Log($"Property is null: {Property == null}; m_OnUpdateCalled = {m_OnUpdateCalled}; m_TotallyRemoved = {m_TotallyRemoved}; ThisHash: {this.GetHashCode()}");
            // Debug.Log("BBBBB " + Property?.PropertyPath + " IsValid: " + Property?.IsValid() + $"; ThisHash: {this.GetHashCode()}");
            if (Property == null)
            {
                // Debug.Log($"STOP HERE! ThisHash: {this.GetHashCode()}");
                return;
            }
            // EDIT: Added "|| !Property.IsValid()"
            if (m_OnUpdateCalled || !Property.IsValid()) return;
            m_OnUpdateCalled = true;
            if (m_TotallyRemoved) return;
            OnUpdate();
        }

        public virtual void OnReset(bool disposing = false)
        {
            // DebugLog.Warning($"<color=#FF0000FF>[RESET]</color> {this.GetType().Name}, ThisHash: {this.GetHashCode()}, " +
            //                  $"Removed: {m_Removed}, Added: {m_Added}, Ready: {m_Ready}");
            // ((IDrawer) this).InspectorElement?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Container?.RemoveFromHierarchy();
            Field = null;
            Container = null;
            m_Ready = false;
            if (disposing)
            {
                Property = null;
                GhostContainer?.RemoveFromHierarchy();
                GhostContainer = null;
            }
        }
        
        private void BindToClosestParentPropertyFieldOf(VisualElement target)
        {
            Field = target.GetClosestParentOfAnyType<BindablePropertyField, PropertyField, InspectorElement>();
            if (Field == null)
            {
                Debug.LogWarning($"<color=#FF0000CC>Could not find parent PropertyField of {target.name} for {attribute.GetType().Name}</color>");
                return;
            }

            try
            {
                Property = Field.GetPropertyNode();
            }
            catch (ObjectDisposedException)
            {
                ((PropertyField)Field).Unbind();
                ((PropertyField)Field).Bind(this.Property.GetController().SerializedObject);
                Property = Field.GetPropertyNode();
            }
            if (UpdateOnAnyValueChange)
                GhostContainer.TrackSerializedObjectValue(Property, OnUpdate);
        }

        public virtual bool HasValidContainer() => 
            Container != null 
            && Field != null 
            && Container.GetNextSiblingOfType<PropertyField, BindablePropertyField>() == Field;

#region IDisposable
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;
            
            // Dirty hack to prevent unexpected calls to dispose between OnAttachedAndReady and the first OnUpdate call.
            // This behaviour has been observed when adding new components to a GameObject with existing annotated components.
            if (disposing && !m_Removed && m_Ready && !m_TotallyRemoved && !m_OnUpdateCalled && m_Added)
                return;
            
            if (disposing)
                m_Disposed = true;

            OnReset(disposing: true);
        }
#endregion
        
#if !UNITY_2023_2_OR_NEWER
        public override bool CanCacheInspectorGUI() => false;
#endif
    }
}
