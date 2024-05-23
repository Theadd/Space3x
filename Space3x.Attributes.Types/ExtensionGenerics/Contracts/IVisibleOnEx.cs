namespace Space3x.Attributes.Types
{
    public interface IVisibleOn : IVisible, ICondition, IContract { }

    public interface IVisibleOnEx : IVisibleOn, ISealedExtension<IVisibleOnEx> { }
    
    public interface IVisibleOnEx<TTypeComposition> : IVisibleOnEx { }
}
