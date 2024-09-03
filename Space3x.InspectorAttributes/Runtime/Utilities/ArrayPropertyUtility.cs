using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Space3x.Properties.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    public static class ArrayPropertyUtility
    {
        private static MethodInfo s_ResizeArrayInternal = null;
        private static MethodInfo s_DeleteArrayElementAtIndexInternal = null;
        private static MethodInfo s_InsertArrayElementAtIndexInternal = null;
        private static MethodInfo s_ClearArrayInternal = null;
        private const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Resizes an array. If a null reference is passed, it will allocate the desired array.
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">Target array to resize</param>
        /// <param name="capacity">New size of array to resize</param>
        private static void ResizeArray<T>(ref T[] array, int capacity)
        {
            if (array == null)
            {
                array = new T[capacity];
                return;
            }

            Array.Resize<T>(ref array, capacity);
        }

        public static void ResizeArrayInProperty(IPropertyNode property, int arraySize)
        {
            var itemType = property.GetUnderlyingElementType();
            s_ResizeArrayInternal ??= typeof(ArrayPropertyUtility).GetMethod("ResizeArrayInternal",
                BindingFlags.Static | BindingFlags.NonPublic);
            var method = s_ResizeArrayInternal!.MakeGenericMethod(itemType);
            method.Invoke(null, StaticFlags, null, new object[] { property.GetValue(), property, arraySize },
                CultureInfo.InvariantCulture);
        }

        [UsedImplicitly]
        private static void ResizeArrayInternal<TItemValue>(IList target, IPropertyNode property, int arraySize)
        {
            IList<TItemValue> targetList = ((IList<TItemValue>)(target?.Cast<TItemValue>() ?? new TItemValue[] {}));
            TItemValue[] targetArray = targetList.ToArray<TItemValue>();
            ResizeArray<TItemValue>(ref targetArray, arraySize);
            property.SetValue(property.IsArray() ? targetArray.ToArray() : targetArray.ToList());
        }

        public static void DeleteArrayElementInProperty(IPropertyNode property, int atIndex)
        {
            var itemType = property.GetUnderlyingElementType();
            s_DeleteArrayElementAtIndexInternal ??= typeof(ArrayPropertyUtility).GetMethod("DeleteArrayElementAtIndexInternal",
                BindingFlags.Static | BindingFlags.NonPublic);
            var method = s_DeleteArrayElementAtIndexInternal!.MakeGenericMethod(itemType);
            method.Invoke(null, StaticFlags, null, new object[] { property.GetValue(), property, atIndex },
                CultureInfo.InvariantCulture);
        }

        [UsedImplicitly]
        private static void DeleteArrayElementAtIndexInternal<TItemValue>(IList target, IPropertyNode property, int index)
        {
            List<TItemValue> targetList = new List<TItemValue>((IList<TItemValue>)(target?.Cast<TItemValue>() ?? new TItemValue[] {}));
            targetList.RemoveAt(index);
            property.SetValue(property.IsArray() ? targetList.ToArray() : targetList.ToList());
        }

        public static void InsertArrayElementAtIndex(IPropertyNode property, int index)
        {
            var itemType = property.GetUnderlyingElementType();
            s_InsertArrayElementAtIndexInternal ??= typeof(ArrayPropertyUtility).GetMethod("InsertArrayElementAtIndexInternal",
                BindingFlags.Static | BindingFlags.NonPublic);
            var method = s_InsertArrayElementAtIndexInternal!.MakeGenericMethod(itemType);
            method.Invoke(null, StaticFlags, null, new object[] { property.GetValue(), property, index },
                CultureInfo.InvariantCulture);
        }

        [UsedImplicitly]
        private static void InsertArrayElementAtIndexInternal<TItemValue>(IList target, IPropertyNode property, int index)
        {
            List<TItemValue> targetList = new List<TItemValue>((IList<TItemValue>)(target?.Cast<TItemValue>() ?? new TItemValue[] {}));
            targetList.Insert(index, default(TItemValue));
            property.SetValue(property.IsArray() ? targetList.ToArray() : targetList.ToList());
        }

        public static bool MoveArrayElement(IPropertyNode property, int srcIndex, int dstIndex)
        {
            try
            {
                object removedItem = property.GetArrayElementAtIndex(srcIndex).GetValue();
                DeleteArrayElementInProperty(property, srcIndex);
                InsertArrayElementAtIndex(property, dstIndex);
                property.GetArrayElementAtIndex(dstIndex).SetValue(removedItem);
                for (var i = Math.Min(srcIndex, dstIndex); i <= Math.Max(srcIndex, dstIndex); i++)
                    (property as INonSerializedPropertyNode)?.NotifyValueChanged(property.GetArrayElementAtIndex(i));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public static void ClearArray(IPropertyNode property)
        {
            var itemType = property.GetUnderlyingElementType();
            s_ClearArrayInternal ??= typeof(ArrayPropertyUtility).GetMethod("ClearArrayInternal",
                BindingFlags.Static | BindingFlags.NonPublic);
            var method = s_ClearArrayInternal!.MakeGenericMethod(itemType);
            method.Invoke(null, StaticFlags, null, new object[] { property.GetValue(), property },
                CultureInfo.InvariantCulture);
        }

        [UsedImplicitly]
        private static void ClearArrayInternal<TItemValue>(IList target, IPropertyNode property)
        {
            IList<TItemValue> v3 = (IList<TItemValue>)(target?.Cast<TItemValue>() ?? new TItemValue[] {});
            v3.Clear();
            property.SetValue(property.IsArray() ? v3.ToArray() : v3.ToList());
        }
    }
}
