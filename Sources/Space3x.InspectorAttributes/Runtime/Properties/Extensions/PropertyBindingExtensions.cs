using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Unity.Properties;
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
#if UNITY_EDITOR                
            if (property.HasSerializedProperty())
            {
                if (property.GetSerializedProperty() is UnityEditor.SerializedProperty serializedProperty)
                    UnityEditor.UIElements.BindingExtensions.BindProperty(field, serializedProperty);
            }
            else
#endif
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
#if UNITY_EDITOR
            if (property.HasSerializedProperty() && property.IsValid())
                UnityEditor.UIElements.BindingExtensions.TrackPropertyValue(element, property.GetSerializedProperty(),
                    callback == null ? null : _ => callback(property));
            else
#endif
            {
                // if (property.IsUnreliable() && property is BindablePropertyNode bindablePropertyNode)
                // {
                //     if (((UnreliableEventHandler)bindablePropertyNode.Controller?.EventHandler) is UnreliableEventHandler handler)
                //         handler.TrackPropertyChanges(bindablePropertyNode, callback);
                // }
                //
                // if (callback != null && property is INonSerializedPropertyNode nonSerializedPropertyNode)
                // {
                //     nonSerializedPropertyNode.ValueChanged -= callback;
                //     nonSerializedPropertyNode.ValueChanged += callback;
                // }

                if (property.IsUnreliable() && property is BindablePropertyNode bindablePropertyNode &&
                    ((UnreliableEventHandler)bindablePropertyNode.Controller?.EventHandler) is UnreliableEventHandler
                    handler)
                    handler.TrackPropertyChanges(bindablePropertyNode, callback);
                else if (callback != null && property is INonSerializedPropertyNode nonSerializedPropertyNode)
                {
                    nonSerializedPropertyNode.ValueChanged -= callback;
                    nonSerializedPropertyNode.ValueChanged += callback;
                }
            }
        }

        /// <summary>
        /// Executes the given <paramref name="callback"/> when any value changes in the declaring object related to
        /// the given <see cref="IPropertyNode"/>. It's a drop-in replacement for
        /// <see cref="UnityEditor.UIElements.BindingExtensions.TrackSerializedObjectValue"/> which works on both
        /// serialized and non-serialized properties.
        /// </summary>
        /// <remarks>
        /// Except for Runtime UI, which tracks changes in the property's direct parent object instead and notifies
        /// changes on any property on that parent object and on any children properties of those properties, at any level.
        /// This behaviour is different from the other <see cref="TrackSerializedObjectValue(UnityEngine.UIElements.VisualElement,Space3x.Properties.Types.IPropertyNode,Action{IPropertyNode})">TrackSerializedObjectValue</see>
        /// overloaded method, where the callback expects an <see cref="IPropertyNode"/> as argument, which is only
        /// executed on value changes for those <see cref="IPropertyNode"/> that are direct children of the declaring object.
        ///
        /// For example, a change in the Y property of a Vector3 field using this overload in Runtime UI, will first
        /// execute the callback as a result of a value changed in the Y property of a Vector3 declaring object, and
        /// then it will execute the callback as a result of a value change in the Vector3 property itself. In this case,
        /// since Vector3 is a ValueType (struct), when retrieving the value of the Vector3 property in the first
        /// callback it will still contain the previous value, as it is expected in struct types.
        /// </remarks>
        /// <seealso cref="UnityEditor.UIElements.BindingExtensions.TrackSerializedObjectValue"/>
        /// <seealso cref="TrackPropertyValue"/>
        /// <param name="element">
        /// The <see cref="VisualElement"/> to bind the tracking to. Only those callbacks registered with elements that
        /// are attached to a <see cref="IPanel">panel</see> are executed.
        /// </param>
        /// <param name="property">
        /// Any <see cref="IPropertyNode"/> within the declaring object to track changes on.
        /// </param>
        /// <param name="callback">
        /// A callback to be executed when a value changed.
        /// </param>
        public static void TrackSerializedObjectValue(this VisualElement element, IPropertyNode property, Action callback = null)
        {
            try
            {
                if (property.GetController() is not PropertyAttributeController controller)
                    return;

                var useCustomHandler = (controller.IsRuntimeUI && Application.isPlaying) 
                                        || (!controller.IsSerialized && controller.EventHandler != null && (controller.IsRuntimeUI || Application.isPlaying));
                if (useCustomHandler)
                {
                    if (controller.EventHandler is not UnreliableEventHandler handler)
                        Debug.LogException(new NotImplementedException(
                            $"<color=#FF007FFF>{nameof(TrackSerializedObjectValue)}() for a runtime controller where its EventHandler is not an UnreliableEventHandler is <u>not implemented</b>.</color>"));
                    else if (callback != null)
                    {
                        // TODO: Unsubscribe
                        ((BindablePropertyNode)handler.SourcePropertyNode).ValueChangedOnChildNode += changedProperty =>
                        {
                            if (element.panel != null)  // && controller == changedProperty.GetController())
                                callback.Invoke();
                        };
                    }

                    return;
                }
#if UNITY_EDITOR
                if (property.GetSerializedObject() != null)
                    UnityEditor.UIElements.BindingExtensions.TrackSerializedObjectValue(element, property.GetSerializedObject(), callback == null ? null : _ =>
                    {
                        if (element.panel != null)
                            callback.Invoke();
                    });
#endif
            }
            catch (Exception e)
            {
                DebugLog.Warning(e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
            }
        }
        
        /// <summary>
        /// Executes the given <paramref name="callback"/> when any value changes in the declaring object related to
        /// the given <see cref="IPropertyNode"/>. It's a drop-in replacement for
        /// <see cref="UnityEditor.UIElements.BindingExtensions.TrackSerializedObjectValue"/> which works on both
        /// serialized and non-serialized properties.
        /// </summary>
        /// <seealso cref="UnityEditor.UIElements.BindingExtensions.TrackSerializedObjectValue"/>
        /// <seealso cref="TrackPropertyValue"/>
        /// <param name="element">
        /// The <see cref="VisualElement"/> to bind the tracking to. Only those callbacks registered with elements that
        /// are attached to a <see cref="IPanel">panel</see> are executed.
        /// </param>
        /// <param name="property">
        /// Any <see cref="IPropertyNode"/> within the declaring object to track changes on.
        /// </param>
        /// <param name="callback">
        /// A callback to be executed when a value changed. With the changed <see cref="IPropertyNode"/> as parameter
        /// except for properties in a serialized object context, where it is always null.
        /// </param>
        public static void TrackSerializedObjectValue(this VisualElement element, IPropertyNode property, Action<IPropertyNode> callback = null)
        {
            try
            {
                if (property.GetController() is not PropertyAttributeController controller)
                    return;

                var useCustomHandler = (controller.IsRuntimeUI && Application.isPlaying) 
                                       || (!controller.IsSerialized && controller.EventHandler != null && (controller.IsRuntimeUI || Application.isPlaying));
                if (useCustomHandler)
                {
                    if (controller.EventHandler is not UnreliableEventHandler handler)
                        Debug.LogException(new NotImplementedException(
                            $"<color=#FF007FFF>{nameof(TrackSerializedObjectValue)}() for a runtime controller where its EventHandler is not an UnreliableEventHandler is <u>not implemented</b>.</color>"));
                    else if (callback != null)
                    {
                        ((BindablePropertyNode)handler.SourcePropertyNode).ValueChangedOnChildNode += changedProperty =>
                        {
                            if (element.panel != null && controller == changedProperty.GetController())
                                callback.Invoke(changedProperty);
                        };
                    }

                    return;
                }
#if UNITY_EDITOR
                if (property.GetSerializedObject() != null)
                    UnityEditor.UIElements.BindingExtensions.TrackSerializedObjectValue(element, property.GetSerializedObject(), callback == null ? null : _ =>
                    {
                        if (element.panel != null)
                            callback.Invoke(null);
                    });
#endif
            }
            catch (Exception e)
            {
                DebugLog.Warning(e.ToString() + Environment.NewLine + Environment.NewLine + e.StackTrace);
            }
        }
    }
}
