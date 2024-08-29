using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;

namespace Space3x.InspectorAttributes.Editor.Utilities
{
    [ExcludeFromDocs]
    public static class SerializedUtility
    {
        private const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        
        public static SerializedPropertyNumericType GetPropertyNumericType(IPropertyNode property)
        {
            Type underlyingType = property.GetUnderlyingType();
            return underlyingType switch
            {
                not null when underlyingType == typeof(sbyte) => SerializedPropertyNumericType.Int8,
                not null when underlyingType == typeof(byte) => SerializedPropertyNumericType.UInt8,
                not null when underlyingType == typeof(short) => SerializedPropertyNumericType.Int16,
                not null when underlyingType == typeof(ushort) => SerializedPropertyNumericType.UInt16,
                not null when underlyingType == typeof(int) => SerializedPropertyNumericType.Int32,
                not null when underlyingType == typeof(uint) => SerializedPropertyNumericType.UInt32,
                not null when underlyingType == typeof(long) => SerializedPropertyNumericType.Int64,
                not null when underlyingType == typeof(ulong) => SerializedPropertyNumericType.UInt64,
                not null when underlyingType == typeof(double) => SerializedPropertyNumericType.Double,
                not null when underlyingType == typeof(float) => SerializedPropertyNumericType.Float,
                _ => SerializedPropertyNumericType.Unknown
            };
        }
        
        public static SerializedPropertyType GetPropertyType(IPropertyNode property)
        {
            Type underlyingType = property.GetUnderlyingType();
            if (typeof(UnityEngine.Object).IsAssignableFrom(underlyingType))
                return SerializedPropertyType.ObjectReference;
            if (property.IsArrayOrList() || property.HasChildren())
                return SerializedPropertyType.Generic;
            if (underlyingType.IsEnum)
                return SerializedPropertyType.Enum;
            // TODO: LayerMask, ArraySize, ExposedReference, FixedBufferSize, RenderingLayerMask
            // NOTE: A ManagedReference property is always a serialized one.
            return underlyingType switch
            {
                not null when underlyingType == typeof(sbyte) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(byte) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(short) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(ushort) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(int) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(uint) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(long) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(ulong) => SerializedPropertyType.Integer,
                not null when underlyingType == typeof(double) => SerializedPropertyType.Float,
                not null when underlyingType == typeof(float) => SerializedPropertyType.Float,
                not null when underlyingType == typeof(string) => SerializedPropertyType.String,
                not null when underlyingType == typeof(bool) => SerializedPropertyType.Boolean,
                not null when underlyingType == typeof(Color) => SerializedPropertyType.Color,
                not null when underlyingType == typeof(Vector2) => SerializedPropertyType.Vector2,
                not null when underlyingType == typeof(Vector3) => SerializedPropertyType.Vector3,
                not null when underlyingType == typeof(Vector4) => SerializedPropertyType.Vector4,
                not null when underlyingType == typeof(Rect) => SerializedPropertyType.Rect,
                not null when underlyingType == typeof(char) => SerializedPropertyType.Character,
                not null when underlyingType == typeof(AnimationCurve) => SerializedPropertyType.AnimationCurve,
                not null when underlyingType == typeof(Bounds) => SerializedPropertyType.Bounds,
                not null when underlyingType == typeof(Gradient) => SerializedPropertyType.Gradient,
                not null when underlyingType == typeof(Quaternion) => SerializedPropertyType.Quaternion,
                not null when underlyingType == typeof(Vector2Int) => SerializedPropertyType.Vector2Int,
                not null when underlyingType == typeof(Vector3Int) => SerializedPropertyType.Vector3Int,
                not null when underlyingType == typeof(RectInt) => SerializedPropertyType.RectInt,
                not null when underlyingType == typeof(BoundsInt) => SerializedPropertyType.BoundsInt,
                not null when underlyingType == typeof(Hash128) => SerializedPropertyType.Hash128,
#if SPACE3X_DEBUG
                _ => throw new NotImplementedException()
#else
                _ => SerializedPropertyType.Generic
#endif
            };
        }
        
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
        
        private static MethodInfo s_ResizeArrayInternal = null;

        public static void ResizeArrayInProperty(IPropertyNode property, int arraySize)
        {
            var itemType = property.GetUnderlyingElementType();
            s_ResizeArrayInternal ??= typeof(SerializedUtility).GetMethod("ResizeArrayInternal",
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
        
        private static MethodInfo s_DeleteArrayElementAtIndexInternal = null;

        public static void DeleteArrayElementInProperty(IPropertyNode property, int atIndex)
        {
            var itemType = property.GetUnderlyingElementType();
            s_DeleteArrayElementAtIndexInternal ??= typeof(SerializedUtility).GetMethod("DeleteArrayElementAtIndexInternal",
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
        
        private static MethodInfo s_InsertArrayElementAtIndexInternal = null;

        public static void InsertArrayElementAtIndex(IPropertyNode property, int index)
        {
            var itemType = property.GetUnderlyingElementType();
            s_InsertArrayElementAtIndexInternal ??= typeof(SerializedUtility).GetMethod("InsertArrayElementAtIndexInternal",
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
        
        private static MethodInfo s_ClearArrayInternal = null;

        public static void ClearArray(IPropertyNode property)
        {
            var itemType = property.GetUnderlyingElementType();
            s_ClearArrayInternal ??= typeof(SerializedUtility).GetMethod("ClearArrayInternal",
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
