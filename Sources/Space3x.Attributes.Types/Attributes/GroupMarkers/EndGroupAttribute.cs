using System;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Ends a group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Method
                    | AttributeTargets.Property
                    | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public class EndGroupAttribute : GroupMarkerAttribute
    {
        public EndGroupAttribute() : base(GroupType.None) { IsOpen = false; }
    }
}
