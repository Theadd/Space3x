using System;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class NonSerializedPropertyNodeBase : INonSerializedPropertyNode
    {
        public event Action<IPropertyNode> ValueChanged;
        public void NotifyValueChanged() => ValueChanged?.Invoke(this);

        public VTypeFlags Flags { get; set; }
        public string Name { get; set; }
        public IPropertyController Controller { get; internal set; }
        public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public string ParentPath { get; set; }
        public IBindableDataSource DataSource { get; set; }
    }
    
    public class NonSerializedPropertyNode : NonSerializedPropertyNodeBase { }
    
    public class NonSerializedPropertyNodeTree : NonSerializedPropertyNodeBase, INodeTree { }
    
    public abstract class NonSerializedPropertyNodeIndexBase : INonSerializedPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }
        public int Index { get; set; }
        public event Action<IPropertyNode> ValueChanged
        {
            add => ((INonSerializedPropertyNode)Indexer).ValueChanged += value;
            remove => ((INonSerializedPropertyNode)Indexer).ValueChanged -= value;
        }
        public void NotifyValueChanged() => ((INonSerializedPropertyNode)Indexer).NotifyValueChanged();
        public VTypeFlags Flags => Indexer.Flags & ~(VTypeFlags.List | VTypeFlags.Array);
        public string Name => "Array.data[" + Index + "]";
        public IPropertyController Controller => Indexer.Controller;
        public SerializedObject SerializedObject => Indexer.SerializedObject;
        public string PropertyPath => ParentPath + "." + Name;
        public string ParentPath => Indexer.PropertyPath;
        public IBindableDataSource DataSource { get; set; }
    }
    
    public class NonSerializedPropertyNodeIndex : NonSerializedPropertyNodeIndexBase { }
    public class NonSerializedPropertyNodeIndexTree : NonSerializedPropertyNodeIndexBase, INodeTree { }
}
