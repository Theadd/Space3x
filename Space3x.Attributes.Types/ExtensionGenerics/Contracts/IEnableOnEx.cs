namespace Space3x.Attributes.Types
{
    public interface IEnableOn : IEnable, ICondition, IContract { }

    public interface IEnableOnEx : IEnableOn, ISealedExtension<IEnableOnEx> { }
    
    public interface IEnableOnEx<TTypeComposition> : IEnableOnEx { }
}
