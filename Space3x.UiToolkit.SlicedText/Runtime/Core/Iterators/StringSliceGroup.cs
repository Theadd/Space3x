using System;

namespace Space3x.UiToolkit.SlicedText.Iterators
{
    public class StringSliceGroup
    {
        /// <summary>
        /// Text lines as StringSlices, they're public for simplicity but u better don't edit them,
        /// use provided methods or implement yours, see <see cref="ApplyChange"/>
        /// </summary>
        public RefList<StringSlice> Slices;

        /// <summary>
        /// Number of lines
        /// </summary>
        public int Count { get; protected set; } = 1;

        public IUndoHistory UndoHistory = null;
        
        internal bool Multiline = true;

        public SliceGroupCursor Cursor;

        public event Action OnChange;

        public int TabSize { get; set; } = 4;

        public (int LineIndex, int LinesRemoved, int LinesAdded) LastChange { get; protected set; } = (-1, 0, 0);

        public StringSliceGroup()
        {
            Slices = new RefList<StringSlice>();
            Slices.Add(new StringSlice(""));
            var that = this;
            Cursor = new SliceGroupCursor(ref that);
        }

        public ref StringSlice SliceAt(int index)
        {
            return ref Slices.Get(index);
        }

        /// <summary>
        /// Total number of characters within all lines
        /// </summary>
        public int Length
        {
            get
            {
                var len = 0;
                for (int i = 0; i < Slices.Count; i++)
                    len += Slices[i].Length;

                return len;
            }
        }

        protected void NotifyChanges()
        {
            OnChange?.Invoke();
            LastChange = (-1, 0, 0);
        }

        // public string GetLineAsString(int lineIndex, bool safetyEscape = true)
        // {
        //     if (!safetyEscape)
        //         return Slices[lineIndex].ToStringLine();
        //
        //     return Slices[lineIndex].ToStringLine()
        //         .Replace(@"\", @"\\");
        // }

        private int m_CachedSliceStart = 0;
        private int m_CachedSliceIndex = 0;

        /// <summary>
        /// Given a character position within the full text, returns the
        /// line index where it belongs and the position of the first
        /// character of that line within the full text.
        /// </summary>
        /// <param name="position">A character position within the full text.</param>
        /// <returns></returns>
        public (int Index, int StartPosition) GetSliceAt(int position)
        {
            var startPosition = 0;
            var i = 0;

            if (position > m_CachedSliceStart)
            {
                i = m_CachedSliceIndex;
                startPosition = m_CachedSliceStart;
            }

            var k = i;

            for (; i < Count; i++)
            {
                ref var slice = ref Slices.Get(i);
                startPosition += slice.Length;

                if (startPosition > position)
                {
                    if (i - k >= 5)
                    {
                        m_CachedSliceIndex = i;
                        m_CachedSliceStart = startPosition - slice.Length;
                    }

                    return (i, startPosition - slice.Length);
                }
            }

            return (Count - 1, startPosition - (Slices[Count - 1].Length));
        }

        public int GetLineStartPosition(int lineIndex)
        {
            var startPosition = 0;
            var i = 0;

            if (lineIndex >= m_CachedSliceIndex)
            {
                i = m_CachedSliceIndex;
                startPosition = m_CachedSliceStart;
            }

            for (; i < Count; i++)
            {
                if (lineIndex == i)
                    break;

                startPosition += Slices[i].Length;
            }

            return startPosition;
        }

        protected static char[] invalid = new[] {'\t', '\r', '\0'};

        protected virtual void Normalize(ref string text)
        {
            if (text.IndexOfAny(invalid) < 0) return;

            text = text.Replace("\t", new string(' ', TabSize))
                .Replace('\0', '\uFFFD')
                .Replace("\r", "");
        }

        public bool Insert(string text)
        {
            return ReplaceSelection(ref text);
        }

        public bool Insert(char c)
        {
            if (c == '\t')
                return Insert(new string(' ', TabSize));

            // TODO: quick insert
            return Insert("" + c);
        }

