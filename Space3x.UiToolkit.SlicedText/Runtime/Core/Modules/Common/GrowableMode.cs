using System;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    public class GrowableMode
    {
        public SlicedTextEditor TextEditor { get; private set; }

        
        public bool IsActive => m_Active;

        public int LowLimit { get; set; } = 240;
        
        public int HighLimit { get; set; } = 300;

        private bool m_Active = false;

        private bool m_Enabled = true;

        private int m_Lines = 0;  // Current number of lines in the StringSliceGroup
        
        private int m_LastUsedLines = 0;  // Last used number of lines to sync height

        private int m_MinLinesToSync;

        private float m_VisibleLinesInViewport = 20f;
        
        private VisualElement m_GrowableElement;
        private int m_GrowableElementIndex = -1;
        
        public GrowableMode() {}

        public void Initialize(SlicedTextEditor textEditor)
        {
            TextEditor = textEditor;

            if (LowLimit == -1 || HighLimit == -1)
                m_Enabled = false;
        }

        public void Sync()
        {
            if (!m_Enabled) return;
            
            m_Lines = TextEditor.Editor.Count;
            m_NeedsResync = false;

            if (m_Active && m_Lines <= LowLimit)
            {
                Deactivate();
            }
            else if (!m_Active && m_Lines >= HighLimit)
            {
                Activate();
            }
            else if (m_Active)
            {
                if (Math.Abs(m_LastUsedLines - m_Lines) >= m_MinLinesToSync)
                    SyncEditorHeight();

                if (m_GrowableElementIndex > 0)
                {
                    var lastChange = TextEditor.Editor.LastChange;
                    var targetIndex = lastChange.LineIndex + lastChange.LinesRemoved + ((int) m_VisibleLinesInViewport);
                    if (targetIndex > m_GrowableElementIndex)
                    {
                        MoveGrowableElementAt(targetIndex);
                        m_NeedsResync = lastChange.LinesRemoved != lastChange.LinesAdded && m_GrowableElementIndex >= 0;
                    }
                }
            }
        }

        private bool m_NeedsResync = false;
        
        public void Resync()
        {
            if (m_NeedsResync && m_Enabled && m_Active)
                m_GrowableElementIndex = TextEditor.BlockProcessor.TargetContainer.IndexOf(m_GrowableElement);
            /*/                                                      \<== TODO               /*/
        }

        protected virtual void Activate()
        {
            m_Active = true;
            
            SyncEditorHeight();

            if (TextEditor.EditorScrollView != null)
            {
                TextEditor.EditorScrollView.verticalScroller.valueChanged += OnScrollerChanged;
                SyncGrowableElement();
            }
        }

        protected virtual void Deactivate()
        {
            m_Active = false;
                
            TextEditor.style.height = StyleKeyword.Auto;
            MoveGrowableElementAt(-1);
            
            if (TextEditor.EditorScrollView != null)
            {
                TextEditor.EditorScrollView.verticalScroller.valueChanged -= OnScrollerChanged;
            }
        }

        public virtual void SyncEditorHeight()
        {
            if (!m_Active || !m_Enabled) return;
                
            var linesHeight = (int)(m_Lines * TextEditor.TextSize.y);
            var thirdOfViewportHeight =
                Math.Max((int) (TextEditor.EditorScrollView?.contentViewport.layout.height ?? 450f), 450) / 3;

            TextEditor.style.height = (float) (linesHeight + thirdOfViewportHeight);
            m_MinLinesToSync = (int)((thirdOfViewportHeight / TextEditor.TextSize.y) * 0.7);
            m_LastUsedLines = m_Lines;
        }
        
        private int m_MinScrollOffset = 300;
        private int m_PrevScrollValue = 0;
        
        protected virtual void OnScrollerChanged(float y)
        {
            var _y = (int) y;
            if (Math.Abs(m_PrevScrollValue - _y) > m_MinScrollOffset)
            {
                SyncGrowableElement();

                m_PrevScrollValue = _y;
            }
        }

        public virtual void SyncGrowableElement()
        {
            if (!m_Active || !m_Enabled) return;

            var lineHeight = TextEditor.TextSize.y;
            var viewHeight = TextEditor.EditorScrollView.contentViewport.layout.height;
            var contentHeight = viewHeight + TextEditor.EditorScrollView.verticalScroller.highValue;
            var viewPosY = TextEditor.EditorScrollView.verticalScroller.value;
            var fromHeight = viewPosY + viewHeight;

            m_VisibleLinesInViewport = viewHeight / lineHeight;
            m_MinScrollOffset = (int) viewHeight;
            
            if ((contentHeight - fromHeight) / viewHeight > 3f)
            {
                
                var fromLine = fromHeight / lineHeight;
                
                if (m_GrowableElementIndex < (int) (fromLine + m_VisibleLinesInViewport * 0.6) 
                    || m_GrowableElementIndex > (int) (fromLine + m_VisibleLinesInViewport * 4))
                    MoveGrowableElementAt((int) (fromLine + m_VisibleLinesInViewport * 1.5));
            }
            else
            {
                MoveGrowableElementAt(-1);
            }

        }

        protected virtual void MoveGrowableElementAt(int index)
        {
            if (m_GrowableElement != null)
                m_GrowableElement.style.flexGrow = 0f;
            
            m_GrowableElement = index >= 0 && index < TextEditor.BlockProcessor.Length() ? TextEditor.BlockProcessor.ElementAt(index) : null;
            m_GrowableElementIndex = m_GrowableElement == null ? -1 : index;

            if (m_GrowableElement != null)
            {
                m_GrowableElement.style.flexGrow = 1f;
            }
        }
    }
}
