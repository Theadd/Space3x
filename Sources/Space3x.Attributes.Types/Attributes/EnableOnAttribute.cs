using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Determines whether the next VisualTarget is enabled or not based on a condition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class EnableOnAttribute : 
        PropertyAttribute,
        IEnableOnEx<EnableOnAttribute>
    {
        public bool Enabled { get; set; } = true;
        public string Condition { get; protected set; }
        
        protected EnableOnAttribute() 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: false)
#endif
        { }
        
        protected EnableOnAttribute(bool applyToCollection) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: applyToCollection)
#endif
        { }
        
        public EnableOnAttribute(string condition) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: true)
#endif
            => Condition = condition;
    }
}
