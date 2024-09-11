using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor
{
    public static class InvokablePropertyNodeExtensions
    {
        public static void SetValue(this IInvokablePropertyNode propertyNode, object value)
        {
            ((InvokablePropertyNodeBase)propertyNode).Value = (object)value;
            propertyNode.NotifyValueChanged();
        }        
    }
}
