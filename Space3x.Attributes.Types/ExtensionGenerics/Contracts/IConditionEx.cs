namespace Space3x.Attributes.Types
{
    public interface ICondition : IContract
    {
        string Condition { get; }
    }
    
    public interface IConditionEx : ICondition, ISealedExtension<IConditionEx> { }
    
    public interface IConditionEx<TTypeComposition> : IConditionEx { }
}
