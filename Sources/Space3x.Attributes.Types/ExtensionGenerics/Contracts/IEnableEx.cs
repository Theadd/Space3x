namespace Space3x.Attributes.Types
{
    public interface IEnable : IContract
    {
        bool Enabled { get; set; }
    }
    
    public interface IEnableEx : IEnable, ISealedExtension<IEnableEx> { }
    
    public interface IEnableEx<TTypeComposition> : IEnableEx { }
}
