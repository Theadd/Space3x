using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Determines the visibility of the next VisualTarget based on a condition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VisibleOnAttribute : 
        PropertyAttribute,
        IVisibleOnEx<VisibleOnAttribute>
    {
        public bool Visible { get; set; } = true;
        public string Condition { get; private set; }
        
        public VisibleOnAttribute(string condition) => Condition = condition;
    }
}
