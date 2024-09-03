using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.Properties.Types
{
    public abstract class PropertyNodeImplementationBase
    {
        
    }
    
    public interface ICreatablePropertyNodeImplementation
    {
        IPropertyNodeImplementation Create(object property);
    }
    
    public interface IPropertyNodeImplementation
    {
        /// <inheritdoc cref="SerializedProperty.name"/>
        string name { get; }
        SerializedProperty serializedProperty { get; }
        /// <inheritdoc cref="SerializedProperty.serializedObject"/>
        SerializedObject serializedObject { get; }
        /// <inheritdoc cref="SerializedProperty.exposedReferenceValue"/>
        UnityEngine.Object exposedReferenceValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.Copy"/>
        IPropertyNodeImplementation Copy();
        /// <inheritdoc cref="SerializedProperty.FindPropertyRelative"/>
        IPropertyNodeImplementation FindPropertyRelative(string relativePropertyPath);

        /// <inheritdoc cref="SerializedProperty.GetEnumerator"/>
        IEnumerator GetEnumerator();
        /// <inheritdoc cref="SerializedProperty.GetArrayElementAtIndex"/>
        IPropertyNodeImplementation GetArrayElementAtIndex(int index);
        /// <inheritdoc cref="SerializedProperty.boxedValue"/>
        object boxedValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.NextVisible"/>
        bool NextVisible(bool enterChildren);
        /// <inheritdoc cref="SerializedProperty.ClearArray"/>
        void ClearArray();
        /// <inheritdoc cref="SerializedProperty.Dispose"/>
        void Dispose();
        /// <inheritdoc cref="SerializedProperty.hasMultipleDifferentValues"/>
        bool hasMultipleDifferentValues { get; }
        /// <inheritdoc cref="SerializedProperty.displayName"/>
        string displayName { get; }
        /// <inheritdoc cref="SerializedProperty.type"/>
        string type { get; }
        /// <inheritdoc cref="SerializedProperty.arrayElementType"/>
        string arrayElementType { get; }
        /// <inheritdoc cref="SerializedProperty.tooltip"/>
        string tooltip { get; }
        /// <inheritdoc cref="SerializedProperty.depth"/>
        int depth { get; }
        /// <inheritdoc cref="SerializedProperty.propertyPath"/>
        string propertyPath { get; }
        /// <inheritdoc cref="SerializedProperty.editable"/>
        bool editable { get; }
        /// <inheritdoc cref="SerializedProperty.isAnimated"/>
        bool isAnimated { get; }
        /// <inheritdoc cref="SerializedProperty.isExpanded"/>
        bool isExpanded { get; set; }
        /// <inheritdoc cref="SerializedProperty.hasChildren"/>
        bool hasChildren { get; }
        /// <inheritdoc cref="SerializedProperty.hasVisibleChildren"/>
        bool hasVisibleChildren { get; }
        /// <inheritdoc cref="SerializedProperty.isInstantiatedPrefab"/>
        bool isInstantiatedPrefab { get; }
        /// <inheritdoc cref="SerializedProperty.prefabOverride"/>
        bool prefabOverride { get; set; }
        /// <inheritdoc cref="SerializedProperty.isDefaultOverride"/>
        bool isDefaultOverride { get; }
        /// <inheritdoc cref="SerializedProperty.propertyType"/>
        SerializedPropertyType propertyType { get; }
        /// <inheritdoc cref="SerializedProperty.numericType"/>
        SerializedPropertyNumericType numericType { get; }
        /// <inheritdoc cref="SerializedProperty.intValue"/>
        int intValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.longValue"/>
        long longValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.ulongValue"/>
        ulong ulongValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.uintValue"/>
        uint uintValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.boolValue"/>
        bool boolValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.floatValue"/>
        float floatValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.doubleValue"/>
        double doubleValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.stringValue"/>
        string stringValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.colorValue"/>
        Color colorValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.animationCurveValue"/>
        AnimationCurve animationCurveValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.gradientValue"/>
        Gradient gradientValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.objectReferenceValue"/>
        UnityEngine.Object objectReferenceValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.managedReferenceValue"/>
        object managedReferenceValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.managedReferenceId"/>
        long managedReferenceId { get; set; }
        /// <inheritdoc cref="SerializedProperty.managedReferenceFullTypename"/>
        string managedReferenceFullTypename { get; }
        /// <inheritdoc cref="SerializedProperty.managedReferenceFieldTypename"/>
        string managedReferenceFieldTypename { get; }
        /// <inheritdoc cref="SerializedProperty.objectReferenceInstanceIDValue"/>
        int objectReferenceInstanceIDValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.enumValueIndex"/>
        int enumValueIndex { get; set; }
        /// <inheritdoc cref="SerializedProperty.enumValueFlag"/>
        int enumValueFlag { get; set; }
        /// <inheritdoc cref="SerializedProperty.enumNames"/>
        string[] enumNames { get; }
        /// <inheritdoc cref="SerializedProperty.enumDisplayNames"/>
        string[] enumDisplayNames { get; }
        /// <inheritdoc cref="SerializedProperty.vector2Value"/>
        Vector2 vector2Value { get; set; }
        /// <inheritdoc cref="SerializedProperty.vector3Value"/>
        Vector3 vector3Value { get; set; }
        /// <inheritdoc cref="SerializedProperty.vector4Value"/>
        Vector4 vector4Value { get; set; }
        /// <inheritdoc cref="SerializedProperty.vector2IntValue"/>
        Vector2Int vector2IntValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.vector3IntValue"/>
        Vector3Int vector3IntValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.quaternionValue"/>
        Quaternion quaternionValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.rectValue"/>
        Rect rectValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.rectIntValue"/>
        RectInt rectIntValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.boundsValue"/>
        Bounds boundsValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.boundsIntValue"/>
        BoundsInt boundsIntValue { get; set; }
        /// <inheritdoc cref="SerializedProperty.hash128Value"/>
        Hash128 hash128Value { get; set; }
        /// <inheritdoc cref="SerializedProperty.Next"/>
        bool Next(bool enterChildren);
        /// <inheritdoc cref="SerializedProperty.Reset"/>
        void Reset();
        /// <inheritdoc cref="SerializedProperty.CountRemaining"/>
        int CountRemaining();
        /// <inheritdoc cref="SerializedProperty.CountInProperty"/>
        int CountInProperty();
        /// <inheritdoc cref="SerializedProperty.DuplicateCommand"/>
        bool DuplicateCommand();
        /// <inheritdoc cref="SerializedProperty.DeleteCommand"/>
        bool DeleteCommand();
        /// <summary>
        /// Retrieves the SerializedProperty that defines the end range of this property.
        /// </summary>
        IPropertyNodeImplementation GetEndProperty();
        /// <summary>
        /// Retrieves the SerializedProperty that defines the end range of this property.
        /// </summary>
        IPropertyNodeImplementation GetEndProperty(bool includeInvisible);
        /// <inheritdoc cref="SerializedProperty.isArray"/>
        bool isArray { get; }
        /// <inheritdoc cref="SerializedProperty.arraySize"/>
        int arraySize { get; set; }
        /// <inheritdoc cref="SerializedProperty.minArraySize"/>
        int minArraySize { get; }
        /// <inheritdoc cref="SerializedProperty.InsertArrayElementAtIndex"/>
        void InsertArrayElementAtIndex(int index);
        /// <inheritdoc cref="SerializedProperty.DeleteArrayElementAtIndex"/>
        void DeleteArrayElementAtIndex(int index);
        /// <inheritdoc cref="SerializedProperty.MoveArrayElement"/>
        bool MoveArrayElement(int srcIndex, int dstIndex);
        /// <inheritdoc cref="SerializedProperty.isFixedBuffer"/>
        bool isFixedBuffer { get; }
        /// <inheritdoc cref="SerializedProperty.fixedBufferSize"/>
        int fixedBufferSize { get; }
        /// <inheritdoc cref="SerializedProperty.GetFixedBufferElementAtIndex"/>
        IPropertyNodeImplementation GetFixedBufferElementAtIndex(int index);
        /// <inheritdoc cref="SerializedProperty.contentHash"/>
        uint contentHash { get; }
        /// <summary>
        /// Creates a <see cref="PropertyField"/> for serialized properties or a <see cref="BindablePropertyField"/>
        /// for non-serialized ones. Optionally bound to that property.
        /// </summary>
        VisualElement CreatePropertyField(bool bindProperty = false, string label = null);
        /// <summary>
        /// Gets the IPropertyNode related to this property. 
        /// </summary>
        IPropertyNode GetPropertyNode();
    }
}
