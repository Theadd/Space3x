using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public class TextInputKeyboardEventHandler<TValueType>
    {
        private TextInputFieldBase<TValueType>.TextInputBase m_TextInputBase;
        private char m_HighSurrogate;
        private static Dictionary<Event, InputAction> s_Keyactions;
        private readonly Event m_ImguiEvent = new Event();

        public TextInputKeyboardEventHandler(TextInputFieldBase<TValueType>.TextInputBase textInputBase)
        {
            m_TextInputBase = textInputBase;
        }

        public void HandleKeyDownEvent(KeyDownEvent ev)
        {
            GetImguiEvent(ev, m_ImguiEvent);
            m_TextInputBase.textElement.CursorIndex = m_TextInputBase.textSelection.cursorIndex;
            
            if (HandleKeyEvent(m_ImguiEvent, m_TextInputBase.textEdition.isReadOnly))
            {
                Debug.Log("E0 " + m_TextInputBase.text + "; @ " + m_TextInputBase.textSelection.cursorIndex);
                m_TextInputBase.UpdateValueFromText();
                Debug.Log("E1 " + m_TextInputBase.text + "; @ " + m_TextInputBase.textSelection.cursorIndex);
                m_TextInputBase.textElement.MarkDirtyRepaint();
                Debug.Log("E2 " + m_TextInputBase.text + "; @ " + m_TextInputBase.textSelection.cursorIndex);
                ev.StopPropagation();
                ev.StopImmediatePropagation();
            }
            else
            {
                OnKeyDown(ev);
            }
        }
        
        private static void GetImguiEvent(KeyDownEvent target, Event outImguiEvent)
        {
            outImguiEvent.type = EventType.KeyDown;
            outImguiEvent.modifiers = target.modifiers;
            outImguiEvent.character = target.character;
            outImguiEvent.keyCode = target.keyCode;
        }

        public bool DeleteSelection()
        {
            var textSelection = m_TextInputBase.textSelection;
            if (textSelection.cursorIndex == textSelection.selectIndex)
                return false;
            if (textSelection.cursorIndex < textSelection.selectIndex)
            {
                m_TextInputBase.text = m_TextInputBase.text.Substring(0, textSelection.cursorIndex) +
                                       m_TextInputBase.text.Substring(textSelection.selectIndex,
                                           m_TextInputBase.text.Length - textSelection.selectIndex);
                textSelection.selectIndex = textSelection.cursorIndex;
            }
            else
            {
                m_TextInputBase.text = m_TextInputBase.text.Substring(0, textSelection.selectIndex) +
                                       m_TextInputBase.text.Substring(textSelection.cursorIndex,
                                           m_TextInputBase.text.Length - textSelection.cursorIndex);
                textSelection.cursorIndex = textSelection.selectIndex;
            }

            // this.m_TextSelectingUtility.ClearCursorPos();
            return true;
        }

        public void ReplaceSelection(string replace)
        {
            // this.RestoreCursorState();
            this.DeleteSelection();
            var textSelection = m_TextInputBase.textSelection;
            m_TextInputBase.text = m_TextInputBase.text.Insert(textSelection.cursorIndex, replace);
            // int num = textSelection.cursorIndex + (replace.Length + 0);
            // textSelection.cursorIndex = num;
            int num = m_TextInputBase.textElement.CursorIndex + (replace.Length + 0);
            m_TextInputBase.textElement.CursorIndex = num;
            textSelection.selectIndex = num;
            m_TextInputBase.textElement.MarkDirtyRepaint();
            // this.m_TextSelectingUtility.ClearCursorPos();
        }

        public bool Insert(char c)
        {
            if (char.IsHighSurrogate(c))
            {
                this.m_HighSurrogate = c;
                return false;
            }

            if (char.IsLowSurrogate(c))
            {
                this.ReplaceSelection(new string(new char[2]
                {
                    this.m_HighSurrogate,
                    c
                }).ToString());
                return true;
            }

            ReplaceSelection(c.ToString());
            return true;
        }

        public bool Insert(string s)
        {
            ReplaceSelection(s);
            return true;
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            char c = evt.character;
            if (evt.actionKey && (!evt.altKey || c == char.MinValue) || c == '\t' && evt.keyCode == KeyCode.None &&
                evt.modifiers == EventModifiers.None)
                return;
            if (evt.keyCode == KeyCode.Tab || evt.keyCode == KeyCode.Tab && evt.character == '\t' &&
                evt.modifiers == EventModifiers.Shift)
            {
                // TODO:
                // if (!m_TextInputBase.Multiline || evt.shiftKey)
                // {
                //     if (!evt.ShouldSendNavigationMoveEvent())
                //         return;
                //     m_TextInputBase.textElement.focusController.FocusNextInDirection((Focusable)this.textElement,
                //         evt.shiftKey
                //             ? VisualElementFocusChangeDirection.left
                //             : VisualElementFocusChangeDirection.right);
                //     evt.StopPropagation();
                //     return;
                // }
                //
                // if (!evt.ShouldSendNavigationMoveEvent())
                //     return;
            }

            if (!m_TextInputBase.Multiline &&
                (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return))
            {
                // Action updateValueFromText = this.textElement.edition.UpdateValueFromText;
                // if (updateValueFromText != null)
                    m_TextInputBase.UpdateValueFromText();
            }

            evt.StopPropagation();
            if (m_TextInputBase.Multiline
                    ? c == '\n' && evt.shiftKey
                    : (c == '\n' || c == '\r' || c == '\n') && !evt.altKey)
            {
                // Action focusToCompositeRoot = this.textElement.edition.MoveFocusToCompositeRoot;
                // if (focusToCompositeRoot == null)
                //     return;
                m_TextInputBase.MoveFocusToCompositeRoot();
                return;
            }

            if (evt.keyCode == KeyCode.Escape)
            {
                // TODO: this.textElement.edition.RestoreValueAndText();
                /* Workaround for line above. */
                m_TextInputBase.text = m_TextInputBase.originalText;
                // Action updateValueFromText = this.textElement.edition.UpdateValueFromText;
                // if (updateValueFromText != null)
                    m_TextInputBase.UpdateValueFromText();
                // Action focusToCompositeRoot = this.textElement.edition.MoveFocusToCompositeRoot;
                // if (focusToCompositeRoot != null)
                    m_TextInputBase.MoveFocusToCompositeRoot();
            }

            if (evt.keyCode == KeyCode.Tab)
                c = '\t';
            if (!m_TextInputBase.AcceptCharacter(c))
                return;
            if (c >= ' ' || evt.keyCode == KeyCode.Tab || m_TextInputBase.Multiline && !evt.altKey &&
                (c == '\n' || c == '\r' || c == '\n'))
            {
                // var textSelection = m_TextInputBase.textSelection;
                // var pos = -1;
                // if (textSelection.cursorIndex == textSelection.selectIndex &&
                //     textSelection.cursorIndex == m_TextInputBase.text.Length)
                // {
                //     pos = m_TextInputBase.text.Length + 1;
                // }
                // DebugLog.Info("Inserting: " + c);
                Insert(c);
                m_TextInputBase.UpdateValueFromText();
                // if (pos != -1)
                // {
                //     // m_TextInputBase.textSelection.selectIndex = pos;
                //     // m_TextInputBase.textSelection.cursorIndex = pos;
                //     // m_TextInputBase.textSelection.selectIndex = pos;
                //     // m_TextInputBase.textSelection.cursorIndex = pos;
                //     // PerformOperation(InputAction.MoveRight, false);
                //     if (m_TextInputBase.textElement is ITextValueElement textValueElement)
                //     {
                //         textValueElement.CursorIndex = pos;
                //     }
                //     Debug.Log("PATCHING TO " + pos + " :: " + m_TextInputBase.textElement.CursorIndex + " :: " + m_TextInputBase.text.Length);
                // }
                m_TextInputBase.textElement.MarkDirtyRepaint();
                Debug.Log("B " + m_TextInputBase.text + "; @ " + m_TextInputBase.textElement.CursorIndex);
                evt.StopPropagation();
                evt.StopImmediatePropagation();
            }
            else
            {
                // DebugLog.Warning("[TextInputKeyboardEventHandler] Unhandled character: " + evt.keyCode + "; (int): " + ((int)evt.keyCode).ToString() + "; (char): " + c);
            }

            m_TextInputBase.UpdateScrollOffset(evt.keyCode == KeyCode.Backspace);
        }
        
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

        public virtual void PerformOperation(InputAction operation, bool textIsReadOnly)
        {
            var textSelection = m_TextInputBase.textSelection;
            TextValueElement element = m_TextInputBase.textElement;
            int pos = 0;
            switch (operation)
            {
                case InputAction.MoveLeft:
                    if (element.CursorIndex != textSelection.selectIndex) // -> HasSelection
                        pos = Math.Min(textSelection.selectIndex, element.CursorIndex);
                    else
                        pos = Math.Max(element.CursorIndex - 1, 0);
                    element.CursorIndex = pos;
                    textSelection.selectIndex = pos;
                    break;
                case InputAction.MoveRight:
                    if (element.CursorIndex != textSelection.selectIndex) // -> HasSelection
                        pos = Math.Max(textSelection.selectIndex, element.CursorIndex);
                    else
                        pos = Math.Min(element.CursorIndex + 1, m_TextInputBase.text.Length);
                    element.CursorIndex = pos;
                    textSelection.selectIndex = pos;
                    break;
                case InputAction.SelectLeft:
                    pos = Math.Max(element.CursorIndex - 1, 0);
                    element.CursorIndex = pos;
                    break;
                case InputAction.SelectRight:
                    pos = Math.Min(element.CursorIndex + 1, m_TextInputBase.text.Length);
                    element.CursorIndex = pos;
                    break;
                case InputAction.MoveLineStart:
                    element.CursorIndex = 0;
                    textSelection.selectIndex = 0;
                    break;
                case InputAction.MoveLineEnd:
                    element.CursorIndex = m_TextInputBase.text.Length;
                    textSelection.selectIndex = m_TextInputBase.text.Length;
                    break;
                case InputAction.SelectLineStart:
                    element.CursorIndex = 0;
                    break;
                case InputAction.SelectLineEnd:
                    element.CursorIndex = m_TextInputBase.text.Length;
                    break;
                case InputAction.MoveWordLeft:
                    pos = GetNextWordPosition(element.CursorIndex, -1);
                    element.CursorIndex = pos;
                    textSelection.selectIndex = pos;
                    break;
                case InputAction.MoveWordRight:
                    pos = GetNextWordPosition(element.CursorIndex, 1);
                    element.CursorIndex = pos;
                    textSelection.selectIndex = pos;
                    break;
                case InputAction.SelectWordLeft:
                    pos = GetNextWordPosition(element.CursorIndex, -1);
                    element.CursorIndex = pos;
                    break;
                case InputAction.SelectWordRight:
                    pos = GetNextWordPosition(element.CursorIndex, 1);
                    element.CursorIndex = pos;
                    break;
                case InputAction.Delete:
                    if (!textIsReadOnly)
                    {
                        if (element.CursorIndex == textSelection.selectIndex) // -> NOT HasSelection
                            PerformOperation(InputAction.SelectRight, false);
                        Insert("");
                    }
                    break;
                case InputAction.Backspace:
                    if (!textIsReadOnly)
                    {
                        if (element.CursorIndex == textSelection.selectIndex) // -> NOT HasSelection
                            PerformOperation(InputAction.SelectLeft, false);
                        Insert("");
                    }
                    break;
                case InputAction.SelectAll:
                    textSelection.SelectAll();
                    break;
                case InputAction.SelectNone:
                    textSelection.SelectNone();
                    break;
                case InputAction.EnterKey:
                case InputAction.ReturnKey:
                    if (!textIsReadOnly && m_TextInputBase.Multiline)
                        Insert("\n");
                    break;
                case InputAction.Paste:
                    if (!textIsReadOnly)
                        Insert(GUIUtility.systemCopyBuffer ?? "");
                    break;
                case InputAction.Copy:
                    if (element.CursorIndex == textSelection.selectIndex) // -> NOT HasSelection
                    {
                        PerformOperation(InputAction.MoveLineEnd, textIsReadOnly);
                        PerformOperation(InputAction.SelectLineStart, textIsReadOnly);
                    }
                    GUIUtility.systemCopyBuffer = m_TextInputBase.text.Substring(
                        Math.Min(element.CursorIndex, textSelection.selectIndex),
                        Math.Max(element.CursorIndex, textSelection.selectIndex) -
                        Math.Min(element.CursorIndex, textSelection.selectIndex)
                    );
                    break;
                case InputAction.Cut:
                    PerformOperation(InputAction.Copy, textIsReadOnly);
                    PerformOperation(InputAction.Delete, textIsReadOnly);
                    break;
                default:
                    Debug.LogWarning((object) ("Not implemented: " + operation.ToString()));
                    break;
            }
            
        }

        private static void MapKey(string key, InputAction action) => s_Keyactions[Event.KeyboardEvent(key)] = action;

        private void InitKeyActions()
        {
            if (s_Keyactions != null) return;
            // # => SHIFT
            // % => CMD
            // & => ALT
            // ^ => CTRL
            s_Keyactions = new Dictionary<Event, InputAction>();
            MapKey("left", InputAction.MoveLeft);
            MapKey("right", InputAction.MoveRight);
            // MapKey("up", InputAction.MoveUp);
            // MapKey("down", InputAction.MoveDown);
            MapKey("#left", InputAction.SelectLeft);
            MapKey("#right", InputAction.SelectRight);
            // MapKey("#up", InputAction.SelectUp);
            // MapKey("#down", InputAction.SelectDown);
            MapKey("delete", InputAction.Delete);
            MapKey("backspace", InputAction.Backspace);
            MapKey("#backspace", InputAction.Backspace);
            MapKey("[esc]", InputAction.SelectNone);
            // MapKey("pgup", InputAction.MovePageUp);
            // MapKey("#pgup", InputAction.SelectPageUp);
            // MapKey("pgdown", InputAction.MovePageDown);
            // MapKey("#pgdown", InputAction.SelectPageDown);
            MapKey("home", InputAction.MoveLineStart);
            MapKey("#home", InputAction.SelectLineStart);
            MapKey("end", InputAction.MoveLineEnd);
            MapKey("#end", InputAction.SelectLineEnd);
            // MapKey("tab", InputAction.AddIndent);
            // MapKey("#tab", InputAction.RemoveIndent);
            MapKey("[enter]", InputAction.EnterKey);
            MapKey("return", InputAction.ReturnKey);

            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            {
                MapKey("%x", InputAction.Cut);
                MapKey("%c", InputAction.Copy);
                MapKey("%v", InputAction.Paste);
                MapKey("^d", InputAction.Delete);
                MapKey("^h", InputAction.Backspace);
                MapKey("%a", InputAction.SelectAll);
                // MapKey("%z", InputAction.Undo);
                // MapKey("#%z", InputAction.Redo);
                // MapKey("%y", InputAction.Redo);
                MapKey("#&left", InputAction.SelectWordLeft);
                MapKey("#&right", InputAction.SelectWordRight);
                MapKey("&left", InputAction.MoveWordLeft);
                MapKey("&right", InputAction.MoveWordRight);
            }
            else
            {
                MapKey("^x", InputAction.Cut);
                MapKey("^c", InputAction.Copy);
                MapKey("^v", InputAction.Paste);
                MapKey("^a", InputAction.SelectAll);
                // MapKey("^z", InputAction.Undo);
                // MapKey("#^z", InputAction.Redo);
                // MapKey("^y", InputAction.Redo);
                MapKey("#^left", InputAction.SelectWordLeft);
                MapKey("#^right", InputAction.SelectWordRight);
                MapKey("^left", InputAction.MoveWordLeft);
                MapKey("^right", InputAction.MoveWordRight);
            }
        }
        
        private int GetNextWordPosition(int fromPosition, int direction)
        {
            if (direction != -1 && direction != 1)
                throw new ArgumentOutOfRangeException(nameof(direction), direction,
                    "Must be -1 to look backwards or 1 for forwards.");

            var text = m_TextInputBase.text;
            var startPosition = 0;
            var endPosition = text.Length;
            var lastCharType = -1;
            var charType = -1;
            var pos = direction == -1 ? fromPosition - 1 : fromPosition;

            for (; pos <= endPosition && pos >= startPosition; pos += direction)
            {
                char c = '\0';
                if (pos < text.Length)
                    c = text[pos];

                if (c == ' ') charType = 0;
                else if (c == '\n') charType = 1;
                else if (char.IsLetterOrDigit(c)) charType = 2;
                else charType = 3;

                if (lastCharType >= 0)
                {
                    if (lastCharType != charType && !(lastCharType == 0 && charType > 0))
                        break;
                }

                lastCharType = charType;
            }

            if (m_TextInputBase.Multiline && direction == -1 && fromPosition == pos + 1 && pos >= 0)
                pos -= 1;

            if (direction == 1 && pos > endPosition && fromPosition < endPosition - 1)
                pos = endPosition - 1;

            return direction == -1 ? pos + 1 : pos;
        }
    }
}
