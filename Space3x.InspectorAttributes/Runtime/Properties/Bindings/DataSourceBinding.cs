using Space3x.InspectorAttributes.Editor;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public class DataSourceBinding : BindableDataSource<object>
    {
        public DataSourceBinding(IPropertyNode property) : base(property) { }
    }
}
