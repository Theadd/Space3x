using System;
using JetBrains.Annotations;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;

namespace Space3x.InspectorAttributes
{
    [UsedImplicitly]
    public class EnableOnEx : Extension<IEnableOnEx>
    {
        public override bool TryApply<TValue, TContent>(IExtensionContext context, TContent content, out TValue outValue, TValue defaultValue)
        {
            TValue result = defaultValue;
            if (ConditionEx.TryCreateInvokable<TValue, TValue, TContent>(context, content, out var invokable))
                result = invokable.Parameters == null ? invokable.Invoke() : invokable.InvokeWith(invokable.Parameters);
            
            if (!(result is bool bValue)) 
                throw new Exception(nameof(EnableOnEx) + " expects an out value to be of boolean type.");
            bValue = bValue ? content.Enabled : !content.Enabled;
            if (!(bValue is TValue value))
                throw new Exception(nameof(EnableOnEx) + " expects an out value to be of type " + typeof(TValue).Name + ".");
            outValue = value;

            if (context is IDrawer drawer)
            {
                DebugLog.Warning($"{drawer.GetType().Name} #{drawer.GetHashCode()} EnableOnEx: {bValue} ON {drawer.VisualTarget.AsString()}");
                drawer.VisualTarget.SetEnabled(bValue);
                return true;
            }
            
            return false;
        }
    }
}
