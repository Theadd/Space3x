using System;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public interface ITextValueElement
    {
        int CursorIndex { get; set; }
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class TextValueElement : TextElement, ITextValueElement
    {
        private static MethodInfo s_DrawHighlightingMethod = typeof(TextElement).GetMethod("DrawHighlighting", BindingFlags.NonPublic | BindingFlags.Instance);
        private static CustomStyleProperty<Color> s_SelectionColorProperty = new ("--unity-selection-color");
        private static CustomStyleProperty<Color> s_CursorColorProperty = new ("--unity-cursor-color");
        
        private Color m_SelectionColor = Color.clear;
        private Color m_CursorColor = Color.grey;
        private int m_CursorIndex = 0;
        private bool m_OverrideCursorIndex = false;

        [CreateProperty]
        public bool ForceRenderCursorCaret { get; set; }

        public TextValueElement() : base()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            generateVisualContent = generateVisualContent + new Action<MeshGenerationContext>(OnGenerateCustomVisualContent);
        }

        [EventInterest(new System.Type[]
        {
            typeof(ContextualMenuPopulateEvent), typeof(KeyDownEvent), typeof(KeyUpEvent), typeof(ValidateCommandEvent),
            typeof(ExecuteCommandEvent), typeof(FocusEvent), typeof(BlurEvent), typeof(FocusInEvent),
            typeof(FocusOutEvent), typeof(PointerDownEvent), typeof(PointerUpEvent), typeof(PointerMoveEvent),
            typeof(NavigationMoveEvent), typeof(NavigationSubmitEvent), typeof(NavigationCancelEvent)
        })]
        protected override void HandleEventBubbleUp(EventBase evt)
        {
            m_OverrideCursorIndex = false;
            base.HandleEventBubbleUp(evt);
        }

        private void OnGenerateCustomVisualContent(MeshGenerationContext mgc)
        {
            if (ForceRenderCursorCaret && !this.IsFocused())
            {
                if (selection.selectIndex != selection.cursorIndex)
                    s_DrawHighlightingMethod.Invoke(this, new object[] { mgc });
                DrawCustomCaret(mgc);
            }
        }
        
        private void DrawCustomCaret(MeshGenerationContext mgc)
        {
            if (CursorIndex > text.Length) return;
            var str = text.Substring(0, CursorIndex);
            var size = this.MeasureTextSize(str, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);
            var origin = new Vector2(this.resolvedStyle.paddingTop, this.resolvedStyle.paddingLeft);
            size.y = this.resolvedStyle.height - origin.y;
            mgc.PaintRectangle(
                origin.x + size.x + 1f,
                origin.y - 1f,
                origin.x + size.x + 2f,
                origin.y + size.y - 2f,
                m_CursorColor);
        }

        [CreateProperty]
        public int CursorIndex
        {
            get => m_OverrideCursorIndex ? m_CursorIndex : selection.cursorIndex;
            set
            {
                m_OverrideCursorIndex = true;
                m_CursorIndex = value;
                selection.cursorIndex = value;
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            Color selectionValue = Color.clear;
            Color cursorValue = Color.clear;
            ICustomStyle customStyle = e.customStyle;
            
            if (customStyle.TryGetValue(s_SelectionColorProperty, out selectionValue))
                m_SelectionColor = selectionValue;
            if (customStyle.TryGetValue(s_CursorColorProperty, out cursorValue))
                m_CursorColor = cursorValue;
        }
    }
}
