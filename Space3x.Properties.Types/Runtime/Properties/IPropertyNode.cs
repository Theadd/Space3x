using System;
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
}
