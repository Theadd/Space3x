using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Displays a button over the next VisualTarget in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class ButtonAttribute : PropertyAttribute
    {
        /// <summary>
        /// The name of the method to be called when the button is clicked.
        /// </summary>
        public string MethodName { get; private set; } = string.Empty;
        
        /// <summary>
        /// Custom button display text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        public ButtonAttribute() 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: false)
#endif
        { }
        
        public ButtonAttribute(string methodName) => MethodName = methodName;
    }
}
