using System;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class YellowOnAttribute : ColorAttribute, IConditionEx<YellowOnAttribute>
    {
        public string Condition { get; }

        public YellowOnAttribute(string condition) : base("yellow")
        {
            Condition = condition;
        }
    }
}
