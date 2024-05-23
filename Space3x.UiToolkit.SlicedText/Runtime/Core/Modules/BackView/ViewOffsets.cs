using UnityEngine;
using UnityEngine.UIElements;
using Space3x.UiToolkit.SlicedText.VisualElements;

namespace Space3x.UiToolkit.SlicedText
{
    public abstract class ViewOffsets : VisualElement
    {
        #region VIEW OFFSETS
        
        /// <summary>
        /// Start X position which usually don't change,
        /// as the view width of line number's margin
        /// </summary>
        public int BaseOffsetX { get; set; } = 0;
        
        /// <summary>
        /// Base Y position to compute offset from.
        /// </summary>
        public int BaseOffsetY { get; set; } = 0;   // TODO

        /// <summary>
        /// The line index corresponding to the BaseOffsetY.
        /// </summary>
        public int BaseLineIndexOffset { get; set; } = 0;   // TODO

        public int ScrollOffsetX { get; set; } = 0;
        
        public int ScrollOffsetY { get; set; } = 0;

        public int ScrollOffsetNextY = 0;
        public int ScrollOffsetPrevY = 0;

        /// <summary>
        /// In pixels
        /// </summary>
        public int OffsetX => BaseOffsetX + AddToOffsetX;

        public int OffsetY => BaseOffsetY + AddToOffsetY;

        /// <summary>
        /// Additional margin between the MarginView and the ContentView
        /// </summary>
        public int AddToOffsetX { get; set; } = 8;
        public int AddToOffsetY { get; set; } = 8;

        public float TextSizeX = 8f;
        public float TextSizeY = 16f;

        /// <summary>
        /// Number of characters that fit within ViewportWidth, rounded down.
        /// </summary>
        public int LineVisibleChars;

        /// <summary>
        /// Number of lines that fit within ViewportHeight, rounded up.
        /// </summary>
        public int MaxVisibleLines;

        /// <summary>
        /// An estimation count of total lines taking into account WordWrap.
        /// </summary>
        public int EstimatedLineCount = 0;

        /// <summary>
        /// Corresponds to the parent Editor's viewport width (layout.width).
        /// </summary>
        public float ViewportWidth { get; protected set; }

        /// <summary>
        /// Height of parent editor (layout.height).
        /// </summary>
        public float ViewportHeight { get; protected set; }

        public float BackScreenHeight { get; protected set; }

        public int ScrollbarSize = 0;

        public int ActiveLineIndex => EditorView.Editor.Cursor.ActiveLine.Index;
        
        //public int ActiveLinePosition => EditorView.Editor.Cursor.ActiveLine.StartPosition;

        /// <summary>
        /// Incremental counter used as version number that increases every time that
        /// LineVisibleChars, WordWrap or TextSizeY changes.
        /// </summary>
        public int Baseline = 0;

        public int FirstLineInViewport = 0;
        public float FirstLineOffsetY = 0;

        public int SliceIndexOfFirstLine = 0;
        public int SliceIndexOfBaseOffset = 0;

        #endregion VIEW OFFSETS
        
        /// <summary>
        /// Rect available to check whether an element is within the drawable area.
        /// </summary>
        public Rect PaintRect { get; protected set; }   // TODO?

        public bool RequiresFullContentViewUpdate = true;
        
        /// <summary>
        /// Only set to true by MakeFullScrollJump() and means that an early injected
        /// full content view update is being resolved in this cycle to avoid expensive
        /// line by line update iterations, etc.
        /// </summary>
        protected bool SkipFullContentViewUpdate = false;
        
        public bool WordWrap
        {
            get => MWordWrap;
            set
            {
                if (MWordWrap != value)
                {
                    MWordWrap = value;
                    Baseline++;
                }
            }
        }

        #region PROTECTED
        
        protected Font FontStyle { get; set; }
        public float FontSize { get; set; }
        
        protected EditorTextView EditorView;
        
        protected ContentView MContentView;
        
        public ViewMargin MarginView;
        
        public Scroller VerticalScroller;

        protected bool HasActiveVerticalScroller = false;

        protected bool MWordWrap = true;

        #endregion PROTECTED
        
        public ViewOffsets(EditorTextView editorView)
        {
            EditorView = editorView;
        }

        public virtual void Sync()
        {
            TextSizeX = EditorView.TextSize.x;
            ScrollbarSize = (int) (VerticalScroller?.layout.width ?? 0);
            
            ViewportWidth = EditorView.layout.width;
            ViewportHeight = EditorView.layout.height;
            
            if (LineVisibleChars != (int) ((ViewportWidth - OffsetX - ScrollbarSize) / TextSizeX) || EditorView.TextSize.y != TextSizeY)
            {
                RequiresFullContentViewUpdate = true;
                Baseline++;
            }

            MContentView.style.width = ViewportWidth - OffsetX;
            MContentView.style.marginLeft = OffsetX;
            
            TextSizeY = EditorView.TextSize.y;

            LineVisibleChars = (int) ((ViewportWidth - OffsetX - ScrollbarSize) / TextSizeX);

            if (FontStyle != null && (FontStyle != MContentView.resolvedStyle.unityFont || FontSize != MContentView.resolvedStyle.fontSize))
                EditorView.RequestStylesUpdate();
            
            FontStyle = MContentView.resolvedStyle.unityFont;
            FontSize = MContentView.resolvedStyle.fontSize;
        }

        protected int LastSingleLineCount = 1;

        public virtual void Update()
        {
            RequiresFullContentViewUpdate = RequiresFullContentViewUpdate || LastSingleLineCount != MContentView.Lines.Count;

            if (RequiresFullContentViewUpdate)
            {
                if (MaxVisibleLines > 0 && !SkipFullContentViewUpdate)
                {
                    var lineIndex = EditorView.Editor.LastChange.LineIndex;
                    
                    if (SliceIndexOfFirstLine > lineIndex && lineIndex != -1)
                    {
                        SliceIndexOfFirstLine = 0;
                        FirstLineOffsetY = 0;
                    }
                    else
                    {
                        if (ScrollOffsetY == 0) SliceIndexOfFirstLine = 0;
                        FirstLineOffsetY = MContentView.GetRoughLineCount(SliceIndexOfFirstLine) * TextSizeY;
                    }
                    MContentView.FullUpdate();
                    AdjustScroll();
                }
                
                LastSingleLineCount = MContentView.Lines.Count;
                RequiresFullContentViewUpdate = false;
            }
        }

        protected abstract void AdjustScroll();

        public float LineToLocalPosition(int lineIndex) => (lineIndex - BaseLineIndexOffset) * TextSizeY;
        
        // TODO: REMOVE
        public void DebugHere(string title)
        {
            VisualElement element;
            Vector3 position = new Vector3();
            if (MContentView.Container.childCount > 5)
            {
                element = MContentView.Container.ElementAt(
                    MContentView.DisplayedLines.IndexOf(MContentView.FirstLineIndex));
                position = element.transform.position;
            }
            Debug.Log($"<b>[{title}]</b> x: {(int)position.x}, y: {(int)position.y}, TextSizeY: {TextSizeY}, TextSizeX: {TextSizeX}, ViewportHeight: {ViewportHeight}, ViewportWidth: {ViewportWidth}");
        }
    }
}
