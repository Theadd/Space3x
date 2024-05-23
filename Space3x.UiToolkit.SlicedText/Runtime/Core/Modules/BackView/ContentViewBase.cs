using System;
using System.Collections.Generic;
using Space3x.UiToolkit.SlicedText.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Space3x.UiToolkit.SlicedText
{
    public abstract class ContentViewBase : VisualElement
    {
        public List<Line> Lines = new List<Line>();

        public List<int> DisplayedLines = new List<int>();
        protected int FirstLine;
        protected int LastLine;
        public int FirstLineIndex = 0;
        public int LastLineIndex = 0;

        public BackView ParentBackView { get; set; }

        protected bool HasMarginView = false;
        
        protected EditorTextView TextView;
        
        public VisualElement Container;

        public ContentViewBase(EditorTextView textView)
        {
            TextView = textView;
            pickingMode = PickingMode.Ignore;
            focusable = false;
            Lines.Add(new Line() { FormattedText = " ", RawLength = 1 });
            
            AddToClassList("sliced-editor__content-view");
            generateVisualContent += OnGenerateVisualContent;
        }

        protected abstract bool UpdateLineAt(int index, Line line, int elementIndex = -1, float y = -1f);

        /// <summary>
        /// Provides an estimation count of wrapped lines when WordWrap is enabled,
        /// line count otherwise.
        /// </summary>
        /// <param name="upToLineIndex"></param>
        /// <param name="fromLineIndex"></param>
        /// <returns></returns>
        public int GetRoughLineCount(int upToLineIndex = -1, int fromLineIndex = 0)
        {
            CounterGetRoughLineCount++;
            var count = upToLineIndex == -1 || upToLineIndex > Lines.Count ? Lines.Count : upToLineIndex;
            
            if (!ParentBackView.WordWrap)
                return count - fromLineIndex;

            var lineLength = ParentBackView.LineVisibleChars + 1;
            var baseline = ParentBackView.Baseline;
            var sum = 0;

            for (var i = fromLineIndex; i < count; i++)
            {
                var line = Lines[i];
                if (!line.Visible) continue;
                
                sum += line.LastBaseline != baseline
                    ? ((int) (line.RawLength / lineLength)) + 1
                    : line.Breakpoints.Count + 1;
            }

            return sum;
        }

        // TODO: DELETE
        public int CounterGetRoughLineIndexAt = 0;
        public int CounterGetRoughLineCount = 0;
        
        public (int lineIndex, int firstSubLineIndex) GetRoughLineIndexAt(int subLineIndex)
        {
            CounterGetRoughLineIndexAt++;
            
            if (!ParentBackView.WordWrap)
                return (subLineIndex, subLineIndex);
            
            var lineLength = ParentBackView.LineVisibleChars + 1;
            var baseline = ParentBackView.Baseline;
            var sum = 0;
            var acc = 0;
            var i = 0;
            var count = Lines.Count;

            for (; i < count; i++)
            {
                var line = Lines[i];
                if (!line.Visible) continue;
                
                acc = line.LastBaseline != baseline
                    ? ((int) (line.RawLength / lineLength)) + 1
                    : line.Breakpoints.Count + 1;

                if (sum <= subLineIndex && sum + acc > subLineIndex)
                    return (i, sum);

                sum += acc;
            }

            if (ParentBackView.ViewportHeight > 1f)
                Debug.LogError("Shouldn't reach this code.");
            
            return (count - 1, sum);
        }
        
        

        public new void Clear()
        {
            Lines.Clear();
            base.Clear();
        }

        public int IndexOf(ILine line)
        {
            var id = ((Line) line).Id;
            
            for (var i = 0; i < Lines.Count; i++)
                if (id == Lines[i].Id)
                    return i;

            return -1;
        }

        public new ILine ElementAt(int index)
        {
            return index < Lines.Count ? (ILine) Lines[index] : (ILine) null;
        }

        public TextElement TextElementAt(int index)
        {
            Debug.LogException(new NotImplementedException());

            return (TextElement) null;
        }

        public new void Insert(int index, VisualElement element)
        {
            Insert(index, ((TextElement) element).text);
        }

        public void Insert(int index, string formattedText)
        {
            var line = new Line()
            {
                FormattedText = formattedText,
                RawLength = TextView.Editor.SliceAt(index).Length
            };
            line.Id = Line.Counter++;
            
            Lines.Insert(index, line);
        }

        public void Replace(int index, string formattedText)
        {
            var line = Lines[index];
            
            //if (line.FormattedText.Length != formattedText.Length)
            //{
                line.RawLength = TextView.Editor.SliceAt(index).Length;
            //}
            line.FormattedText = formattedText;
            line.LastBaseline = 0;
            line.DisplayText = string.Empty;
            line.Breakpoints.Clear();

            if (UpdateLineAt(index, line))
            {
                ParentBackView.RequiresFullContentViewUpdate = true;
            }
        }

        public new void RemoveAt(int index)
        {
            Lines.RemoveAt(index);
        }
        
        // ReSharper disable once InconsistentNaming
        public new int childCount => Lines.Count;

        public float GetLineHeight(Line line, int lineIndex = -1)
        {
            if (line.LastBaseline != ParentBackView.Baseline)
                UpdateLineValues(line, lineIndex);
            
            return line.Height;
        }
        
        private void UpdateLineValues(Line line, int lineIndex = -1)
        {
            if (!ParentBackView.WordWrap)
            {
                line.Height = (int) ParentBackView.TextSizeY;
                line.DisplayText = line.FormattedText;
                line.LastBaseline = ParentBackView.Baseline;
            }
            else
            {
                ref var slice = ref TextView.Editor.SliceAt(lineIndex == -1 ? Lines.IndexOf(line) : lineIndex);
                var remaining = slice.ToString();
                var bv = ParentBackView;
                var lineLength = ParentBackView.LineVisibleChars;

                line.LastBaseline = ParentBackView.Baseline;
                line.RawLength = slice.Length;
                line.Breakpoints.Clear();

                if (remaining.Length <= lineLength)
                {
                    line.Height = (int) ParentBackView.TextSizeY;
                    line.DisplayText = line.FormattedText;
                    return;
                }
                
                var lastBreakWordPosInFormattedText = 0;
                var formattedTextLenght = line.FormattedText.Length;
                var numLines = 0;
                var lastBreakpoint = 0;
                line.DisplayText = string.Empty;

                while (remaining.Length > lineLength)
                {
                    var breakWordIndex = bv.UseHardWordWrapOnly ? -1 : remaining.LastIndexOf(' ', lineLength - ParentBackView.AdditionalLineWrapSpace);
                    var e = lastBreakWordPosInFormattedText;
                    var isHardBreak = false;
                    
                    if (breakWordIndex == -1)
                    {
                        breakWordIndex = lineLength - 1 - ParentBackView.AdditionalLineWrapSpace;
                        isHardBreak = true;

                        var match = IColorize.MatchCharacterInFormattedText(remaining,
                            line.FormattedText.Substring(lastBreakWordPosInFormattedText), breakWordIndex);

                        if (match == -1)
                        {
                            Debug.LogWarning("TODO: Break word match not found within line length\n" + remaining);
                            break;
                        }

                        e += match;
                    }
                    else
                    {
                        var breakWordCountAtIndex = 0;
                        for (var i = breakWordIndex; i >= 0; i--)
                        {
                            if (remaining[i] == ' ') breakWordCountAtIndex++;
                        }
                        
                        var breakCount = 0;
                        for (; e < formattedTextLenght; e++)
                        {
                            if (line.FormattedText[e] == ' ') breakCount++;
                            if (breakCount == breakWordCountAtIndex) break;
                        }
                    }
                    
                    line.DisplayText += line.FormattedText.Substring(lastBreakWordPosInFormattedText,
                        e - lastBreakWordPosInFormattedText + 1) + (isHardBreak
                        ? ParentBackView.HardWordWrapSeparator
                        : ParentBackView.SoftWrapSeparator);

                    lastBreakWordPosInFormattedText = e + 1;
                    remaining = remaining.Substring(breakWordIndex + 1);
                    lastBreakpoint += breakWordIndex + 1;
                    line.Breakpoints.Add(lastBreakpoint);
                    numLines++;
                }

                line.DisplayText += line.FormattedText.Substring(lastBreakWordPosInFormattedText,
                    formattedTextLenght - lastBreakWordPosInFormattedText);

                numLines++;
                line.Height = (int) ParentBackView.TextSizeY * numLines;
            }
        }
        
        #region GENERATE VISUAL CONTENT
        
        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Rect r = contentRect;
            if (r.width < 1f || r.height < 1f)
                return; // Skip rendering when too small.

            if (ParentBackView.WordWrap)
            {
                OnGenerateWrappedVisualContent(mgc, r);
                return;
            }
            
            var activeLineY = ParentBackView.LineToLocalPosition(TextView.ActiveLineIndex);
            var activeLineX = TextView.ActiveLinePosition * ParentBackView.TextSizeX;

            // Draw background rect of current active line 
            if (TextView.Multiline && (!TextView.Empty || TextView.HasFocus))
                mgc.PaintRectangle(
                    0, 
                    activeLineY, 
                    r.width,
                    activeLineY + ParentBackView.TextSizeY,
                    TextView.StyleAsLocked ? TextView.ActiveLineColorLocked : TextView.ActiveLineColor
                );
            
            // Draw selection if any
            if (TextView.IsSelectionActive)
                MeshDrawSelection(mgc, r);
            
            // Draw cursor caret
            if (TextView.HasFocus)
                mgc.PaintRectangle(
                    activeLineX, 
                    activeLineY, 
                    activeLineX + 1f,
                    activeLineY + ParentBackView.TextSizeY,
                    TextView.StyleAsLocked ? Color.clear : TextView.CursorColor
                );
        }

        protected virtual void MeshDrawSelection(MeshGenerationContext mgc, Rect r)
        {
            var (fromLine, fromColumn, toLine, toColumn) = TextView.ActiveSelection;
            var bv = ParentBackView;
            var textSizeY = bv.TextSizeY;

            for (var line = fromLine; line <= toLine; line++)
            {
                if (toLine - fromLine > 5 && line == fromLine + 1)
                {
                    // batch drawing
                    mgc.PaintRectangle(
                        0,
                        line * textSizeY,
                        r.width,
                        (toLine - 1) * textSizeY + textSizeY,
                        TextView.SelectionColor);
                    
                    line = toLine - 1;
                } 
                else
                {
                    mgc.PaintRectangle(
                        line != fromLine ? 0 : 0 + fromColumn * bv.TextSizeX,
                        line * textSizeY,
                        line != toLine ? r.width : 0 + toColumn * bv.TextSizeX,
                        line * textSizeY + textSizeY,
                        TextView.SelectionColor);
                }
            }
        }

        private void OnGenerateWrappedVisualContent(MeshGenerationContext mgc, Rect r)
        {
            var elementIndex = DisplayedLines.IndexOf(TextView.ActiveLineIndex);
            var activeLineY = 0;
            var line = Lines[TextView.ActiveLineIndex];
            
            if (elementIndex != -1)
            {
                activeLineY = (int) Container.ElementAt(elementIndex).transform.position.y;

                // Draw background rect of current active line 
                if (TextView.Multiline && (!TextView.Empty || TextView.HasFocus))
                    mgc.PaintRectangle(
                        -ParentBackView.AddToOffsetX, 
                        activeLineY, 
                        r.width,
                        activeLineY + GetLineHeight(line),
                        TextView.StyleAsLocked ? TextView.ActiveLineColorLocked : TextView.ActiveLineColor
                    );
            }
            
            // Draw selection if any
            if (TextView.IsSelectionActive)
            {
                MeshDrawWrappedSelection(mgc, r);
            }

            if (elementIndex != -1)
            {
                var (i, linePos) = GetWrappedLinePosition(line, TextView.ActiveLinePosition);
                var activeLineX = linePos * ParentBackView.TextSizeX;

                // Draw cursor caret
                if (TextView.HasFocus)
                    mgc.PaintRectangle(
                        activeLineX,
                        activeLineY + (i * ParentBackView.TextSizeY),
                        activeLineX + 1f,
                        (activeLineY + (i * ParentBackView.TextSizeY)) + ParentBackView.TextSizeY,
                        TextView.StyleAsLocked ? Color.clear : TextView.CursorColor
                    );
            }
        }
        
        protected virtual bool MeshDrawWrappedSelection(MeshGenerationContext mgc, Rect r)
        {
            var (fromLine, fromColumn, toLine, toColumn) = TextView.ActiveSelection;
            var bv = ParentBackView;
            var textSizeY = bv.TextSizeY;
            var stretchSelectionEnd = false;

            if (fromLine < FirstLineIndex)
            {
                if (toLine < FirstLineIndex) return false;
                fromColumn = 0;
                fromLine = FirstLineIndex;
            }

            if (toLine > LastLineIndex)
            {
                toLine = LastLineIndex;
                stretchSelectionEnd = true;
            }

            if (fromLine == toLine)
            {
                var line = Lines[fromLine];
                var (fromSubLine, fromSubColumn) = GetWrappedLinePosition(line, fromColumn);
                var (toSubLine, toSubColumn) = GetWrappedLinePosition(line, toColumn);
                var elementIndex = DisplayedLines.IndexOf(fromLine);
                if (elementIndex == -1) return false;
                    
                var fromY = (int) Container.ElementAt(elementIndex).transform.position.y;

                for (var subLine = fromSubLine; subLine <= toSubLine; subLine++)
                {
                    mgc.PaintRectangle(
                        subLine != fromSubLine ? 0 : fromSubColumn * bv.TextSizeX,
                        fromY + (subLine * textSizeY),
                        subLine != toSubLine || stretchSelectionEnd ? r.width : toSubColumn * bv.TextSizeX,
                        fromY + (subLine * textSizeY) + textSizeY,
                        TextView.SelectionColor);
                }
            }
            else
            {
                var (fromSubLine, fromSubColumn) = GetWrappedLinePosition(Lines[fromLine], fromColumn);
                var (toSubLine, toSubColumn) = GetWrappedLinePosition(Lines[toLine], toColumn);
                var elementIndex = DisplayedLines.IndexOf(fromLine);
                var toElementIndex = DisplayedLines.IndexOf(toLine);
                
                if (elementIndex == -1 || toElementIndex == -1) return false;
                    
                var fromY = (int) Container.ElementAt(elementIndex).transform.position.y + (fromSubLine * textSizeY);
                var toY = (int) Container.ElementAt(toElementIndex).transform.position.y + (toSubLine * textSizeY);
                
                mgc.PaintRectangle(
                    fromSubColumn * bv.TextSizeX,
                    fromY,
                    r.width,
                    fromY + textSizeY,
                    TextView.SelectionColor);
                
                mgc.PaintRectangle(
                    0,
                    toY,
                    stretchSelectionEnd ? r.width : toSubColumn * bv.TextSizeX,
                    toY + textSizeY,
                    TextView.SelectionColor);

                if (toY - (fromY + textSizeY) > 1f)
                {
                    mgc.PaintRectangle(
                        0,
                        fromY + textSizeY,
                        r.width,
                        toY,
                        TextView.SelectionColor);
                }

                
            }
            return true;
        }

        public (int subLine, int subLinePosition) GetWrappedLinePosition(Line line, int position)
        {
            var i = line.Breakpoints.Count - 1;
            var linePos = position;

            for (; i >= 0; i--)
            {
                if (line.Breakpoints[i] <= linePos)
                {
                    linePos -= line.Breakpoints[i];
                    break;
                }
            }

            i++;
            
            return (i, linePos);
        }
        
        #endregion GENERATE VISUAL CONTENT
    }
}
