using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ButtonAttribute), useForChildren: false)]
    public class ButtonDecorator : Decorator<BlockDecorator, ButtonAttribute>, IAttributeExtensionContext<ButtonAttribute>
    {
        public override ButtonAttribute Target => (ButtonAttribute) attribute;

        private Button m_Button;
        private MethodInfo m_ButtonMethod;
        
        protected override void OnCreatePropertyGUI(VisualElement container)
        {
            // DebugLog.Info($"ButtonDrawer OnCreatePropertyGUI: {this.GetHashCode()}");
            m_Button = new Button(OnClick)
            {
                text = string.IsNullOrEmpty(Target.Text) ? ObjectNames.NicifyVariableName(Target.MethodName) : Target.Text,
                name = "ui-button_" + Target.MethodName,
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
            container.Add(m_Button);
        }
        
        private void OnClick()
        {
            if (m_ButtonMethod != null)
                m_ButtonMethod.Invoke(Property.GetDeclaringObject(), null);
        }

        public override void OnAttachedAndReady(VisualElement element)
        {
            // DebugLog.Info($"ButtonDrawer OnAttachedAndReady: {this.GetHashCode()}");
        }

        public override void OnUpdate()
        {
            // DebugLog.Info($"ButtonDrawer OnUpdate: {this.GetHashCode()}");
            if (Property == null) return;
            var target = Property.GetDeclaringObject();
            m_ButtonMethod = ReflectionUtility.FindFunction(Target.MethodName, target);
        }
    }
}
