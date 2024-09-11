using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Draws a Slider limited within provided values. Creates a MinMaxSlider on <see cref="Vector2"/> properties and
    /// regular sliders for numeric values. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RangeSliderAttribute : PropertyAttribute
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public RangeSliderAttribute(float min = 0f, float max = 1f)
        {
            Min = min;
            Max = max;
        }
    }
}
