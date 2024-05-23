using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    public class TextEditorKeyboardEventHandler : TextEditorEventHandler
    {
        public override void HandleEventBubbleUp(EventBase evt)
        {
            base.HandleEventBubbleUp(evt);

            if (evt.eventTypeId == KeyDownEvent.TypeId())
            {
                OnKeyDown(evt as KeyDownEvent);
            }
        }

//        public override void ExecuteDefaultAction(EventBase evt) { }

        private readonly Event m_ImguiEvent = new Event();

        public override void OnKeyDown(KeyDownEvent evt)
        {
            evt.GetImguiEvent(m_ImguiEvent);
            
            if (HandleKeyEvent(m_ImguiEvent, TextView.ReadOnly))
            {
                // evt.PreventDefault();    // TODO: Commented out due to obsolete warnings, needs to be further investigated.
                evt.StopPropagation();
                evt.StopImmediatePropagation();
            }
            else
            {
                char c = evt.character;

                // Handle KeyCode.KeypadEnter as KeyCode.Return
                if (c == 3) c = '\n';
                // Ignore command and control keys, but not AltGr characters
                if (evt.actionKey && !(evt.altKey && c != '\0'))
                    return;

                if (c >= ' ' || c == '\n')
                {
                    evt.StopPropagation();

                    if (!TextView.ReadOnly)
                        Editor.Insert(c);
                }
            }
        }


        private static Dictionary<Event, EditorAction> s_Keyactions;

        public virtual bool HandleKeyEvent(Event e) => HandleKeyEvent(e, TextView.ReadOnly);

        public virtual bool HandleKeyEvent(Event e, bool textIsReadOnly)
        {
            if (s_Keyactions == null) InitKeyActions();
            EventModifiers modifiers = e.modifiers;
            e.modifiers &= ~EventModifiers.CapsLock;
            if (s_Keyactions.ContainsKey(e))
            {
                PerformOperation(s_Keyactions[e], textIsReadOnly);
                e.modifiers = modifiers;
                return true;
            }

            e.modifiers = modifiers;
            return false;
        }

        public override void PerformOperation(EditorAction operation, bool textIsReadOnly)
        {
            switch (operation)
            {
                case EditorAction.MoveLeft:
                    Editor.Cursor.MoveTo(Editor.Cursor.Position - 1);
                    break;
                case EditorAction.MoveRight:
                    Editor.Cursor.MoveTo(Editor.Cursor.Position + 1);
                    break;
                case EditorAction.MoveUp:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index - 1);
                    break;
                case EditorAction.MoveDown:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index + 1);
                    break;
                case EditorAction.SelectLeft:
                    Editor.Cursor.MoveTo(Editor.Cursor.Position - 1, true);
                    break;
                case EditorAction.SelectRight:
                    Editor.Cursor.MoveTo(Editor.Cursor.Position + 1, true);
                    break;
                case EditorAction.SelectUp:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index - 1, true);
                    break;
                case EditorAction.SelectDown:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index + 1, true);
                    break;
                case EditorAction.MoveLineStart:
                    Editor.Cursor.MoveTo(Editor.Cursor.ActiveLine.StartPosition);
                    break;
                case EditorAction.MoveLineEnd:
                    Editor.Cursor.MoveTo(Editor.Cursor.ActiveLine.StartPosition +
                                         Editor.Slices[Editor.Cursor.ActiveLine.Index].Length - 1);
                    break;
                case EditorAction.SelectLineStart:
                    Editor.Cursor.MoveTo(Editor.Cursor.ActiveLine.StartPosition, true);
                    break;
                case EditorAction.SelectLineEnd:
                    Editor.Cursor.MoveTo(Editor.Cursor.ActiveLine.StartPosition +
                                         Editor.Slices[Editor.Cursor.ActiveLine.Index].Length - 1, true);
                    break;
                case EditorAction.MovePageUp:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index + 20);
                    break;
                case EditorAction.MovePageDown:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index - 20);
                    break;
                case EditorAction.SelectPageUp:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index + 20, true);
                    break;
                case EditorAction.SelectPageDown:
                    Editor.Cursor.MoveToLine(Editor.Cursor.ActiveLine.Index - 20, true);
                    break;
                case EditorAction.MoveWordLeft:
                    Editor.Cursor.MoveTo(Editor.GetNextWordPosition(Editor.Cursor.Position, -1), false);
                    break;
                case EditorAction.MoveWordRight:
                    Editor.Cursor.MoveTo(Editor.GetNextWordPosition(Editor.Cursor.Position, 1), false);
                    break;
                case EditorAction.SelectWordLeft:
                    Editor.Cursor.MoveTo(Editor.GetNextWordPosition(Editor.Cursor.Position, -1), true);
                    break;
                case EditorAction.SelectWordRight:
                    Editor.Cursor.MoveTo(Editor.GetNextWordPosition(Editor.Cursor.Position, 1), true);
                    break;
                case EditorAction.Delete:
                    if (!textIsReadOnly)
                    {
                        if (!TextView.IsSelectionActive)
                            Editor.Cursor.MoveTo(Editor.Cursor.Position + 1, true);

                        Editor.Insert("");
                    }

                    break;
                case EditorAction.Backspace:
                    if (!textIsReadOnly)
                    {
                        if (!TextView.IsSelectionActive)
                            Editor.Cursor.MoveTo(Editor.Cursor.Position - 1, true);

                        Editor.Insert("");
                    }

                    break;
                case EditorAction.SelectAll:
                    Editor.Cursor.MoveToLine(Editor.Count + 1);
                    Editor.Cursor.MoveToLine(-1, true);
                    break;
                case EditorAction.SelectNone:
                    if (Editor.Multiline && TextView.IsSelectionActive)
                        Editor.Cursor.MoveTo(Editor.Cursor.Position, false);
                    else
                        TextView.OnCancel?.Invoke();
                    break;
                case EditorAction.EnterKey:
                case EditorAction.ReturnKey:
                    if (Editor.Multiline)
                        Editor.Insert('\n');
                    else
                        TextView.OnSubmit?.Invoke();
                    break;
                case EditorAction.Paste:
                    if (!textIsReadOnly)
                        Editor.Insert(GUIUtility.systemCopyBuffer ?? "");
                    break;
                case EditorAction.Copy:
                    if (!TextView.IsSelectionActive)
                    {
                        PerformOperation(EditorAction.MoveLineEnd, textIsReadOnly);
                        PerformOperation(EditorAction.SelectLineStart, textIsReadOnly);
                    }
                    GUIUtility.systemCopyBuffer = Editor.GetPositionedRange(
                        Editor.Cursor.SelectionStart, 
                        Editor.Cursor.SelectionEnd).ToSlice(null, false).ToString();
                    break;
                case EditorAction.Cut:
                    PerformOperation(EditorAction.Copy, textIsReadOnly);
                    PerformOperation(EditorAction.Delete, textIsReadOnly);
                    break;
                case EditorAction.Undo:
                    if (!textIsReadOnly) Editor.UndoHistory?.Undo(Editor);
                    break;
                case EditorAction.Redo:
                    if (!textIsReadOnly) Editor.UndoHistory?.Redo(Editor);
                    break;
                case EditorAction.AddIndent:
                    if (Editor.Multiline)
                    {
                        if (!textIsReadOnly)
                            Editor.Insert(new string(' ', Editor.TabSize));

                        TextView.AvoidNextFocusOut = true;
                    }
                    break;
                case EditorAction.RemoveIndent:
                    if (Editor.Multiline)
                    {
                        // Debug.LogException(new NotImplementedException());
                        TextView.AvoidNextFocusOut = true;
                    }
                    break;
                default:
                    Debug.LogWarning((object) ("Not implemented: " + operation.ToString()));
                    break;
            }
            
        }

        private static void MapKey(string key, EditorAction action) =>
            s_Keyactions[Event.KeyboardEvent(key)] = action;

        private void InitKeyActions()
        {
            if (s_Keyactions != null) return;

            // # => SHIFT
            // % => CMD
            // & => ALT
            // ^ => CTRL

            s_Keyactions = new Dictionary<Event, EditorAction>();
            MapKey("left", EditorAction.MoveLeft);
            MapKey("right", EditorAction.MoveRight);
            MapKey("up", EditorAction.MoveUp);
            MapKey("down", EditorAction.MoveDown);
            MapKey("#left", EditorAction.SelectLeft);
            MapKey("#right", EditorAction.SelectRight);
            MapKey("#up", EditorAction.SelectUp);
            MapKey("#down", EditorAction.SelectDown);
            MapKey("delete", EditorAction.Delete);
            MapKey("backspace", EditorAction.Backspace);
            MapKey("#backspace", EditorAction.Backspace);
            MapKey("[esc]", EditorAction.SelectNone);
            MapKey("pgup", EditorAction.MovePageUp);
            MapKey("#pgup", EditorAction.SelectPageUp);
            MapKey("pgdown", EditorAction.MovePageDown);
            MapKey("#pgdown", EditorAction.SelectPageDown);
            MapKey("home", EditorAction.MoveLineStart);
            MapKey("#home", EditorAction.SelectLineStart);
            MapKey("end", EditorAction.MoveLineEnd);
            MapKey("#end", EditorAction.SelectLineEnd);
            MapKey("tab", EditorAction.AddIndent);
            MapKey("#tab", EditorAction.RemoveIndent);
            MapKey("[enter]", EditorAction.EnterKey);
            MapKey("return", EditorAction.ReturnKey);

            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                MapKey("%x", EditorAction.Cut);
                MapKey("%c", EditorAction.Copy);
                MapKey("%v", EditorAction.Paste);
                MapKey("^d", EditorAction.Delete);
                MapKey("^h", EditorAction.Backspace);
                MapKey("%a", EditorAction.SelectAll);
                MapKey("%z", EditorAction.Undo);
                MapKey("#%z", EditorAction.Redo);
                MapKey("%y", EditorAction.Redo);
                MapKey("#&left", EditorAction.SelectWordLeft);
                MapKey("#&right", EditorAction.SelectWordRight);
                MapKey("&left", EditorAction.MoveWordLeft);
                MapKey("&right", EditorAction.MoveWordRight);
            }
            else
            {
                MapKey("^x", EditorAction.Cut);
                MapKey("^c", EditorAction.Copy);
                MapKey("^v", EditorAction.Paste);
                MapKey("^a", EditorAction.SelectAll);
                MapKey("^z", EditorAction.Undo);
                MapKey("#^z", EditorAction.Redo);
                MapKey("^y", EditorAction.Redo);
                MapKey("#^left", EditorAction.SelectWordLeft);
                MapKey("#^right", EditorAction.SelectWordRight);
                MapKey("^left", EditorAction.MoveWordLeft);
                MapKey("^right", EditorAction.MoveWordRight);
            }
        }
    }

    public static class KeyDownEventEx
    {
        public static void GetImguiEvent(this KeyDownEvent target, Event outImguiEvent)
        {
            outImguiEvent.type = EventType.KeyDown;
            outImguiEvent.modifiers = target.modifiers;
            outImguiEvent.character = target.character;
            outImguiEvent.keyCode = target.keyCode;
        }
    }
}
