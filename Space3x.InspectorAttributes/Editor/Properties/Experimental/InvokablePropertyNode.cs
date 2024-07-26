using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class InvokablePropertyNodeBase : IInvokablePropertyNode
    {
        public VTypeFlags Flags { get; set; }
        public string Name { get; set; }
        public SerializedObject SerializedObject { get; set; }
        public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public string ParentPath { get; set; }
    }
    
    public class InvokablePropertyNode : InvokablePropertyNodeBase { }
}
