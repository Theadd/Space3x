namespace Space3x.Properties.Types
{
    public interface IUnreliableEventHandler
    {
        IBindablePropertyNode SourcePropertyNode { get; }

        bool IsTopLevelRuntimeEventHandler { get; }

        IUnreliableEventHandler GetTopLevelEventHandler();

        IBindablePropertyNode GetReliablePropertyNode();
        
        void NotifyValueChanged(IBindablePropertyNode propertyNode);
    }
}
