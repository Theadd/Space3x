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
    }
}
