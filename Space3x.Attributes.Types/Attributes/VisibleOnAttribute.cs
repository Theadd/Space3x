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
        
        protected VisibleOnAttribute() : base(applyToCollection: false) { }
        
        protected VisibleOnAttribute(bool applyToCollection) : base(applyToCollection: applyToCollection) { }
        
        public VisibleOnAttribute(string condition) : base(applyToCollection: true) => Condition = condition;
    }
}
