namespace Space3x.Properties.Types
{
    /// <summary>
    /// It is a non-standard event handler custom implementation only for a very specific subset of the property tree
    /// and provides change event propagation on controllers (all properties on the same declaring object share a single
    /// controller instance) descending from elements in a non-serialized array or list, down to the closest property
    /// for an <see cref="UnityEngine.Object"/> derived type.    
    /// </summary>
    public interface IUnreliableEventHandler
    {
        IBindablePropertyNode SourcePropertyNode { get; }

        bool IsTopLevelRuntimeEventHandler { get; }

        IUnreliableEventHandler GetTopLevelEventHandler();

        IBindablePropertyNode GetReliablePropertyNode();
        
        void NotifyValueChanged(IBindablePropertyNode propertyNode);
    }
}
