using System;
using Space3x.Properties.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;

namespace Space3x.InspectorAttributes.Editor.Utilities
{
    [ExcludeFromDocs]
    public static class SerializedUtility
    {
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
    }
}
