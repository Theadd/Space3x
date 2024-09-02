using Space3x.Properties.Types.Editor;

namespace Space3x.Properties.Types
{
    public interface IUnreliableEventHandler
    {
        IBindablePropertyNode SourcePropertyNode { get; }
        
        IUnreliableEventHandler GetTopLevelEventHandler();

        IBindablePropertyNode GetReliablePropertyNode();
        
        void NotifyValueChanged(IBindablePropertyNode propertyNode);
    }
}
