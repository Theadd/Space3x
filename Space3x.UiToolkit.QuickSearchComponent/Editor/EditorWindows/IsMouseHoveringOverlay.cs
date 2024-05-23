//using UnityEditor.Overlays;
//using UnityEngine.UIElements;
//
//namespace Space3x.UiToolkit.QuickSearchComponent.Editor
//{
//    [Overlay(typeof(QuickSearchWindow), "Is Mouse Hovering Me?", true)]
//    class IsMouseHoveringMe : Overlay
//    {
//        Label m_MouseLabel;
//
//        public override VisualElement CreatePanelContent()
//        {
//            m_MouseLabel = new Label();
//            m_MouseLabel.style.minHeight = 40;
//            m_MouseLabel.RegisterCallback<MouseEnterEvent>(evt => m_MouseLabel.text = "Mouse is hovering this Overlay content!");
//            m_MouseLabel.RegisterCallback<MouseLeaveEvent>(evt => m_MouseLabel.text = "Mouse is not hovering this Overlay content.");
//            return m_MouseLabel;
//        }
//    }
//}
