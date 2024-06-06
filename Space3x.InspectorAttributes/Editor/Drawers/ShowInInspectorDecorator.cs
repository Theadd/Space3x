// using System.Reflection;
// using Space3x.Attributes.Types;
// using Space3x.InspectorAttributes.Editor.Utilities;
// using Space3x.InspectorAttributes.Editor.VisualElements;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace Space3x.InspectorAttributes.Editor.Drawers
// {
//     [CustomPropertyDrawer(typeof(ShowInInspectorAttribute), useForChildren: false)]
//     public class ShowInInspectorDecorator : Decorator<BlockDecorator, ShowInInspectorAttribute>, IAttributeExtensionContext<ShowInInspectorAttribute>
//     {
//         public override ShowInInspectorAttribute Target => (ShowInInspectorAttribute) attribute;
//
//         private Button m_Button;
//         private MethodInfo m_ButtonMethod;
//         
//         protected override void OnCreatePropertyGUI(VisualElement container)
//         {
//             m_Button = new Button(OnClick)
//             {
//                 text = "Hi there!",
//                 name = "ui-button_sample",
//                 style =
//                 {
//                     marginBottom = 0,
//                     marginLeft = 0,
//                     marginRight = 0,
//                     marginTop = 0,
//                     flexGrow = 1,
//                     whiteSpace = WhiteSpace.Normal
//                 }
//             };
//             container.Add(m_Button);
//         }
//         
//         private void OnClick()
//         {
//             Debug.Log("OnClick");
//         }
//
//         public override void OnAttachedAndReady(VisualElement element)
//         {
//             Debug.Log($"OnAttachedAndReady {Target.GetType().Name} Property: {Property.name}");
//         }
//
//         public override void OnUpdate()
//         {
//             Debug.Log($"OnUpdate {Target.GetType().Name} Property: {Property.name}");
//         }
//     }
// }
