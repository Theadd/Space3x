using Space3x.Properties.Types.Editor;
using UnityEngine.Internal;

namespace Space3x.InspectorAttributes.Editor
{
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
