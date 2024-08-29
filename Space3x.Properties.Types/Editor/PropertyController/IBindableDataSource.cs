namespace Space3x.Properties.Types.Editor
{
    public interface IBindableDataSource
    {
        public object BoxedValue { get; set; }

        public IPropertyNode GetPropertyNode();

        public void IncreaseVersionNumber();
    }
}
