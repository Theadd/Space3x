using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public partial class BindablePropertyField
    {
        private long m_ValueHash = 0;
        
        private VisualElement ConfigureChildrenFields(Type propertyType, bool isNullValue = false)
        {
            var field = TreeRendererUtility.Create(Property);
            // TODO: var renderer = field.GetTreeRenderer(); => SET TreeNode
            Field?.RemoveFromHierarchy();
            Field = field;
            Add(field);
            // TODO: Field.tooltip = vType?.Tooltip ?? "";
            
            if (!isNullValue)
            {
                var controller = PropertyAttributeController.GetOrCreateInstance(Property, propertyType, true);
                m_FieldFactoryBuilder = new FieldFactoryBuilder(controller);
                m_FieldFactoryBuilder.Rebuild(field.contentContainer);
            }
            ((ITreeRenderer)field).Render(true);
            m_ValueHash = Property.GetUnderlyingValue()?.GetHashCode() ?? 0;
            this.TrackPropertyValue(Property, OnTrackedPropertyValueChanged);
            return field;
        }

        private void OnTrackedPropertyValueChanged(IPropertyNode trackedNode)
        {
            // DebugLog.Error($"[PAC!] OnTrackedPropertyValueChanged: ({Property.PropertyPath} == {trackedNode.PropertyPath})?");
            if (!Equals(Property, trackedNode))
            {
                DebugLog.Warning($"[PAC!] <u>[ConfigureChildrenFields] <b>NOT EQUALS!</b> OnTrackedPropertyValueChanged</u>: {Property.PropertyPath} != {trackedNode.PropertyPath}");
                // return;
            }
            var node = Property;
            object nodeValue = null;
            try
            {
                // nodeValue = node.GetUnderlyingValue();
                nodeValue = node.GetValue();
            }
            catch (ArgumentOutOfRangeException)
            {
                m_FieldFactoryBuilder?.Clear();
                return;
            }
            catch (IndexOutOfRangeException)
            {
                // IndexOutOfRangeException thrown when removing an element from the NonSerializedRecipe MonoBehaviour.
                m_FieldFactoryBuilder?.Clear();
                return;
            }
            if ((nodeValue?.GetHashCode() ?? 0) == m_ValueHash)
                return;
            m_ValueHash = nodeValue?.GetHashCode() ?? 0;
            m_FieldFactoryBuilder?.Clear();
            if (nodeValue == null) return;
            var controller = PropertyAttributeController.GetOrCreateInstance(node, nodeValue.GetType());
            m_FieldFactoryBuilder = new FieldFactoryBuilder(controller)
            {
                EnableReadOnly = m_FieldFactoryBuilder?.EnableReadOnly ?? false
            };
           m_FieldFactoryBuilder.Rebuild(Field.contentContainer);
        }
    }
}
