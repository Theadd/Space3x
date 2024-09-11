using System;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public abstract class InvokablePropertyNodeBase : BindablePropertyNode, IInvokablePropertyNode
    {
        public override string ParentPath { get; set; }
        public event Action<IInvokablePropertyNode> ValueChanged;
        public void NotifyValueChanged() => ValueChanged?.Invoke(this);
        public object Value { get; set; }
    }

    public class InvokablePropertyNode : InvokablePropertyNodeBase { }
}
