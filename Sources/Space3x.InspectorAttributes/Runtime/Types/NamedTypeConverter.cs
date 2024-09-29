#if UNITY_EDITOR
using UnityEditor.UIElements;

namespace Space3x.InspectorAttributes.Types
{
    public class NamedTypeConverter : UxmlAttributeConverter<NamedType>
    {
        public override NamedType FromString(string value) => new(value?.Replace('¨', ','));
        public override string ToString(NamedType value) => value.ToString()?.Replace(',', '¨');
    }
}
#endif