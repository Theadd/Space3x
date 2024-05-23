using System;
using JetBrains.Annotations;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor
{
    [UsedImplicitly]
    public class VisibleEx : Extension<IVisibleOn>
    {
        public override bool TryApply<TValue, TContent>(IExtensionContext context, TContent content, out TValue outValue, TValue defaultValue)
        {
            if (context is IDrawer drawer)
            {
                drawer.VisualTarget.SetVisible(content.Visible);
                if (!(content.Visible is TValue value))
                    value = defaultValue;
                
                outValue = value;
                return true;
            }

            outValue = defaultValue;
            return false;
        }
    }
}
