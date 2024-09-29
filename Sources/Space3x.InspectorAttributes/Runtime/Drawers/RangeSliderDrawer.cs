using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(RangeSliderAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(RangeSliderAttribute), true)]
    public class RangeSliderDrawer : Drawer<RangeSliderAttribute>
    {
        public override RangeSliderAttribute Target => (RangeSliderAttribute) attribute;
        
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            var declaredPropertyType = property.GetUnderlyingType();
            VisualElement slider = declaredPropertyType switch
            {
                _ when declaredPropertyType == typeof(Vector2) || declaredPropertyType == typeof(Vector2Int) => 
                    new MinMaxSlider() { lowLimit = Target.Min, highLimit = Target.Max, label = preferredLabel },
                _ when declaredPropertyType == typeof(double) || declaredPropertyType == typeof(float) => 
                    new Slider() { lowValue = Target.Min, highValue = Target.Max, label = preferredLabel, showInputField = Target.ShowValue },
                _ when declaredPropertyType.IsPrimitive => 
                    new SliderInt() { lowValue = (int)Target.Min, highValue = (int)Target.Max, label = preferredLabel, showInputField = Target.ShowValue  },
                _ => new BindableElement()
            };
            slider.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            ((IBindable)slider).BindProperty(Property);
            return slider;
        }
    }
}
