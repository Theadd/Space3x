using System;
using UnityEngine;
using UnityEngine.UIElements;
using Space3x.UiToolkit.SlicedText.Iterators;

namespace Space3x.UiToolkit.SlicedText.VisualElements
{
    public abstract class EditorTextView : VisualElement
    {
        public Action OnCancel { get; set; }
        
        public Action OnSubmit { get; set; }
        
        public StringSliceGroup Editor { get; protected set; }

        public TextEditorEventHandler EditorEventHandler { get; set; } = null;

        private bool m_Multiline = true;
        
        public bool Multiline
        {
            get => m_Multiline;
            set
            {
                m_Multiline = value;
                
                if (Editor != null)
                    Editor.Multiline = m_Multiline;
            }
        }

        public virtual bool ReadOnly { get; set; } = false;
                
        protected Color m_SelectionColor = Color.clear;
        protected Color m_CursorColor = Color.grey;
        
        /// <summary>
        /// Background color of selected text.
        /// </summary>
        public Color SelectionColor => m_SelectionColor;

        /// <summary>
        /// Color of the cursor.
        /// </summary>
        public Color CursorColor => m_CursorColor;

        public Color32 ActiveLineColor { get; set; } = new Color32(0, 0, 0, 40);
        
        public Color32 ActiveLineColorLocked { get; set; } = Color.clear;

        public Vector2 TextSize = new Vector2(8f, 16f);
        
        /// <summary>
        /// Cursor's ActiveLine.Index
        /// </summary>
        public int ActiveLineIndex = 0;

        /// <summary>
        /// Cursor position relative to ActiveLine start position.
        /// </summary>
        public int ActiveLinePosition = 0;

        public bool IsSelectionActive = false;

        internal bool AvoidNextFocusOut = false;

        public bool StyleAsLocked { get; protected set; } = false;
        
        public bool IsFocusedSelf { get; private set; } = false;
        
        public bool ForceRenderCursorCaret { get; set; } = false;

        /// <summary>
        /// Required values to draw the selected lines
        /// </summary>
        public (int FromLine, int FromColumn, int ToLine, int ToColumn) ActiveSelection { get; set; } = (0, 0, 0, 0);

        private VisualElement m_ContentView;

        private BackView m_BackView;

        public bool HasBackView { get; private set; } = false;
        
        public bool Empty { get; protected set; } = true;

        public override VisualElement contentContainer
        {
            get { return m_ContentView; }
        }
        
        public EditorTextView()
        {
            m_ContentView = this;
            // focusable = true;
            // tabIndex = 0;


            generateVisualContent += OnGenerateVisualContent;
        }

        public bool HasFocus =>
            panel != null && (panel.focusController.focusedElement == this ||
                              (panel.focusController.focusedElement == this.parent &&
                               this.parent?.delegatesFocus == true));

        public abstract void RequestStylesUpdate();

        protected virtual void AttachBackView(BackView view)
        {
            if (m_BackView != null && hierarchy.IndexOf(m_BackView) >= 0)
                hierarchy.Remove(m_BackView);
            
            m_BackView = view;
            
            if (m_BackView != null)
            {
                generateVisualContent -= OnGenerateVisualContent;
                m_ContentView = m_BackView.ContentView;
                hierarchy.Add(m_BackView);
                HasBackView = true;
            }
            else
            {
                generateVisualContent -= OnGenerateVisualContent;
                hierarchy.Clear();
                m_ContentView = this;
                HasBackView = false;
                generateVisualContent += OnGenerateVisualContent;
            }
        }

        /// <summary>
        /// Mouse/Pointer coordinates to line index and "character/column" position within the line.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public (int LineIndex, int LinePosition) LocalToLinePosition(Vector3 coords)
        {
            if (HasBackView)
                return m_BackView.LocalToLinePosition(coords);
            var r = contentRect;
            coords = new Vector3(coords.x - r.x, coords.y - r.y, coords.z);
            var lineIndex = Math.Min(((int)Math.Max(coords.y, 0)) / ((int)TextSize.y), Editor.Count - 1);
            var maxLen = Editor.Slices[lineIndex].Length - (lineIndex + 1 == Editor.Count ? 0 : 1);
            var linePosition = Math.Min((int)((Math.Max(coords.x, 0) + 2f) / TextSize.x), maxLen);

            return (lineIndex, linePosition);
        }

        protected int LastCursorPosition = -1;
        protected int LastCursorOffset = 0;
        
        public void CursorChangeHandler()
        {
            var (index, startPosition) = Editor.Cursor.ActiveLine;
            ActiveLineIndex = index;
            ActiveLinePosition = Editor.Cursor.Position - startPosition;

            if (Editor.Cursor.Offset != 0)
            {
                var selectionStart = Editor.Cursor.SelectionStart;
                var selectionEnd = Editor.Cursor.SelectionEnd;
                var (indexStart, positionStart) = Editor.GetSliceAt(selectionStart);
                var (indexEnd, positionEnd) = Editor.GetSliceAt(selectionEnd);

                ActiveSelection = (
                    indexStart,
                    selectionStart - positionStart,
                    indexEnd,
                    selectionEnd - positionEnd);

                IsSelectionActive = true;
            }
            else
                IsSelectionActive = false;

            LastCursorPosition = Editor.Cursor.Position;
            LastCursorOffset = Editor.Cursor.Offset;

            UpdateScrollOffset();
            if (HasBackView) m_BackView.UpdateScrollOffset();

            /*if (HasBackView)*/ contentContainer.MarkDirtyRepaint();
            // else MarkDirtyRepaint();
        }

