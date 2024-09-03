using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public abstract class SerializedPropertyNodeBase : BindablePropertyNode, ISerializedPropertyNode { }
    
    public class SerializedPropertyNode : SerializedPropertyNodeBase { }
    
    public class SerializedPropertyNodeTree : SerializedPropertyNodeBase, INodeTree { }
    
    public abstract class SerializedPropertyNodeIndexBase : BindablePropertyNode, ISerializedPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }
        public int Index { get; set; }
        public override VTypeFlags Flags => Indexer.Flags & ~(VTypeFlags.List | VTypeFlags.Array);
        public override string Name => "Array.data[" + Index + "]";
        public override IPropertyController Controller => Indexer.Controller;
        public object SerializedObject => Indexer.SerializedObject;
        public override string PropertyPath => ParentPath + "." + Name;
        public override string ParentPath => Indexer.PropertyPath;
    }
    
    public class SerializedPropertyNodeIndex : SerializedPropertyNodeIndexBase { }
    
    public class SerializedPropertyNodeIndexTree : SerializedPropertyNodeIndexBase, INodeTree { }
}
