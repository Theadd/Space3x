using System;

namespace Space3x.InspectorAttributes.Editor
{
    [Flags]
    public enum VTypeFlags
    {
        None = 0,
        HideInInspector = 1,
        ShowInInspector = 2,
        Serializable = 4,
        NonReorderable = 8,
    }
}