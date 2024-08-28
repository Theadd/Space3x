using System;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class InvokablePropertyNodeBase : BindablePropertyNode, IInvokablePropertyNode
    {
        // public VTypeFlags Flags { get; set; }
        // public string Name { get; set; }
        // public override IPropertyController Controller { get; set; }
        
        // public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
        //     ? (ParentPath ?? "") + (Name ?? "")
        //     : ParentPath + "." + Name;
        public override string ParentPath { get; set; }
        public event Action<IInvokablePropertyNode> ValueChanged;
        public void NotifyValueChanged() => ValueChanged?.Invoke(this);
        public object Value { get; set; }
        // public IBindableDataSource DataSource { get; set; }
    }
    
    public class InvokablePropertyNode : InvokablePropertyNodeBase { }
}
