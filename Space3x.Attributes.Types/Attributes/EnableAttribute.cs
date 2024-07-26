using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Determines whether the next VisualTarget is enabled or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class EnableAttribute : 
        PropertyAttribute,
        IEnableEx<EnableAttribute>
    {
        public bool Enabled { get; set; }
        
        public EnableAttribute(bool enable = true) => Enabled = enable;
    }
}
