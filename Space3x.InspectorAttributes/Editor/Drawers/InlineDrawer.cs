using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InlineAttribute), useForChildren: true)]
    public class InlineDrawer : Drawer<InlineAttribute>
    {
        protected VisualElement InspectorContainer { get; private set; }
        
        public override InlineAttribute Target => (InlineAttribute) attribute;

        private VisualElement m_InnerField;

        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            var container = new VisualElement();

            if (m_InnerField != null)
                DebugLog.Error($"<color=#FF0000FF><b>Duplicate call on OnCreatePropertyGUI of InlineDrawer for \"{property.PropertyPath}\".");

            if (property.HasSerializedProperty())
                m_InnerField = new PropertyField(property.GetSerializedProperty());
            else
                m_InnerField = new BindablePropertyField(property);
            InspectorContainer = new BindableElement();
            InspectorContainer.TrackPropertyValue(property, CheckInline);
            container.Add(m_InnerField);
            container.Add(InspectorContainer);
            OnUpdate();
            
            return container;
        }

        public override void OnUpdate()
        {
            InspectorContainer.Clear();
            if (Property.HasSerializedProperty())
            {
                var property = Property.GetSerializedProperty();
                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        if (property.objectReferenceValue != null)
                        {
                            var inlineInspector = new InspectorElement(property.objectReferenceValue);
                            inlineInspector.SetEnabled(Target.ContentEnabled);
                            InspectorContainer.Add(inlineInspector);
                        }

                        break;
                    case SerializedPropertyType.ManagedReference:
                        if (property.managedReferenceValue != null)
                        {
                            var inlineManagedObject = new PropertyField(property);
                            inlineManagedObject.SetEnabled(Target.ContentEnabled);
                            InspectorContainer.Add(inlineManagedObject);
                        }

                        break;
                    case SerializedPropertyType.ExposedReference:
                        if (property.exposedReferenceValue != null)
                        {
                            var inlineInspector = new InspectorElement(property.exposedReferenceValue);
                            inlineInspector.SetEnabled(Target.ContentEnabled);
                            InspectorContainer.Add(inlineInspector);
                        }

                        break;
                    default:
                        Debug.LogWarning(
                            $"InlineAttribute can't handle a property of type {property.propertyType}. (Property: {property.name})");
                        break;
                }
            }
            else
            {
                if (m_InnerField.dataSource is IBindableDataSource bindableDataSource)
                {
                    var boxedValue = bindableDataSource.BoxedValue;
                    if (boxedValue is UnityEngine.Object unityObject)
                    {
                        var customInspector = new InspectorElement(unityObject);
                        customInspector.SetEnabled(Target.ContentEnabled);
                        InspectorContainer.Add(customInspector);
                    }
                }
            }
        }

        private void CheckInline(IPropertyNode property) => OnUpdate();
    }
}
