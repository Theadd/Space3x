using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(GradientSliderAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(GradientSliderAttribute), true)]
    public class GradientSliderDrawer : Drawer<GradientSliderAttribute>
    {
        private Invokable<object, Color> m_MinColorInvokable;
        private Invokable<object, Color> m_MaxColorInvokable;
        private string m_PrevMinColor;
        private string m_PrevMaxColor;
        
        public override GradientSliderAttribute Target => (GradientSliderAttribute) attribute;
        
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            var declaredPropertyType = property.GetUnderlyingType();
            VisualElement slider = declaredPropertyType switch
            {
                _ when declaredPropertyType == typeof(double) || declaredPropertyType == typeof(float) => 
                    new LinearGradientSlider() { lowValue = Target.Min, highValue = Target.Max, label = preferredLabel, showInputField = Target.ShowValue, colorModel = Target.Model, pixelsWidth = Target.PixelsWidth },
                _ when declaredPropertyType.IsPrimitive => 
                    new LinearGradientSliderInt() { lowValue = (int)Target.Min, highValue = (int)Target.Max, label = preferredLabel, showInputField = Target.ShowValue, colorModel = Target.Model, pixelsWidth = Target.PixelsWidth },
                _ => new BindableElement()
            };
            slider.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            ((IBindable)slider).BindProperty(Property);
            Container = slider;
            OnUpdate();
            Container.TrackSerializedObjectValue(Property, OnUpdate);
            
            return slider;
        }

        public override void OnUpdate()
        {
            if (Container is not IGradientSlider slider)
                throw new NotSupportedException();

            var lowColor = Target.MinColor != m_PrevMinColor
                ? GetColor(slider.lowValueColor, Target.MinColor, out m_MinColorInvokable)
                : m_MinColorInvokable != null
                    ? (Color)(m_MinColorInvokable.Parameters == null
                        ? m_MinColorInvokable.Invoke()
                        : m_MinColorInvokable.InvokeWith(m_MinColorInvokable.Parameters))
                    : slider.lowValueColor;
            
            var highColor = Target.MaxColor != m_PrevMaxColor
                ? GetColor(slider.highValueColor, Target.MaxColor, out m_MaxColorInvokable)
                : m_MaxColorInvokable != null
                    ? (Color)(m_MaxColorInvokable.Parameters == null
                        ? m_MaxColorInvokable.Invoke()
                        : m_MaxColorInvokable.InvokeWith(m_MaxColorInvokable.Parameters))
                    : slider.highValueColor;

            m_PrevMinColor = Target.MinColor;
            m_PrevMaxColor = Target.MaxColor;
            
            slider.lowValueColor = lowColor;
            slider.highValueColor = highColor;
        }

        private Color GetColor(Color currentColor, string colorValue, out Invokable<object, Color> generatedInvokable)
        {
            generatedInvokable = null;

            if (ColorUtility.TryParseHtmlString(colorValue, out var color))
                return color;

            if (Property.TryCreateInvokable<object, Color>(colorValue, out var invokable, drawer: this))
            {
                generatedInvokable = invokable;
                return (Color)(invokable.Parameters == null ? invokable.Invoke() : invokable.InvokeWith(invokable.Parameters));
            }

            return currentColor;
        }
    }
}
