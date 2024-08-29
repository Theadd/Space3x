using System;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class NonSerializedPropertyNodeBase : BindablePropertyNode, INonSerializedPropertyNode
    {
        public event Action<IPropertyNode> ValueChanged;
        public void NotifyValueChanged(IPropertyNode propertyNode) => ValueChanged?.Invoke(propertyNode ?? this);

        // public VTypeFlags Flags { get; set; }
        // public override string Name { get; set; }
        // public IPropertyController Controller { get; internal set; }
        // public IPropertyController Controller { get; internal set; }
        // public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
        //     ? (ParentPath ?? "") + (Name ?? "")
        //     : ParentPath + "." + Name;
        // public string ParentPath { get; set; }
    }
    
    public class NonSerializedPropertyNode : NonSerializedPropertyNodeBase { }
    
    public class NonSerializedPropertyNodeTree : NonSerializedPropertyNodeBase, INodeTree { }
    
    public abstract class NonSerializedPropertyNodeIndexBase : BindablePropertyNode, INonSerializedPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }
        public int Index { get; set; }
        public event Action<IPropertyNode> ValueChanged
        {
            add => ((INonSerializedPropertyNode)Indexer).ValueChanged += value;
            remove => ((INonSerializedPropertyNode)Indexer).ValueChanged -= value;
        }
        public void NotifyValueChanged(IPropertyNode propertyNode) => ((INonSerializedPropertyNode)Indexer).NotifyValueChanged(propertyNode);
        public override VTypeFlags Flags => Indexer.Flags & ~(VTypeFlags.List | VTypeFlags.Array);
        public override string Name => "Array.data[" + Index + "]";
        public override IPropertyController Controller => Indexer.Controller;
        public SerializedObject SerializedObject => Indexer.SerializedObject;
        public override string PropertyPath => ParentPath + "." + Name;
        public override string ParentPath => Indexer.PropertyPath;
        // public IBindableDataSource DataSource { get; set; }
    }
    
    public class NonSerializedPropertyNodeIndex : NonSerializedPropertyNodeIndexBase { }
    public class NonSerializedPropertyNodeIndexTree : NonSerializedPropertyNodeIndexBase, INodeTree { }
}
