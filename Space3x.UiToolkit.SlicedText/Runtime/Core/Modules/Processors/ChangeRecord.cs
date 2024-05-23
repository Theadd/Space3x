using System;

namespace Space3x.UiToolkit.SlicedText
{
    public class ChangeRecord
    {
        /// <summary>
        /// Element position where this change begins within your self-managed list of elements that
        /// render the whole text.
        /// 
        /// Whether the elements represent a single line of text each one or a block of lines can
        /// vary depending on the BlockProcessor used.
        ///
        /// e.g. SingleLineProcessor threats each element as a single text line but MarkdownBlockProcessor
        /// Merges lines into meaningful blocks to properly render them separately.
        /// 
        /// <seealso cref="SlicedEditor.Processors.SingleLineProcessor"/>
        /// <seealso cref="SlicedEditor.Processors.MarkdownBlockProcessor"/>
        /// <seealso cref="SlicedEditor.Processors.LineBlockProcessor"/>
        /// </summary>
        public int Index { get; set; }
        public int RemoveCount { get; set; }
        public int InsertCount { get; set; }

        public Func<int, string> ElementAt { get; set; }

        public SlicedTextEditor Target { get; set; }
    }
}
