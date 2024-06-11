using System;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    public interface ICreateDrawerOnPropertyNode
    {
        public VisualElement CreatePropertyNodeGUI(IProperty property);
    }
    
    public abstract class Drawer<TAttribute> : PropertyDrawer, IDrawer<TAttribute>, ICreateDrawerOnPropertyNode
        where TAttribute : PropertyAttribute
    {
        public virtual TAttribute Target => (TAttribute) attribute;
        public IProperty Property { get; set; }
        public VisualElement Container { get; set; }
        public VisualElement VisualTarget => Field;
        public VisualElement Field => m_Field ??= Container.GetClosestParentOfType<PropertyField, InspectorElement>();

        private VisualElement m_Field;

        public MarkerDecoratorsCache DecoratorsCache => 
            m_DecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(
                Field?.GetParentPropertyField()?.GetSerializedProperty()?.GetHashCode() 
                ?? Property.GetSerializedObject().GetHashCode(),
                Property.GetSerializedObject().GetHashCode());

        private MarkerDecoratorsCache m_DecoratorsCache;
        
        private bool m_Disposed;
        
        protected abstract VisualElement OnCreatePropertyGUI(IProperty property);

        public VisualElement CreatePropertyNodeGUI(IProperty property)
        {
            Property = property;
            Container = OnCreatePropertyGUI(Property);
            Container.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
            return Container;
        }
        
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var pNode = property.GetPropertyNode();
            Debug.Log($"// TODO: remove; @CreatePropertyGUI: {(property?.propertyPath ?? "--NONE--")}; pNode.PropertyPath: {(pNode?.PropertyPath ?? "--NONE--")}");
            return CreatePropertyNodeGUI(property.GetPropertyNode());
        }

        private void OnAttachToPanel(AttachToPanelEvent evt) => 
            ((IDrawer)this).AddDefaultStyles();

        /// <summary>
        /// Override this method to perform any custom logic when the property drawer needs to be updated.
        /// </summary>
        public virtual void OnUpdate() { }
        
        #region IDisposable

        public virtual void OnReset(bool disposing = false)
        {
            if (disposing)
            {
                Container?.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
                Container?.RemoveFromHierarchy();
                Property = null;
                m_Field = null;
                Container = null;
            }
        }
        
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
