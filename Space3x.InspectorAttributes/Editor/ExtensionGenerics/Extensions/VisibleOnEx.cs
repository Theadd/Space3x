using System;
using JetBrains.Annotations;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.UiToolkit.Types;

namespace Space3x.InspectorAttributes.Editor
{
    [UsedImplicitly]
    public class VisibleOnEx : Extension<IVisibleOnEx>
    {
        public override bool TryApply<TValue, TContent>(IExtensionContext context, TContent content, out TValue outValue, TValue defaultValue)
        {
            TValue result = defaultValue;
            if (ConditionEx.TryCreateInvokable<TValue, TValue, TContent>(context, content, out var invokable))
                result = invokable.Invoke();
            
            if (!(result is bool bValue)) 
                throw new Exception(nameof(VisibleOnEx) + " expects an out value to be of boolean type.");
            bValue = bValue ? content.Visible : !content.Visible;
            if (!(bValue is TValue value))
                throw new Exception(nameof(VisibleOnEx) + " expects an out value to be of type " + typeof(TValue).Name + ".");
            outValue = value;

            if (context is IDrawer drawer)
            {
                // drawer.VisualTarget.SetVisible(bValue);
                drawer.VisualTarget.WithClasses(!bValue, UssConstants.UssHidden);
                return true;
            }
            
            return false;
        }
    }
}
