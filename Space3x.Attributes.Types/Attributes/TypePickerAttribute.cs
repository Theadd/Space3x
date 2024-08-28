using System;
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
        
        /// <summary>
        /// Whether to remove duplicates from the final array of available <see cref="Types"/>.
        /// </summary>
        bool RemoveDuplicates { get; set; }
        
        // <summary>
        // This property is publicly exposed instead of <c>internal</c> only to allow third party developers to
        // create their own versions of the Type picker outside this project.
        // </summary>
        // <remarks>
        // The main purpose of this property being written here is to avoid performance drawbacks when using a large
        // set of types to search through in an array or list property. Since the PropertyDrawer is instantiated for
        // every single element on the list, rebuilding the list of available types to chose from would be computed
        // on every element and, if caching those, would be even worse. The same PropertyAttribute instance is shared
        // between all PropertyDrawers in the same collection property, hence the performance impact is minimal in
        // such cases.
        // Simply put, this allows us to have a single list of available Types to choose from for all PropertyDrawer
        // instances in the same collection.
        // </remarks>
        ITypePickerAttributeHandler Handler { get; set; }
    }

    /// <summary>
    /// Shows a <see cref="Type"/> picker in the inspector.
    /// </summary>
    /// <remarks>
    /// This can be used as a <see cref="Type"/> picker or as a <see cref="Type"/> instance picker depending on the
    /// <see cref="Type"/> of the annotated property (or the underlying element type in case of an array or list
    /// property).
    /// For the <see cref="Type"/> instance picker, it creates an instance of the user-selected <see cref="Type"/>
    /// when the declared <see cref="Type"/> of the property is not <see cref="Type"/>-"ish"*. Such as an interface,
    /// class or base class.
    /// Regarding that <see cref="Type"/>-"ish"* reference above, it is due to Unity's limitation to directly deal with
    /// properties of type <see cref="Type"/>. Because of this, the
    /// <see cref="Space3x.InspectorAttributes.Types.NamedType">NamedType</see> and the 
    /// <see cref="Space3x.InspectorAttributes.Types.SerializableType">SerializableType</see> types representing a
    /// <see cref="Type"/> (which also supports explicit cast to and from <see cref="Type"/>) are provided.
    /// Both types are equivalent but the first one is declared as a struct (value type) and the latter as a class
    /// (reference type).
    /// That said, any property whose type is one of the above two types will be used as a <see cref="Type"/> picker
    /// instead of a <see cref="Type"/> instance picker.
    /// </remarks>
    /// <example>
    /// <code>
    /// [TypePicker(typeof(PropertyAttribute), DerivedTypes = true)]
    /// public NamedType SelectedAttribute;
    /// </code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// This can occur when creating an instance of a <see cref="Type"/> derived from <see cref="UnityEngine.Object"/>
    /// (<see cref="MonoBehaviour"/>, <see cref="ScriptableObject"/>, etc.) to a serialized property (which also has to be
    /// annotated with the <see cref="SerializeReference"/> attribute for other types not deriving from
    /// <see cref="UnityEngine.Object"/> to work as expected) due to Unity's behaviour (Object reference vs managed reference).
    /// One way to get around this is to declare the property as private and swap the <see cref="SerializeReference"/>
    /// attribute for the <see cref="ShowInInspectorAttribute">ShowInInspector</see> attribute.
    /// </exception>
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
        /// Whether to remove duplicates from the final array of available <see cref="Types"/>.
        /// </summary>
        public bool RemoveDuplicates { get; set; } = false;

        /// <summary>
        /// This property is publicly exposed instead of <c>internal</c> only to allow third party developers to
        /// create their own versions of the Type picker outside this project.
        /// </summary>
        /// <remarks>
        /// The main purpose of this property being written here is to avoid performance drawbacks when using a large
        /// set of types to search through in an array or list property. Since the PropertyDrawer is instantiated for
        /// every single element on the list, rebuilding the list of available types to chose from would be computed
        /// on every element and, if caching those, would be even worse. The same PropertyAttribute instance is shared
        /// between all PropertyDrawers in the same collection property, hence the performance impact is minimal in
        /// such cases.
        /// Simply put, this allows us to have a single list of available Types to choose from for all PropertyDrawer
        /// instances in the same collection.
        /// </remarks>
        ITypePickerAttributeHandler ITypePickerAttribute.Handler { get; set; } = null;

        /// <summary>
        /// Shows a <see cref="Type"/> picker in the inspector.
        /// </summary>
        /// <param name="types">An array of types used to populate the list of available types to pick from.
        /// If null, an empty array is used.</param>
        public TypePickerAttribute(params Type[] types) => Types = types ?? Type.EmptyTypes;
        
        /// <summary>
        /// Shows a <see cref="Type"/> picker in the inspector.
        /// </summary>
        /// <param name="types">An array of types used to populate the list of available types to pick from.
        /// If null, an empty array is used.</param>
        /// <param name="derivedTypes">Whether to include derived types from the <paramref name="types"/> array.</param>
        public TypePickerAttribute(bool derivedTypes, params Type[] types)
        {
            DerivedTypes = derivedTypes;
            Types = types ?? Type.EmptyTypes;
        }
        
        /// <summary>
        /// Shows a <see cref="Type"/> picker in the inspector.
        /// </summary>
        /// <param name="derivedTypes">Whether to include derived types from the <paramref name="baseType"/>.</param>
        /// <param name="baseType">The base type from which to derive the types.</param>
        /// <param name="genericParameterTypes">Generic parameter types to filter the list of available types.</param>
        public TypePickerAttribute(Type baseType, bool derivedTypes, params Type[] genericParameterTypes)
        {
            DerivedTypes = derivedTypes;
            Types = new[] { baseType };
            GenericParameterTypes = genericParameterTypes;
        }

        /// <summary>
        /// Shows a <see cref="Type"/> picker in the inspector.
        /// </summary>
        /// <param name="propertyName">Name of a field, property or method from which the <see cref="Types"/> array will be populated.</param>
        public TypePickerAttribute(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// Shows a <see cref="Type"/> picker in the inspector.
        /// </summary>
        /// <param name="derivedTypes">Whether to include derived types from the <see cref="Types"/> array.</param>
        /// <param name="propertyName">Name of a field, property or method from which the <see cref="Types"/> array will be populated.</param>
        public TypePickerAttribute(bool derivedTypes, string propertyName)
        {
            DerivedTypes = derivedTypes;
            PropertyName = propertyName;
        }
    }
}
