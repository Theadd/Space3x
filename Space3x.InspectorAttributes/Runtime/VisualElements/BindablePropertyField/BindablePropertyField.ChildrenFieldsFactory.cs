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
            DebugLog.Info($"[USK3] [BindablePropertyField] ConfigureChildrenFields FOR propertyType {propertyType.Name}: {Property.PropertyPath}");
            var field = new Foldout()
            {
                text = Property.DisplayName(),
                value = true
            };
            if (!isNullValue)
            {
                // TODO: remove block
                if (Property.PropertyPath == "UIView.primary")
                {
                    Debug.Log("STOP");
                    var declaringObjectOnController = m_Controller.DeclaringObject;
                    var declaringObject = Property.GetDeclaringObject();
                    var aux = Property.GetValue();
                    var auxOrThrow =
                        PropertyNodeExtensions.GetFieldValueOrThrow(Property.GetDeclaringObject(), Property.Name);
                    var succeeded = TryGetAssignedPropertyValueAndType(out var auxAssigned, out var auxAssignedType);
                    Debug.Log("STOP");

                }
                var controller = PropertyAttributeController.GetOrCreateInstance(Property, propertyType, true);
                m_FieldFactoryBuilder = new FieldFactoryBuilder(controller);
                m_FieldFactoryBuilder.Rebuild(field.contentContainer);
            }
            m_ValueHash = Property.GetUnderlyingValue()?.GetHashCode() ?? 0;
            this.TrackPropertyValue(Property, OnTrackedPropertyValueChanged);

            return field;
        }

        private void OnTrackedPropertyValueChanged(IPropertyNode trackedNode)
        {
            // DebugLog.Error($"[PAC!] OnTrackedPropertyValueChanged: ({Property.PropertyPath} == {trackedNode.PropertyPath})?");
            if (!Equals(Property, trackedNode))
            {
                Debug.Log($"[PAC!] <u>[ConfigureChildrenFields] <b>NOT EQUALS!</b> OnTrackedPropertyValueChanged</u>: {Property.PropertyPath} != {trackedNode.PropertyPath}");
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
            m_FieldFactoryBuilder.Rebuild(((Foldout)Field).contentContainer);
        }
    }
}