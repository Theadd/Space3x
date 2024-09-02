using System;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public static class PropertyBindingExtensions
    {
        /// <summary>
        /// Binds the given <see cref="IPropertyNode"/> to a field and synchronizes their values, using the "value"
        /// property on this field as the binding target.
        /// </summary>
        /// <remarks>
        /// It's an implicit drop-in replacement for <see cref="BindingExtensions.BindProperty(IBindable,SerializedProperty)"/>.
        /// </remarks>
        /// <seealso cref="BindingExtensions.BindProperty(IBindable,SerializedProperty)"/>
        /// <seealso cref="BindProperty(IBindable,IPropertyNode,BindingId)"/>
        /// <param name="field">The (<see cref="IBindable"/>) <see cref="VisualElement"/> field editing a property.</param>
        /// <param name="property">The <see cref="IPropertyNode"/> to bind.</param>
        public static void BindProperty(this IBindable field, IPropertyNode property) =>
            BindProperty(field, property, "value");

        /// <summary>
        /// Binds the given <see cref="IPropertyNode"/> to a field and synchronizes their values using the specified
        /// <paramref name="bindingId"/> as binding target on this <see cref="VisualElement"/>.
        /// </summary>
        /// <seealso cref="BindingExtensions.BindProperty(IBindable,SerializedProperty)"/>
        /// <seealso cref="BindProperty(IBindable,IPropertyNode)"/>
        /// <param name="field">The (<see cref="IBindable"/>) <see cref="VisualElement"/> field editing a property.</param>
        /// <param name="property">The <see cref="IPropertyNode"/> to bind.</param>
        /// <param name="bindingId">Target property on <paramref name="field"/> for the binding.</param>
        public static void BindProperty(this IBindable field, IPropertyNode property, BindingId bindingId)
        {
            if (property.HasSerializedProperty())
            {
                if (property.GetSerializedProperty() is SerializedProperty serializedProperty)
                    BindingExtensions.BindProperty(field, serializedProperty);
            }
            else
            {
                if (field is VisualElement element)
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(property.GetUnderlyingType()))
                    {
                        element.dataSource = new DataSourceObjectBinding(property);
                        element.SetBinding(bindingId, new DataBinding
                        {
                            dataSourcePath = new PropertyPath(nameof(DataSourceObjectBinding.Value)),
                            bindingMode = BindingMode.TwoWay
                        });
                    }
                    else
                    {
                        element.dataSource = new DataSourceBinding(property);
                        element.SetBinding(bindingId, new DataBinding
                        {
                            dataSourcePath = new PropertyPath(nameof(DataSourceBinding.Value)),
                            bindingMode = BindingMode.TwoWay
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Synchronizes the value of the given <see cref="IPropertyNode"/> on this <see cref="VisualElement"/>.
        /// </summary>
        /// <seealso cref="BindingExtensions.TrackPropertyValue"/>
        /// <seealso cref="TrackSerializedObjectValue"/>
        public static void TrackPropertyValue(this VisualElement element, IPropertyNode property, Action<IPropertyNode> callback = null)
        {
            if (property.HasSerializedProperty())
                element.TrackPropertyValue(property.GetSerializedProperty(), callback == null ? null : _ => callback(property));
            else
            {
                if (property.IsUnreliable() && property is BindablePropertyNode bindablePropertyNode)
                {
                    if ((UnreliableEventHandler)bindablePropertyNode.Controller?.EventHandler is UnreliableEventHandler handler)
                    {
                        handler.TrackPropertyChanges(bindablePropertyNode, callback);
                        Debug.LogWarning($"[PAC] TRACKING! UnreliableEventHandler: {property.PropertyPath}");
                    }
                }
                
                if (callback != null && property is INonSerializedPropertyNode nonSerializedPropertyNode)
                {
                    nonSerializedPropertyNode.ValueChanged -= callback;
                    nonSerializedPropertyNode.ValueChanged += callback;
                }
            }
        }

        /// <summary>
        /// Executes the given <paramref name="callback"/> when any value changes in the serialized object related to the given
        /// <see cref="IPropertyNode"/>.
        /// Except for Runtime UI, which tracks changes in the property's direct parent object instead and notifies
        /// changes on any property on that parent object and on any children properties of those properties, at any level.
        /// </summary>
        /// <seealso cref="BindingExtensions.TrackSerializedObjectValue"/>
        /// <seealso cref="TrackPropertyValue"/>
        public static void TrackSerializedObjectValue(this VisualElement element, IPropertyNode property, Action callback = null)
        {
            try
            {
                if (property.GetController().IsRuntimeUI)
                {
                    if (property.GetController().EventHandler is not UnreliableEventHandler handler)
                        throw new NotImplementedException();
                    if (callback != null)
                        ((BindablePropertyNode)handler.SourcePropertyNode).ValueChangedOnChildNode += _ => callback.Invoke();
                    return;
                }
#if UNITY_EDITOR
                element.TrackSerializedObjectValue(property.GetSerializedObject(), callback == null ? null : _ => callback.Invoke());
#endif
            }
            catch (Exception e)
            {
                DebugLog.Warning(e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
