using System;
using Space3x.UiToolkit.SlicedText.Iterators;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.Processors
{
    public interface ILineBlockProcessor<out T>
    {
        Action<ChangeRecord> OnReadyChangeRecord { get; set; }

        IColorize Colorizer { get; set; }

        bool MuteChangeEvents { get; set; }

        bool TryGetValueFromCache(int blockIndex, out string text);
        
        StringSliceGroup Editor { get; set; }

        VisualElement TargetContainer { get; set; }

        void Clear();

        void Insert(int index, ref StringSlice element);
        void Insert(int index, StringSlice element);

        void Replace(int index, ref StringSlice element);
        void Replace(int index, StringSlice element);

        T ElementAt(int index);

        void RemoveAt(int index);

        void GetReadyForChanges();

        void CommitChanges();

        /// <summary>
        /// Total number of blocks of elements
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// Total number of elements among all blocks
        /// </summary>
        /// <returns></returns>
        int Length();
    }
}
