using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Determines whether the next VisualTarget is enabled or not based on a condition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class EnableOnAttribute : 
        PropertyAttribute,
        IEnableOnEx<EnableOnAttribute>
    {
        public bool Enabled { get; set; } = true;
        public string Condition { get; private set; }
        
        public EnableOnAttribute(string condition) => Condition = condition;
    }
}
