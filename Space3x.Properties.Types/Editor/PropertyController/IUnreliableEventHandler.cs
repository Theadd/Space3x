namespace Space3x.Properties.Types.Editor
{
    public interface IUnreliableEventHandler
    {
        IBindablePropertyNode SourcePropertyNode { get; }
        
        IUnreliableEventHandler GetTopLevelEventHandler();

        IBindablePropertyNode GetReliablePropertyNode();
        
        void NotifyValueChanged(IBindablePropertyNode propertyNode);
    }
}
