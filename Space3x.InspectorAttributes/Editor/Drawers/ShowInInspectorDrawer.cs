//using Space3x.Attributes.Types;
//using UnityEditor;
//using UnityEditor.UIElements;
//using UnityEngine;
//using UnityEngine.UIElements;
//
//namespace Space3x.InspectorAttributes.Editor.Drawers
//{
//    [CustomPropertyDrawer(typeof(ShowInInspectorAttribute), useForChildren: true)]
//    public class ShowInInspectorDrawer : Drawer<ShowInInspectorAttribute>
//    {
//        protected VisualElement InspectorContainer { get; private set; }
//        
//        public override ShowInInspectorAttribute Target => (ShowInInspectorAttribute) attribute;
//        
//        protected override VisualElement OnCreatePropertyGUI(SerializedProperty property)
//        {
//            Debug.Log($"[ShowInInspectorDrawer] OnCreatePropertyGUI: {property.name} {property.type} {property.propertyType.ToString()}");
//            var container = new VisualElement();
//            var field = new PropertyField(property);
//            field.Unbind();
//            field.TrackPropertyValue(property, CheckInline);
//            field.BindProperty(property);
//            InspectorContainer = new VisualElement();
//            container.Add(field);
//            container.Add(InspectorContainer);
//            OnUpdate();
//            
//            return container;
//        }
//
//        public override void OnUpdate()
//        {
//
//        }
//
//        private void CheckInline(SerializedProperty property) => OnUpdate();
//    }
//}
