using System;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Begins a group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Method
                    | AttributeTargets.Property
                    | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public class BeginGroupAttribute : GroupMarkerAttribute
    {
        public BeginGroupAttribute() : base(GroupType.None) { IsOpen = true; }
    }
}
