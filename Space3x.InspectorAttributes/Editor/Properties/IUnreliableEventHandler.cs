namespace Space3x.InspectorAttributes.Editor
{
    public interface IUnreliableEventHandler
    {
        BindablePropertyNode SourcePropertyNode { get; }
        
        IUnreliableEventHandler GetTopLevelEventHandler();

        BindablePropertyNode GetReliablePropertyNode();
        
        void NotifyValueChanged(IBindablePropertyNode propertyNode);
    }
}
