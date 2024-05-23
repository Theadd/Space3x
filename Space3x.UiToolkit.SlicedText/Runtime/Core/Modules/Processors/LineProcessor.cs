

using System;
using System.Collections;
using Space3x.UiToolkit.SlicedText.Iterators;

namespace Space3x.UiToolkit.SlicedText.Processors
{
    public class LineProcessor: IEnumerable
    {
        public LineBlockProcessor Owner { get; set; }
        
        // public (int FirstIndex, int LastIndex) LastBlock = (-1, -1);

        protected Func<int, int, int> Process;

        private StringSliceGroup m_MEditor;

        // public (int LineIndex, int LinesRemoved, int LinesAdded) LastChange { get; set; } = (-1, 0, 0);

        private bool m_Disabled = true;   // TODO

        // Used to iterate the buffer (List<string>) position
        private int m_BufferSize = 0;
        private int m_Start = 0;

        public int NextIndexOfBlock { get; private set; } = 0;
        public int IndexOfBlock { get; private set; } = 0;
        public int NextIndexOfFirstElementInBlock { get; private set; } = 0;
        public int IndexOfFirstElementInBlock { get; private set; } = 0;

        public IEnumerator GetEnumerator()
        {
            while (!m_Disabled)
            {
                if (m_Start >= m_BufferSize)
                {
                    if (m_BufferSize != 0)
                        yield return -1;

                    if (NextIndexOfFirstElementInBlock >= m_MEditor.Count)
                        break;
                    
                    m_BufferSize = Process.Invoke(NextIndexOfBlock, NextIndexOfFirstElementInBlock);
                    m_Start = -1;

                    IndexOfBlock = NextIndexOfBlock;
                    IndexOfFirstElementInBlock = NextIndexOfFirstElementInBlock;
                    
                    NextIndexOfBlock++;
                    NextIndexOfFirstElementInBlock += m_BufferSize;

                    if (m_BufferSize == 0)
                        break;
                }
                else
                {
                    yield return m_Start;
                }

                m_Start++;
            }

            m_Disabled = true;
        }

        public void Setup(ref StringSliceGroup editor, Func<int, int, int> action)
        {
            m_MEditor = editor;
            // LastChange = editor.LastChange;
            var (blockHeadIndex, elementIndex) = Owner.GetBlockHeadOf(editor.LastChange.LineIndex);
            NextIndexOfBlock = blockHeadIndex;
            NextIndexOfFirstElementInBlock = elementIndex; // Math.Max(elementIndex, 0);
            IndexOfBlock = NextIndexOfBlock;
            IndexOfFirstElementInBlock = NextIndexOfFirstElementInBlock;
            m_BufferSize = 0;
            m_Start = 0;
            
            Process = action;

            m_Disabled = false;
        }
        
    }
}