        /// <summary>
        /// Creates a shallow copy of a range of lines into a StringLineGroup, allowing
        /// seamless char iteration regardless of being an array of StringSlices/StringLines,
        /// direct <code>.ToString()</code> conversion or to a single StringSlice.
        /// 
        /// <see cref="StringLineGroup.ToCharIterator"/>
        /// <see cref="StringLineGroup.ToSlice"/>
        /// </summary>
        /// 
        /// <param name="fromLineIndex">Must be LOWER or equal to the second parameter. It won't throw otherwise.</param>
        /// <param name="toLineIndex"></param>
        /// <returns>StringLineGroup</returns>
        public StringLineGroup GetRange(int fromLineIndex, int toLineIndex)
        {
            var lines = new StringLineGroup(Math.Abs(fromLineIndex - toLineIndex) + 1);
            
            for (int i = fromLineIndex; i <= toLineIndex; i++)
                lines.Add(Slices[i]);

            return lines;
        }

        /// <summary>
        /// Same as <see cref="GetRange"/> but takes character positions as parameters instead.
        /// starting from 0 as first character of first line, to slice the text.
        /// </summary>
        /// <param name="fromPosition">A character index "absolute" position within text, same as in Cursor.Position</param>
        /// <param name="toPosition">Second character position, can be lower than the first parameter, they'll be swapped.</param>
        /// <returns>StringLineGroup</returns>
        public StringLineGroup GetPositionedRange(int fromPosition, int toPosition)
        {
            var fromPos = fromPosition < toPosition ? fromPosition : toPosition;
            var toPos = fromPosition < toPosition ? toPosition : fromPosition;
            
            var (indexStart, positionStart) = GetSliceAt(fromPos);
            var (indexEnd, positionEnd) = GetSliceAt(toPos);

            var lines = GetRange(indexStart, indexEnd);
            lines.Lines[lines.Count - 1].Slice.End -= lines.Lines[lines.Count - 1].Slice.Length - (toPos - positionEnd);
            lines.Lines[0].Slice.Start += fromPos - positionStart;

            return lines;
        }

        public bool Replace(ref string text, int selectionStart, int selectionEnd)
        {
            Normalize(ref text);

            var (indexStart, positionStart) = GetSliceAt(selectionStart);
            var (indexEnd, positionEnd) = GetSliceAt(selectionEnd);

            var prefix = new StringSlice(ref Slices.Get(indexStart));
            prefix.End = prefix.Start + (selectionStart - positionStart) - 1;

            var suffix = new StringSlice(ref Slices.Get(indexEnd));
            suffix.Start += (selectionEnd - positionEnd);

            var newSlices = SliceToLines(
                new StringSlice(prefix.ToString() + text + suffix.ToString()));

            // Fixes for special cases
            if (Multiline)
            {
                if (indexEnd + 1 == Count && text.Length > 0 && text[text.Length - 1] == '\n' &&
                    suffix.End < suffix.Start)
                    newSlices.Add(new StringSlice(""));

                if (indexEnd - indexStart + 1 == Count && newSlices.Count == 0)
                    newSlices.Add(new StringSlice(""));

                if (indexEnd + 1 == Count && Count > 1 && prefix.Length == 0 && suffix.Length == 0 && text.Length == 0)
                    if (indexStart > 0 && Slices[indexStart - 1].Length > 0)
                        newSlices.Add(new StringSlice(""));
            }
            //
            
            var nextPos = suffix.Length;

            ApplyChange(indexStart, indexEnd - indexStart + 1, newSlices, nextPos,
                /*Cursor.Offset != 0 || text.Length > 1*/ true);

            return true;
        }

        public bool ReplaceSelection(ref string text)
        {
            return Replace(ref text, Cursor.SelectionStart, Cursor.SelectionEnd);
        }

        public void ApplyChange(int atSliceIndex, int removedSlicesCount, RefList<StringSlice> newSlices,
            int nextPosOffsetFromEnd, bool invalidateCache = true)
        {
            LastChange = (atSliceIndex, removedSlicesCount, newSlices.Count);
            UndoHistory?.Add(this);
            
            if (removedSlicesCount > 0)
                Slices.RemoveRange(atSliceIndex, removedSlicesCount);
            Slices.InsertRange(atSliceIndex, newSlices);
            Count = Slices.Count;

            var nextNewSliceIndex = atSliceIndex + newSlices.Count;
            var len = 0;
            for (var i = 0; i < nextNewSliceIndex; i++)
                len += Slices[i].Length;


            if (invalidateCache)
            {
                m_CachedSliceIndex = 0;
                m_CachedSliceStart = 0;
            }
            
            Cursor.MoveTo(len - nextPosOffsetFromEnd, false);
        }

