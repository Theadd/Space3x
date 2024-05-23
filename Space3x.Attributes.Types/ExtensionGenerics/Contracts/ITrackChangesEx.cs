namespace Space3x.Attributes.Types
{
    public interface ITrackChanges : IContract
    {
        
    }
    
    public interface ITrackChangesEx : ITrackChanges, ISealedExtension<ITrackChangesEx> { }
    
    public interface ITrackChangesEx<TTypeComposition> : ITrackChangesEx { }
}
