using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public partial class BindablePropertyField
    {
        
        /// <summary>
        /// Binds the given <paramref name="property"/> to this <see cref="BindablePropertyField"/>,
        /// optionally creating all decorator drawers annotated on the property.
        /// </summary>
        /// <param name="property">A non-serialized <see cref="IPropertyNode"/>.</param>
        /// <param name="applyCustomDrawers">Whether to create all property and decorator drawers annotated on the property.</param>
        public void BindProperty(IPropertyNode property, bool applyCustomDrawers = false)
        {
            // if (!(property is INonSerializedPropertyNode nonSerializedPropertyNode))
            //     throw new ArgumentException($"Invalid IPropertyNode, it must be a non serialized property in order to bind it to a {nameof(BindablePropertyField)}, for serialized properties, just use {nameof(PropertyField)} instead.");
            // Property = nonSerializedPropertyNode;
            Property = property;
            bindingPath = Property.PropertyPath;
            // m_Controller = PropertyAttributeController.GetInstance(Property);
            m_Controller = ((PropertyAttributeController)Property.GetController());
            if (m_Controller == null)
                throw new ArgumentException("Unexpected, value for PropertyAttributeController is null.");
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null && applyCustomDrawers)
            {
                if (property.IsArrayOrListElement())
                {
                    var collectionProperty = property.GetParentProperty();
                    vType = ((PropertyAttributeController)collectionProperty.GetController()).AnnotatedType.GetValue(collectionProperty.Name);
                }
                else
                {
                    DebugLog.Error(new ArgumentException("Unexpected value.").ToString());
                    Field?.RemoveFromHierarchy();
                    Field = new Label("Unexpected value.");
                    Add(Field);
                    return;
                }
            }
#if UNITY_EDITOR
            PropertyDrawer drawer = null;
#else
            PropertyDrawerAdapter drawer = null;
#endif
            m_HasCustomDrawerOnCollectionItems = vType != null 
                                                 && property.IsArrayOrList() 
                                                 && vType.PropertyDrawerOnCollectionItems != null
                                                 && !typeof(IDrawer).IsAssignableFrom(vType.PropertyDrawerOnCollectionItems);
            if (applyCustomDrawers)
            {
                var propertyDrawer = property.IsArrayOrListElement() ? vType.PropertyDrawerOnCollectionItems : vType.PropertyDrawer;
                if (propertyDrawer != null)
                {
                    if (typeof(ICreatePropertyNodeGUI).IsAssignableFrom(propertyDrawer))
                    {
                        try
                        {
#if UNITY_EDITOR
                            drawer = (PropertyDrawer)
#else
                            drawer = (PropertyDrawerAdapter)
#endif
                                DrawerUtility.CreatePropertyDrawer(
                                    propertyDrawer,
                                    property.IsArrayOrListElement() ? vType.PropertyDrawerOnCollectionItemsAttribute : vType.PropertyDrawerAttribute, 
                                    vType.RuntimeField, 
                                    Property.DisplayName());
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            drawer = null;
                        }
                    }
                }
            }

            VisualElement field = null;
            if (drawer == null)
            {
                field = Property.IsInvokable() ? BindToInvokablePropertyNode() : BindToBuiltInField();
            }
            else
            {
                if (drawer is ICreatePropertyNodeGUI customDrawer)
                {
                    field = customDrawer.CreatePropertyNodeGUI(Property);
                    // Debug.LogError("<b>BindableUtility.AutoNotifyValueChangedOnNonSerialized(field, Property);</b>");
                    // TODO: remove .BindProperty() call, drawers are expected to call .BindProperty() on their own.
                    if (field is IBindable bindable)
                        bindable.BindProperty(Property);
                    this.TrackPropertyValue(Property, changedProperty =>
                    {
                        if (!Equals(Property, changedProperty))
                            DebugLog.Error($"<color=#7F00FFFF>customDrawer.CreatePropertyNodeGUI</color> -> TrackPropertyValue <b>NOT EQUALS!</b> '{Property.PropertyPath}' != '{changedProperty.PropertyPath}'");
                        else
                        {
                            DebugLog.Error($"<color=#FF007FFF>customDrawer.CreatePropertyNodeGUI</color> -> TrackPropertyValue -> SetValueWithoutNotify <color=#00FF00FF><b>EQUALS!</b></color> -> SetValueWithoutNotify: '{Property.PropertyPath}' == '{changedProperty.PropertyPath}'");
                            // BindableUtility.SetValueWithoutNotify(field, changedProperty.GetValue());
                        }
                    });
                    // END EDIT
                    // TODO: Uncomment
                    // BindableUtility.AutoNotifyValueChangedOnNonSerialized(field, Property);
                }
                else
                {
                    // TODO: For IMGUI drawers, wrap them in a custom drawer with an IMGUIContainer element.
                    DebugLog.Error("// TODO: For IMGUI drawers, wrap them in a custom drawer with an IMGUIContainer element.");
                }
            }
            Field?.RemoveFromHierarchy();
            Field = field;
            if (Field != null)
            {
                // TODO: Remove next line
                // Field.WithClasses(false, UssConstants.UssAligned);
                Field.AddToClassList("BINDABLEPROPERTYFIELD_FIELD");
                Add(Field);
                Field.tooltip = vType?.Tooltip ?? "";
            }
        }
    }
}