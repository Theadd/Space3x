using System;
using JetBrains.Annotations;
using Space3x.Attributes.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [UsedImplicitly]
    public class TrackChangesOnEx : Extension<ITrackChangesOnEx>
    {
        public override bool TryApply<TTarget, TContent>(IExtensionContext context, TContent content, TTarget target)
        {
            if (context is not IDrawer drawer)
                return Fail(new NotImplementedException($"{nameof(TrackChangesOnEx)} can only (currently) be applied to <u>{nameof(IDrawer)}</u>s."));
            
            if (target is not (IBindable and VisualElement element))
                return Fail(new NotImplementedException($"{nameof(TrackChangesOnEx)}'s Target for an {nameof(IDrawer)} must be an {nameof(IBindable)} {nameof(VisualElement)}."));
            
            if (!string.IsNullOrEmpty(content.PropertyName))
            {
                var trackedProperty = drawer.Property.GetParentProperty().FindPropertyRelative(content.PropertyName);
                if (trackedProperty != null)
                    element.TrackPropertyValue(trackedProperty, _ => drawer.OnUpdate());
                else
                    Debug.LogWarning($"Unable to find related property {content.PropertyName} on {drawer.Property.PropertyPath}");
            }

            return true;
        }
    }
}
