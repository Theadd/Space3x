using System;
using System.Collections;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    [InitializeOnLoad]
    public class PropertyNodeImplementation : PropertyNodeImplementationBase, IPropertyNodeImplementation, ICreatablePropertyNodeImplementation
    {
        private IPropertyNode m_Property;
        private SerializedProperty m_SerializedProperty;
        private bool m_HasValidSerializedProperty;
        
        static PropertyNodeImplementation() =>
            PropertyAdapter.RegisterImplementationProvider(new PropertyNodeImplementation(), -1);

        IPropertyNodeImplementation ICreatablePropertyNodeImplementation.Create(object property)
        {
            var instance = new PropertyNodeImplementation();
            instance.m_Property = property switch
            {
                SerializedProperty serializedProp => serializedProp.GetPropertyNode(),
                IPropertyNode propertyNode => propertyNode,
                _ => throw new ArgumentException("Unexpected property type", nameof(property))
            };
            return instance;
        }
        
        public string name => m_Property.Name;

        public SerializedProperty serializedProperty
        {
            get
            {
                if (!m_HasValidSerializedProperty)
                {
                    m_SerializedProperty = m_Property.GetSerializedProperty();
                    m_HasValidSerializedProperty = true;
                }
                return m_SerializedProperty;
            }
        }

        public SerializedObject serializedObject => m_Property.GetSerializedObject();

        public UnityEngine.Object exposedReferenceValue
        {
            get => (UnityEngine.Object)boxedValue;
            set => boxedValue = value;
        }

        public IPropertyNodeImplementation Copy() => ((ICreatablePropertyNodeImplementation)this).Create(
            m_Property.HasSerializedProperty() ? serializedProperty?.Copy() : m_Property);

        public IPropertyNodeImplementation FindPropertyRelative(string relativePropertyPath) =>
            ((ICreatablePropertyNodeImplementation)this).Create(m_Property.HasSerializedProperty()
                ? serializedProperty?.FindPropertyRelative(relativePropertyPath)
                : m_Property.FindPropertyRelative(relativePropertyPath));

        public IEnumerator GetEnumerator()
        {
            if (this.isArray)
            {
                for (int i = 0; i < this.arraySize; ++i)
                    yield return (PropertyAdapter) this.GetArrayElementAtIndex(i);
            }
            else
            {
                IPropertyNodeImplementation end = this.GetEndProperty();
                while (this.NextVisible(true) && !PropertyAdapter.EqualContents(this, end))
                    yield return (PropertyAdapter) this;
                end = null;
            }
        }

        public IPropertyNodeImplementation GetArrayElementAtIndex(int index) =>
            ((ICreatablePropertyNodeImplementation)this).Create(m_Property.HasSerializedProperty()
                ? serializedProperty?.GetArrayElementAtIndex(index)
                : m_Property.GetArrayElementAtIndex(index));

        public object boxedValue
        {
            get => m_Property.GetValue();
            set => m_Property.SetValue(value);
        }

        public bool NextVisible(bool enterChildren)
        {
            if (enterChildren && m_Property.HasChildren())
            {
                PropertyAttributeController controller = null;
                if (((PropertyAttributeController)m_Property.GetController()).TryGetInstance(m_Property.PropertyPath, out controller))
                {
                    m_Property = controller.GetNextVisibleProperty(string.Empty);
                    m_HasValidSerializedProperty = false;
                }
                else
                {
                    controller = PropertyAttributeController.GetOrCreateInstance(m_Property,
                        m_Property.GetValue()?.GetType() ?? m_Property.GetUnderlyingType(), forceCreate: false);
                    if (controller != null)
                    {
                        m_Property = controller.GetNextVisibleProperty(string.Empty);
                        m_HasValidSerializedProperty = false;
                    }
                    else
                        throw new NotImplementedException("Couldn't get controller for children properties on " + m_Property);
                }
            }
            else
            {
                m_Property = ((PropertyAttributeController)m_Property.GetController()).GetNextVisibleProperty(m_Property.Name);
                m_HasValidSerializedProperty = false;
            }

            return m_Property != null;
        }

        public void ClearArray()
        {
            if (m_Property.HasSerializedProperty())
                serializedProperty.ClearArray();
            else if (m_Property.IsArrayOrList())
                ArrayPropertyUtility.ClearArray(m_Property);
        }

        public void Dispose() { }

        public bool hasMultipleDifferentValues => serializedProperty?.hasMultipleDifferentValues ??
                                                  m_Property.GetSerializedObject()?.isEditingMultipleObjects ?? false;

        public string displayName => m_Property.DisplayName();

        public string type => serializedProperty?.type ?? m_Property.GetUnderlyingType().Name;

        public string arrayElementType => serializedProperty?.arrayElementType ?? m_Property.GetUnderlyingElementType().Name;

        public string tooltip => serializedProperty?.tooltip ?? m_Property.Tooltip();

        // TODO: depth
        public int depth => serializedProperty?.depth ?? throw new NotImplementedException();

        public string propertyPath => m_Property.PropertyPath;

        public bool editable => serializedProperty?.editable ?? !m_Property.IsReadOnly();

        // TODO: isAnimated
        public bool isAnimated => serializedProperty?.isAnimated ?? false;

        public bool isExpanded
        {
            get => m_Property.IsExpanded();
            set => m_Property.SetExpanded(value);
        }

        public bool hasChildren => serializedProperty?.hasChildren ?? m_Property.HasChildren();

        // TODO: hasVisibleChildren
        public bool hasVisibleChildren => serializedProperty?.hasVisibleChildren ?? m_Property.HasChildren();

        public bool isInstantiatedPrefab => serializedProperty?.isInstantiatedPrefab ??
                                            PrefabUtility.IsPartOfPrefabInstance(
                                                m_Property.GetTargetObject());

        // TODO: prefabOverride
        public bool prefabOverride
        {
            get => serializedProperty?.prefabOverride ?? throw new NotImplementedException();
            set
            {
                if (!m_Property.HasSerializedProperty())
                    throw new NotImplementedException();
                serializedProperty.prefabOverride = value;
            }
        }

        // TODO: isDefaultOverride
        public bool isDefaultOverride => serializedProperty?.isDefaultOverride ?? throw new NotImplementedException();

        public SerializedPropertyType propertyType => serializedProperty?.propertyType ?? SerializedUtility.GetPropertyType(m_Property);

        public SerializedPropertyNumericType numericType => serializedProperty?.numericType ?? SerializedUtility.GetPropertyNumericType(m_Property);

        public int intValue
        {
            get => serializedProperty?.intValue ?? (int)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.intValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public long longValue
        {
            get => serializedProperty?.longValue ?? (long)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.longValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public ulong ulongValue
        {
            get => serializedProperty?.ulongValue ?? (ulong)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.ulongValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public uint uintValue
        {
            get => serializedProperty?.uintValue ?? (uint)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.uintValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public bool boolValue
        {
            get => serializedProperty?.boolValue ?? (bool)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.boolValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public float floatValue
        {
            get => serializedProperty?.floatValue ?? (float)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.floatValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public double doubleValue
        {
            get => serializedProperty?.doubleValue ?? (double)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.doubleValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public string stringValue
        {
            get => serializedProperty?.stringValue ?? (string)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.stringValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Color colorValue
        {
            get => serializedProperty?.colorValue ?? (Color)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.colorValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public AnimationCurve animationCurveValue
        {
            get => serializedProperty?.animationCurveValue ?? (AnimationCurve)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.animationCurveValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Gradient gradientValue
        {
            get => serializedProperty?.gradientValue ?? (Gradient)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.gradientValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public UnityEngine.Object objectReferenceValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.objectReferenceValue : (UnityEngine.Object)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.objectReferenceValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public object managedReferenceValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.managedReferenceValue : m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.managedReferenceValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        // TODO: managedReferenceId
        // SEE: SerializationUtility.GetManagedReferenceIdForObject
        // SEE: SerializationUtility.SetManagedReferenceIdForObject
        public long managedReferenceId
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.managedReferenceId : m_Property.GetValue()?.GetHashCode() ?? 0;
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.managedReferenceId = value;
#if SPACE3X_DEBUG
                else
                    throw new NotImplementedException();
#endif
            }
        }

        public string managedReferenceFullTypename => m_Property.HasSerializedProperty()
            ? serializedProperty.managedReferenceFullTypename
            : m_Property.GetValue()?.GetType().FullName;

        public string managedReferenceFieldTypename => m_Property.HasSerializedProperty()
            ? serializedProperty.managedReferenceFieldTypename
            : m_Property.GetUnderlyingField()?.GetType().FullName;

        public int objectReferenceInstanceIDValue
        {
            get => m_Property.HasSerializedProperty()
                ? serializedProperty.objectReferenceInstanceIDValue
                : (m_Property.GetValue() as UnityEngine.Object)?.GetInstanceID() 
                  ?? m_Property.GetValue()?.GetHashCode() ?? 0;
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.objectReferenceInstanceIDValue = value;
#if SPACE3X_DEBUG
                else
                    throw new NotImplementedException();
#endif
            }
        }
        
        public int enumValueIndex
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.enumValueIndex : (int)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.enumValueIndex = value;
                else
                    m_Property.SetValue(value);
            }
        }
        
        public int enumValueFlag
        {
            get => intValue;
            set => intValue = value;
        }

        public string[] enumNames
        {
            get
            {
                if (m_Property.HasSerializedProperty())
                    return serializedProperty.enumNames;
                Type underlyingType = m_Property.GetUnderlyingType();
                if (underlyingType.IsEnum)
                    return Enum.GetNames(underlyingType);
                return Array.Empty<string>();
            }
        }

        public string[] enumDisplayNames
        {
            get
            {
                if (m_Property.HasSerializedProperty())
                    return serializedProperty.enumDisplayNames;
                Type underlyingType = m_Property.GetUnderlyingType();
                if (underlyingType.IsEnum)
                    return Enum
                        .GetNames(underlyingType)
                        .Select(ObjectNames.NicifyVariableName)
                        .ToArray();
                return Array.Empty<string>();
            }
        }

        public Vector2 vector2Value
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.vector2Value : (Vector2)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.vector2Value = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Vector3 vector3Value
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.vector3Value : (Vector3)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.vector3Value = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Vector4 vector4Value
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.vector4Value : (Vector4)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.vector4Value = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Vector2Int vector2IntValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.vector2IntValue : (Vector2Int)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.vector2IntValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Vector3Int vector3IntValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.vector3IntValue : (Vector3Int)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.vector3IntValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Quaternion quaternionValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.quaternionValue : (Quaternion)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.quaternionValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Rect rectValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.rectValue : (Rect)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.rectValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public RectInt rectIntValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.rectIntValue : (RectInt)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.rectIntValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Bounds boundsValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.boundsValue : (Bounds)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.boundsValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public BoundsInt boundsIntValue
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.boundsIntValue : (BoundsInt)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.boundsIntValue = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public Hash128 hash128Value
        {
            get => m_Property.HasSerializedProperty() ? serializedProperty.hash128Value : (Hash128)m_Property.GetValue();
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.hash128Value = value;
                else
                    m_Property.SetValue(value);
            }
        }

        public bool Next(bool enterChildren)
        {
            if (enterChildren && m_Property.HasChildren())
            {
                PropertyAttributeController controller = null;
                if (((PropertyAttributeController)m_Property.GetController()).TryGetInstance(m_Property.PropertyPath, out controller))
                {
                    m_Property = controller.GetNextProperty(string.Empty);
                    m_HasValidSerializedProperty = false;
                }
                else
                {
                    controller = PropertyAttributeController.GetOrCreateInstance(m_Property,
                        m_Property.GetValue()?.GetType() ?? m_Property.GetUnderlyingType(), forceCreate: false);
                    if (controller != null)
                    {
                        m_Property = controller.GetNextProperty(string.Empty);
                        m_HasValidSerializedProperty = false;
                    }
                    else
                        throw new NotImplementedException("Couldn't get controller for children properties on " + m_Property);
                }
            }
            else
            {
                m_Property = ((PropertyAttributeController)m_Property.GetController()).GetNextProperty(m_Property.Name);
                m_HasValidSerializedProperty = false;
            }

            return m_Property != null;
        }

        /// <summary>
        /// It doesn't produce the exact same result as <see cref="SerializedProperty.Reset"/> does, the original
        /// method resets the serialized property to the serialized object's `Base` property and by calling .Next(true)
        /// or .NextVisible(true), advances within the SerializedObject internal properties or the m_Script property
        /// when calling .NextVisible(true), followed by the actual properties on its targetObject.
        ///
        /// Current PropertyAttributeController's implementation constructs the list of children properties using
        /// reflection on the actual underlying value of a given property and those SerializedObject internal properties
        /// doesn't exist as members of the underlying value's Type.
        ///
        /// So, after calling .Reset() on a property, the .propertyPath is an empty string, the same as after calling
        /// .Reset() on a SerializedProperty but the after the first call to .NextVisible(true), .propertyPath will
        /// be the path of the first visible property declared on that Type, while on a SerializedProperty, it would be
        /// <c>m_Script</c> instead.
        /// </summary>
        public void Reset()
        {
            if (((PropertyAttributeController)m_Property.GetController()).TryGetInstance(string.Empty, out var controller))
            {
                m_Property = controller.GetProperty(string.Empty);
                m_HasValidSerializedProperty = false;
            }
            else
            {
                throw new NotImplementedException(
                    "Couldn't get controller for the first property on the root object of " + m_Property);
            }
        }

        public int CountRemaining()
        {
            return ((PropertyAttributeController)m_Property.GetController())
                .GetAllProperties()
                .SkipWhile(n => n.Name != m_Property.Name)
                .Skip(1)
                .Count(n => !n.IsHidden() && !string.IsNullOrEmpty(n.Name));
        }

        public int CountInProperty()
        {
            if (!m_Property.HasChildren()) return 1;
            if (((PropertyAttributeController)m_Property.GetController()).TryGetInstance(m_Property.PropertyPath, out var controller))
            {
                return 1 + controller
                    .GetAllProperties()
                    .Count(n => !n.IsHidden() && !string.IsNullOrEmpty(n.Name));
            }
            else
            {
                throw new NotImplementedException(
                    "Couldn't get controller for children properties on " + m_Property);
            }
        }

        public bool DuplicateCommand()
        {
            if (m_Property.HasSerializedProperty())
                return serializedProperty.DuplicateCommand();
            if (m_Property.IsArrayOrListElement() && m_Property is IPropertyNodeIndex itemNode)
            {
                ArrayPropertyUtility.InsertArrayElementAtIndex(itemNode.Indexer, itemNode.Index + 1);
                itemNode.Indexer.GetArrayElementAtIndex(itemNode.Index + 1).SetValue(m_Property.GetValue());
                return true;
            }
            return false;
        }

        public bool DeleteCommand()
        {
            if (m_Property.HasSerializedProperty())
                return serializedProperty.DeleteCommand();
            if (m_Property.IsArrayOrListElement() && m_Property is IPropertyNodeIndex itemNode)
            {
                ArrayPropertyUtility.DeleteArrayElementAtIndex(itemNode.Indexer, itemNode.Index);
                return true;
            }
            return false;
        }

        public IPropertyNodeImplementation GetEndProperty() => GetEndProperty(false);
        
        public IPropertyNodeImplementation GetEndProperty(bool includeInvisible)
        {
            IPropertyNodeImplementation endProperty = this.Copy();
            if (includeInvisible)
                endProperty.Next(false);
            else
                endProperty.NextVisible(false);
            return endProperty;
        }

        public bool isArray => m_Property.IsArrayOrList();
        
        public int arraySize
        {
            get => m_Property.HasSerializedProperty()
                ? serializedProperty.arraySize
                : (m_Property.GetValue() as IList)?.Count ?? 0;
            set
            {
                if (m_Property.HasSerializedProperty())
                    serializedProperty.arraySize = value;
                else
                    ArrayPropertyUtility.ResizeArray(m_Property, value);
            }
        }

        public int minArraySize => serializedProperty?.minArraySize ?? arraySize;

        public void InsertArrayElementAtIndex(int index)
        {
            if (m_Property.HasSerializedProperty())
                serializedProperty.InsertArrayElementAtIndex(index);
            else
                ArrayPropertyUtility.InsertArrayElementAtIndex(m_Property, index);
        }

        public void DeleteArrayElementAtIndex(int index)
        {
            if (m_Property.HasSerializedProperty())
                serializedProperty.DeleteArrayElementAtIndex(index);
            else
                ArrayPropertyUtility.DeleteArrayElementAtIndex(m_Property, index);
        }

        public bool MoveArrayElement(int srcIndex, int dstIndex)
        {
            if (m_Property.HasSerializedProperty())
                return serializedProperty.MoveArrayElement(srcIndex, dstIndex);
            
            return ArrayPropertyUtility.MoveArrayElement(m_Property, srcIndex, dstIndex);
        }

        // TODO: isFixedBuffer
        public bool isFixedBuffer => serializedProperty?.isFixedBuffer ?? false;

        public int fixedBufferSize => serializedProperty?.fixedBufferSize ?? arraySize;

        public IPropertyNodeImplementation GetFixedBufferElementAtIndex(int index) => GetArrayElementAtIndex(index);

        public uint contentHash => serializedProperty?.contentHash ?? (uint)m_Property.GetHashCode();
        
        public VisualElement CreatePropertyField(bool bindProperty = false, string label = null)
        {
            VisualElement propertyField = null;
            if (m_Property.HasSerializedProperty())
            {
                propertyField = label == null
                    ? new PropertyField(serializedProperty)
                    : new PropertyField(serializedProperty, label);
                if (bindProperty)
                    ((IBindable)propertyField).BindProperty(m_Property);
            }
            else
            {
                // TODO: label
                propertyField = BindablePropertyField.Create(this, m_Property, applyCustomDrawers: false)
                    .Resolve(showInInspector: true);
                if (bindProperty && propertyField is BindablePropertyField bindableField)
                {
                    ((IBindable)bindableField.Field).BindProperty(m_Property);
                    bindableField.TrackPropertyValue(m_Property, changedProperty =>
                    {
                        if (!Equals(m_Property, changedProperty))
                            DebugLog.Error($"{nameof(PropertyNodeImplementation)} -> TrackPropertyValue <b>NOT EQUALS!</b> '{m_Property.PropertyPath}' != '{changedProperty.PropertyPath}'");
                        else
                        {
                            DebugLog.Error($"{nameof(PropertyNodeImplementation)} -> TrackPropertyValue -> SetValueWithoutNotify <color=#00FF00FF><b>EQUALS!</b></color> -> SetValueWithoutNotify: '{m_Property.PropertyPath}' == '{changedProperty.PropertyPath}'");
                            BindableUtility.SetValueWithoutNotify(bindableField.Field, changedProperty.GetValue());
                        }
                    });
                }
            }
            return propertyField;
        }

        public IPropertyNode GetPropertyNode() => m_Property;
    }
}
