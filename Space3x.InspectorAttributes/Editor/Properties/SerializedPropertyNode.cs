using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class SerializedPropertyNodeBase : ISerializedPropertyNode
    {
        public VTypeFlags Flags { get; set; }
        public string Name { get; set; }
        public SerializedObject SerializedObject { get; set; }
        public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public string ParentPath { get; set; }
    }
    
    public class SerializedPropertyNode : SerializedPropertyNodeBase { }
    
    public class SerializedPropertyNodeTree : SerializedPropertyNodeBase, INodeTree { }
    
    public abstract class SerializedPropertyNodeIndexBase : ISerializedPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }
        public int Index { get; set; }
        public VTypeFlags Flags => Indexer.Flags;
        public string Name => "Array.data[" + Index + "]";
        public SerializedObject SerializedObject => Indexer.SerializedObject;
        public string PropertyPath => ParentPath + "." + Name;
        public string ParentPath => Indexer.PropertyPath;
    }
    
    public class SerializedPropertyNodeIndex : SerializedPropertyNodeIndexBase { }
    
    public class SerializedPropertyNodeIndexTree : SerializedPropertyNodeIndexBase, INodeTree { }
}
