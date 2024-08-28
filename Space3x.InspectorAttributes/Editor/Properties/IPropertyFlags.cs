using Space3x.Attributes.Types;

namespace Space3x.InspectorAttributes.Editor
{
    /// <summary>
    /// Flags for <see cref="IPropertyNode"/>.
    /// </summary>
    public interface IPropertyFlags
    {
        public VTypeFlags Flags { get; }
        
        /// <summary>
        /// Serializable properties are not hidden, even if they are not public (for example,
        /// using <see cref="UnityEngine.SerializeField"/> on a private field). But non serialized ones, such as
        /// private fields or using the <see cref="System.NonSerializedAttribute">NonSerialized</see> attribute,
        /// are flagged as hidden.
        /// </summary>
        /// <remarks>
        /// The <see cref="UnityEngine.HideInInspector">HideInInspector</see> and
        /// <see cref="ShowInInspectorAttribute">ShowInInspector</see> attributes also apply here.
        /// </remarks>
        public bool IsHidden => (Flags | VTypeFlags.HideInInspector) == Flags ||
                                !((Flags | VTypeFlags.Serializable) == Flags ||
                                  (Flags | VTypeFlags.ShowInInspector) == Flags);
        
        // /// <summary>
        // /// Only the most basic information is extracted using reflection when accessing the node tree for the first
        // /// time on any given object type, in order to determine the property nodes automatically handled by Unity (1)
        // /// from those that won't be included in the tree (2) unless they're properly annotated (3).
        // /// </summary>
        // /// <remarks>
        // /// 1 - Serializable nodes are fully handled by Unity, populating SerializedProperties, SerializedObjects, 
        // /// PropertyDrawers, PropertyDecorators, and so on. Those are the ones where IsSerializable is true.
        // /// 2 - The ones matching: <c>!IsSerializable && IsHidden</c>.
        // /// 3 - The rest, matching: <c>!IsSerializable && !IsHidden</c>.
        // /// </remarks>
        // public bool IsSerializable => (Flags | VTypeFlags.Serializable) == Flags;
        
        /// <summary>
        /// Properties that are marked with the <see cref="System.SerializableAttribute">Serializable</see> attribute.
        /// Only the most basic information is extracted using reflection when accessing the node tree for the first
        /// time on any given object type, in order to determine the property nodes automatically handled by Unity [^1]
        /// from those that won't be included in the tree [^2] unless they're properly annotated [^3].
        /// </summary>
        /// <remarks>
        /// [^1]: Serializable nodes are fully handled by Unity, populating SerializedProperties, SerializedObjects, 
        /// PropertyDrawers, PropertyDecorators, and so on. Those are the ones where IsSerializable is true.
        /// 
        /// [^2]: The ones matching: <c>!IsSerializable &amp;&amp; IsHidden</c>.
        /// 
        /// [^3]: The rest, matching: <c>!IsSerializable &amp;&amp; !IsHidden</c>.
        /// </remarks>
        public bool IsSerializable => (Flags | VTypeFlags.Serializable) == Flags;
        
        public bool IncludeInInspector => (Flags | VTypeFlags.IncludeInInspector) == Flags;
        
        public bool ShowInInspector => (Flags | VTypeFlags.ShowInInspector) == Flags;
        
        /// <summary>
        /// Makes elements of array or list properties, non-reorderable. See the
        /// <see cref="UnityEngine.NonReorderableAttribute">NonReorderable</see> attribute.
        /// </summary>
        public bool IsNonReorderable => (Flags | VTypeFlags.NonReorderable) == Flags;
        
        /// <summary>
        /// Properties that are arrays.
        /// </summary>
        public bool IsArray => (Flags | VTypeFlags.Array) == Flags;
        
        /// <summary>
        /// Properties that derive from <see cref="System.Collections.IList">IList</see>.
        /// </summary>
        public bool IsList => (Flags | VTypeFlags.List) == Flags;
        
        /// <summary>
        /// Readonly fields or properties with no setter.
        /// </summary>
        public bool IsReadOnly => (Flags | VTypeFlags.ReadOnly) == Flags;
        
        internal bool IsUnreliable => (Flags | VTypeFlags.Unreliable) == Flags;
    }
}
