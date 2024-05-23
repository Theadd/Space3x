using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Property, 
        AllowMultiple = true, 
        Inherited = true)]
    public class VisibleAttribute : 
        PropertyAttribute,
        IVisibleEx<VisibleAttribute>
    {
        public bool Visible { get; private set; }
        
        public VisibleAttribute(bool visible = true) => Visible = visible;
    }
}
