using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    public enum ColorModel
    {
        RGB = 0,
        HSV = 1
    }
    
    /// <summary>
    /// Draws a two-color linear gradient over the track of a slider field on numeric fields or properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class GradientSliderAttribute : PropertyAttribute
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public ColorModel Model { get; set; } = ColorModel.RGB;

        /// <summary>
        /// The horizontal resolution of the background image.
        /// </summary>
        public int PixelsWidth { get; set; } = 100;
        
        public bool ShowValue { get; set; } = false;
        
        /// <summary>
        /// An string of either a color in HTML/Hex format (see <see cref="ColorUtility.TryParseHtmlString"/>) or a
        /// member name returning a <see cref="Color"/> value.
        /// </summary>
        public string MinColor { get; set; } = "#00000000";
        
        /// <summary>
        /// An string of either a color in HTML/Hex format (see <see cref="ColorUtility.TryParseHtmlString"/>) or a
        /// member name returning a <see cref="Color"/> value.
        /// </summary>
        public string MaxColor { get; set; } = "#FFFFFFFF";

        public GradientSliderAttribute(float min = 0f, float max = 1f)
        {
            Min = min;
            Max = max;
        }
    }
}
