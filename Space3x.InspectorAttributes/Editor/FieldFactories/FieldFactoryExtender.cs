using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
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
            m_PreviousField = PropertyFieldOrigin;
            
            foreach (var propertyNode in Controller.Properties.Values)
            {
                if (string.IsNullOrEmpty(propertyNode.Name)
                    || (!propertyNode.IncludeInInspector() && !propertyNode.ShowInInspector()))
                    continue;

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
                            existingField.TrackPropertyValue(propertyNode, PropertyAttributeController.OnPropertyValueChanged);
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

        private BindablePropertyField AddField(IPropertyNode propertyNode)
        {
            var bindableField = new BindablePropertyField();
            bindableField.WithClasses(propertyNode.ShowInInspector(), UssConstants.UssShowInInspector);
            if (propertyNode is not InvokablePropertyNodeBase && !IsReadOnlyEnabled && propertyNode.IsReadOnly())
                bindableField.SetEnabled(false);
            bindableField.BindProperty(propertyNode, applyCustomDrawers: true);
            m_PreviousField.AddAfter(bindableField);
            bindableField.AttachDecoratorDrawers();
            m_PreviousField = bindableField;
            BindableFields.Add(bindableField);
            return bindableField;
        }

        private Dictionary<string, VisualElement> GetExistingFields(VisualElement propertyFieldOrigin)
        {
            var allFields = new Dictionary<string, VisualElement>();
            foreach (var element in Container.hierarchy.Children().ToList())
            {
                if (element is not PropertyField propertyField) continue;

                if (!TryGetBoundProperty(propertyField, out var serializedProperty))
                {
                    // Avoid rebinding the property field where this call is coming from to avoid an infinite loop
                    if (propertyField != propertyFieldOrigin)
                    {
#if UNITY_EDITOR
                        UnityEditor.UIElements.BindingExtensions.Unbind(propertyField);
                        UnityEditor.UIElements.BindingExtensions.Bind(propertyField, Controller.SerializedObject as UnityEditor.SerializedObject);
#endif
                    }
                    serializedProperty = propertyField.GetSerializedProperty();
                }
                if (serializedProperty == null) continue;
                allFields[serializedProperty.name] = propertyField;
            }

            return allFields;
        }
        
        // Tries to get the bound SerializedProperty from a correctly rendered PropertyField.
        private static bool TryGetBoundProperty(PropertyField propertyField, out SerializedProperty serializedProperty)
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
