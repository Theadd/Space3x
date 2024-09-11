namespace Space3x.Attributes.Types
{
    public interface IVisible : IContract
    {
        bool Visible { get; }
    }

    public interface IVisibleEx : IVisible, ISealedExtension<IVisibleEx> { }
    public interface IVisibleEx<TTypeComposition> : IVisibleEx { }
}
