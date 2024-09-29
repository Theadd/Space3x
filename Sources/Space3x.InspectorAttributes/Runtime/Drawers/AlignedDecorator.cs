using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(AlignedAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(AlignedAttribute), true)]
    public class AlignedDecorator : Decorator<AutoDecorator, AlignedAttribute>
    {
        public override AlignedAttribute Target => (AlignedAttribute) attribute;

        public override void OnAttachedAndReady(VisualElement element)
        {
            VisualTarget.WithClasses(true, /*UssConstants.UssInspector,*/ UssConstants.UssInspectorContainer, UssConstants.UssAligned);
        }

        public override void OnUpdate()
        {
            if (VisualTarget.Q<Label>(className: "unity-label") is Label label)
            {
                label.style.width = StyleKeyword.Null;
                if (!Property.IsRuntimeUI())
                    label.parent.EnableInClassList(UssConstants.UssAlignedAuto, true);
                
                if (!float.IsNaN(Target.MinWidth)) 
                    label.style.minWidth = (float)Target.MinWidth;
                else
                    label.style.minWidth = StyleKeyword.Null;
                
                if (!float.IsNaN(Target.MarginRight))
                    label.style.marginRight = (float)Target.MarginRight;
            }
#if SPACE3X_DEBUG
            else
                throw new NullReferenceException($"{nameof(AlignedDecorator)} found no label within its VisualTarget: {VisualTarget.AsString()}");
#endif
        }
    }
}
