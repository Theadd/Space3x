using Space3x.UiToolkit.SlicedText.Iterators;
using Space3x.UiToolkit.SlicedText.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

namespace Space3x.UiToolkit.SlicedText
{
    public class TextEditorEventHandler
    {
        public StringSliceGroup Editor { get; set; }
        
        public EditorTextView TextView { get; set; }

        /// <summary>
        /// Double click interval in milliseconds
        /// </summary>
        public int DoubleClickInterval { get; set; } = 500;

        protected SnappingMode Snapping = SnappingMode.None;

        public TextEditorEventHandler() {}

        public virtual void PerformOperation(EditorAction operation, bool textIsReadOnly) {}

        public virtual void OnKeyDown(KeyDownEvent evt) {}
        
        public virtual void HandleEventBubbleUp(EventBase evt)
        {
            if (evt.eventTypeId == PointerDownEvent.TypeId() && ((PointerDownEvent) evt).pointerType != PointerType.mouse)
            {
                OnPointerDown(evt as PointerDownEvent);
            }
            else if (evt.eventTypeId == PointerUpEvent.TypeId() && ((PointerUpEvent) evt).pointerType != PointerType.mouse)
            {
                OnPointerUp(evt as PointerUpEvent);
            }
            else if (evt.eventTypeId == PointerMoveEvent.TypeId() && ((PointerMoveEvent) evt).pointerType != PointerType.mouse)
            {
                OnPointerMove(evt as PointerMoveEvent);
            }
            else if (evt.eventTypeId == MouseDownEvent.TypeId())
            {
                OnMouseDown(evt as MouseDownEvent);
            }
            else if (evt.eventTypeId == MouseUpEvent.TypeId())
            {
                OnMouseUp(evt as MouseUpEvent);
            }
            else if (evt.eventTypeId == MouseMoveEvent.TypeId())
            {
                OnMouseMove(evt as MouseMoveEvent);
            }
        }

        #region MOUSE EVENTS
        
        protected int LastCursorPos;
        protected long LastMouseDownTimestamp = 0;
        protected int ClickCount = 0;

        protected int WordSnappingStart;
        protected int WordSnappingEnd;
        protected void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                var (lineIndex, linePosition) = TextView.LocalToLinePosition(evt.localMousePosition);
                var pos = Editor.GetLineStartPosition(lineIndex) + linePosition;

                // Custom ClickCount since I wasn't getting any evt.clickCount > 2 here, whatever. 
                if (evt.timestamp - LastMouseDownTimestamp > DoubleClickInterval || (ClickCount > 0 && LastCursorPos != pos))
                    ClickCount = 0;
                
                LastMouseDownTimestamp = evt.timestamp;
                ClickCount++;
                //

                if (ClickCount == 1)
                {
                    Snapping = SnappingMode.None;
                    LastCursorPos = pos;
                    
                    Editor.Cursor.MoveTo(pos, evt.shiftKey);
                }
                else if (ClickCount == 2)
                {
                    Snapping = SnappingMode.Word;

                    WordSnappingStart = Editor.GetNextWordPosition(pos, -1);
                    WordSnappingEnd = Editor.GetNextWordPosition(WordSnappingStart, 1);
                    Editor.Cursor.MoveTo(WordSnappingStart, evt.shiftKey);
                    Editor.Cursor.MoveTo(WordSnappingEnd, true);
                }
                else
                {
                    Snapping = SnappingMode.Line;
                }

                TextView.CaptureMouse();
                evt.StopPropagation();
            }
        }
        
        protected void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (TextView.HasMouseCapture())
                {
                    var (lineIndex, linePosition) = TextView.LocalToLinePosition(evt.localMousePosition);
                    var pos = Editor.GetLineStartPosition(lineIndex) + linePosition;
                    
                    switch (Snapping)
                    {
                        case SnappingMode.Word:
                            // Automatically handled by OnMouseDown and OnMouseMove
                            break;
                        case SnappingMode.Line:
                            break;
                        default:
                            Editor.Cursor.MoveTo(pos, true);
                            break;
                    }

                    Snapping = SnappingMode.None;
                    TextView.ReleaseMouse();
                    evt.StopPropagation();
                }
            }
        }
        
        protected void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (TextView.HasMouseCapture())
                {
                    var (lineIndex, linePosition) = TextView.LocalToLinePosition(evt.localMousePosition);
                    var pos = Editor.GetLineStartPosition(lineIndex) + linePosition;
                    
                    if (LastCursorPos != pos)
                    {
                        switch (Snapping)
                        {
                            case SnappingMode.Word:
                                if (pos < WordSnappingStart)
                                {
                                    Editor.Cursor.MoveTo(WordSnappingEnd, false);
                                    Editor.Cursor.MoveTo(Editor.GetNextWordPosition(pos, -1), true);
                                }
                                else if (pos > WordSnappingEnd)
                                {
                                    Editor.Cursor.MoveTo(WordSnappingStart, false);
                                    Editor.Cursor.MoveTo(Editor.GetNextWordPosition(pos, 1), true);
                                }
                                else
                                {
                                    Editor.Cursor.MoveTo(WordSnappingStart, false);
                                    Editor.Cursor.MoveTo(WordSnappingEnd, true);
                                }
                                
                                break;
                            case SnappingMode.Line:
                                // Debug.Log("<color=#AEAEAEFF>OnMouseMove() case SnappingMode.Line @</color> " + pos);
                                break;
                            default:
                                Editor.Cursor.MoveTo(pos, true);
                                break;
                        }
                    }

                    LastCursorPos = pos;
                    evt.StopPropagation();
                }
            }
        }
        
        #endregion MOUSE EVENTS

        #region OTHER POINTER EVENTS
        
        protected int ActivePointerId = PointerId.invalidPointerId;

        protected void OnPointerDown(PointerDownEvent e)
        {
            if (e.isPrimary && ActivePointerId == PointerId.invalidPointerId)
            {
                var (lineIndex, linePosition) = TextView.LocalToLinePosition(e.localPosition);
                var pos = Editor.GetLineStartPosition(lineIndex) + linePosition;
                
                Editor.Cursor.MoveTo(pos, e.shiftKey);

                ActivePointerId = e.pointerId;
                e.StopPropagation();
            }
        }
        
        protected void OnPointerUp(PointerUpEvent e)
        {
            if (e.isPrimary && e.pointerId == ActivePointerId)
            {
                var (lineIndex, linePosition) = TextView.LocalToLinePosition(e.localPosition);
                var pos = Editor.GetLineStartPosition(lineIndex) + linePosition;

                Editor.Cursor.MoveTo(pos, true);

                ActivePointerId = PointerId.invalidPointerId;
                e.StopPropagation();
            }
        }
        
        protected void OnPointerMove(PointerMoveEvent e)
        {
            if (e.isPrimary && e.pointerId == ActivePointerId)
            {
                var (lineIndex, linePosition) = TextView.LocalToLinePosition(e.localPosition);
                var pos = Editor.GetLineStartPosition(lineIndex) + linePosition;

                Editor.Cursor.MoveTo(pos, true);
                
                e.StopPropagation();
            }
        }
        
        #endregion OTHER POINTER EVENTS
    }
}
