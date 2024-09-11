using System;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Ends a group in column mode.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Method
                    | AttributeTargets.Property
                    | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public class EndColumnAttribute : GroupMarkerAttribute
    {
        public EndColumnAttribute() : base(GroupType.Column) { IsOpen = false; }
    }
}
