using System;
using System.Collections.Generic;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// For each item in the enumerable, execute the action.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self)
            {
                action(item);
            }
        }
        
        public static void ForEach<T>(this IEnumerable<T> self, Func<T, T> action)
        {
            foreach (var item in self)
            {
                action(item);
            }
        }
        
        public static void AddSorted<T>(this List<T> list, T item, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (comparer == null)
                comparer = (IComparer<T>) Comparer<T>.Default;
            if (list.Count == 0)
                list.Add(item);
            else
            {
                T x = list[^1];
                T y = item;
                if (comparer.Compare(x, y) <= 0)
                    list.Add(item);
                else if (comparer.Compare(list[0], item) >= 0)
                    list.Insert(0, item);
                else
                {
                    int index = list.BinarySearch(item, comparer);
                    if (index < 0)
                        index = ~index;
                    list.Insert(index, item);
                }
            }
        }
    }
}
