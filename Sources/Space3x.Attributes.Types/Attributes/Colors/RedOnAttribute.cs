using System;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class RedOnAttribute : ColorAttribute, IConditionEx<RedOnAttribute>
    {
        public string Condition { get; }

        public RedOnAttribute(string condition) : base("red")
        {
            Condition = condition;
        }
    }
}
