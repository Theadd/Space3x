using System;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public class RedAttribute : ColorAttribute
    {
        public RedAttribute() : base("red") { }
    }
}
