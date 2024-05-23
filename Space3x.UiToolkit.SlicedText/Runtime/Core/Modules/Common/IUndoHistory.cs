using Space3x.UiToolkit.SlicedText.Iterators;

namespace Space3x.UiToolkit.SlicedText
{
    public interface IUndoHistory
    {
        public void Add(StringSliceGroup editor);

        public void Undo(StringSliceGroup editor);
        
        public void Redo(StringSliceGroup editor);

        public void Clear();
    }
}