        /// <summary>
        /// Provided a *dirty* StringSlice, with raw text that might contain multiple lines,
        /// returns a list of StringSlices sharing the same original text string but referring
        /// to their respective line parts.
        /// </summary>
        /// <param name="fullSlice"></param>
        /// <returns></returns>
        public RefList<StringSlice> SliceToLines(StringSlice fullSlice)
        {
            var slices = new RefList<StringSlice>();
            
            if (Multiline)
            {
                var currentSlice = new StringSlice(ref fullSlice);

                var i = fullSlice.Start;
                var end = fullSlice.End;

                while (i <= end)
                {
                    if (fullSlice[i] == '\n')
                    {
                        currentSlice.End = i;
                        slices.Add(currentSlice);
                        i = ++fullSlice.Start;
                        currentSlice = new StringSlice(ref fullSlice);
                    }
                    else
                    {
                        i = ++fullSlice.Start;
                    }
                }

                currentSlice.End = fullSlice.End;

                if (!(currentSlice.End < currentSlice.Start))
                    slices.Add(currentSlice);

                return slices;
            }

            slices.Add(new StringSlice(fullSlice.ToString().Replace("\n", "")));

            return slices;
        }

        public int GetNextWordPosition(int fromPosition, int direction)
        {
            if (direction != -1 && direction != 1)
                throw new ArgumentOutOfRangeException("direction", direction,
                    "Must be -1 to look backwards or 1 for forwards.");

            var (lineIndex, startPosition) = GetSliceAt(fromPosition);
            ref var slice = ref SliceAt(lineIndex);
            var endPosition = startPosition + slice.Length;
            var lastCharType = -1;
            var charType = -1;
            var pos = direction == -1 ? fromPosition - 1 : fromPosition;

            for (; pos <= endPosition && pos >= startPosition; pos += direction)
            {
                var c = slice.PeekChar(pos - startPosition);

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

            if (Multiline && direction == -1 && fromPosition == pos + 1 && pos >= 0)
                pos -= 1;

            if (direction == 1 && pos > endPosition && fromPosition < endPosition - 1)
                pos = endPosition - 1;

            return direction == -1 ? pos + 1 : pos;
        }
        
        public struct SliceGroupCursor
        {
            public SliceGroupCursor(ref StringSliceGroup sliceGroup) : this()
            {
                SliceGroup = sliceGroup;
                Position = 0;
                Offset = 0;
                Column = -1;
                ActiveLine = (0, 0);
            }

            public StringSliceGroup SliceGroup;

            public int Position { get; private set; }
            public int Offset { get; private set; }
            public int Column { get; private set; }
            public (int Index, int StartPosition) ActiveLine { get; private set; }

            public int SelectionStart => Offset < 0 ? Position + Offset : Position;
            public int SelectionEnd => Offset < 0 ? Position : Position + Offset;

            /// <summary>
            /// Move Cursor from current Position within StringSliceGroup to <para>nextPos</para>.
            /// </summary>
            /// <param name="nextPos">Next Position, in absolute values</param>
            /// <param name="keepOffset">Whether to update selection offset from last position or reset it to 0</param>
            public void MoveTo(int nextPos, bool keepOffset = false)
            {
                if (nextPos >= 0)
                {
                    if (nextPos > Position && nextPos > SliceGroup.Length)
                        nextPos = SliceGroup.Length;

                    Offset -= nextPos - Position;
                    Position = nextPos;
                    ActiveLine = SliceGroup.GetSliceAt(nextPos);
                    Column = -1;
                }

                if (keepOffset == false) Offset = 0;
                SliceGroup.NotifyChanges();
            }

            public void MoveToLine(int lineIndex, bool keepOffset = false)
            {
                var nextPos = Position;

                if (Column == -1)
                    Column = Position - ActiveLine.StartPosition;

                if (lineIndex < 0)
                {
                    nextPos = 0;
                }
                else if (lineIndex >= SliceGroup.Count)
                {
                    nextPos = SliceGroup.Length;
                }
                else
                {
                    nextPos = SliceGroup.GetLineStartPosition(lineIndex)
                              + Math.Min(SliceGroup.Slices[lineIndex].Length - 1, Column);
                }

                Offset -= nextPos - Position;
                Position = nextPos;
                ActiveLine = SliceGroup.GetSliceAt(nextPos);

                if (keepOffset == false) Offset = 0;
                SliceGroup.NotifyChanges();
            }
        }
    }
}
