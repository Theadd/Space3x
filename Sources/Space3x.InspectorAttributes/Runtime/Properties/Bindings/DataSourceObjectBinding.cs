using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public class DataSourceObjectBinding : BindableDataSource<UnityEngine.Object>
    {
        public DataSourceObjectBinding(IPropertyNode property) : base(property) { }
    }
}
