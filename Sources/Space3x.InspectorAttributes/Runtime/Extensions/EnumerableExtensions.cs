﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Internal;

namespace Space3x.InspectorAttributes.Extensions
{
    [ExcludeFromDocs]
    public static class EnumerableExtensions
    {
        /// <summary>
        /// For each item in the enumerable, execute the action.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self)
                action(item);
        }
        
        /// <summary>
        /// For each item in the enumerable, execute the action.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> self, Func<T, T> action)
        {
            foreach (var item in self)
                action(item);
        }
        
        /// <summary>
        /// Foreach with the element's index being enumerated.
        /// </summary>
        /// <example>
        /// <code>
        ///     foreach (var (item, index) in collection.WithIndex()) { }
        /// </code>
        /// </example>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) => 
            self.Select((item, index) => (item, index));

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