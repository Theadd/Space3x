using System;
using Space3x.Properties.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    public abstract class NonSerializedPropertyNodeBase : BindablePropertyNode, INonSerializedPropertyNode
    {
        public event Action<IPropertyNode> ValueChanged;
        public void NotifyValueChanged(IPropertyNode propertyNode) => ValueChanged?.Invoke(propertyNode ?? this);
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
            remove
            {
                try
                {
                    ((INonSerializedPropertyNode)Indexer).ValueChanged -= value;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void NotifyValueChanged(IPropertyNode propertyNode) => ((INonSerializedPropertyNode)Indexer).NotifyValueChanged(propertyNode);
        public override VTypeFlags Flags => Indexer.Flags & ~(VTypeFlags.List | VTypeFlags.Array);
        public override string Name => "Array.data[" + Index + "]";
        public override IPropertyController Controller => Indexer.Controller;
        public object SerializedObject => Indexer.SerializedObject;
        public override string PropertyPath => ParentPath + "." + Name;
        public override string ParentPath => Indexer.PropertyPath;
    }
    
    public class NonSerializedPropertyNodeIndex : NonSerializedPropertyNodeIndexBase { }
    public class NonSerializedPropertyNodeIndexTree : NonSerializedPropertyNodeIndexBase, INodeTree { }
}