        public virtual void UpdateScrollOffset() {}

        protected virtual void StylesResolvedHandler(bool isLastCall)
        {
            if (HasBackView && isLastCall) m_BackView.Sync();
        }

        protected virtual void UpdateBackView()
        {
            if (HasBackView) m_BackView.Update();
        }
        
        #region GENERATE VISUAL CONTENT

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Rect r = contentRect;
            if (r.width < 0.01f || r.height < 0.01f)
                return; // Skip rendering when too small.
            
            // Draw background rect of current active line 
            if (Multiline && (!Empty || HasFocus))
                mgc.PaintRectangle(
                    0, 
                    r.y + ActiveLineIndex * TextSize.y, 
                    r.x + r.width,
                    r.y + (ActiveLineIndex * TextSize.y) + TextSize.y,
                    StyleAsLocked ? ActiveLineColorLocked : ActiveLineColor
                );

            // Draw selection if any
            if (IsSelectionActive)
            {
                var (fromLine, fromColumn, toLine, toColumn) = ActiveSelection;

                for (var line = fromLine; line <= toLine; line++)
                {
                    if (toLine - fromLine > 5 && line == fromLine + 1)
                    {
                        // batch drawing
                        mgc.PaintRectangle(
                            r.x,
                            r.y + line * TextSize.y,
                            r.x + r.width,
                            r.y + (toLine - 1) * TextSize.y + TextSize.y,
                            m_SelectionColor);
                        
                        line = toLine - 1;
                    } 
                    else
                    {
                        mgc.PaintRectangle(
                            r.x + (line != fromLine ? 0 : fromColumn * TextSize.x),
                            r.y + line * TextSize.y,
                            r.x + (line != toLine ? r.width : toColumn * TextSize.x),
                            r.y + line * TextSize.y + TextSize.y,
                            m_SelectionColor);
                    }
                }
            }

            // Draw cursor caret
            if (IsFocusedSelf || ForceRenderCursorCaret)
                mgc.PaintRectangle(
                    r.x + ActiveLinePosition * TextSize.x, 
                    r.y + ActiveLineIndex * TextSize.y, 
                    r.x + (ActiveLinePosition * TextSize.x) + 1f,
                    r.y + (ActiveLineIndex * TextSize.y) + TextSize.y,
                    StyleAsLocked ? Color.clear : m_CursorColor
                    );
        }
        
        #endregion GENERATE VISUAL CONTENT

        protected virtual void OnFocusOut(FocusOutEvent e) {}
        protected virtual void OnFocusIn(FocusInEvent e) {}
        
        private void OnBeforeFocusOut(FocusOutEvent e)
        {
            if (e.target == this && AvoidNextFocusOut)
            {
                e.StopPropagation();
                e.StopImmediatePropagation();
                // e.PreventDefault(); // TODO: Commented out due to obsolete warnings, needs to be further investigated.
                schedule.Execute(this.Focus).ExecuteLater(1);
                AvoidNextFocusOut = false;
            } 
            else if (e.target == this)
            {
                OnFocusOut(e);
                IsFocusedSelf = false;
                schedule.Execute(MarkAsDirty).ExecuteLater(1);
            }
        }

        private void OnBeforeFocusIn(FocusInEvent e)
        {
            OnFocusIn(e);
            IsFocusedSelf = true;
            schedule.Execute(MarkAsDirty).ExecuteLater(1);
        }
        
        private void RegisterEvents()
        {
            RegisterCallback<FocusOutEvent>(OnBeforeFocusOut);
            RegisterCallback<FocusInEvent>(OnBeforeFocusIn);
        }

        public void MarkAsDirty() => contentContainer.MarkDirtyRepaint();
        
        protected override void HandleEventBubbleUp(EventBase ev)
        {
            switch (ev)
            {
                case FocusOutEvent focusOut:
                    OnBeforeFocusOut(focusOut);
                    break;
                case FocusInEvent focusIn:
                    OnBeforeFocusIn(focusIn);
                    break;
                case NavigationMoveEvent navigationMove:
                    if (navigationMove.target == this)
                    {
                        navigationMove.StopPropagation();
                        navigationMove.StopImmediatePropagation();
                        // navigationMove.PreventDefault(); // TODO: Commented out due to obsolete warnings, needs to be further investigated.
                    }
                    break;
            }

            base.HandleEventBubbleUp(ev);
            EditorEventHandler?.HandleEventBubbleUp(ev);
        }
    }
}
