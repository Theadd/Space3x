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
        
        protected override VisualElement OnCreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var field = new PropertyField(property);
            field.Unbind();
            field.TrackPropertyValue(property, CheckInline);
            field.BindProperty(property);
            InspectorContainer = new VisualElement();
            container.Add(field);
            container.Add(InspectorContainer);
            OnUpdate();
            
            return container;
        }

        public override void OnUpdate()
        {
            InspectorContainer.Clear();
            switch (Property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    if (Property.objectReferenceValue != null)
                    {
                        var inlineInspector = new InspectorElement(Property.objectReferenceValue);
                        inlineInspector.SetEnabled(Target.ContentEnabled);
                        InspectorContainer.Add(inlineInspector);
                    }

                    break;
                case SerializedPropertyType.ManagedReference:
                    if (Property.managedReferenceValue != null)
                    {
                        var inlineManagedObject = new PropertyField(Property);
                        inlineManagedObject.SetEnabled(Target.ContentEnabled);
                        InspectorContainer.Add(inlineManagedObject);
                    }

                    break;
                case SerializedPropertyType.ExposedReference:
                    if (Property.exposedReferenceValue != null)
                    {
                        var inlineInspector = new InspectorElement(Property.exposedReferenceValue);
                        inlineInspector.SetEnabled(Target.ContentEnabled);
                        InspectorContainer.Add(inlineInspector);
                    }

                    break;
                default:
                    Debug.LogWarning($"InlineAttribute can't handle a property of type {Property.propertyType}. (Property: {Property.name})");
                    break;
            }
        }

        private void CheckInline(SerializedProperty property) => OnUpdate();
    }
}
