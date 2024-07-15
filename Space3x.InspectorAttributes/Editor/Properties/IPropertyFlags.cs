using JetBrains.Annotations;
using Space3x.Attributes.Types;

namespace Space3x.InspectorAttributes.Editor
{
    /// <summary>
    /// Flags for <see cref="IProperty"/> and <see cref="Space3x.InspectorAttributes.Editor.VisualElements.BindablePropertyField"/>.
    /// </summary>
    public interface IPropertyFlags
    {
        [UsedImplicitly]
        public VTypeFlags Flags { get; }
        
        /// <summary>
        /// Serializable properties are not hidden, even if they are not public (for example, using SerializeField
        /// on a private field). But non serialized ones, such as private fields or using the [NonSerialized] attribute,
        /// are flagged as hidden. The [HideInInspector] and
        /// <see cref="ShowInInspectorAttribute">[ShowInInspector]</see> attributes also apply here.
        /// </summary>
        [UsedImplicitly]
        public bool IsHidden => (Flags | VTypeFlags.HideInInspector) == Flags ||
                                !((Flags | VTypeFlags.Serializable) == Flags ||
                                  (Flags | VTypeFlags.ShowInInspector) == Flags);
        
        /// <summary>
        /// Only the most basic information is extracted using reflection when accessing the node tree for the first
        /// time on any given object type, in order to determine the property nodes automatically handled by Unity (1)
        /// from those that won't be included in the tree (2) unless they're properly annotated (3).
        ///
        /// (1): Serializable nodes are fully handled by Unity, populating SerializedProperties, SerializedObjects, 
        /// PropertyDrawers, PropertyDecorators, and so on. Those are the ones where IsSerializable is true.
        /// 
        /// (2): The ones matching: !IsSerializable && IsHidden.
        /// 
        /// (3): The rest, matching: !IsSerializable && !IsHidden.
        /// </summary>
        [UsedImplicitly]
        public bool IsSerializable => (Flags | VTypeFlags.Serializable) == Flags;
        
        /// <summary>
        /// Makes elements of array or list properties, non-reorderable.
        /// </summary>
        [UsedImplicitly]
        public bool IsNonReorderable => (Flags | VTypeFlags.NonReorderable) == Flags;
    }
}
