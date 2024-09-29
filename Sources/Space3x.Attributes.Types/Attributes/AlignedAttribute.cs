using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AlignedAttribute : PropertyAttribute
    {
        public float MinWidth { get; set; } = float.NaN;
        
        public float MarginRight { get; set; } = float.NaN;

        public AlignedAttribute() : base(applyToCollection: false) { }
        
        protected AlignedAttribute(bool applyToCollection) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: applyToCollection)
#endif
        { }
    }
}
