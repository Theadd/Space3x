using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ButtonAttribute), false)]
#endif
    [CustomRuntimeDrawer(typeof(ButtonAttribute), false)]
    public class ButtonDecorator : Decorator<BlockDecorator, ButtonAttribute>, IAttributeExtensionContext<ButtonAttribute>
    {
        public override ButtonAttribute Target => (ButtonAttribute) attribute;

        private Button m_Button;

        private Invokable<object, object> m_Invokable;
        
        protected override void OnCreatePropertyGUI(VisualElement container)
        {
            m_Button = new Button(OnClick)
            {
#if UNITY_EDITOR
                text = string.IsNullOrEmpty(Target.Text) ? UnityEditor.ObjectNames.NicifyVariableName(Target.MethodName) : Target.Text,
#else
                text = string.IsNullOrEmpty(Target.Text) ? Target.MethodName : Target.Text,
#endif
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
            if (Property.TryCreateInvokable<object, object>(Target.MethodName, out var invokable, drawer: this))
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
