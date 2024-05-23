using System.Collections.Generic;
using System.Linq;
using Space3x.UiToolkit.SlicedText.Iterators;

namespace Space3x.UiToolkit.SlicedText
{
    public class UndoHistory : IUndoHistory
    {
        public int Capacity { get; set; } = 30;

        protected Stack<Record> UndoStack = new Stack<Record>();
        
        protected Stack<Record> RedoStack = new Stack<Record>();

        protected bool IsAnUndoAction = false;
        
        protected bool IsARedoAction = false;
        
        public void Add(StringSliceGroup editor)
        {
            var (lineIndex, linesRemoved, linesAdded) = editor.LastChange;

            if (lineIndex >= 0)
            {
                var slices = new RefList<StringSlice>(linesRemoved);

                for (var i = 0; i < linesRemoved; i++)
                    slices.Add(editor.SliceAt(lineIndex + i));
                
                (IsAnUndoAction ? RedoStack : UndoStack).Push(new Record()
                {
                    LineIndex = lineIndex,
                    RemoveCount = linesAdded,
                    SlicesToAdd = slices
                });

                if (!IsAnUndoAction && !IsARedoAction)
                    RedoStack.Clear();

                if (UndoStack.Count >= Capacity)
                    UndoStack = new Stack<Record>(UndoStack.ToArray().Reverse().Where((item, i) => i >= Capacity / 2));
            }
        }

        public void Undo(StringSliceGroup editor)
        {
            if (UndoStack.Count > 0)
            {
                var record = UndoStack.Pop();
                
                IsAnUndoAction = true;
                
                editor.ApplyChange(record.LineIndex, record.RemoveCount, record.SlicesToAdd, 0, true);

                IsAnUndoAction = false;
            }
        }

        public void Redo(StringSliceGroup editor)
        {
            if (RedoStack.Count > 0)
            {
                var record = RedoStack.Pop();
                
                IsAnUndoAction = false;
                IsARedoAction = true;
                
                editor.ApplyChange(record.LineIndex, record.RemoveCount, record.SlicesToAdd, 0, true);
                
                IsARedoAction = false;
            }
        }

        public void Clear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            IsAnUndoAction = false;
        }

        protected class Record
        {
            public int LineIndex { get; set; }
            public int RemoveCount { get; set; }
            public RefList<StringSlice> SlicesToAdd { get; set; }
        }
    }
}
