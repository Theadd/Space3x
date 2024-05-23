namespace Space3x.UiToolkit.SlicedText
{
    public enum SnappingMode { None = 0, Word = 1, Line = 2 }
    
    public enum EditorAction
    {
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        MoveLineStart,
        MoveLineEnd,
        SelectLineStart,
        SelectLineEnd,
        MovePageUp,
        MovePageDown,
        MoveWordLeft,
        MoveWordRight,
        SelectLeft,
        SelectRight,
        SelectUp,
        SelectDown,
        SelectPageUp,
        SelectPageDown,
        SelectWordLeft,
        SelectWordRight,
        Delete,
        Backspace,
        Cut,
        Copy,
        Paste,
        SelectAll,
        SelectNone,
        Undo,
        Redo,
        AddIndent,
        RemoveIndent,
        EnterKey,
        ReturnKey
    }
}
