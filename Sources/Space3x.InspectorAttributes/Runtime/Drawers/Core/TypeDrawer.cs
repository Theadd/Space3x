using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public abstract class TypeDrawer : PropertyDrawerAdapter, IDrawer, ICreatePropertyNodeGUI
    {
        public IPropertyNode Property { get; set; }
        public VisualElement Container { get; set; }
        public VisualElement VisualTarget => Field;
#if UNITY_EDITOR
        public VisualElement Field => m_Field ??= Container.GetClosestParentOfAnyType<UnityEditor.UIElements.PropertyField, BindablePropertyField, UnityEditor.UIElements.InspectorElement>();
#else
        public VisualElement Field => m_Field ??= Container.GetClosestParentOfType<BindablePropertyField, TemplateContainer>();
#endif

        private VisualElement m_Field;

        public MarkerDecoratorsCache DecoratorsCache => 
            m_DecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(this);
        
        private MarkerDecoratorsCache m_DecoratorsCache;
        
        private bool m_Disposed;
        
        // ReSharper disable once PublicConstructorInAbstractClass VirtualMemberCallInConstructor
        public TypeDrawer() => Initialize();

        public virtual void Initialize() { }
        
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
                return ((ICreatePropertyNodeGUI)DrawerUtility.CopyPropertyDrawer(this)).CreatePropertyNodeGUI(property);
            Property = property;
            Container = OnCreatePropertyGUI(Property);
            Container.WithDevTools(this);
            // Container.RegisterOnAttachToPanelEventOnce(OnAttachToPanel);
            Container.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
            return Container;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Since it is the only entry point of any <see cref="PropertyDrawer"/>, it's been sealed and the abstract
        /// method <see cref="OnCreatePropertyGUI"/> is provided instead.
        /// Except that now it receives an <see cref="IPropertyNode"/> as parameter, allowing your property drawers to
        /// not only work with <see cref="SerializedProperty"/>, but also with the non-serialized ones.
        /// </summary>
        /// <seealso cref="OnCreatePropertyGUI"/>
        /// <param name="property">The <see cref="SerializedProperty"/> being drawn.</param>
        /// <returns>The <see cref="VisualElement"/> returned by the <see cref="OnCreatePropertyGUI"/> method.</returns>
        public sealed override VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property) =>
            CreatePropertyNodeGUI(property.GetPropertyNode());
#endif
        
        private void OnAttachToPanel(AttachToPanelEvent ev)
        {
            if ((Container?.panel ?? ev.destinationPanel) is IPanel editorPanel && editorPanel.contextType == ContextType.Editor)
                ((IDrawer)this).AddDefaultStyles(editorPanel.visualTree);
        }

        /// <summary>
        /// Override this method in order to perform any custom logic when this property drawer needs to be updated.
        /// </summary>
        public virtual void OnUpdate() { }

        #region IDisposable

        ~TypeDrawer() => Dispose(false);
        
        public virtual void OnReset(bool disposing = false)
        {
            if (disposing)
            {
                if (Container != null)
                {
                    Container.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
#if UNITY_EDITOR
                    UnityEditor.UIElements.BindingExtensions.Unbind(Container);
#endif
                    Container.RemoveFromHierarchy();
                }
                Property = null;
                m_Field = null;
                Container = null;
            }
        }
        
        public void Dispose()
        {
            Dispose(disposing: true);
            // GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
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
