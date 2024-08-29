using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Internal;

namespace Space3x.InspectorAttributes.Editor
{
    /// <summary>
    /// IPropertyNode interface that can be used instead of (or alongside) a <see cref="SerializedProperty"/> but
    /// also works with the non-serialized properties. 
    /// </summary>
    public interface IPropertyNode : IEquatable<IPropertyNode>
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
        public IPropertyController Controller { get; }
        
        public SerializedObject SerializedObject => Controller.SerializedObject;
    }
    
    /// <summary>
    /// Represents a property which is also a container for other properties. For example, an object or struct.
    /// </summary>
    [ExcludeFromDocs]
    public interface INodeTree : IPropertyNode { }
    
    [ExcludeFromDocs]
    public interface IPropertyNodeWithFlags : IPropertyNode, IPropertyFlags { }

    /// <summary>
    /// Base interface for properties that can be bound to a bindable object.
    /// </summary>
    /// <seealso cref="ISerializedPropertyNode"/>
    /// <seealso cref="ISerializedPropertyNodeIndex"/>
    /// <seealso cref="INonSerializedPropertyNode"/>
    /// <seealso cref="INonSerializedPropertyNodeIndex"/>
    [ExcludeFromDocs]
    public interface IBindablePropertyNode : IPropertyNodeWithFlags, IPropertyWithSerializedObject
    {
        public IBindableDataSource DataSource { get; set; }
    }

    /// <summary>
    /// Base interface for serialized properties.
    /// </summary>
    [ExcludeFromDocs]
    public interface ISerializedPropertyNode : IBindablePropertyNode { }

    /// <summary>
    /// Base interface for non-serialized properties.
    /// </summary>
    [ExcludeFromDocs]
    public interface INonSerializedPropertyNode : IBindablePropertyNode
    {
        public event Action<IPropertyNode> ValueChanged;
        
        public void NotifyValueChanged(IPropertyNode propertyNode);
    }

    /// <summary>
    /// Base interface for properties which are elements of an array or IList.
    /// </summary>
    /// <seealso cref="ISerializedPropertyNodeIndex"/>
    /// <seealso cref="INonSerializedPropertyNodeIndex"/>
    [ExcludeFromDocs]
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
    /// </summary>
    /// <seealso cref="INonSerializedPropertyNodeIndex"/>
    [ExcludeFromDocs]
    public interface ISerializedPropertyNodeIndex : ISerializedPropertyNode, IPropertyNodeIndex { }
    
    /// <summary>
    /// Base interface for non-serialized properties which are elements of an array or IList.
    /// </summary>
    /// <seealso cref="ISerializedPropertyNodeIndex"/>
    [ExcludeFromDocs]
    public interface INonSerializedPropertyNodeIndex : INonSerializedPropertyNode, IPropertyNodeIndex { }

    /// <summary>
    /// Base interface for invokable properties, such as methods annotated with any valid <see cref="UnityEngine.PropertyAttribute"/>.
    /// </summary>
    [ExcludeFromDocs]
    public interface IInvokablePropertyNode : IBindablePropertyNode
    {
        public event Action<IInvokablePropertyNode> ValueChanged;
        
        public void NotifyValueChanged();
        
        public object Value { get; }
    }
}
