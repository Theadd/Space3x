using System;
using System.Linq;
using Space3x.UiToolkit.SlicedText.Iterators;
using Space3x.UiToolkit.SlicedText.VisualElements;
using Space3x.UiToolkit.SlicedText.Processors;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    [UxmlElement]
    [HideInInspector]
    public partial class SlicedTextEditor : EditorTextView
    {

        /// <summary>
        /// Used by callbaks like: MdxViewBase.SetBusyFlag(int busyFlagId, bool value)
        /// </summary>
        public event Action<int, bool> OnBusyChanged;

        public int BusyFlagId { get; protected set; } = 1 << 3;

        private string m_PendingText = null;
        
        // [UxmlAttribute]
        [CreateProperty]
        public virtual string Text
        {
            get
            {
                if (Editor == null)
                    return string.Empty;
                
                return string.Concat(Editor.Slices.GetArray().Select(line => line.ToString()));
            }
            set
            {
                if (panel == null)
                {
                    m_PendingText = value;
                    return;
                }
                
                if (Editor == null) Initialize();
                
                Editor.Replace(ref value, 0, Editor.Length);

                if (!ReadOnly)
                {
                    if (Editor.UndoHistory == null)
                        Editor.UndoHistory = History ?? new UndoHistory();

                    if (ClearHistoryWhenTextChangedFromCode)
                        Editor.UndoHistory.Clear();
                }
            }
        }

        public IUndoHistory History { get; set; } = null;

        public override bool ReadOnly
        {
            get => base.ReadOnly;
            set
            {
                base.ReadOnly = value;
                
                if (!value && Editor != null)
                {
                    if (Editor.UndoHistory == null)
                        Editor.UndoHistory = History ?? new UndoHistory();

                    Editor.UndoHistory.Clear();
                }
            }
        }

        private string m_ColorSchemeClass = "sliced-editor--rider";

        public string ColorSchemeClass
        {
            get => m_ColorSchemeClass;
            set 
            {
                if (value != m_ColorSchemeClass)
                {
                    if (!string.IsNullOrEmpty(m_ColorSchemeClass) && ClassListContains(m_ColorSchemeClass))
                        RemoveFromClassList(m_ColorSchemeClass);
                    
                    m_ColorSchemeClass = value;
                    
                    if (!string.IsNullOrEmpty(m_ColorSchemeClass))
                        AddToClassList((m_ColorSchemeClass));
                }
            }
        }

        /// <summary>
        /// Get or set limits to auto enable/disable Growable mode when number of
        /// lines goes past these limits.
        ///
        /// Set to (-1, -1) to disable entering growable mode. Editing these values
        /// only take effect before setting Text property for the first time.
        /// </summary>
        public (int LowLimit, int HighLimit) GrowableValues { get; set; } = (240, 300);

        public GrowableMode Growable { get; set; } = new GrowableMode();

        public IColorize SyntaxHighlighter
        {
            get => m_SyntaxHighlighter;
            set {
                m_SyntaxHighlighter = value ?? IColorize.Default; /* STRIPPED CODE: != null ? value : new Colorize(); */
                
                if (BlockProcessor != null && m_SyntaxHighlighter != null)
                    BlockProcessor.Colorizer = m_SyntaxHighlighter;
            }
        }

        public bool ClearHistoryWhenTextChangedFromCode { get; set; } = true;

        public EditorTextView TextView;
        private IColorize m_SyntaxHighlighter;

        public ScrollView EditorScrollView { get; set; }

        public bool UpdateAncestorScrollView { get; set; } = true;

        public virtual ILineBlockProcessor<TextLine> BlockProcessor { get; set; }
        
        private static CustomStyleProperty<Color> s_SelectionColorProperty =
            new CustomStyleProperty<Color>("--unity-selection-color");

        private static CustomStyleProperty<Color> s_CursorColorProperty =
            new CustomStyleProperty<Color>("--unity-cursor-color");

        public SlicedTextEditor()
        {
            AddToClassList("sliced-editor");
            AddToClassList(m_ColorSchemeClass);
            if (Empty) AddToClassList("sliced-editor--empty");
            TextView = this;

            // --unity-cursor-color: rgb(255, 255, 255);
            // --unity-selection-color: rgba(220, 220, 220, 0.35);

            focusable = true;
            pickingMode = PickingMode.Position;
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            RequestStylesUpdate();
        }

        protected void OnAttachToPanel(AttachToPanelEvent e)
        {
            if (m_PendingText != null && panel != null && e.destinationPanel == panel)
            {
                Text = m_PendingText;
                m_PendingText = null;
            }
            else if (m_PendingText == null)
            {
                if (Editor == null) Initialize();
            }
        }

        private bool m_RequestingStylesUpdate = false;
        
        public override void RequestStylesUpdate()
        {
            if (m_RequestingStylesUpdate)
                return;

            m_RequestingStylesUpdate = true;
            // UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private static readonly string k_MeasureTextSample =
            "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

        private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            Color selectionValue = Color.clear;
            Color cursorValue = Color.clear;

            ICustomStyle customStyle = e.customStyle;
            if (customStyle.TryGetValue(s_SelectionColorProperty, out selectionValue))
                m_SelectionColor = selectionValue;

            if (customStyle.TryGetValue(s_CursorColorProperty, out cursorValue))
                m_CursorColor = cursorValue;

            UpdateTextSizeMeasure();
        }

        public void UpdateTextSizeMeasure()
        {
            if (TextView.contentContainer.childCount > 0)
            {
                TextElement line;
                if (HasBackView) line = (TextElement) (contentContainer as VisualElement).ElementAt(0);
                else line = ((TextElement) TextView.contentContainer.ElementAt(0));
                var textSize = line.MeasureTextSize(
                    k_MeasureTextSample,
                    0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                TextSize.y = textSize.y > 0 ? textSize.y : TextSize.y;
                TextSize.x = textSize.x > 0 ? textSize.x / 120f : TextSize.x;
                if (textSize.x / 120f > 1f)
                {
                    // UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
                    // requestingStylesUpdate = false;
                    Growable.SyncEditorHeight();
                    Growable.SyncGrowableElement();
                    StylesResolvedHandler(true);
                    return;
                }
            }

            StylesResolvedHandler(false);
        }

        public virtual void Initialize()
        {
            if (Editor != null)
                Editor.OnChange -= OnEditorChange;

            BlockProcessor ??= new LineBlockProcessor();
            BlockProcessor.TargetContainer = TextView.contentContainer;
            SyntaxHighlighter ??= IColorize.Default; /* STRIPPED CODE: new Colorize(); */
            BlockProcessor.Colorizer = SyntaxHighlighter;
            BlockProcessor.Clear();
            BlockProcessor.Insert(0, new StringSlice(""));
            
            Editor ??= new StringSliceGroup();
            Editor.Multiline = Multiline;
            // Skip whole content loaded from code to be undoable.
            Editor.UndoHistory = null;

            EditorEventHandler ??= new TextEditorKeyboardEventHandler();
            EditorEventHandler.Editor = Editor;
            EditorEventHandler.TextView = TextView;

            BlockProcessor.Editor = Editor;
            
            if (EditorScrollView == null && UpdateAncestorScrollView)
                EditorScrollView = GetFirstOfType<ScrollView>();

            if (Growable == null)
                Growable = new GrowableMode();

            Growable.LowLimit = GrowableValues.LowLimit;
            Growable.HighLimit = GrowableValues.HighLimit;
            
            Growable.Initialize(this);
            
            Editor.OnChange += OnEditorChange;
        }

//        public System.Diagnostics.Stopwatch WatchOnEditorChange = new System.Diagnostics.Stopwatch();
        
        void OnEditorChange()
        {
//            WatchOnEditorChange.Start();
            Empty = Editor.Count == 1 && Editor.Length == 0;
            if (Empty && !ClassListContains("sliced-editor--empty")) ToggleInClassList("sliced-editor--empty");
            if (!Empty && ClassListContains("sliced-editor--empty")) ToggleInClassList("sliced-editor--empty");
            
            var (lineIndex, linesRemoved, linesAdded) = Editor.LastChange;
            
            if (lineIndex >= 0)
            {
                OnBusyChanged?.Invoke(BusyFlagId, true);
                
                Growable.Sync();
                
                BlockProcessor.GetReadyForChanges();
                
                // Debug.Log($"<color=green>SLICES @{lineIndex} REMOVE {linesRemoved}, INSERT {linesAdded}</color>");
                
                if (linesRemoved == linesAdded)
                {
                    for (var i = 0; i < linesAdded; i++)
                    {
                        BlockProcessor.Replace(lineIndex + i, ref Editor.SliceAt(lineIndex + i));
                    }
                }
                else if (linesRemoved < linesAdded)
                {
                    for (var i = linesRemoved - 1; i >= 0; i--)
                    {
                        BlockProcessor.Replace(lineIndex + i, ref Editor.SliceAt(lineIndex + i));
                    }

                    for (var i = linesRemoved; i < linesAdded; i++)
                    {
                        BlockProcessor.Insert(lineIndex + i, ref Editor.SliceAt(lineIndex + i));
                    }
                }
                else
                {
                    for (var i = linesRemoved - 1; i >= linesAdded; i--)
                    {
                        BlockProcessor.RemoveAt(lineIndex + i);
                    }
                    
                    for (var i = linesAdded - 1; i >= 0; i--)
                    {
                        BlockProcessor.Replace(lineIndex + i, ref Editor.SliceAt(lineIndex + i));
                    }
                }
                
                BlockProcessor.CommitChanges();

                Growable.Resync();

                if (HasBackView) UpdateBackView();
                
                OnBusyChanged?.Invoke(BusyFlagId, false);
            }

            if (LastCursorPosition != Editor.Cursor.Position || LastCursorOffset != Editor.Cursor.Offset)
                CursorChangeHandler();

//            WatchOnEditorChange.Stop();
        }

        protected virtual void SendChangeEvent(ChangeRecord record) {}

        public override void UpdateScrollOffset()
        {
            if (EditorScrollView != null)
            {
                var (index, startPosition) = Editor.Cursor.ActiveLine;
                var lastScrollX = EditorScrollView.horizontalScroller.value;
                VisualElement scrollTarget = BlockProcessor.ElementAt(index);
                if (scrollTarget == null) scrollTarget = this;
                EditorScrollView.ScrollTo(scrollTarget);
                EditorScrollView.horizontalScroller.value = lastScrollX;

                var column = Editor.Cursor.Position - startPosition;
                
                var viewWidth = EditorScrollView.contentViewport.layout.width;
                var contentWidth = viewWidth + EditorScrollView.horizontalScroller.highValue;
                var viewPosX = EditorScrollView.horizontalScroller.value;

                var xMax = viewPosX + (viewWidth - (viewWidth / 5));
                var xMin = viewPosX + (viewWidth / 4);
                var cursorPosition = column * TextSize.x;

                if (cursorPosition < xMin && viewPosX > 0)
                    EditorScrollView.horizontalScroller.value -= xMin - cursorPosition;
                
                if (cursorPosition > xMax && viewPosX < EditorScrollView.horizontalScroller.highValue)
                    EditorScrollView.horizontalScroller.value += cursorPosition - xMax;
            }
        }
    }
}
