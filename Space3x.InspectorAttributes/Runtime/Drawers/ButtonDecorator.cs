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

        private Invokable<object, object> m_Invokable;
        
        protected override void OnCreatePropertyGUI(VisualElement container)
        {
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
            if (m_Invokable != null)
            {
                var res = m_Invokable.Parameters == null
                    ? m_Invokable.Invoke()
                    : m_Invokable.InvokeWith(m_Invokable.Parameters);
            }
        }

        // public override void OnAttachedAndReady(VisualElement element) { }

        public override void OnUpdate()
        {
            if (Property == null) return;
            if (!Property.TryCreateInvokable<object, object>(Target.MethodName, out var invokable, drawer: this))
            {
                var target = Property.GetDeclaringObject();
                m_Invokable = new Invokable<object, object>()
                {
                    TargetObject = target,
                    CallableMember = ReflectionUtility.FindFunction(Target.MethodName, target)
                };
            }
        }
    }
}
