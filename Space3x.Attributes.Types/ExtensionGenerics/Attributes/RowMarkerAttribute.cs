using System;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Method
                    | AttributeTargets.Property
                    | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public class BeginRowAttribute : GroupMarkerAttribute
    {
        public BeginRowAttribute() : base(GroupType.Row) { IsOpen = true; }
    }

    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Method
                    | AttributeTargets.Property
                    | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public class EndRowAttribute : GroupMarkerAttribute
    {
        public EndRowAttribute() : base(GroupType.Row) { IsOpen = false; }
    }
}
