using System;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class GreenOnAttribute : ColorAttribute, IConditionEx<GreenOnAttribute>
    {
        public string Condition { get; }

        public GreenOnAttribute(string condition) : base("green")
        {
            Condition = condition;
        }
    }
}
