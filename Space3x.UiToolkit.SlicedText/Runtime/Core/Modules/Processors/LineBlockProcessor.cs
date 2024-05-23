using System;
using System.Collections.Generic;
using System.Diagnostics;
using Space3x.UiToolkit.SlicedText.Iterators;
using Space3x.UiToolkit.SlicedText.VisualElements;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.Processors
{
    public class LineBlockProcessor : ProcessorEventHandler, ILineBlockProcessor<TextLine>
    {
        public override Action<ChangeRecord> OnReadyChangeRecord { get; set; }
        
        public IColorize Colorizer { get; set; }

        /// <summary>
        /// <param name="int">Block index</param>
        /// <param name="int">Index of block's first element within TargetContainer.Children()</param>
        /// <param name="List">A List of string lines composing the block, before applying any formatting.</param>
        /// </summary>
        public event Action<int, int, List<string>> OnBlockProcessedAsStringList;

        public List<ILine> BlockHeads = new List<ILine>();

        public int MinBlockSize { get; set; } = 4;

        protected List<string> Buffer = new List<string>(32);

        public StringSliceGroup Editor
        {
            get => MEditor;
            set => MEditor = value;
        }

        public VisualElement TargetContainer { get; set; }

        public ContentViewBase TargetContentView = null;

        protected bool IsContentView = false;

        protected StringSliceGroup MEditor;

        private StringSlice m_DummySlice = new StringSlice(" ");

        protected LineProcessor Processor { get; private set; } = new LineProcessor();

        // public IEnumerator Blocks { get; private set; }

        protected bool WriteUnlocked = true;

        protected bool WriteLockIgnoreLayout = true;

        protected int NewestElementIndex = 0;

        public TextLine Create(ref StringSlice element)
        {
            return new TextLine()
            {
                text = string.Empty,
            };
        }

        public int CounterBlockHeads = 0;
        public int CounterGetBlockHeadOf = 0;
        public int CounterGetBlockHeadOfLoop = 0;
        public Stopwatch WatchGetBlockHeadOf = new Stopwatch();

        private int m_BlockHeadsCacheStepSize = 5;

        private List<int> m_BlockHeadsCache = new List<int>();
        
        /// <summary>
        /// GetBlockHeadOf
        /// </summary>
        /// <param name="elementIndex">Index of a TextLine to get the first element index of it's block (it's BlockHead).</param>
        /// <returns>
        /// <value>BlockHeadIndex as index within blocks</value>
        /// <value>ElementIndex as index of the block element (it's TextLine) within TargetContainer.Children()</value>
        /// </returns>
        public (int BlockHeadIndex, int ElementIndex) GetBlockHeadOf(int elementIndex)
        {
            var lastIndex = 0;
            var index = 0;
            var i = 0;

            if (BlockHeads.Count == 0) return (0, 0);
            
            for (var e = m_BlockHeadsCache.Count - 1; e >= 0; e--)
            {
                if (m_BlockHeadsCache[e] <= elementIndex)
                {
                    lastIndex = m_BlockHeadsCache[e];
                    i = e * m_BlockHeadsCacheStepSize;
                    break;
                }
            }

            CounterBlockHeads = BlockHeads.Count;
            CounterGetBlockHeadOf++;
            WatchGetBlockHeadOf.Start();
            
            for (; i < BlockHeads.Count; i++)
            {
                CounterGetBlockHeadOfLoop++;
                index = IsContentView
                    ? TargetContentView.IndexOf(BlockHeads[i])
                    : TargetContainer.IndexOf((TextLine) BlockHeads[i]);
                if (elementIndex < index)
                {
                    WatchGetBlockHeadOf.Stop();
                    return (i - 1, lastIndex);
                }
                
                if (i % m_BlockHeadsCacheStepSize == 0)
                {
                    var cacheIndex = (int) (i / m_BlockHeadsCacheStepSize);

                    if (cacheIndex < m_BlockHeadsCache.Count)
                    {
                        if (m_BlockHeadsCache[cacheIndex] != index)
                        {
                            m_BlockHeadsCache.RemoveRange(cacheIndex, m_BlockHeadsCache.Count - cacheIndex);
                            m_BlockHeadsCache.Insert(cacheIndex, index);
                        }
                    }
                    else
                    {
                        m_BlockHeadsCache.Insert(cacheIndex, index);
                    }
                }

                lastIndex = index;
            }

            WatchGetBlockHeadOf.Stop();
            return (i - 1, lastIndex);
        }

        public int GetFirstElementIndexOfBlock(int blockIndex)
        {
            if (BlockHeads.Count > blockIndex)
                return IsContentView
                    ? TargetContentView.IndexOf(BlockHeads[blockIndex])
                    : TargetContainer.IndexOf((TextLine) BlockHeads[blockIndex]);

            return -1;
        }
        
        #region PROCESSING

        public virtual void FillBufferWithBlockLines(int blockHeadIndex, int elementIndex)
        {
            var i = elementIndex;
            var foundEmpty = false;
            var empty = false;
            var numLines = Editor.Count;
            Buffer.Clear();

            // Retrieve current block lines
            while (i < numLines)
            {
                ref var line = ref MEditor.SliceAt(i);
                if (i - elementIndex >= MinBlockSize && numLines - i > MinBlockSize)
                {
                    empty = line.IsEmptyOrWhitespace();

                    if (!empty && foundEmpty)
                        break;

                    if (empty) foundEmpty = true;
                }

                Buffer.Add(line.ToString());

                i++;
            }
        }

        protected virtual void FixOrphanLines(int blockHeadIndex, int elementIndex) {}

        public virtual void GetReadyForChanges()
        {
            if (Editor == null)
                throw new NotImplementedException();

            TargetContentView = TargetContainer as ContentViewBase;
            IsContentView = TargetContentView != null;
            
            WriteUnlocked = false;
            Processor.Owner = this;
            // NewestElementIndex = Processor.LastChange.LineIndex + Processor.LastChange.LinesAdded - 1;
            NewestElementIndex = Editor.LastChange.LineIndex + Editor.LastChange.LinesAdded - 1;
            var auxNewestElementIndex =
                GetFirstElementIndexOfBlock(GetBlockHeadOf(NewestElementIndex).BlockHeadIndex + 1);
            auxNewestElementIndex = auxNewestElementIndex == -1 ? Editor.Count - 1 : auxNewestElementIndex;
            NewestElementIndex = Math.Max(NewestElementIndex, auxNewestElementIndex);
            
            Buffer.Clear();
            Processor.Setup(ref MEditor, (blockHeadIndex, elementIndex) =>
            {
                
                FillBufferWithBlockLines(blockHeadIndex, elementIndex);

                // FixOrphanLines(blockHeadIndex, elementIndex);
                
                // Remove overlapped blocks
                var upToElementIndex = elementIndex + Buffer.Count - 1;
                var nextBlockIndex = blockHeadIndex;
                var foundIndex = 0;
                var overlappedBlocks = false;
                
                while (true)
                {
                    foundIndex = GetFirstElementIndexOfBlock(++nextBlockIndex);
                    if (foundIndex == -1 || foundIndex >= upToElementIndex)
                        break;
                }

                for (int i = nextBlockIndex - 1; i > blockHeadIndex; i--)
                {
                    overlappedBlocks = true;
                    // Debug.Log($"<color=#AA3333FF>    REMOVE BLOCK <b>{i}</b> FROM OVERLAPPED BLOCKS WHILE PROCESSING <b>{blockHeadIndex}</b></color>");
                    // if (i < BlockHeads.Count)
                        RemoveBlockAt(i);
                }
                //
                
                var blockHead = IsContentView ? TargetContentView.ElementAt(elementIndex) : (ILine) TargetContainer.ElementAt(elementIndex);
                if (blockHead != null)
                {
                    if (blockHeadIndex >= 0 && blockHeadIndex <= BlockHeads.Count)
                    {
                         
                        if (overlappedBlocks || ((blockHeadIndex < BlockHeads.Count) && BlockHeads[blockHeadIndex] == blockHead))
                        {
                            // Debug.Log($"<color=#AA3333FF>    REMOVE BLOCK <b>{blockHeadIndex}</b> BEFORE INSERTING IT AGAIN</color>");
                            RemoveBlockAt(blockHeadIndex);
                        }

                        BlockHeads.Insert(blockHeadIndex, blockHead);
                        // Debug.Log($"<color=#AAAAAAFF>    INTERNAL BLOCK INSERT: <b>{blockHeadIndex}</b> @ line {elementIndex}</color>");
                    }
                    if (blockHeadIndex + 5 >= BlockHeads.Capacity)
                        BlockHeads.Capacity *= 2;
                }
                
                

                OnBlockProcessedAsStringList?.Invoke(blockHeadIndex, elementIndex, Buffer);

                var lastCount = Buffer.Count;
                
                var builder = StringBuilderCache.Local();
                builder.Length = 0;
                for (var b = 0; b < lastCount; b++)
                    builder.Append(Buffer[b]);

                InsertBlockAt(blockHeadIndex, elementIndex, builder.Replace("\\", "\\\u0003").ToString());
                
                Buffer.Clear();

                var slice = new StringSlice(Colorizer.Format(builder));
                var prevSlice = new StringSlice(ref slice);
                var e = slice.Start - 1;
                var end = slice.End;
                ref var txt = ref slice.Text;

                while (++e <= end)
                {
                    if (txt[e] == '\n')
                    {
                        prevSlice.End = e;
                        Buffer.Add(prevSlice.ToString());
                        prevSlice.End = end;
                        prevSlice.Start = e + 1;
                    }
                }

                if (prevSlice.Start <= end) Buffer.Add(prevSlice.ToString());
                if (lastCount > Buffer.Count) Buffer.Add("");
                
                // Update NewestElementIndex if necessary
                if (elementIndex < NewestElementIndex && elementIndex + lastCount - 1 > NewestElementIndex)
                {
                    // Force to process all lines in next block
                    NewestElementIndex = elementIndex + lastCount;
                    // Debug.Log($"<color=#AAAA33FF>Force to process all lines in next block.</color> NewestElementIndex: {NewestElementIndex}");
                    if (GetFirstElementIndexOfBlock(blockHeadIndex + 1) < NewestElementIndex)
                    {
                        // Debug.Log($"<color=#AAAA33FF>    REMOVE BLOCK <b>{(blockHeadIndex + 1)}</b> SINCE WE FORCE AN ADDITIONAL INSERT</color>");
                        if (BlockHeads.Count > blockHeadIndex + 1)
                            RemoveBlockAt(blockHeadIndex + 1);
                    }
                }
                //

                return Buffer.Count;
            });
        }

        public virtual void CommitChanges()
        {
            var lastBufferIndex = 0;
            
            WriteUnlocked = true;

            foreach (int indexInBuffer in Processor)
            {
                if (indexInBuffer >= 0)
                {
                    Replace(Processor.IndexOfFirstElementInBlock + indexInBuffer, Buffer[indexInBuffer]);
                }
                else
                {
                    if (lastBufferIndex == -1 || Processor.IndexOfFirstElementInBlock + lastBufferIndex >= NewestElementIndex)
                        break;
                }
                
                lastBufferIndex = indexInBuffer;
            }
            
            Buffer.Clear();
            // Debug.Log($"<color=#339933FF>Should send change record with: -{SeqOfBlocksToRemove.Count} +{SeqOfBlocksToAdd.Count} {(OnReadyChangeRecord == null)}</color>");
            SendChangeRecordEvent();
        }
        
        #endregion PROCESSING
        
        public override void RemoveBlockAt(int blockIndex)
        {
            BlockHeads.RemoveAt(blockIndex);
            
            base.RemoveBlockAt(blockIndex);
        }

        public void Clear()
        {
            //if (WriteUnlocked || WriteLockIgnoreLayout)
            if (IsContentView) TargetContentView.Clear();
            else TargetContainer.Clear();
            
            BlockHeads.Clear();
        }

        public void Insert(int index, ref StringSlice element)
        {
            if (WriteUnlocked || WriteLockIgnoreLayout)
            {
                if (IsContentView) TargetContentView.Insert(index, string.Empty);
                else TargetContainer.Insert(index, Create(ref element));
            }
        }

        public void Insert(int index, StringSlice element)
        {
            Insert(index, ref element);
        }

        public void Replace(int index, ref StringSlice element)
        {
            var blockIndex = BlockHeads.IndexOf(IsContentView ? TargetContentView.ElementAt(index) : (ILine) TargetContainer.ElementAt(index));
            if (blockIndex >= 0)
            {
                //Debug.Log($"<color=#AA33AA88>    SHOULD WE REMOVE BLOCK <b>{blockIndex}</b> IN REPLACE() WHILE PROCESSING LAYOUT CHANGES</color>");
                RemoveBlockAt(blockIndex);
            }

            if (IsContentView)
            {
                if (WriteUnlocked)
                {
                    TargetContentView.Replace(index, element.ToStringLine().Replace("\\", "\\\u0003"));
                }
                
                return;
            }
            
            var target = ElementAt(index);

            if (WriteUnlocked && target != null)
                target.text = element.ToStringLine().Replace(@"\", @"\\");
        }

        public void Replace(int index, StringSlice element)
        {
            Replace(index, ref element);
        }

        private void Replace(int index, string element)
        {
            if (index >= 0 && index < (IsContentView ? TargetContentView.childCount : TargetContainer.childCount))
            {
                if (IsContentView) TargetContentView.Replace(index, element);
                else ((TextLine) TargetContainer.ElementAt(index)).text = element;
            }
        }

        public TextLine ElementAt(int index)
        {
            if (IsContentView)
                return (TextLine) TargetContentView.TextElementAt(index);
            
            return (TextLine) ((index >= 0 && index < TargetContainer.childCount) ? TargetContainer.ElementAt(index) : null);
        }

        public void RemoveAt(int index)
        {
            if (index >= (IsContentView ? TargetContentView.childCount : TargetContainer.childCount))
                return;
            
            var blockIndex = BlockHeads.IndexOf(IsContentView ? TargetContentView.ElementAt(index) : (ILine) TargetContainer.ElementAt(index));
            if (blockIndex >= 0)
            {
                // Debug.Log($"<color=#AA33AAFF>    REMOVE BLOCK <b>{blockIndex}</b> WHILE PROCESSING LAYOUT CHANGES</color>");
                RemoveBlockAt(blockIndex);
            }

            if (WriteUnlocked || WriteLockIgnoreLayout)
            {
                if (IsContentView)
                {
                    TargetContentView.RemoveAt(index);
                }
                else TargetContainer.RemoveAt(index);
            }
        }

        public int Count()
        {
            return IsContentView ? TargetContentView.childCount : TargetContainer.childCount;
        }

        public int Length()
        {
            // Count and Length are the same here since LineBlockProcessor group
            // elements within "virtual" blocks instead
            return IsContentView ? TargetContentView.childCount : TargetContainer.childCount;
        }
    }
}
