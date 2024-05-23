using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor
{
    public struct StringFilter
    {
        public string Value { get; private set; }
        public bool Active { get; set; }

        public StringFilter(string value, bool active = true)
        {
            Value = value;
            Active = active;
        }
        
        public override string ToString() => Value;
    }

    public struct EnumFilter<T> where T : Enum
    {
        public T Value { get; private set; }
        public bool Active { get; set; }
        public int RawValue { get; private set; }
        public string DisplayName { get; set; }

        public EnumFilter(T value, string displayName = null)
        {
            Value = value;
            Active = true;
            RawValue = (int) (object) value;
            DisplayName = displayName;
        }

        public override string ToString() => string.IsNullOrEmpty(DisplayName) ? Enum.GetName(typeof(T), Value) : DisplayName;
        
        public static explicit operator T(EnumFilter<T> filter) => filter.Value;
        
        public static explicit operator EnumFilter<T>(T value) => new EnumFilter<T>(value);
        
        public static explicit operator int(EnumFilter<T> filter) => (int)filter.RawValue;

        public static IEnumerable<EnumFilter<T>> GetValues() => ((T[]) Enum.GetValues(typeof(T))).ToList<T>().Select(v => new EnumFilter<T>((T) v)).ToList();
    }
    
    public struct EnumFlagsFilter<T> where T : Enum
    {
        public T Value { get; private set; }
        public bool Active { get; set; }
        public int RawValue { get; private set; }

        public EnumFlagsFilter(T value)
        {
            Value = value;
            Active = true;
            RawValue = (int) (object) value;
            // var allValues = ((T[])Enum.GetValues(typeof(T))).ToList<T>().Select(v => new EnumFlagsFilter<T>((T) v)).ToList();
            // var allValuesX = Enum.GetNames(typeof(T)).To
        }

        public override string ToString() => Enum.GetName(typeof(T), Value);
        
        public static explicit operator T(EnumFlagsFilter<T> filter) => filter.Value;
        
        public static explicit operator EnumFlagsFilter<T>(T value) => new EnumFlagsFilter<T>(value);
        
        public static explicit operator int(EnumFlagsFilter<T> filter) => (int)filter.RawValue;

        public static IEnumerable<EnumFlagsFilter<T>> GetValues() => ((T[]) Enum.GetValues(typeof(T))).ToList<T>().Select(v => new EnumFlagsFilter<T>((T) v)).ToList();
    }

    public static class EnumFlagsFilterExtensions
    {
        public static int ToBitmask<T>(this IEnumerable<EnumFlagsFilter<T>> self) where T : Enum
        {
            int mask = 0;
            foreach (var filter in self)
                if (filter.Active) mask |= filter.RawValue;

            return mask;
        }
    }
}
