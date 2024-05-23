using System;
using System.Linq;
using Space3x.UiToolkit.SlicedText.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Space3x.UiToolkit.SlicedText
{
    public class BackView : ViewOffsets
    {
        public ContentView ContentView
        {
            get => MContentView;
            protected set
            {
                if (value == MContentView) 
                    return;
                
                if (MContentView != null && hierarchy.IndexOf(MContentView) >= 0)
                    hierarchy.Remove(MContentView);
                
                MContentView = value;
                MContentView.ParentBackView = this;
                hierarchy.Add(MContentView);
            }
        }

        public bool ShowVerticalScroller { get; set; } = true;
        
        public bool ShowViewMargin { get; set; } = true;

        public string SoftWrapSeparator { get; set; } = "<br>";
        
        /// <summary>
        /// RichText string used to hard break lines that had no
        /// available space character within length limits. (Not
        /// a hard return, but wrapps lines without respecting
        /// word boundaries).
        /// </summary>
        public string HardWordWrapSeparator { get; set; } = "<br>";

        /// <summary>
        /// Additional spaces (in characters) to leave for custom
        /// word wrap separators.
        /// </summary>
        public int AdditionalLineWrapSpace { get; set; } = 0;

        /// <summary>
        /// If enabled and WordWrap is also enabled, it will wrap
        /// all lines at the same character position instead of
        /// trying to use soft-wrap first.
        /// </summary>
        public bool UseHardWordWrapOnly { get; set; } = false;

        public BackView(EditorTextView editorView) : base(editorView)
        {
            pickingMode = PickingMode.Ignore;
            focusable = false;
            
            ContentView = new ContentView(editorView)
            {
                style =
                {
                    position = Position.Absolute,
                    top = 0,
                    left = 0
                }
            };
            
            editorView.RegisterCallback<GeometryChangedEvent>(OnEditorViewGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            
            AddToClassList("sliced-editor__back-view");
        }

        private bool m_RequestedTextSizeMeasure = false;
        
        private void OnEditorViewGeometryChanged(GeometryChangedEvent evt)
        {
            // Debug.Log("<color=#AAAA33FF>OnEditorViewGeometryChanged</color>");
            
            if (evt.oldRect.size == evt.newRect.size)
                return;

            Sync();
            
            if (!m_RequestedTextSizeMeasure)
            {
                m_RequestedTextSizeMeasure = true;
                ((SlicedTextEditor) EditorView).UpdateTextSizeMeasure();
            }
        }

        public override void Sync()
        {
            BaseOffsetX = (ShowViewMargin && MarginView != null) ? (int) MarginView.layout.width : 0;
            
            base.Sync();
            
            if (HasActiveVerticalScroller)
            {
                // When the user resizes the viewport, absolute positioned views
                // need an update, using those redundant scroller calls, prevents
                // empty views and flickering when WordWrap is active.
                // When there's no WordWrap, refreshing view positions is enough.
                if (MWordWrap)
                {
                    VerticalScroller.value += 1f;
                    VerticalScroller.value -= 1f;
                }
                else ScrollToY(-ScrollOffsetY);
            }
            
            if (LineVisibleChars > 0)
                SyncWithContentView();
            AdjustBackScreenSize();
        }

        protected void SyncWithContentView()
        {
            if (SkipFullContentViewUpdate) return;
            
            if (MaxVisibleLines != Mathf.CeilToInt(ViewportHeight / TextSizeY))
                RequiresFullContentViewUpdate = true;

            if (!RequiresFullContentViewUpdate && HasActiveVerticalScroller && ScrollOffsetY == 0 &&
                SliceIndexOfFirstLine != 0)
                RequiresFullContentViewUpdate = true;
            
            if (!RequiresFullContentViewUpdate && SliceIndexOfFirstLine <= 30 && SliceIndexOfFirstLine % 5 == 0)
                if (FirstLineOffsetY != MContentView.GetRoughLineCount(SliceIndexOfFirstLine) * TextSizeY)
                    RequiresFullContentViewUpdate = true;
            
            DebugHere("SyncWithContentView FullUpdate: " + (RequiresFullContentViewUpdate));

            if (RequiresFullContentViewUpdate)
            {
                MaxVisibleLines = Mathf.CeilToInt(ViewportHeight / TextSizeY);
                Update();
            }
            else
            {
                AdjustScroll();
            }
        }

        #region PANEL EVENTS
        
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel == null)
                return;


            if (ShowVerticalScroller && (VerticalScroller?.parent == null))
            {
                EditorView.hierarchy.Add(VerticalScroller ?? AddVerticalScroller());
                EditorView.UnregisterCallback<WheelEvent>(OnScrollWheel);
                EditorView.RegisterCallback<WheelEvent>(OnScrollWheel);
            }

            if (ShowViewMargin && MarginView == null)
            {
                MarginView = new ViewMargin()
                {
                    style =
                    {
                        position = Position.Absolute,
                        top = 0,
                        left = 0
                    }
                };
                hierarchy.Add(MarginView);
            }

            #if UNITY_EDITOR
            var isBuilderPreview = (EditorView.parent?.GetClasses().FirstOrDefault() ?? "")
                .StartsWith("unity-builder-tooltip-preview");

            if (isBuilderPreview)
            {
                if (MarginView != null)
                {
                    MarginView.style.position = Position.Absolute;
                    MarginView.style.top = 0;
                    MarginView.style.left = 0;
                    MarginView.style.width = 50f;
                    MarginView.style.backgroundColor = new StyleColor(new Color32(40, 40, 40, 255));
                    MarginView.style.borderRightColor = new StyleColor(new Color32(53, 53, 52, 255));
                    MarginView.style.borderRightWidth = 1;
                    MarginView.style.borderLeftWidth = 8;
                    MarginView.style.color = new StyleColor(new Color32(154, 154, 154, 183));
                }

                if (VerticalScroller != null)
                {
                    VerticalScroller.style.position = Position.Absolute;
                    VerticalScroller.style.top = 0;
                    VerticalScroller.style.bottom = 0;
                    VerticalScroller.style.right = 0;
                }
                MContentView.style.position = Position.Absolute;
                MContentView.style.top = 0;
                MContentView.style.left = 0;
                EditorView.style.backgroundColor = new StyleColor(new Color32(38, 38, 38, 255));
                EditorView.style.maxHeight = 235f;
                EditorView.style.fontSize = 12f;
                EditorView.style.overflow = Overflow.Hidden;
            }
            #endif
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel == null)
                return;

            if (EditorView.hierarchy.IndexOf(VerticalScroller) != -1)
            {
                VerticalScroller.RemoveFromHierarchy();
                VerticalScroller = null;
            }
        }
        
        #endregion PANEL EVENTS
        
        protected void AdjustBackScreenSize()
        {
            var oldValue = BackScreenHeight;
            
            if (HasActiveVerticalScroller)
            {
                BackScreenHeight = ((int) ((VerticalScroller.highValue + ViewportHeight) / 500) + 1) * 500 + 100;
            }
            else
            {
                BackScreenHeight = Mathf.Floor(ViewportHeight * 2);
            }
            
            MContentView.style.width = ViewportWidth - OffsetX;
            MContentView.style.marginLeft = OffsetX;

            if ((int) oldValue == (int) BackScreenHeight) return;
            
            MContentView.style.height = BackScreenHeight;
            
            if (MarginView != null)
                MarginView.style.height = BackScreenHeight;

            DebugHere($"AdjustBackScreenSize {oldValue} => " + ((int) BackScreenHeight));
        }
        
        #region SCROLL
        
        protected override void AdjustScroll()
        {
            if (ShowVerticalScroller && VerticalScroller != null)
            {
                var highValue = 0f;
                var lastElementIndex = MContentView.DisplayedLines.IndexOf(MContentView.LastLineIndex);
                var lineCount = MContentView.Lines.Count;
                
                if (lastElementIndex != -1 && MContentView.LastLineIndex < lineCount)
                {
                    highValue = (int) MContentView.Container.ElementAt(lastElementIndex).transform.position.y +
                                (int) MContentView.GetLineHeight(MContentView.Lines[MContentView.LastLineIndex]);

                    if (MContentView.LastLineIndex < lineCount - 1)
                    {
                        highValue += MContentView.GetRoughLineCount(-1, MContentView.LastLineIndex + 1) * TextSizeY;
                    }
                }
                else
                {
                    EstimatedLineCount = MContentView.GetRoughLineCount();
                    highValue = EstimatedLineCount * TextSizeY;
                }

                var minHeight = (lineCount > 1) ? highValue : TextSizeY;

                if (minHeight != resolvedStyle.minHeight.value)
                    style.minHeight = minHeight;
                
                highValue = highValue < ViewportHeight ? 0 : highValue - ViewportHeight;

                VerticalScroller.lowValue = 0;
                VerticalScroller.highValue = highValue + (highValue == 0 ? ViewportHeight : 0);
                VerticalScroller.slider.pageSize = TextSizeY * ((int) (MaxVisibleLines / 2));
                var factor = ViewportHeight / (highValue + ViewportHeight);

                if (HasActiveVerticalScroller && factor >= 1f)
                    VerticalScroller.value = 0;
                
                HasActiveVerticalScroller = factor < 1f;
                VerticalScroller.Adjust(factor);
            }
        }
        
        public virtual void ScrollToY(float y)
        {
            // TODO: Move BaseOffsetY and BaseLineIndexOffset to keep FirstLineOffsetY between
            // TODO  2-4 * ViewportHeight (since BackScreenHeight value).

            ScrollOffsetY = (int) -y;
            DebugHere($"ScrollToY({y})");
            var pos = new Vector3(ScrollOffsetX, ScrollOffsetY, 0);

            MContentView.transform.position = pos;
            if (MarginView != null)
                MarginView.transform.position = pos;
            
            if (ScrollOffsetPrevY > -ScrollOffsetY || ScrollOffsetNextY < -ScrollOffsetY)
            {
                MContentView.ScrollSync();
                SyncWithContentView();
                AdjustBackScreenSize();
            }
            
            if (EditorView.IsSelectionActive)
                MContentView.MarkDirtyRepaint();
        }

        protected int ScrollJumpLowerLimitY = 400;
        protected int LineByLineScrollLimit => (int) TextSizeY * 11;

        void VerticalScrollChanged(float value)
        {
            var deltaY = Mathf.Abs(value + ScrollOffsetY);
            var byLineLimit = LineByLineScrollLimit;

            // scrolling to a near position, no jumping.
            if (deltaY < byLineLimit)
            {
                ScrollToY(value);
                return;
            }
            var (lineIndex, firstSubLineIndex) = MContentView.GetRoughLineIndexAt((int) (value / TextSizeY));
            
            var isScrollJump = MakeFullScrollJump(lineIndex, firstSubLineIndex);
            
            ScrollToY(value);
        }

        protected virtual Scroller AddVerticalScroller()
        {
            VerticalScroller = new Scroller(
                0, 
                int.MaxValue,
                VerticalScrollChanged, 
                SliderDirection.Vertical)
            {
                viewDataKey = "VScrollKey", 
                visible = true
            };
            
            VerticalScroller.AddToClassList("sliced-editor__scroller");

            return VerticalScroller;
        }
        
        protected ValueAnimation<float> VScrollAnim;
        
        public int AnimScrollDurationMs { get; set; } = 220;

        public float ScrollPageMultiplier { get; set; } = 2f;
        
        public float ScrollAcceleration { get; set; } = 1.7f;

        private float m_VScrollValueToAdd = 0f;
        private float m_VScrollValueAdded = 0f;
        private int m_VScrollDirection = 1;

        protected void OnVScrollAnim(VisualElement e, float v)
        {
            VerticalScroller.value += (v - m_VScrollValueAdded) * m_VScrollDirection;
            m_VScrollValueAdded = v;
        }
        
        private void OnScrollWheel(WheelEvent evt)
        {
            if (HasActiveVerticalScroller && evt.modifiers == EventModifiers.None)
            {
                // var vScrollValue = verticalScroller.value;
                var deltaY = Math.Abs(evt.delta.y);
                var direction = (int) (evt.delta.y / deltaY);
                var increment = ScrollPageMultiplier * TextSizeY * deltaY;

                if (VScrollAnim == null)
                {
                    VScrollAnim = VerticalScroller.experimental.animation.Start(
                        0f,
                        increment,
                        AnimScrollDurationMs,
                        OnVScrollAnim).Ease(Easing.OutCubic).KeepAlive();
                    VScrollAnim.Stop();
                }

                var rest = m_VScrollValueToAdd - m_VScrollValueAdded;
                
                m_VScrollValueToAdd = m_VScrollDirection == direction
                    ? ((m_VScrollValueToAdd - m_VScrollValueAdded) * ScrollAcceleration) + increment
                    : increment;
                m_VScrollValueAdded = 0;

                if (VScrollAnim.isRunning)
                    VScrollAnim.Stop();
                
                m_VScrollDirection = direction;

                VScrollAnim.to = m_VScrollValueToAdd;
                VScrollAnim.from = 0;

                VScrollAnim.Start();

                // if (verticalScroller.value != vScrollValue)
                // {
                    evt.StopPropagation();
                // }
            }
        }

        private int m_ActiveLineIndex = -1;
        
        public virtual void UpdateScrollOffset()
        {
            if (HasActiveVerticalScroller && m_ActiveLineIndex != EditorView.ActiveLineIndex)
            {
                m_ActiveLineIndex = EditorView.ActiveLineIndex;
                
                var line = MContentView.Lines[m_ActiveLineIndex];

                if (!line.Visible) return;
                
                var elementIndex = MContentView.DisplayedLines.IndexOf(m_ActiveLineIndex);
                var elementY = 0;
                // var isScrollJump = false;

                var estimatedWrapLine = -1;
                
                // workaround to allow FullScrollJump to call a nested UpdateScrollOffset
                var savedActiveLineIndex = m_ActiveLineIndex;
                // activeLineIndex = -1;
                
                if (elementIndex != -1)
                {
                    elementY = (int) MContentView.Container.ElementAt(elementIndex).transform.position.y;
                }
                else
                {
                    estimatedWrapLine = MContentView.GetRoughLineCount(savedActiveLineIndex);
                    // isScrollJump = MakeFullScrollJump(savedActiveLineIndex, estimatedWrapLine, false);
                    elementY = (int) (estimatedWrapLine * TextSizeY);
                }

                /*if (isScrollJump)
                    return;
                else
                    activeLineIndex = savedActiveLineIndex;*/

                var subLine = MWordWrap
                        ? MContentView.GetWrappedLinePosition(line, EditorView.ActiveLinePosition).subLine
                        : 0;
                
                var minY = (int) (elementY + (subLine * TextSizeY));

                DebugHere($"ScrollOffsetY: {ScrollOffsetY}");
                
                if (ScrollOffsetY + minY <= 0)
                {
                    VerticalScroller.value = minY;
                } 
                else if (ScrollOffsetY + minY + TextSizeY > ViewportHeight)
                {
                    VerticalScroller.value = minY + TextSizeY - ViewportHeight;
                }
                
            }
        }

        #endregion SCROLL

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toLineIndex">Will be the next `SliceIndexOfFirstLine` if MakeFullScrollJump
        /// returns true.</param>
        /// <param name="estimatedWrapLine">Set this only if the caller has already calculated this
        /// value, to avoid multiple calls to expensive methods.</param>
        /// <param name="updateScrollOffset">It will call UpdateScrollOffset() automatically to
        /// update verticalScroller.value unless caller will deal with it, setting this to false.</param>
        /// <returns></returns>
        public virtual bool MakeFullScrollJump(int toLineIndex, int estimatedWrapLine = -1 /*, bool updateScrollOffset = true*/)
        {
            if (MContentView.FirstLineIndex > toLineIndex + 10 || MContentView.LastLineIndex + 10 < toLineIndex)
            {
                estimatedWrapLine = estimatedWrapLine == -1 ? MContentView.GetRoughLineCount(toLineIndex) : estimatedWrapLine;

                SliceIndexOfFirstLine = toLineIndex;
                FirstLineOffsetY = estimatedWrapLine * TextSizeY;
                
                // Directly call FullUpdate() and prevent any duplicated call to FullUpdate()
                RequiresFullContentViewUpdate = false;
                SkipFullContentViewUpdate = true;
                MaxVisibleLines = Mathf.CeilToInt(ViewportHeight / TextSizeY);
                // Getting FirstLineIndex, LastLineIndex, ScrollOffsetPrevY and
                // ScrollOffsetNextY up to date with FullUpdate(), so the scroller
                // should not call any other line by line update.
                MContentView.FullUpdate();
                
                AdjustScroll();

                // if (updateScrollOffset)  // ignore for now
                {
                    // TODO
                   UpdateScrollOffset(); 
                   // MContentView.MarkDirtyRepaint();
                   AdjustBackScreenSize();
                }

                SkipFullContentViewUpdate = false;
                return true;
            }

            return false;
        }
        
        public (int LineIndex, int LinePosition) LocalToLinePosition(Vector3 coords)
        {
            // var pos = new Vector2(coords.x + OffsetX, coords.y + 0);

            var pos = EditorView.ChangeCoordinatesTo(ContentView, new Vector2(coords.x, coords.y));
            var editor = EditorView.Editor;
            
            if (!WordWrap)
            {
                var lineIndex = Math.Min(((int) Math.Max(pos.y, 0)) / ((int) TextSizeY), editor.Count - 1);
                var maxLen = editor.Slices[lineIndex].Length - (lineIndex + 1 == editor.Count ? 0 : 1);
                var linePosition = Math.Min((int) ((Math.Max(pos.x, 0) + 2f) / TextSizeX), maxLen);

                return (lineIndex, linePosition);
            }
            else
            {
                var relativeY = (int) Math.Max(pos.y - FirstLineOffsetY, 0);
                var i = MContentView.FirstLineIndex;
                var lastLineIndex = /*i == MContentView.LastLineIndex ? -1 :*/ MContentView.LastLineIndex;
                var countLines = MContentView.Lines.Count;
                var sumY = 0;
                Line line;

                for (; i <= lastLineIndex; i++)
                {
                    line = MContentView.Lines[i];
                    if (line.Visible == false)
                        continue;

                    var height = (int) MContentView.GetLineHeight(line, i);

                    if (relativeY >= sumY && relativeY < sumY + height)
                        break;

                    sumY += height;
                }
                
                if (i >= countLines)
                {
                    return (countLines - 1, MContentView.Lines[countLines - 1].RawLength);
                }
                
                var lineIndex = i;
                line = MContentView.Lines[i];
                var subLine = (int) ((relativeY - sumY) / TextSizeY);

                if (subLine != 0 && (line.Breakpoints.Count <= subLine - 1 || line.Breakpoints.Count == 0))
                    return (lineIndex, 0);
                
                var subLineStart = subLine == 0 ? 0 : line.Breakpoints[subLine - 1];
                var subLineEnd = line.Breakpoints.Count > subLine ? line.Breakpoints[subLine] : line.RawLength + (lineIndex + 1 == countLines ? 1 : 0);
                var linePosition = Math.Min(subLineStart + ((int) ((Math.Max(pos.x, 0) + 2f) / TextSizeX)), subLineEnd - 1/*subLineEnd > 0 ? subLineEnd - 1 : 0*/);

                return (lineIndex, linePosition);
            }
        }
    }
}
