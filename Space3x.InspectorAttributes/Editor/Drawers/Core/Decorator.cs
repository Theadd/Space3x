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
            m_DecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(
                Field?.GetParentPropertyField()?.GetSerializedProperty()?.GetHashCode() 
                ?? Property.serializedObject.GetHashCode());

        private MarkerDecoratorsCache m_DecoratorsCache;
        
        private bool m_Detached;
        private bool m_Ready;
        private bool m_Invalid;
        private bool m_GeometryChangePending;
        private bool m_GeometryChangeCompleted;
        private bool m_Disposed = false;
        
        private IVisualElementScheduledItem m_DelayedTask;

        protected virtual bool RedrawOnAnyValueChange => false;
        
        /// <summary>
        /// There are a few cases where <c>CreatePropertyGUI</c> is called multiple times, such when encountering a
        /// <c>[field: SerializeField]</c> attribute on a public property member, which causes all previously rendered
        /// decorators to be redrawn and since we re-parent our decorators to the same parent as their attribute target
        /// we need to keep them together as a single element. Override this property and return <c>false</c> to get rid
        /// of the existing element and create a new one, or just leave it as is to keep the existing element and ignore
        /// the new one.
        /// </summary>
        protected virtual bool KeepExistingOnCreatePropertyGUI => false; // TODO: true;
        
        protected virtual void OnCreatePropertyGUI(VisualElement container) {}

        /// <summary>
        /// Override this method to perform any custom logic when the decorator needs to be updated.
        /// </summary>
        public virtual void OnUpdate() {}
        
        public sealed override VisualElement CreatePropertyGUI()
        {
            if (Container != null || m_Invalid)
            {
                Debug.LogWarning($"<color=#FF0000CC>Container already created: {Container?.name}; INVALID: {m_Invalid}, {attribute.GetType().Name}</color>");
//                if (this is GroupMarkerDecorator groupMarkerDecorator)
//                {
//                    DebugMe();
//                    Debug.Log("... continue from 'Container already created' ..." + groupMarkerDecorator.ValidateAllString());
//                }
                if (attribute is ShowInInspectorAttribute attr)
                    throw new Exception($"ShowInInspectorAttribute is not expected here.");
                if (m_Invalid || KeepExistingOnCreatePropertyGUI)
                    throw new Exception($"Unexpected code path here. m_Invalid: {m_Invalid}, KeepExistingOnCreatePropertyGUI: {KeepExistingOnCreatePropertyGUI}");
                    // return IgnoreCreatePropertyGUI();
                
                OnReset(disposing: false);
            }
            
            if (Container == null)
            {
                Container = new T() { /*name = attribute.GetType().Name + "_Drawer" + $"--{Target.GetType().Name}"*/ };
                Container.name = attribute.GetType().Name + "_Drawer" + $"--{Container.GetHashCode()}";
                Container.WithClasses(this.GetType().Name + "-" + RuntimeHelpers.GetHashCode(this), Target.GetType().Name + "-" + Target.GetHashCode());
                OnCreatePropertyGUI(Container);
            }
            
            GhostContainer = new GhostDecorator() { TargetDecorator = this };
            GhostContainer.WithClasses(this.GetType().Name + "-" + RuntimeHelpers.GetHashCode(this), Target.GetType().Name + "-" + Target.GetHashCode());
            m_Detached = false;
            m_Ready = false;
            GhostContainer.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachGhostToPanel);

            GhostContainer.LogThis("CREATE GHOST DECORATOR");
            return GhostContainer;
        }

        private void OnAttachGhostToPanel(AttachToPanelEvent ev)
        {
            if ((VisualElement)ev.target != GhostContainer)
                Debug.LogError($"Target {((VisualElement)ev.target).name} is not the same as {GhostContainer.name}");
            
            if (m_Detached)
                Debug.LogError($"GhostContainer already attached: {((VisualElement)ev.target).name}");

            BindToClosestParentPropertyFieldOf(GhostContainer);     // ((VisualElement)ev.target);
            if (Field != null)
            {
                ((IDrawer) this).AddDefaultStyles();
//                m_Detached = true;
//                Container.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachContainerToPanel);
////                this.Detach();
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
//            if (m_GeometryChangeCompleted && !KeepExistingOnCreatePropertyGUI)
//                return;
//            
//            m_GeometryChangePending = false;
//            if (m_Invalid)
//            {
//                m_GeometryChangePending = KeepExistingOnCreatePropertyGUI;
//                return;
//            }

            m_GeometryChangeCompleted = true;
            Debug.Log($"In GeometryChangedEvent: {attribute.GetType().Name}");
            if (((IDrawer) this).InspectorElement is InspectorElement inspectorElement)
            {
                inspectorElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                Container.LogThis("CALL ONUPDATE FROM GEOMETRYCHANGED");
                OnUpdate();
            }
            else
                Debug.LogError($"<color=#FF0000FF><b>Could not find InspectorElement for {attribute.GetType().Name}</b> {((VisualElement)ev.target)?.AsString()}</color>");
        }

        public virtual void OnAttachedAndReady(VisualElement element) { }

        public virtual void OnReset(bool disposing = false)
        {
            Container?.LogThis("RESET");
            ((IDrawer) this).InspectorElement?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Container?.RemoveFromHierarchy();
            Property = null;
            Field = null;
            Container = null;
            m_Detached = false;
            // m_Invalid = !disposing;
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

            DebugMe();
            // TODO
            Property = typeof(PropertyField).GetField(
                    "m_SerializedProperty", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(Field) as SerializedProperty;
            
            // TODO
            if (Property != null)
            {
                Debug.Log("Getting parent property field... " + this.GetType().Name + " " + this.GetHashCode());
                var parentField = Field.GetParentPropertyField();
                if (parentField != null)
                {
                    var parentProperty = parentField.GetSerializedProperty();
                    Debug.Log($"<color=#FF0099FF>  ..IN OnCreatePropertyGUI, propertyPath: {Property.propertyPath}, parentPropertyPath: {parentProperty.propertyPath}, " +
                              $"property.hash: {Property.GetHashCode()}, parentProperty.hash: {parentProperty.GetHashCode()}, " +
                              $"serializedObject.hash: {Property.serializedObject.GetHashCode()}, " +
                              $"parentSerializedObject.hash: {parentProperty.serializedObject.GetHashCode()}" +
                              $"</color>\n{parentField.AsString()}");
                }
                else
                {
                    Debug.Log($"<color=#FF0099FF>  ..IN !!OnCreatePropertyGUI, propertyPath: {Property.propertyPath}, " +
                              $"property.hash: {Property.GetHashCode()}, " +
                              $"serializedObject.hash: {Property.serializedObject.GetHashCode()}</color>");
                }
            }
            
            if (RedrawOnAnyValueChange && !m_Invalid)
                TrackAllChangesOnInspectorElement();

            target.LogThis("BOUND TO PARENT PROPERTY FIELD");
            m_Invalid = false;
        }

        private void TrackAllChangesOnInspectorElement()
        {
            ((IDrawer) this).InspectorElement.TrackSerializedObjectValue(Property.serializedObject, OnSerializedObjectValueChanged);
        }
        
        private void OnSerializedObjectValueChanged(SerializedObject serializedObject) => OnUpdate();

        internal void DebugMe()
        {
            var allText = ((IDrawer) this).InspectorElement.AsHierarchyString();
            var count = RichTextViewer.Count();
            var title = $"{count}: {attribute.GetType().Name}-{attribute.GetHashCode()} {this.GetType().Name}-{RuntimeHelpers.GetHashCode(this)}";
            RichTextViewer.AddText(title, allText);
            Debug.Log("<color=#FF00FFCC>[DebugMe] " + title + "</color>");
//            if (Target is not BeginColumnAttribute)
//                return;
//
//            var allText = ((IDrawer) this).InspectorElement.AsHierarchyString();
//
//            var chunkSize = 4600;
//            var chunkCount = ((allText.Length - 1) / chunkSize) + 1;
//            
//            for (var i = 0; i < chunkCount; i++)
//            {
//                var start = i * chunkSize;
//                var end = (i + 1) * chunkSize;
//                end = end > allText.Length ? allText.Length : end;
//                Debug.Log($"<color=#FF0000CC>DEBUGGING ({i + 1}): </color>\n" + allText.Substring(start, end - start));
//            }
        }
        
        
//        #region KeepExistingOnCreatePropertyGUI
//        
//        private VisualElement IgnoreCreatePropertyGUI()
//        {
//            m_Invalid = true;
//            Container.LogThis("KEEPING EXISTING");
//            Debug.Log($"Creating an invalid drawer: {attribute.GetType().Name}");
//            var invalidContainer = new VisualElement() { name = attribute.GetType().Name + "_InvalidDrawer" };
//            invalidContainer.LogThis("NEW & INVALID");
//            invalidContainer.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanelInvalid);
//            return invalidContainer;
//        }
//
//        private void OnAttachToPanelInvalid(AttachToPanelEvent ev)
//        {
//            ((VisualElement) ev.target).LogThis("REMOVE INVALID FROM HIERARCHY");
//            ((VisualElement) ev.target).RemoveFromHierarchy();
//            Field.AddBefore(Container);
//            m_Invalid = false;
//            if (m_GeometryChangePending)
//                OnGeometryChanged(null);
//        }
//        
//        #endregion

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
            else
                Debug.LogWarning("<b>@<color=#FFFF00FF>Decorator.Dispose(false!?);</color></b>");
            
            OnReset(disposing: true);
        }
        
//        static Decorator()
//        {
//            // TODO
//            s_CachedInstances = new Dictionary<int, IDrawer>();
//            //EditorApplicationUtility.onSelectionChange += ClearCache;
//        }
//
//        private static Dictionary<int, IDrawer> s_CachedInstances;
//
//        private static void ClearCache()
//        {
//            try
//            {
//                foreach (var cachedInstance in s_CachedInstances.Values)
//                {
//                    cachedInstance.Dispose();
//                }
//            }
//            finally
//            {
//                s_CachedInstances.Clear();
//            }
//        }
        
        #endregion

    }
}
