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
        
        protected VisibleOnAttribute() 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: false)
#endif
        { }
        
        protected VisibleOnAttribute(bool applyToCollection) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: applyToCollection)
#endif
        { }
        
        public VisibleOnAttribute(string condition) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: true)
#endif
            => Condition = condition;
    }
}
