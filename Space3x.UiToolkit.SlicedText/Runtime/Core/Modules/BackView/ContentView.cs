using System.Collections.Generic;
using System.Linq;
using Space3x.UiToolkit.SlicedText.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Space3x.UiToolkit.SlicedText
{
    public class ContentView : ContentViewBase
    {
        public ContentView(EditorTextView textView) : base(textView)
        {
            Container = ((VisualElement) this);
            Container.Add(new TextLine() { text = " " });
        }

        public void SetFirstLine(int elementIndex)
        {
            FirstLine = elementIndex;
            LastLine = elementIndex == 0 ? DisplayedLines.Count - 1 : elementIndex - 1;
            var nextIndex = elementIndex == DisplayedLines.Count - 1 ? 0 : elementIndex + 1;
            
            ParentBackView.ScrollOffsetPrevY = (int) Container.ElementAt(elementIndex).transform.position.y;
            ParentBackView.ScrollOffsetNextY = (int) Container.ElementAt(nextIndex).transform.position.y;
            
        }

        public void UpdateLineIndexRange()
        {
            FirstLineIndex = DisplayedLines[FirstLine];

            var prevLastLine = LastLine;

            while (DisplayedLines[prevLastLine] == -1 && prevLastLine != FirstLine)
                prevLastLine = prevLastLine == 0 ? DisplayedLines.Count - 1 : prevLastLine - 1;

            // prevLastLine = prevLastLine != FirstLine ? prevLastLine : LastLine;
            LastLineIndex = DisplayedLines[prevLastLine];

            if (FirstLineIndex == -1 && LastLineIndex == -1)
            {
                FirstLineIndex = 0;
                LastLineIndex = 0;
                DisplayedLines[FirstLine] = 0;
            }
        }

        public int CounterFullUpdate = 0;
        public void FullUpdate()
        {
            CounterFullUpdate++;
            
            var visibleLines = ParentBackView.MaxVisibleLines + 1;

            // Debug.Log("FullUpdate(" + Container.childCount + " => " + visibleLines + $"), FirstSliceIndex: {ParentBackView.SliceIndexOfFirstLine} @ {ParentBackView.FirstLineOffsetY}px");
            
            while (Container.childCount != visibleLines)
            {
                if (Container.childCount < visibleLines)
                    Container.Add(new TextLine() { text = string.Empty });
                else
                    Container.RemoveAt(Container.childCount - 1);
            }

            DisplayedLines = new List<int>(Enumerable.Repeat(-1, visibleLines));
            HasMarginView = ParentBackView.ShowViewMargin && ParentBackView.MarginView != null;
            
            if (HasMarginView)
                ParentBackView.MarginView.Rebuild(visibleLines);

            var y = ParentBackView.FirstLineOffsetY;
            var i = ParentBackView.SliceIndexOfFirstLine;
            var displayedLines = 0;

            while (displayedLines < visibleLines)
            {
                if (i < Lines.Count)
                {
                    var line = Lines[i];

                    if (line.Visible)
                    {
                        var lineHeight = GetLineHeight(line);
                        
                        UpdateLineAt(i, line, displayedLines, y);
                        
                        DisplayedLines[displayedLines] = i;
                        // LastLineIndex = i;
                        y += lineHeight;
                        displayedLines++;
                    }
                }
                else
                {
                    var element = (TextLine) Container.ElementAt(displayedLines);
                    
                    element.style.display = DisplayStyle.None;
                    DisplayedLines[displayedLines] = -1;
                    
                    if (HasMarginView)
                        ParentBackView.MarginView.Update(displayedLines);
                    
                    displayedLines++;
                }

                i++;
            }

            SetFirstLine(0);
            UpdateLineIndexRange();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="line"></param>
        /// <param name="elementIndex"></param>
        /// <param name="y"></param>
        /// <returns>true if full update required.</returns>
        protected override bool UpdateLineAt(int index, Line line, int elementIndex = -1, float y = -1f)
        {
            elementIndex = elementIndex == -1 ? DisplayedLines.IndexOf(index) : elementIndex;
            if (elementIndex == -1) return false;
            
            var lineHeight = GetLineHeight(line);
            var element = (TextLine) Container.ElementAt(elementIndex);
            var updateRequired = element.style.height.value.value != lineHeight;
            element.style.height = lineHeight;
            element.text = line.DisplayText;
            element.style.display = DisplayStyle.Flex;
            element.style.position = Position.Absolute;
            element.style.top = 0;
            element.style.left = 0;
            y = y == -1f ? element.transform.position.y : y;
            element.transform.position = new Vector3(0, y, 0);

            if (HasMarginView)
                ParentBackView.MarginView.Update(elementIndex, index, line, y);

            return updateRequired;
        }

        private int m_LastScrollSyncY = 0;

        public void ScrollSync()
        {
            if (DisplayedLines.Count == 0) return;
            var bv = ParentBackView;

            if (bv.RequiresFullContentViewUpdate)
            {
                Debug.LogError($"[FIXME] Performing a line by line update when a full content view update is already required.");
            }
            
            m_LastScrollSyncY = -bv.ScrollOffsetY;
            ParentBackView.DebugHere($"ScrollToY->ScrollSync({m_LastScrollSyncY})");
            
            if (bv.ScrollOffsetNextY < -bv.ScrollOffsetY)
            {
                // Scroll down
                while (bv.ScrollOffsetNextY < -bv.ScrollOffsetY)
                {
                    var prevLastLine = LastLine;

                    while (DisplayedLines[prevLastLine] == -1)
                        prevLastLine = prevLastLine == 0 ? DisplayedLines.Count - 1 : prevLastLine - 1;

                    var nextLineIndex = DisplayedLines[prevLastLine] + 1;

                    if (nextLineIndex >= Lines.Count)
                        break;

                    var nextLine = Lines[nextLineIndex];

                    while (nextLine.Visible == false)
                    {
                        nextLineIndex++;
                        if (nextLineIndex >= Lines.Count) break;
                        nextLine = Lines[nextLineIndex];
                    }

                    SetFirstLine(FirstLine == DisplayedLines.Count - 1 ? 0 : FirstLine + 1);
                    bv.FirstLineOffsetY = bv.ScrollOffsetPrevY;
                    bv.SliceIndexOfFirstLine = DisplayedLines[FirstLine];

                    var lastElement = (TextLine) Container.ElementAt(prevLastLine);
                    var nextY = lastElement.style.height.value.value + lastElement.transform.position.y;
                    var elementIndexToUpdate = prevLastLine == DisplayedLines.Count - 1 ? 0 : prevLastLine + 1;

                    UpdateLineAt(nextLineIndex, nextLine, elementIndexToUpdate, nextY);
                    DisplayedLines[elementIndexToUpdate] = nextLineIndex;
                }
                UpdateLineIndexRange();
            }
            else if (bv.ScrollOffsetPrevY > -bv.ScrollOffsetY)
            {
                // Scroll up
                while (bv.ScrollOffsetPrevY > -bv.ScrollOffsetY)
                {
                    var nextLineIndex = DisplayedLines[FirstLine] - 1;
                    if (nextLineIndex < 0) break;
                    var nextLine = Lines[nextLineIndex];

                    while (nextLine.Visible == false)
                    {
                        nextLineIndex--;
                        if (nextLineIndex < 0) break;
                        nextLine = Lines[nextLineIndex];
                    }
                    
                    var firstElement = (TextLine) Container.ElementAt(FirstLine);
                    
                    var nextY = firstElement.transform.position.y - GetLineHeight(nextLine);

                    UpdateLineAt(nextLineIndex, nextLine, LastLine, nextY);
                    DisplayedLines[LastLine] = nextLineIndex;

                    SetFirstLine(LastLine);
                    bv.FirstLineOffsetY = nextY;
                    bv.SliceIndexOfFirstLine = nextLineIndex;
                }
                UpdateLineIndexRange();
            }
        }
    }
}
