using JetBrains.Annotations;
using Space3x.Attributes.Types;

namespace Space3x.InspectorAttributes
{
    [UsedImplicitly]
    public class EnableEx : Extension<IEnableEx>
    {
        public override bool TryApply<TValue, TContent>(IExtensionContext context, TContent content, out TValue outValue, TValue defaultValue)
        {
            if (context is IDrawer drawer)
            {
                drawer.VisualTarget.SetEnabled(content.Enabled);
                if (!(content.Enabled is TValue value))
                    value = defaultValue;
                
                outValue = value;
                return true;
            }

            outValue = defaultValue;
            return false;
        }
    }
}
