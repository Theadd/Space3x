using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public static class BindablePropertyNodeExtensions
    {
        public static BindablePropertyNode GetIndexerOrItself(this BindablePropertyNode propertyNode) =>
            (BindablePropertyNode) (propertyNode is IPropertyNodeIndex propertyNodeIndex
                ? propertyNodeIndex.Indexer
                : propertyNode);
    }
}
