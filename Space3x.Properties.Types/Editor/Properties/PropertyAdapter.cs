using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Space3x.Properties.Types.Editor
{
    public class PropertyAdapter : IPropertyNodeImplementation
    {
        public static bool EqualContents(IPropertyNodeImplementation x, IPropertyNodeImplementation y) =>
            (x?.contentHash ?? 0) == (y?.contentHash ?? 0);
        public static bool DataEquals(IPropertyNodeImplementation x, IPropertyNodeImplementation y) => 
            throw new NotImplementedException();

        private static int currentProviderPriority = 0;
        private static IPropertyNodeImplementation defaultProvider = null;
        
        public static void RegisterImplementationProvider(IPropertyNodeImplementation provider, int priority = 0)
        {
            if (currentProviderPriority < priority) return;
            if (provider is not ICreatablePropertyNodeImplementation)
                throw new Exception($"Specified {nameof(IPropertyNodeImplementation)} does not implement " +
                                    $"required {nameof(ICreatablePropertyNodeImplementation)} interface in " +
                                    $"{nameof(PropertyAdapter)}.{nameof(RegisterImplementationProvider)}.");
            defaultProvider = provider;
            currentProviderPriority = priority;
        }

        private IPropertyNodeImplementation m_Impl;

        protected PropertyAdapter(object target) =>
            m_Impl = target is IPropertyNodeImplementation srcImpl
                ? srcImpl
                : defaultProvider is ICreatablePropertyNodeImplementation instancer
                    ? instancer.Create(target)
                    : throw new NotImplementedException(
                        $"No valid implementation provider registered in {nameof(PropertyAdapter)}.");

        public static PropertyAdapter Create(object target) =>
            target switch
            {
                PropertyAdapter other => other,
                _ => new PropertyAdapter(target)
            };

        public static implicit operator PropertyAdapter(SerializedProperty other) => Create(other);
        public static implicit operator SerializedProperty(PropertyAdapter other) => other.serializedProperty;
        public static implicit operator PropertyAdapter(PropertyNodeImplementationBase other) => Create(other);

        public string name => m_Impl.name;
        
        public SerializedProperty serializedProperty => m_Impl.serializedProperty;
        
        public SerializedObject serializedObject => m_Impl.serializedObject;
        
        public UnityEngine.Object exposedReferenceValue
        {
            get => m_Impl.exposedReferenceValue; 
            set => m_Impl.exposedReferenceValue = value;
        }

        IPropertyNodeImplementation IPropertyNodeImplementation.Copy() => m_Impl.Copy();
        
        public PropertyAdapter Copy() => Create(((IPropertyNodeImplementation)this).Copy());

        IPropertyNodeImplementation IPropertyNodeImplementation.FindPropertyRelative(string relativePropertyPath) => 
            m_Impl.FindPropertyRelative(relativePropertyPath);
        
        public PropertyAdapter FindPropertyRelative(string relativePropertyPath) => 
            Create(((IPropertyNodeImplementation)this).FindPropertyRelative(relativePropertyPath));

        public IEnumerator GetEnumerator() => m_Impl.GetEnumerator();

        IPropertyNodeImplementation IPropertyNodeImplementation.GetArrayElementAtIndex(int index) => 
            m_Impl.GetArrayElementAtIndex(index);
        
        public PropertyAdapter GetArrayElementAtIndex(int index) => 
            Create(((IPropertyNodeImplementation)this).GetArrayElementAtIndex(index));

        public object boxedValue
        {
            get => m_Impl.boxedValue;
            set => m_Impl.boxedValue = value;
        }

        public bool NextVisible(bool enterChildren) => m_Impl.NextVisible(enterChildren);

        public void ClearArray() => m_Impl.ClearArray();

        public void Dispose() => m_Impl.Dispose();

        public bool hasMultipleDifferentValues => m_Impl.hasMultipleDifferentValues;

        public string displayName => m_Impl.displayName;

        public string type => m_Impl.type;

        public string arrayElementType => m_Impl.arrayElementType;

        public string tooltip => m_Impl.tooltip;

        public int depth => m_Impl.depth;

        public string propertyPath => m_Impl.propertyPath;

        public bool editable => m_Impl.editable;

        public bool isAnimated => m_Impl.isAnimated;

        public bool isExpanded
        {
            get => m_Impl.isExpanded;
            set => m_Impl.isExpanded = value;
        }

        public bool hasChildren => m_Impl.hasChildren;

        public bool hasVisibleChildren => m_Impl.hasVisibleChildren;

        public bool isInstantiatedPrefab => m_Impl.isInstantiatedPrefab;

        public bool prefabOverride
        {
            get => m_Impl.prefabOverride;
            set => m_Impl.prefabOverride = value;
        }

        public bool isDefaultOverride => m_Impl.isDefaultOverride;

        public SerializedPropertyType propertyType => m_Impl.propertyType;

        public SerializedPropertyNumericType numericType => m_Impl.numericType;

        public int intValue
        {
            get => m_Impl.intValue;
            set => m_Impl.intValue = value;
        }

        public long longValue
        {
            get => m_Impl.longValue;
            set => m_Impl.longValue = value;
        }

        public ulong ulongValue
        {
            get => m_Impl.ulongValue;
            set => m_Impl.ulongValue = value;
        }

        public uint uintValue
        {
            get => m_Impl.uintValue;
            set => m_Impl.uintValue = value;
        }

        public bool boolValue
        {
            get => m_Impl.boolValue;
            set => m_Impl.boolValue = value;
        }

        public float floatValue
        {
            get => m_Impl.floatValue;
            set => m_Impl.floatValue = value;
        }

        public double doubleValue
        {
            get => m_Impl.doubleValue;
            set => m_Impl.doubleValue = value;
        }

        public string stringValue
        {
            get => m_Impl.stringValue;
            set => m_Impl.stringValue = value;
        }

        public Color colorValue
        {
            get => m_Impl.colorValue;
            set => m_Impl.colorValue = value;
        }

        public AnimationCurve animationCurveValue
        {
            get => m_Impl.animationCurveValue;
            set => m_Impl.animationCurveValue = value;
        }

        public Gradient gradientValue
        {
            get => m_Impl.gradientValue;
            set => m_Impl.gradientValue = value;
        }

        public Object objectReferenceValue
        {
            get => m_Impl.objectReferenceValue;
            set => m_Impl.objectReferenceValue = value;
        }

        public object managedReferenceValue
        {
            get => m_Impl.managedReferenceValue;
            set => m_Impl.managedReferenceValue = value;
        }

        public long managedReferenceId
        {
            get => m_Impl.managedReferenceId;
            set => m_Impl.managedReferenceId = value;
        }

        public string managedReferenceFullTypename => m_Impl.managedReferenceFullTypename;

        public string managedReferenceFieldTypename => m_Impl.managedReferenceFieldTypename;

        public int objectReferenceInstanceIDValue
        {
            get => m_Impl.objectReferenceInstanceIDValue;
            set => m_Impl.objectReferenceInstanceIDValue = value;
        }

        public int enumValueIndex
        {
            get => m_Impl.enumValueIndex;
            set => m_Impl.enumValueIndex = value;
        }
        
        public int enumValueFlag
        {
            get => m_Impl.enumValueFlag;
            set => m_Impl.enumValueFlag = value;
        }

        public string[] enumNames => m_Impl.enumNames;

        public string[] enumDisplayNames => m_Impl.enumDisplayNames;

        public Vector2 vector2Value
        {
            get => m_Impl.vector2Value;
            set => m_Impl.vector2Value = value;
        }

        public Vector3 vector3Value
        {
            get => m_Impl.vector3Value;
            set => m_Impl.vector3Value = value;
        }

        public Vector4 vector4Value
        {
            get => m_Impl.vector4Value;
            set => m_Impl.vector4Value = value;
        }

        public Vector2Int vector2IntValue
        {
            get => m_Impl.vector2IntValue;
            set => m_Impl.vector2IntValue = value;
        }

        public Vector3Int vector3IntValue
        {
            get => m_Impl.vector3IntValue;
            set => m_Impl.vector3IntValue = value;
        }

        public Quaternion quaternionValue
        {
            get => m_Impl.quaternionValue;
            set => m_Impl.quaternionValue = value;
        }

        public Rect rectValue
        {
            get => m_Impl.rectValue;
            set => m_Impl.rectValue = value;
        }

        public RectInt rectIntValue
        {
            get => m_Impl.rectIntValue;
            set => m_Impl.rectIntValue = value;
        }

        public Bounds boundsValue
        {
            get => m_Impl.boundsValue;
            set => m_Impl.boundsValue = value;
        }

        public BoundsInt boundsIntValue
        {
            get => m_Impl.boundsIntValue;
            set => m_Impl.boundsIntValue = value;
        }

        public Hash128 hash128Value
        {
            get => m_Impl.hash128Value;
            set => m_Impl.hash128Value = value;
        }

        public bool Next(bool enterChildren) => m_Impl.Next(enterChildren);

        public void Reset() => m_Impl.Reset();

        public int CountRemaining() => m_Impl.CountRemaining();

        public int CountInProperty() => m_Impl.CountInProperty();

        public bool DuplicateCommand() => m_Impl.DuplicateCommand();

        public bool DeleteCommand() => m_Impl.DeleteCommand();

        IPropertyNodeImplementation IPropertyNodeImplementation.GetEndProperty() => 
            m_Impl.GetEndProperty();
        
        IPropertyNodeImplementation IPropertyNodeImplementation.GetEndProperty(bool includeInvisible) => 
            m_Impl.GetEndProperty(includeInvisible);
        
        public PropertyAdapter GetEndProperty(bool includeInvisible = false) => 
            Create(((IPropertyNodeImplementation)this).GetEndProperty(includeInvisible));

        public bool isArray => m_Impl.isArray;

        public int arraySize
        {
            get => m_Impl.arraySize;
            set => m_Impl.arraySize = value;
        }

        public int minArraySize => m_Impl.minArraySize;

        public void InsertArrayElementAtIndex(int index) => m_Impl.InsertArrayElementAtIndex(index);

        public void DeleteArrayElementAtIndex(int index) => m_Impl.DeleteArrayElementAtIndex(index);

        public bool MoveArrayElement(int srcIndex, int dstIndex) => m_Impl.MoveArrayElement(srcIndex, dstIndex);

        public bool isFixedBuffer => m_Impl.isFixedBuffer;

        public int fixedBufferSize => m_Impl.fixedBufferSize;

        IPropertyNodeImplementation IPropertyNodeImplementation.GetFixedBufferElementAtIndex(int index) => 
            m_Impl.GetFixedBufferElementAtIndex(index);
        
        public PropertyAdapter GetFixedBufferElementAtIndex(int index) => 
            Create(((IPropertyNodeImplementation)this).GetFixedBufferElementAtIndex(index));

        public uint contentHash => m_Impl.contentHash;

        public VisualElement CreatePropertyField(bool bindProperty = false, string label = null) =>
            m_Impl.CreatePropertyField(bindProperty, label);

        public IPropertyNode GetPropertyNode() => m_Impl.GetPropertyNode();
    }
}
