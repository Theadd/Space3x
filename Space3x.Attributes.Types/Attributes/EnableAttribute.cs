using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Determines whether the next VisualTarget is enabled or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class EnableAttribute : 
        PropertyAttribute,
        IEnableEx<EnableAttribute>
    {
        public bool Enabled { get; set; }

        protected EnableAttribute(bool applyToCollection, bool enable) : base(applyToCollection: applyToCollection) => Enabled = enable;
        
        public EnableAttribute(bool enable = true) : base(applyToCollection: true) => Enabled = enable;
    }
}
