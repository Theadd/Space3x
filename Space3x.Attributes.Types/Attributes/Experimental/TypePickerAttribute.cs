using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;

namespace Space3x.Attributes.Types
{
    [ExcludeFromDocs]
    public interface ITypePickerAttributeHandler { }

    [ExcludeFromDocs]
    public interface ITypePickerAttribute
    {
        /// <summary>
        /// An array of types used to populate the list of available types to pick from.
        /// </summary>
        Type[] Types { get; set; }

        /// <summary>
        /// Generic parameter types to filter the <see cref="Types"/> array. Mainly used when
        /// <see cref="DerivedTypes"/> is set to true.
        /// </summary>
        Type[] GenericParameterTypes { get; set; }

        /// <summary>
        /// Whether to include derived types from the <see cref="Types"/> array.
        /// </summary>
        bool DerivedTypes { get; set; }

        /// <summary>
        /// Name of a field, property or method from which the <see cref="Types"/> array will be populated.
        /// </summary>
        string PropertyName { get; set; }

        ITypePickerAttributeHandler Handler { get; set; }
    }

    /// <summary>
    /// Shows a <see cref="Type"/> picker in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TypePickerAttribute : PropertyAttribute, ITypePickerAttribute
    {
        /// <summary>
        /// An array of types used to populate the list of available types to pick from.
        /// </summary>
        public Type[] Types { get; set; } = null;
        
        /// <summary>
        /// Generic parameter types to filter the <see cref="Types"/> array. Mainly used when
        /// <see cref="DerivedTypes"/> is set to true.
        /// </summary>
        public Type[] GenericParameterTypes { get; set; } = null;
        
        /// <summary>
        /// Whether to include derived types from the <see cref="Types"/> array.
        /// </summary>
        public bool DerivedTypes { get; set; } = false;
        
        /// <summary>
        /// Name of a field, property or method from which the <see cref="Types"/> array will be populated.
        /// </summary>
        public string PropertyName { get; set; } = null;
        
        /// <summary>
        /// This property is publicly exposed instead of <c>internal</c> only to allow third party developers to
        /// create their own versions of the Type picker outside this project. Ignore it when annotating your fields
        /// or properties with this attribute.
        /// </summary>
        /// <remarks>
        /// The main purpose of this property being written here is to avoid performance drawbacks when using a large
        /// set of types to search through in an array or list property. Since the PropertyDrawer is instantiated for
        /// every single element on the list, rebuilding the list of available types to chose from would be computed
        /// on every element and, if caching those, would be even worse. The same PropertyAttribute instance is shared
        /// between all PropertyDrawers in the same collection property, hence the performance impact is minimal in
        /// such cases.
        /// </remarks>
        public ITypePickerAttributeHandler Handler { get; set; } = null;
        
        /// <summary>
        /// Shows a <see cref="Type"/> picker in the inspector.
        /// </summary>
        /// <param name="types">An array of types used to populate the list of available types to pick from.
        /// If null, an empty array is used.</param>
        /// <param name="derivedTypes">Whether to include derived types from the <paramref name="types"/> array.</param>
        /// <param name="baseType">The base type from which to derive the types.</param>
        /// <param name="genericParameterTypes">Generic parameter types to filter the <paramref name="types"/> array.</param>
        public TypePickerAttribute(params Type[] types) => Types = types ?? Type.EmptyTypes;
        
        public TypePickerAttribute(bool derivedTypes, params Type[] types)
        {
            DerivedTypes = derivedTypes;
            Types = types ?? Type.EmptyTypes;
        }
        
        // public TypePickerAttribute(Type baseType, params Type[] genericParameterTypes)
        // {
        //     Types = new[] { baseType };
        //     GenericParameterTypes = genericParameterTypes;
        // }
        
        public TypePickerAttribute(Type baseType, bool derivedTypes, params Type[] genericParameterTypes)
        {
            DerivedTypes = derivedTypes;
            Types = new[] { baseType };
            GenericParameterTypes = genericParameterTypes;
        }
        
        public TypePickerAttribute(string propertyName) => PropertyName = propertyName;

        public TypePickerAttribute(bool derivedTypes, string propertyName)
        {
            DerivedTypes = derivedTypes;
            PropertyName = propertyName;
        }
    }
}

