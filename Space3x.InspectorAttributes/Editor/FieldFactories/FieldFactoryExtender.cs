using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Extensions;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class FieldFactoryExtender : FieldFactoryBase
    {
        public VisualElement PropertyFieldOrigin { get; set; } = null;

        private VisualElement m_PreviousField { get; set; } = null;

        public FieldFactoryExtender(PropertyAttributeController controller) => Controller = controller;

        public FieldFactoryExtender Rebuild(VisualElement container = null)
        {
            if (PropertyFieldOrigin == null) 
                throw new ArgumentNullException(nameof(PropertyFieldOrigin));
            container ??= Container;
            if (container == null) return this;
            RemoveAllBindableFields();
            Container = container;
            var allFields = GetExistingFields(PropertyFieldOrigin);
            // Workaround to rewrite PropertyFields as non-serialized ones (BindablePropertyFields) in editor inspector
            // views with content already present on runtime UI (Game View).
            if (Application.isPlaying && container.panel is not IRuntimePanel && Controller.IsRuntimeUI)
                return Rewrite(container, allFields);
            m_PreviousField = PropertyFieldOrigin;
            
            foreach (var propertyNode in Controller.Properties.Values)
            {
                if (string.IsNullOrEmpty(propertyNode.Name)
                    || (!propertyNode.IncludeInInspector() && !propertyNode.ShowInInspector()))
                    continue;

                // if (Application.isPlaying && container.panel is not IRuntimePanel && Controller.IsRuntimeUI)
                // {
                //     if (allFields.TryGetValue(propertyNode.Name, out var existingField))
                //     {
                //         existingField.LogThis("ALREADY EXISTING!");
                //         ((IBindable)existingField).BindProperty(propertyNode);
                //         continue;
                //     }
                // }

                switch (propertyNode)
                {
                    case ISerializedPropertyNode:
                    {
                        if (!allFields.TryGetValue(propertyNode.Name, out var existingField))
                        {
                            DebugLog.Error($"No existing PropertyField found for {propertyNode.Name}.");
                            continue;
                        }

                        if (propertyNode.HasChildren() && !propertyNode.IsArrayOrList())
                            // EDIT: ORIGINAL = existingField.TrackPropertyValue(propertyNode, PropertyAttributeController.OnPropertyValueChanged);
                        {
                            try
                            {
                                existingField.TrackPropertyValue(propertyNode,
                                    PropertyAttributeController.OnPropertyValueChanged);
                            }
                            catch (NotSupportedException ex)
                            {
                                Debug.LogError(ex.Message + $" @ <color=#00FF00FF>{propertyNode}</color>\n{ex.StackTrace}");
                            }
                        }
                        m_PreviousField = existingField;
                        break;
                    }
                    case INonSerializedPropertyNode:
                    {
                        var bindableField = AddField(propertyNode);
                        if (propertyNode.HasChildren() && !propertyNode.IsArrayOrList())
                            bindableField.TrackPropertyValue(propertyNode, PropertyAttributeController.OnPropertyValueChanged);
                        break;
                    }
                    case IInvokablePropertyNode:
                        AddField(propertyNode);
                        break;
                }
            }
            
            return this;
        }
        
        private FieldFactoryExtender Rewrite(VisualElement container, Dictionary<string, VisualElement> existingFields)
        {
            // var allFields = GetExistingFields(PropertyFieldOrigin);
            // var otherFields = existingFields.Where((key, field) => field != PropertyFieldOrigin).ToList();
            var originPair = existingFields
                .Where(entry => entry.Value == PropertyFieldOrigin)
                .ToList();
            var originFieldKey = originPair.Any()
                ? originPair.FirstOrDefault().Key 
                : ((PropertyField)PropertyFieldOrigin).bindingPath;
            existingFields.Where(entry => entry.Value != PropertyFieldOrigin).ForEach(pair =>
            {
                if (pair.Value is PropertyField other) other.ProperlyRemoveFromHierarchy();
            });
            m_PreviousField = PropertyFieldOrigin;
            
            foreach (var propertyNode in Controller.Properties.Values)
            {
                if (string.IsNullOrEmpty(propertyNode.Name)
                    || (!propertyNode.IncludeInInspector() && !propertyNode.ShowInInspector())
                    || (originFieldKey == propertyNode.Name))
                    continue;

                // if (Application.isPlaying && container.panel is not IRuntimePanel && Controller.IsRuntimeUI)
                // {
                //     if (allFields.TryGetValue(propertyNode.Name, out var existingField))
                //     {
                //         existingField.LogThis("ALREADY EXISTING!");
                //         ((IBindable)existingField).BindProperty(propertyNode);
                //         continue;
                //     }
                // }

                switch (propertyNode)
                {
                    case IInvokablePropertyNode:
                        AddField(propertyNode);
                        break;
                    default:
                    {
                        var bindableField = AddField(propertyNode);
                        if (propertyNode.HasChildren() && !propertyNode.IsArrayOrList())
                            bindableField.TrackPropertyValue(propertyNode, PropertyAttributeController.OnPropertyValueChanged);
                        break;
                    }
                }
            }
            
            return this;
        }

        private BindablePropertyField AddField(IPropertyNode propertyNode)
        {
            var bindableField = new BindablePropertyField();
            bindableField.WithClasses(propertyNode.ShowInInspector(), UssConstants.UssShowInInspector);
            if (propertyNode is not InvokablePropertyNodeBase && !IsReadOnlyEnabled && propertyNode.IsReadOnly())
                bindableField.SetEnabled(false);
            bindableField.BindProperty(propertyNode, applyCustomDrawers: true);
            bindableField.TrackPropertyValue(propertyNode, changedProperty =>
            {
                try
                {
                    BindableUtility.SetValueWithoutNotify(bindableField.Field, changedProperty.GetValue());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
            m_PreviousField.AddAfter(bindableField);
            bindableField.AttachDecoratorDrawers();
            m_PreviousField = bindableField;
            BindableFields.Add(bindableField);
            return bindableField;
        }

        private Dictionary<string, VisualElement> GetExistingFields(VisualElement propertyFieldOrigin)
        {
            var allFields = new Dictionary<string, VisualElement>();
#if UNITY_EDITOR
            foreach (var element in Container.hierarchy.Children().ToList())
            {
                if (element is not UnityEditor.UIElements.PropertyField propertyField) continue;

                if (!TryGetBoundProperty(propertyField, out var serializedProperty))
                {
                    // Avoid rebinding the property field where this call is coming from to avoid an infinite loop
                    if (propertyField != propertyFieldOrigin)
                    {
                        UnityEditor.UIElements.BindingExtensions.Unbind(propertyField);
                        UnityEditor.UIElements.BindingExtensions.Bind(propertyField, Controller.SerializedObject as UnityEditor.SerializedObject);
                    }
                    serializedProperty = propertyField.GetSerializedProperty();
                }
                if (serializedProperty == null) continue;
                allFields[serializedProperty.name] = propertyField;
            }
#endif

            return allFields;
        }
        
#if UNITY_EDITOR
        // Tries to get the bound SerializedProperty from a correctly rendered PropertyField.
        private static bool TryGetBoundProperty(UnityEditor.UIElements.PropertyField propertyField, out UnityEditor.SerializedProperty serializedProperty)
        {
            serializedProperty = propertyField.GetSerializedProperty();
            if (serializedProperty == null) return false;

            try
            {
                if (!(propertyField.bindingPath.EndsWith(serializedProperty.name) ||
                      propertyField.name.EndsWith(serializedProperty.name)))
                    return false;
            } catch { return false; }

            return propertyField.hierarchy.childCount > 0 && !HasInvalidDecorators(propertyField);
        }
#endif
        
        private static bool HasInvalidDecorators(VisualElement propertyField)
        {
            if (propertyField.hierarchy[0] is VisualElement decoratorsContainer 
                && decoratorsContainer.ClassListContains("unity-decorator-drawers-container"))
                for (var i = decoratorsContainer.hierarchy.childCount - 1; i >= 0; i--)
                    if (decoratorsContainer.hierarchy[i] is GhostDecorator ghostDecorator 
                        && !ghostDecorator.TargetDecorator.HasValidContainer())
                        return true;

            return false;
        }
    }
}
