using System;
using UnityEngine.Internal;

namespace Space3x.InspectorAttributes.Editor
{
    [Flags]
    public enum VTypeFlags
    {
        None = 0,
        HideInInspector = 1,
        ShowInInspector = 2,
        IncludeInInspector = 4,
        Serializable = 8,
        NonReorderable = 16,
        Array = 32,
        List = 64,
        ReadOnly = 128,
        /// <summary>
        /// IPropertyNode instances with this flag descend from a non-serialized collection property. Since the
        /// properties for the elements in a collection aren't mapped by an <see cref="AnnotatedRuntimeType"/>
        /// due to its dynamic nature, there might be multiple IPropertyNode instances targeting the same
        /// element in the collection. So there can also be multiple instances for nested child properties and
        /// by notifying changes in an instance of such an unreliable property, listeners on the other properties
        /// which target the same underlying value won't be notified. This flag tags those unreliable properties
        /// to provide an accurate event propagation implementation.
        /// </summary>
        Unreliable = 256
    }
    
    [ExcludeFromDocs]
    public static class VTypeFlagsExtensions
    {
        /// <inheritdoc cref="VTypeFlags.Unreliable"/>
        public static VTypeFlags ToUnreliable(this VTypeFlags other) => 
            (other & ~VTypeFlags.Serializable) 
            | VTypeFlags.Unreliable 
            | ((other | VTypeFlags.Serializable) == other
                ? VTypeFlags.ShowInInspector 
                : VTypeFlags.None);
    }
}
