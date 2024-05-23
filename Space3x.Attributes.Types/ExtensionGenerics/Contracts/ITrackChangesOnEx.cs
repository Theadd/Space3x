namespace Space3x.Attributes.Types
{
    public interface ITrackChangesOn : IContract
    {
        string PropertyName { get; }
    }
    
    public interface ITrackChangesOnEx : ITrackChangesOn, ISealedExtension<ITrackChangesOnEx> { }
    
    public interface ITrackChangesOnEx<TTypeComposition> : ITrackChangesOnEx { }
}
