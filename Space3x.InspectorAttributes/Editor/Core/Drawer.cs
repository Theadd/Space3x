﻿using System;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    /// <summary>
    /// The base class to derive from when implementing your custom <see cref="PropertyDrawer"/> on a
    /// <see cref="PropertyAttribute"/>.
    /// That's right, it's tied to a PropertyAttribute and, unlike the ordinary PropertyDrawer, it is not
    /// designed to be used directly on a Type, which would remove the need to annotate a custom <see cref="Type"/>
    /// meant to be drawn, always, using the same <see cref="PropertyDrawer"/>.
    /// </summary>
    /// <remarks>
    /// However, by simply annotating the relevant members on that custom <see cref="Type"/>'s definition,
    /// you can get whatsoever you were looking for in just a tiny fraction of the time you would have spent
    /// implementing a custom PropertyDrawer.
    /// 
    /// If for any reason that would not be feasible and neither having to annotate that type every time its used,
    /// like in a public API, post an issue on the project repo in GitHub, otherwise it won't be even considered.
    /// </remarks>
    /// <typeparam name="TAttribute">The <see cref="PropertyAttribute"/> type being decorated.</typeparam>
    public abstract class Drawer<TAttribute> : PropertyDrawer, IDrawer<TAttribute>, ICreatePropertyNodeGUI
        where TAttribute : PropertyAttribute
    {
        public virtual TAttribute Target => (TAttribute) attribute;
        public IPropertyNode Property { get; set; }
        public VisualElement Container { get; set; }
        public VisualElement VisualTarget => Field;
        public VisualElement Field => m_Field ??= Container.GetClosestParentOfType<PropertyField, InspectorElement>();

        private VisualElement m_Field;

        public MarkerDecoratorsCache DecoratorsCache => 
            m_DecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(this);
        
        private MarkerDecoratorsCache m_DecoratorsCache;
        
        private bool m_Disposed;
        
        /// <summary>
        /// It's the <see cref="Drawer{TAttribute}"/> version of the <see cref="PropertyDrawer.CreatePropertyGUI"/>
        /// method, that is, the entry point in which you create the initial VisualElement hierarchy for this
        /// <see cref="Drawer{TAttribute}"/>.
        /// Except that now it receives an <see cref="IPropertyNode"/> as parameter, allowing your property drawers to
        /// not only work with <see cref="SerializedProperty"/>, but also with the non-serialized ones.
        /// </summary>
        /// <param name="property">The <see cref="IPropertyNode"/> being drawn.</param>
        protected abstract VisualElement OnCreatePropertyGUI(IPropertyNode property);

        public VisualElement CreatePropertyNodeGUI(IPropertyNode property)
        {
            if (Property != null)
                return ((ICreatePropertyNodeGUI)this.CreateCopy()).CreatePropertyNodeGUI(property);
            DebugLog.Info($"[CREATE DRAWER] <color=#FFFF00FF><b>[CREATE]</b> {this.GetType().Name} {this.GetHashCode()} on {property.PropertyPath}</color>");
            Property = property;
            Container = OnCreatePropertyGUI(Property);
            Container.WithDevTools(this);
            Container.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
            return Container;
        }
        
        /// <summary>
        /// Since it is the only entry point of any <see cref="PropertyDrawer"/>, it's been sealed and the abstract
        /// method <see cref="OnCreatePropertyGUI"/> is provided instead.
        /// Except that now it receives an <see cref="IPropertyNode"/> as parameter, allowing your property drawers to
        /// not only work with <see cref="SerializedProperty"/>, but also with the non-serialized ones.
        /// </summary>
        /// <seealso cref="OnCreatePropertyGUI"/>
        /// <param name="property">The <see cref="SerializedProperty"/> being drawn.</param>
        /// <returns>The <see cref="VisualElement"/> returned by the <see cref="OnCreatePropertyGUI"/> method.</returns>
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            CreatePropertyNodeGUI(property.GetPropertyNode());

        private void OnAttachToPanel(AttachToPanelEvent evt) => ((IDrawer)this).AddDefaultStyles();

        /// <summary>
        /// Override this method in order to perform any custom logic when this property drawer needs to be updated.
        /// </summary>
        public virtual void OnUpdate() { }

        // void ICreateDrawerOnPropertyNode.SetPropertyNode(IPropertyNode property)
        // {
        //     Debug.Log($"IN Drawer.SetPropertyNode() FROM {Property.PropertyPath} TO {property.PropertyPath}");
        //     // if (Property.Equals(property)) return;
        //     Property = property;
        //     // BindableUtility.AutoNotifyValueChangedOnNonSerialized(Container, Property);
        //     OnUpdate();
        // }
        
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
