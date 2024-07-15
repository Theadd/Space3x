using System;
using System.Collections.Generic;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    /// <summary>
    /// IProperty interface that can be used instead of (or alongside) a SerializedProperty but
    /// also works with the non-serialized properties. 
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The <see cref="PropertyPath"/> of the parent property. For properties on the root object, this will be an empty string.
        /// </summary>
        public string ParentPath { get; }
        
        /// <summary>
        /// The PropertyPath of the property, which also corresponds to the
        /// <see cref="SerializedProperty.propertyPath"/> on a serialized property. 
        /// </summary>
        public string PropertyPath { get; }
    }

    public interface IPropertyWithSerializedObject
    {
        public SerializedObject SerializedObject { get; }
    }
    
    /// <summary>
    /// Represents a property which is also a container for other properties. For example, an object or struct.
    /// </summary>
    public interface INodeTree : IProperty
    {
        // public IEnumerable<IPropertyNode> Children();
    }
    
    public interface IPropertyNode : IProperty, IPropertyFlags { }
    
    /// <summary>
    /// Base interface for properties that can be bound to a bindable object.
    /// </summary>
    /// <seealso cref="ISerializedPropertyNode"/>
    /// <seealso cref="ISerializedPropertyNodeIndex"/>
    /// <seealso cref="INonSerializedPropertyNode"/>
    /// <seealso cref="INonSerializedPropertyNodeIndex"/>
    public interface IBindablePropertyNode : IPropertyNode, IPropertyWithSerializedObject
    {
        // public VisualElement Field { get; set; }
    }

    /// <summary>
    /// Base interface for serialized properties.
    /// </summary>
    public interface ISerializedPropertyNode : IBindablePropertyNode { }

    /// <summary>
    /// Base interface for non-serialized properties.
    /// </summary>
    public interface INonSerializedPropertyNode : IBindablePropertyNode
    {
        public event Action<IProperty> ValueChanged;
        public void NotifyValueChanged();
    }

    /// <summary>
    /// Base interface for properties which are elements of an array or IList.
    /// <seealso cref="ISerializedPropertyNodeIndex"/>
    /// <seealso cref="INonSerializedPropertyNodeIndex"/>
    /// </summary>
    public interface IPropertyNodeIndex
    {
        /// <summary>
        /// A reference to the array that this property is an element of.
        /// </summary>
        public IBindablePropertyNode Indexer { get; set; }

        /// <summary>
        /// The index of this property in the array.
        /// </summary>
        public int Index { get; set; }
    }
    
    /// <summary>
    /// Base interface for serialized properties which are elements of an array or IList.
    /// <seealso cref="INonSerializedPropertyNodeIndex"/>
    /// </summary>
    public interface ISerializedPropertyNodeIndex : ISerializedPropertyNode, IPropertyNodeIndex { }
    
    /// <summary>
    /// Base interface for non-serialized properties which are elements of an array or IList.
    /// <seealso cref="ISerializedPropertyNodeIndex"/>
    /// </summary>
    public interface INonSerializedPropertyNodeIndex : INonSerializedPropertyNode, IPropertyNodeIndex { }
    
    // TODO: INodeArray is not implemented yet
    public interface INodeArray : IProperty
    {
        public IEnumerable<IPropertyNode> Children();

        public int ChildCount { get; }
    }
}
