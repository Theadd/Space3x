using System;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Begins a group in column mode.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Method
                    | AttributeTargets.Property
                    | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public class BeginColumnAttribute : GroupMarkerAttribute
    {
        public BeginColumnAttribute() : base(GroupType.Column) { IsOpen = true; }
    }
}
