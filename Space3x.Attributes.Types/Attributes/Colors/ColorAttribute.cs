using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ColorAttribute : PropertyAttribute
    {
        public string Color { get; }

        public ColorAttribute(string color) => Color = color;
    }
}
