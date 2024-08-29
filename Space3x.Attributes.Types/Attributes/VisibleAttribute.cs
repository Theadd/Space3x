using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Determines the visibility of the next VisualTarget.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VisibleAttribute : 
        PropertyAttribute,
        IVisibleEx<VisibleAttribute>
    {
        public bool Visible { get; private set; }
        
        public VisibleAttribute(bool visible = true) => Visible = visible;
    }
}
