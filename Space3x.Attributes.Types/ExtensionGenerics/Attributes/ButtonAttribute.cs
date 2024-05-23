using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string MethodName { get; private set; } = string.Empty;
        
        public string Text { get; set; } = string.Empty;

        public ButtonAttribute() { }
        
        public ButtonAttribute(string methodName) => MethodName = methodName;
    }
}
