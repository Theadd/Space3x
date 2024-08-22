using System;

namespace Space3x.InspectorAttributes.Editor
{
    [Flags]
    public enum VTypeFlags
    {
        None = 0,
        HideInInspector = 1,
        ShowInInspector = 2,
        IncludeInInspector = 4,
        Serializable = 8,
        NonReorderable = 16,
        Array = 32,
        List = 64,
    }
}