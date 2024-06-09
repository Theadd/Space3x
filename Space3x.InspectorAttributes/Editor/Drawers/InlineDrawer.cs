using Space3x.Attributes.Types;
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
        
        protected override VisualElement OnCreatePropertyGUI(IProperty property)
        {
            var container = new VisualElement();
            if (property.GetSerializedProperty() is SerializedProperty serializedProperty)
            {
                var field = new PropertyField(serializedProperty);
                field.Unbind();
                field.TrackPropertyValue(serializedProperty, CheckInline);
                field.BindProperty(serializedProperty);
                InspectorContainer = new VisualElement();
                container.Add(field);
                container.Add(InspectorContainer);
                OnUpdate();
            }
            
            return container;
        }

        public override void OnUpdate()
        {
            InspectorContainer.Clear();
            // TODO
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
                    Debug.LogWarning($"InlineAttribute can't handle a property of type {property.propertyType}. (Property: {property.name})");
                    break;
            }
        }

        private void CheckInline(SerializedProperty property) => OnUpdate();
    }
}
