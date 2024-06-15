using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute), useForChildren: false)]
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
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    flexGrow = 1,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            container.Add(m_HelpBox);
        }
    }
}
