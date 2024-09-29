using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    /// <summary>
    /// Like <see cref="UnityEditor.UIElements.PropertyField"/> but for non-serialized properties (<see cref="IPropertyNode"/>).
    /// </summary>
    [UxmlElement]
    [HideInInspector]
    public partial class BindablePropertyField : VisualElement, IBindable
    {
        public static readonly string ussClassName = "ui3x-bindable-property-field";
        public static readonly string decoratorDrawersContainerClassName = "unity-decorator-drawers-container";
        public static readonly string listViewNamePrefix = "unity-list-";
        public VisualElement Field;
        public IPropertyNode Property;
        public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer ??= CreateDecoratorDrawersContainer();
        private VisualElement m_DecoratorDrawersContainer;
        private PropertyAttributeController m_Controller;
        private FieldFactoryBuilder m_FieldFactoryBuilder;
        private bool m_HasCustomDrawerOnCollectionItems;
        private object m_ExpectedParent;
        
        // IBindable interface
        public IBinding binding { get; set; }
        public string bindingPath { get; set; }

        public BindablePropertyField()
        {
            // throw new InvalidOperationException();
        }

        protected BindablePropertyField(object expectedParent)
        {
            m_ExpectedParent = expectedParent;
            if (DecoratorDrawersContainer != null)
                this.WithClasses(ussClassName, "unity-property-field");
        }

        // /// <summary>
        // /// Creates a new <see cref="BindablePropertyField"/> bound to the given <paramref name="property"/>.
        // /// </summary>
        // /// <param name="property">A non-serialized <see cref="IPropertyNode"/>.</param>
        // /// <param name="applyCustomDrawers">Whether to create all property and decorator drawers annotated on the property.</param>
        // protected BindablePropertyField(IPropertyNode property, bool applyCustomDrawers = false) : this() => BindProperty(property, applyCustomDrawers);

        public static BindablePropertyField Create(object expectedParent) =>
            new(expectedParent);

        public static BindablePropertyField Create(object expectedParent, IPropertyNode property, bool applyCustomDrawers = false)
        {
            var element = new BindablePropertyField(expectedParent);
            element.BindProperty(property, applyCustomDrawers);
            return element;
        }

        internal VisualElement GetParentOrExpectedParent()
        {
            if (this.parent != null)
                return this.parent;

            if (m_ExpectedParent is VisualElement parentElement)
                return parentElement;
            
            Debug.LogException(new NotImplementedException($"{nameof(GetParentOrExpectedParent)} not implemented for {m_ExpectedParent?.GetType().Name}."));
            return null;
        }
        
        private VisualElement CreateDecoratorDrawersContainer()
        {
            var element = new VisualElement();
            element.WithClasses(decoratorDrawersContainerClassName);
            this.Insert(0, element);
            return element;
        }

        /// <summary>
        /// Instantiates and attaches all decorator drawers annotated on the property.
        /// </summary>
        public void AttachDecoratorDrawers()
        {
            DecoratorDrawersContainer.Clear();
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null)
            {
                if (Property.IsArrayOrListElement())
                {
                    var collectionProperty = Property.GetParentProperty();
                    vType = ((PropertyAttributeController)collectionProperty.GetController()).AnnotatedType.GetValue(collectionProperty.Name);
                }
                else
                {
                    DebugLog.Error(new ArgumentException("Unexpected value.").ToString());
                    return;
                }
            }
            
            for (var i = 0; i < vType.DecoratorDrawers.Count; i++)
            {
                var decorator = vType.DecoratorDrawers[i];
                if (decorator == null) continue;
#if UNITY_EDITOR && RUNTIME_UITOOLKIT_DRAWERS
                if (m_Controller.IsRuntimeUI)
                {
                    decorator = CachedDrawers.GetRuntimeDrawerOverride(decorator);
                    if (decorator == null) continue;
                }
                else
                {
                    if (typeof(DecoratorDrawerAdapter).IsAssignableFrom(decorator) &&
                        !decorator.IsDefined(typeof(UnityEditor.CustomPropertyDrawer), false)) 
                        continue;
                }
#endif
                try
                {
                    if (Property.IsArrayOrListElement() && (vType.PropertyAttributes[i].applyToCollection || vType.PropertyAttributes[i] is HeaderAttribute)) continue;
                    if (Property.IsArrayOrList() && vType.PropertyAttributes[i] is not HeaderAttribute && !vType.PropertyAttributes[i].applyToCollection) continue;
#if UNITY_EDITOR
                    var drawer = (UnityEditor.DecoratorDrawer)
#else
                    var drawer = (DecoratorDrawerAdapter)
#endif
                        DrawerUtility.CreateDecoratorDrawer(decorator, vType.PropertyAttributes[i]);
                    if (CreateDecoratorGUI(drawer) is VisualElement decoratorElement)
                        DecoratorDrawersContainer.Add(decoratorElement);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal static BindablePropertyField s_ExpectedDecoratorParent;

#if UNITY_EDITOR
        private VisualElement CreateDecoratorGUI(UnityEditor.DecoratorDrawer drawer)
#else
        private VisualElement CreateDecoratorGUI(DecoratorDrawerAdapter drawer)
#endif
        {
            var prevDecoratorParent = s_ExpectedDecoratorParent;
            s_ExpectedDecoratorParent = this;
            var element = drawer.CreatePropertyGUI();
            s_ExpectedDecoratorParent = prevDecoratorParent;
            return element;
        }
        
        /// <summary>
        /// Removes the <see cref="BindablePropertyField"/> from the VisualElement's hierarchy, properly removing
        /// all its decorator drawers.
        /// </summary>
        public void ProperlyRemoveFromHierarchy()
        {
            for (var i = DecoratorDrawersContainer.hierarchy.childCount - 1; i >= 0; i--)
            {
                if (DecoratorDrawersContainer.hierarchy[i] is GhostDecorator ghostDecorator)
                {
                    ghostDecorator.TargetDecorator.ProperlyRemoveFromHierarchy();
                }
            }
            DecoratorDrawersContainer.Clear();
            try
            {
                RemoveFromHierarchy();
            }
            catch (Exception e)
            {
                DebugLog.Error(e.ToString());
            }
        }
    }
}
