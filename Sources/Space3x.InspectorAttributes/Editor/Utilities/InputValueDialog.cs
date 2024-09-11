using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class InputValueDialog : EditorWindow
    {
        private static Func<VisualElement> s_FieldFactory;
        
        private static Action<object> s_Callback;
        
        private static string s_Message;
        
        private static FieldInfo s_ValueFieldInfo;
        
        public static void Prompt<TField, TValue>(Func<TField> fieldFactory, Action<TValue> callback, string message, string title)
            where TField : BaseField<TValue>
        {
            if (HasOpenInstances<InputValueDialog>())
            {
                var prevWindow = GetWindow<InputValueDialog>();
                prevWindow.Close();
            }
            s_FieldFactory = fieldFactory;
            s_Message = message;
            s_ValueFieldInfo = typeof(BaseField<TValue>).GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
            s_Callback = o => callback.Invoke((TValue) o);

            var h = string.IsNullOrEmpty(s_Message) ? 87 : 108;
            InputValueDialog window = GetWindowWithRect<InputValueDialog>(new Rect(0, 0, 403, h), true, title);
            window.minSize = new Vector2(403, h);
            window.Show();  // ObjectField does not support modal windows
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var topContainer = new VisualElement()
            {
                style =
                {
                    paddingTop = 6,
                    paddingRight = 8,
                    paddingBottom = 4,
                    paddingLeft = 8
                }
            };
            if (!string.IsNullOrEmpty(s_Message))
            {
                var label = new Label(s_Message)
                {
                    style =
                    {
                        marginBottom = 6,
                        marginLeft = 3,
                        whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal)
                    }
                };
                topContainer.Add(label);
            }
            
            var field = s_FieldFactory();
            topContainer.Add(field);
            root.Add(topContainer);
            
            var bottomContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    justifyContent = new StyleEnum<Justify>(Justify.FlexEnd),
                    paddingRight = 8,
                    paddingBottom = 6,
                    position = new StyleEnum<Position>(Position.Absolute),
                    bottom = 0,
                    right = 0
                }
            };
            var acceptButton = new Button
            {
                text = "Accept"
            };
            acceptButton.clicked += () =>
            {
                object v = s_ValueFieldInfo?.GetValue(field);
                s_Callback(v);
                Close();
            };
            bottomContainer.Add(acceptButton);
            var cancelButton = new Button
            {
                text = "Cancel",
                style =
                {
                    opacity = 0.7f
                }
            };
            cancelButton.clicked += Close;
            bottomContainer.Add(cancelButton);
            root.Add(bottomContainer);
        }
    }
}
