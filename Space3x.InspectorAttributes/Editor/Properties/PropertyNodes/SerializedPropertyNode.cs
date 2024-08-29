using Space3x.Properties.Types.Editor;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class SerializedPropertyNodeBase : BindablePropertyNode, ISerializedPropertyNode
    {
        // public VTypeFlags Flags { get; set; }
        // public string Name { get; set; }
        // public IPropertyController Controller { get; internal set; }
        // public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
        //     ? (ParentPath ?? "") + (Name ?? "")
        //     : ParentPath + "." + Name;
        // public string ParentPath { get; set; }
        // public IBindableDataSource DataSource { get; set; }
    }
    
    public class SerializedPropertyNode : SerializedPropertyNodeBase { }
    
    public class SerializedPropertyNodeTree : SerializedPropertyNodeBase, INodeTree { }
    
    public abstract class SerializedPropertyNodeIndexBase : BindablePropertyNode, ISerializedPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }
        public int Index { get; set; }
        public override VTypeFlags Flags => Indexer.Flags & ~(VTypeFlags.List | VTypeFlags.Array);
        public override string Name => "Array.data[" + Index + "]";
        public override IPropertyController Controller => Indexer.Controller;
        public SerializedObject SerializedObject => Indexer.SerializedObject;
        public override string PropertyPath => ParentPath + "." + Name;
        public override string ParentPath => Indexer.PropertyPath;
        // public IBindableDataSource DataSource { get; set; }
    }
    
    public class SerializedPropertyNodeIndex : SerializedPropertyNodeIndexBase { }
    
    public class SerializedPropertyNodeIndexTree : SerializedPropertyNodeIndexBase, INodeTree { }
}
