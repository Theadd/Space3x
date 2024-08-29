namespace Space3x.InspectorAttributes.Editor.Extensions
{
    public static class BindablePropertyNodeExtensions
    {
        public static BindablePropertyNode GetIndexerOrItself(this BindablePropertyNode propertyNode) =>
            (BindablePropertyNode) (propertyNode is IPropertyNodeIndex propertyNodeIndex
                ? propertyNodeIndex.Indexer
                : propertyNode);
    }
}
