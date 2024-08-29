using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Displays a LayerField selector for the annotated property in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class LayerPickerAttribute : PropertyAttribute
    {
        public LayerPickerAttribute() : base(applyToCollection: false) { }
    }
}
