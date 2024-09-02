using System;
using Space3x.Properties.Types.Internal;

// using UnityEditor;

namespace Space3x.Properties.Types
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
    
    public interface IControlledProperty
    {
        public IPropertyController Controller { get; }
    
        // public SerializedObject SerializedObject => Controller.SerializedObject;
        public object SerializedObject => Controller.SerializedObject;
    }
    
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
    public interface IBindablePropertyNode : IPropertyNodeWithFlags, IControlledProperty
    {
        public IBindableDataSource DataSource { get; set; }
    }
}
