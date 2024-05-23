using System;
using System.Collections.Generic;
using System.Linq;

namespace Space3x.UiToolkit.SlicedText.Processors
{
    public abstract class ProcessorEventHandler
    {
        public abstract Action<ChangeRecord> OnReadyChangeRecord { get; set; }

        public virtual bool SingleInsertPerChangeRecord { get; set; } = false;

        public virtual bool MuteChangeEvents { get; set; } = false;

        /// <summary>
        /// <param name="int">Index of this block.</param>
        /// <param name="int">Line's 0-index based position of first line in block. (Index of block's first element within TargetContainer.Children().)</param>
        /// <param name="string">A string containing the lines within the block, before applying any formatting.</param>
        /// </summary>
        public event Action<int, int, string> OnBlockProcessed;
        
        /// <summary>
        /// <param name="int">Block index</param>
        /// </summary>
        public event Action<int> OnBlockRemoved;
        
        protected Stack<int> SeqOfBlocksToRemove = new Stack<int>();
        
        protected Stack<int> SeqOfBlocksToAdd = new Stack<int>();

        protected Dictionary<int, string> CachedRecords = new Dictionary<int, string>();
        
        public virtual void RemoveBlockAt(int blockIndex)
        {
            if (SeqOfBlocksToRemove.Count > 0 && blockIndex + 1 != SeqOfBlocksToRemove.Peek())
            {
                SendChangeRecordEvent();
            }
            SeqOfBlocksToRemove.Push(blockIndex);

            OnBlockRemoved?.Invoke(blockIndex);
        }

        public virtual void InsertBlockAt(int blockIndex, int lineIndex, string text)
        {
            if ((SeqOfBlocksToAdd.Count > 0 && SeqOfBlocksToAdd.Peek() + 1 != blockIndex) ||
                (SeqOfBlocksToAdd.Count == 0 && SeqOfBlocksToRemove.Count > 0 && SeqOfBlocksToRemove.Peek() != blockIndex))
                SendChangeRecordEvent();

            SeqOfBlocksToAdd.Push(blockIndex);
            if (CachedRecords.ContainsKey(blockIndex))
                CachedRecords[blockIndex] = text;
            else
                CachedRecords.Add(blockIndex, text);
            
            OnBlockProcessed?.Invoke(blockIndex, lineIndex, text);

            if (SingleInsertPerChangeRecord)
                SendChangeRecordEvent();
        }

        protected void SendChangeRecordEvent()
        {
            if (MuteChangeEvents || (SeqOfBlocksToRemove.Count == 0 && SeqOfBlocksToAdd.Count == 0))
                return;
            
            var record = new ChangeRecord()
            {
                Index = SeqOfBlocksToRemove.Count > 0 ? SeqOfBlocksToRemove.Peek() : SeqOfBlocksToAdd.Last(),
                RemoveCount = SeqOfBlocksToRemove.Count,
                InsertCount = SeqOfBlocksToAdd.Count,
                ElementAt = PopCachedChange
            };

            SeqOfBlocksToAdd.Clear();
            SeqOfBlocksToRemove.Clear();
            
            OnReadyChangeRecord?.Invoke(record);
        }

        string PopCachedChange(int blockIndex)
        {
            if (CachedRecords.TryGetValue(blockIndex, out string text))
            {
                CachedRecords.Remove(blockIndex);
                
                return text;
            }

            return string.Empty;
        }

        public virtual bool TryGetValueFromCache(int blockIndex, out string text)
        {
            if (CachedRecords.TryGetValue(blockIndex, out string blockText))
            {
                text = blockText;
                return true;
            }
            
            text = string.Empty;
            return false;
        }
    }
}
