using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using UnityEditor.Build.Content;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    public interface IDecorator : IDrawer
    {
        GhostDecorator GhostContainer { get; set; }
        
        /// <summary>
        /// Ensures that the container is properly attached and positioned.
        /// </summary>
        /// <param name="onProperlyAttachedCallback"></param>
        /// <returns>Whether the container was properly attached or not.</returns>
        bool EnsureContainerIsProperlyAttached(Action onProperlyAttachedCallback = null);

        bool HasValidContainer();

        void OnAttachedAndReady(VisualElement element);
    }
    
    public abstract class Decorator<T, TAttribute> : 
            DecoratorDrawer, 
            IDrawer<TAttribute>,
            IDecorator
        where T : VisualElement, new()
        where TAttribute : PropertyAttribute
    {
        public PropertyField Field { get; set; }
        
        public SerializedProperty Property { get; set; }
        
        public virtual TAttribute Target => (TAttribute) attribute;

        public VisualElement VisualTarget => Container.GetNextSibling<AutoDecorator, IElementBlock>();
        
        public GhostDecorator GhostContainer { get; set; }
        
        VisualElement IDrawer.Container => Container;
        
        public T Container { get; private set; }
        
        public MarkerDecoratorsCache DecoratorsCache => 
            CachedDecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(
                Field?.GetParentPropertyField()?.GetSerializedProperty()?.GetHashCode() ?? Property.serializedObject.GetHashCode(),
                Property.serializedObject.GetHashCode());

        protected MarkerDecoratorsCache CachedDecoratorsCache;
        
        private bool m_Detached;
        private bool m_Ready;
        private bool m_GeometryChangePending;
        private bool m_GeometryChangeCompleted;
        private bool m_Disposed = false;
        
        private IVisualElementScheduledItem m_DelayedTask;

        protected virtual bool RedrawOnAnyValueChange => false;

        protected virtual void OnCreatePropertyGUI(VisualElement container) {}

        /// <summary>
        /// Override this method to perform any custom logic when the decorator needs to be updated.
        /// </summary>
        public virtual void OnUpdate() {}
        
        public virtual void OnAttachedAndReady(VisualElement element) { }
        
        public sealed override VisualElement CreatePropertyGUI()
        {
            if (Container != null)
                OnReset(disposing: false);

            if (Container == null)
            {
                Container = new T();
                OnCreatePropertyGUI(Container);
            }
            
            GhostContainer = new GhostDecorator() { TargetDecorator = this };
            m_Detached = false;
            m_Ready = false;
            GhostContainer.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachGhostToPanel);

            return GhostContainer;
        }

        private void OnAttachGhostToPanel(AttachToPanelEvent ev)
        {
            if ((VisualElement)ev.target != GhostContainer)
                Debug.LogError($"Target {((VisualElement)ev.target).name} is not the same as {GhostContainer.name}");
            
            if (m_Detached)
                Debug.LogError($"GhostContainer already attached: {((VisualElement)ev.target).name}");

            BindToClosestParentPropertyFieldOf(GhostContainer);
            if (Field != null)
            {
                ((IDrawer) this).AddDefaultStyles();
                EnsureContainerIsProperlyAttached(OnAttachContainerToPanel);
            }
            else
                Debug.LogWarning($"Could not find parent PropertyField of {((VisualElement)ev.target).name} for {attribute.GetType().Name}");
            GhostContainer.UnregisterCallback<AttachToPanelEvent>(OnAttachGhostToPanel);

            ((IDrawer) this).InspectorElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// Ensures that the container is properly attached and positioned.
        /// </summary>
        /// <param name="onProperlyAttachedCallback"></param>
        /// <returns>Whether the container was properly attached or not.</returns>
        public bool EnsureContainerIsProperlyAttached(Action onProperlyAttachedCallback = null)
        {
            if (!HasValidContainer())
            {
                m_Ready = false;
                Container.RemoveFromHierarchy();
                Container.WithClasses("ui3x-detached");
                m_Detached = true;
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
                VisualElement previous;
                VisualElement aux;
                for (var i = 0; i < posFromEnd; i++)
                {
                    previous = element.GetPreviousSibling();
                    if (previous == null)
                        break;
                    if (previous is AutoDecorator)
                    {
                        aux = previous.GetPreviousSibling();
                        if (aux is GroupMarker)
                        {
                            previous = aux;
                        }
                        element = previous;
                    }
                }
            }
            
            element.AddBefore(Container);
        }

        private void OnAttachContainerToPanel()
        {
            if (!m_Detached || m_Ready)
                Debug.LogError($"Unexpected flags found: {Container.name}, Attribute: {attribute.GetType().Name}, Detached: {m_Detached}, Ready: {m_Ready}");

            m_Ready = true;
            OnAttachedAndReady(Container);
        }

        private void OnGeometryChanged(GeometryChangedEvent ev)
        {
            m_GeometryChangeCompleted = true;
            if (((IDrawer) this).InspectorElement is InspectorElement inspectorElement)
            {
                inspectorElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                OnUpdate();
            }
            else
                Debug.LogError($"<color=#FF0000FF><b>Could not find InspectorElement for {attribute.GetType().Name}</b> {((VisualElement)ev.target)?.AsString()}</color>");
        }

        public virtual void OnReset(bool disposing = false)
        {
            ((IDrawer) this).InspectorElement?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Container?.RemoveFromHierarchy();
            Property = null;
            Field = null;
            Container = null;
            m_Detached = false;
            m_Ready = false;
            m_GeometryChangePending = false;
            m_GeometryChangeCompleted = false;
            if (disposing)
            {
                GhostContainer?.RemoveFromHierarchy();
                GhostContainer = null;
            }
        }
        
        private void BindToClosestParentPropertyFieldOf(VisualElement target)
        {
            Field = target.GetClosestParentOfType<PropertyField, InspectorElement>();
            if (Field == null)
            {
                Debug.LogWarning($"<color=#FF0000CC>Could not find parent PropertyField of {target.name} for {attribute.GetType().Name}</color>");
                return;
            }

            Property = Field.GetSerializedProperty();
            if (RedrawOnAnyValueChange)
                TrackAllChangesOnInspectorElement();
        }

        private void TrackAllChangesOnInspectorElement()
        {
            ((IDrawer) this).InspectorElement.TrackSerializedObjectValue(Property.serializedObject, OnSerializedObjectValueChanged);
        }
        
        private void OnSerializedObjectValueChanged(SerializedObject serializedObject) => OnUpdate();

        public virtual bool HasValidContainer() => 
            Container != null 
            && Field != null 
            && Container.GetNextSiblingOfType<PropertyField>() == Field;

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
            if (disposing)
                m_Disposed = true;

            OnReset(disposing: true);
        }
        
        #endregion
    }
}
