using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property, 
        AllowMultiple = true, 
        Inherited = true)]
    public class EnableAttribute : 
        PropertyAttribute,
        IEnableEx<EnableAttribute>
    {
        public bool Enabled { get; set; }
        
        public EnableAttribute(bool enable = true, bool applyToCollection = false)
            : base(applyToCollection: applyToCollection) => Enabled = enable;
    }
}
