using System;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public abstract class BindablePropertyNode : IBindablePropertyNode
    {
        public IBindableDataSource DataSource { get; set; }

        internal event Action<IBindablePropertyNode> ValueChangedOnChildNode;
        
        internal void NotifyValueChangedOnChildNode(IBindablePropertyNode propertyNode) => ValueChangedOnChildNode?.Invoke(propertyNode);
        
        public virtual string Name { get; set; }
        public virtual string ParentPath { get; set; }
        public virtual string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public virtual VTypeFlags Flags { get; set; }
        public virtual IPropertyController Controller { get; set; }
        
        public bool Equals(IPropertyNode other) => ((IEquatable<IPropertyNode>)this).Equals(other);

        public override bool Equals(object obj) => 
            obj is IPropertyNodeIndex otherNode 
                ? ((IEquatable<IPropertyNode>)otherNode).Equals(this)
                : obj is BindablePropertyNode other && GetHashCode() == other.GetHashCode();
        
        bool IEquatable<IPropertyNode>.Equals(IPropertyNode other)
        {
            if (this is IPropertyNodeIndex node && other is IPropertyNodeIndex otherNode)
                return node.Indexer.Equals(otherNode.Indexer) && node.Index == otherNode.Index;
            return ReferenceEquals(this, other);
        }

        public override int GetHashCode() => Controller.DeclaringObject.GetHashCode() * 397 ^ PropertyPath.GetHashCode();

        public override string ToString() => PropertyPath;
        
        public static bool operator ==(BindablePropertyNode left, IPropertyNode right) => left?.Equals(right) ?? false;

        public static bool operator !=(BindablePropertyNode left, IPropertyNode right) => !left?.Equals(right) ?? false;
    }
}
