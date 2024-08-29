using System;
using System.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Space3x.Properties.Types.Editor
{
    [InitializeOnLoad]
    public class SerializedPropertyNodeImplementation : PropertyNodeImplementationBase, IPropertyNodeImplementation, ICreatablePropertyNodeImplementation
    {
        private SerializedProperty m_SerializedProperty;

        static SerializedPropertyNodeImplementation() =>
            VirtualPropertyNode.RegisterImplementationProvider(new SerializedPropertyNodeImplementation(), 0);
        
        IPropertyNodeImplementation ICreatablePropertyNodeImplementation.Create(object property)
        {
            var instance = new SerializedPropertyNodeImplementation();
            instance.m_SerializedProperty = property switch
            {
                SerializedProperty serializedProperty => serializedProperty,
                IPropertyNode propertyNode => propertyNode.GetSerializedProperty(),
                _ => throw new ArgumentException("Unexpected property type", nameof(property))
            };
            return instance;
        }
        
        public string name => m_SerializedProperty.name;

        public SerializedProperty serializedProperty => m_SerializedProperty;
        
        public SerializedObject serializedObject => m_SerializedProperty.serializedObject;

        public Object exposedReferenceValue
        {
            get => m_SerializedProperty.exposedReferenceValue;
            set => m_SerializedProperty.exposedReferenceValue = value;
        }

        // public IPropertyNodeImplementation Copy() => (VirtualPropertyNode)m_SerializedProperty.Copy();
        public IPropertyNodeImplementation Copy() => ((ICreatablePropertyNodeImplementation)this).Create(m_SerializedProperty.Copy());

        public IPropertyNodeImplementation FindPropertyRelative(string relativePropertyPath) => 
            ((ICreatablePropertyNodeImplementation)this).Create(m_SerializedProperty.FindPropertyRelative(relativePropertyPath));

        // public IEnumerator GetEnumerator() => m_SerializedProperty.GetEnumerator();
        public IEnumerator GetEnumerator()
        {
            if (this.isArray)
            {
                for (int i = 0; i < this.arraySize; ++i)
                    yield return (VirtualPropertyNode) this.GetArrayElementAtIndex(i);
            }
            else
            {
                IPropertyNodeImplementation end = this.GetEndProperty();
                while (this.NextVisible(true) && !VirtualPropertyNode.EqualContents(this, end))
                    yield return (VirtualPropertyNode) this;
                end = null;
            }
        }

        public IPropertyNodeImplementation GetArrayElementAtIndex(int index) =>
            ((ICreatablePropertyNodeImplementation)this).Create(m_SerializedProperty.GetArrayElementAtIndex(index));

        public object boxedValue
        {
            get => m_SerializedProperty.boxedValue;
            set => m_SerializedProperty.boxedValue = value;
        }

        public bool NextVisible(bool enterChildren) => m_SerializedProperty.NextVisible(enterChildren);

        public void ClearArray() => m_SerializedProperty.ClearArray();

        public void Dispose() => m_SerializedProperty.Dispose();

        public bool hasMultipleDifferentValues => m_SerializedProperty.hasMultipleDifferentValues;

        public string displayName => m_SerializedProperty.displayName;

        public string type => m_SerializedProperty.type;

        public string arrayElementType => m_SerializedProperty.arrayElementType;

        public string tooltip => m_SerializedProperty.tooltip;

        public int depth => m_SerializedProperty.depth;

        public string propertyPath => m_SerializedProperty.propertyPath;

        public bool editable => m_SerializedProperty.editable;

        public bool isAnimated => m_SerializedProperty.isAnimated;

        public bool isExpanded
        {
            get => m_SerializedProperty.isExpanded;
            set => m_SerializedProperty.isExpanded = value;
        }

        public bool hasChildren => m_SerializedProperty.hasChildren;

        public bool hasVisibleChildren => m_SerializedProperty.hasVisibleChildren;

        public bool isInstantiatedPrefab => m_SerializedProperty.isInstantiatedPrefab;

        public bool prefabOverride
        {
            get => m_SerializedProperty.prefabOverride;
            set => m_SerializedProperty.prefabOverride = value;
        }

        public bool isDefaultOverride => m_SerializedProperty.isDefaultOverride;

        public SerializedPropertyType propertyType => m_SerializedProperty.propertyType;

        public SerializedPropertyNumericType numericType => m_SerializedProperty.numericType;

        public int intValue
        {
            get => m_SerializedProperty.intValue;
            set => m_SerializedProperty.intValue = value;
        }

        public long longValue
        {
            get => m_SerializedProperty.longValue;
            set => m_SerializedProperty.longValue = value;
        }

        public ulong ulongValue
        {
            get => m_SerializedProperty.ulongValue;
            set => m_SerializedProperty.ulongValue = value;
        }

        public uint uintValue
        {
            get => m_SerializedProperty.uintValue;
            set => m_SerializedProperty.uintValue = value;
        }

        public bool boolValue
        {
            get => m_SerializedProperty.boolValue;
            set => m_SerializedProperty.boolValue = value;
        }

        public float floatValue
        {
            get => m_SerializedProperty.floatValue;
            set => m_SerializedProperty.floatValue = value;
        }

        public double doubleValue
        {
            get => m_SerializedProperty.doubleValue;
            set => m_SerializedProperty.doubleValue = value;
        }

        public string stringValue
        {
            get => m_SerializedProperty.stringValue;
            set => m_SerializedProperty.stringValue = value;
        }

        public Color colorValue
        {
            get => m_SerializedProperty.colorValue;
            set => m_SerializedProperty.colorValue = value;
        }

        public AnimationCurve animationCurveValue
        {
            get => m_SerializedProperty.animationCurveValue;
            set => m_SerializedProperty.animationCurveValue = value;
        }

        public Gradient gradientValue
        {
            get => m_SerializedProperty.gradientValue;
            set => m_SerializedProperty.gradientValue = value;
        }

        public Object objectReferenceValue
        {
            get => m_SerializedProperty.objectReferenceValue;
            set => m_SerializedProperty.objectReferenceValue = value;
        }

        public object managedReferenceValue
        {
            get => m_SerializedProperty.managedReferenceValue;
            set => m_SerializedProperty.managedReferenceValue = value;
        }

        public long managedReferenceId
        {
            get => m_SerializedProperty.managedReferenceId;
            set => m_SerializedProperty.managedReferenceId = value;
        }

        public string managedReferenceFullTypename => m_SerializedProperty.managedReferenceFullTypename;

        public string managedReferenceFieldTypename => m_SerializedProperty.managedReferenceFieldTypename;

        public int objectReferenceInstanceIDValue
        {
            get => m_SerializedProperty.objectReferenceInstanceIDValue;
            set => m_SerializedProperty.objectReferenceInstanceIDValue = value;
        }

        public int enumValueIndex
        {
            get => m_SerializedProperty.enumValueIndex;
            set => m_SerializedProperty.enumValueIndex = value;
        }
        
        public int enumValueFlag
        {
            get => intValue;
            set => intValue = value;
        }

        public string[] enumNames => m_SerializedProperty.enumNames;

        public string[] enumDisplayNames => m_SerializedProperty.enumDisplayNames;

        public Vector2 vector2Value
        {
            get => m_SerializedProperty.vector2Value;
            set => m_SerializedProperty.vector2Value = value;
        }

        public Vector3 vector3Value
        {
            get => m_SerializedProperty.vector3Value;
            set => m_SerializedProperty.vector3Value = value;
        }

        public Vector4 vector4Value
        {
            get => m_SerializedProperty.vector4Value;
            set => m_SerializedProperty.vector4Value = value;
        }

        public Vector2Int vector2IntValue
        {
            get => m_SerializedProperty.vector2IntValue;
            set => m_SerializedProperty.vector2IntValue = value;
        }

        public Vector3Int vector3IntValue
        {
            get => m_SerializedProperty.vector3IntValue;
            set => m_SerializedProperty.vector3IntValue = value;
        }

        public Quaternion quaternionValue
        {
            get => m_SerializedProperty.quaternionValue;
            set => m_SerializedProperty.quaternionValue = value;
        }

        public Rect rectValue
        {
            get => m_SerializedProperty.rectValue;
            set => m_SerializedProperty.rectValue = value;
        }

        public RectInt rectIntValue
        {
            get => m_SerializedProperty.rectIntValue;
            set => m_SerializedProperty.rectIntValue = value;
        }

        public Bounds boundsValue
        {
            get => m_SerializedProperty.boundsValue;
            set => m_SerializedProperty.boundsValue = value;
        }

        public BoundsInt boundsIntValue
        {
            get => m_SerializedProperty.boundsIntValue;
            set => m_SerializedProperty.boundsIntValue = value;
        }

        public Hash128 hash128Value
        {
            get => m_SerializedProperty.hash128Value;
            set => m_SerializedProperty.hash128Value = value;
        }

        public bool Next(bool enterChildren) => m_SerializedProperty.Next(enterChildren);

        public void Reset() => m_SerializedProperty.Reset();

        public int CountRemaining() => m_SerializedProperty.CountRemaining();

        public int CountInProperty() => m_SerializedProperty.CountInProperty();

        public bool DuplicateCommand() => m_SerializedProperty.DuplicateCommand();

        public bool DeleteCommand() => m_SerializedProperty.DeleteCommand();

        public IPropertyNodeImplementation GetEndProperty(bool includeInvisible = false)
        {
            IPropertyNodeImplementation endProperty = this.Copy();
            if (includeInvisible)
                endProperty.Next(false);
            else
                endProperty.NextVisible(false);
            return endProperty;
        }

        public bool isArray => m_SerializedProperty.isArray;

        public int arraySize
        {
            get => m_SerializedProperty.arraySize;
            set => m_SerializedProperty.arraySize = value;
        }

        public int minArraySize => m_SerializedProperty.minArraySize;

        public void InsertArrayElementAtIndex(int index) => m_SerializedProperty.InsertArrayElementAtIndex(index);

        public void DeleteArrayElementAtIndex(int index) => m_SerializedProperty.DeleteArrayElementAtIndex(index);

        public bool MoveArrayElement(int srcIndex, int dstIndex) => m_SerializedProperty.MoveArrayElement(srcIndex, dstIndex);

        public bool isFixedBuffer => m_SerializedProperty.isFixedBuffer;

        public int fixedBufferSize => m_SerializedProperty.fixedBufferSize;

        public IPropertyNodeImplementation GetFixedBufferElementAtIndex(int index) => 
            ((ICreatablePropertyNodeImplementation)this).Create(m_SerializedProperty.GetFixedBufferElementAtIndex(index));

        public uint contentHash => m_SerializedProperty.contentHash;

        public VisualElement CreatePropertyField(bool bindProperty = false, string label = null)
        {
            var propertyField = label == null
                ? new PropertyField(m_SerializedProperty)
                : new PropertyField(m_SerializedProperty, label);
            if (bindProperty)
                propertyField.BindProperty(m_SerializedProperty);
            return propertyField;
        }
    }
}
