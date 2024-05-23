using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property, 
        AllowMultiple = true, 
        Inherited = true)]
    public class VisibleOnAttribute : 
        PropertyAttribute,
        IVisibleOnEx<VisibleOnAttribute>
    {
        public bool Visible { get; set; } = true;
        public string Condition { get; private set; }
        
        public VisibleOnAttribute(string condition) => Condition = condition;
    }
}
