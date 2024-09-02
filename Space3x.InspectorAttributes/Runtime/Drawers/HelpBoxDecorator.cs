using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(HelpBoxAttribute), false)]
#endif
    [CustomRuntimeDrawer(typeof(HelpBoxAttribute), false)]
    public class HelpBoxDecorator : Decorator<BlockDecorator, HelpBoxAttribute>, IAttributeExtensionContext<HelpBoxAttribute>
    {
        public override HelpBoxAttribute Target => (HelpBoxAttribute) attribute;

        private HelpBox m_HelpBox;
        
        protected override void OnCreatePropertyGUI(VisualElement container)
        {
            m_HelpBox = new HelpBox(Target.Text ?? "", Target.MessageType)
            {
                style =
                {
                    // marginBottom = 0,
                    // marginLeft = 0,
                    // marginRight = 0,
                    // marginTop = 0,
                    flexGrow = 1,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            container.Add(m_HelpBox);
        }
    }
}
