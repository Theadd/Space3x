using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Creates a DecoratorDrawer as an empty shell which can be implemented in-place by providing any suitable
    /// member name for the <see cref="OnCreate"/>, the <see cref="OnAttached"/> or the <see cref="OnUpdate"/>
    /// properties.
    /// </summary>
    /// <remarks>
    /// Or it could just be used to get access to the exposed UI Toolkit VisualElements hierarchy tree.
    /// </remarks>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class VirtualDecoratorAttribute : PropertyAttribute
    {
        /// <inheritdoc cref="Space3x.InspectorAttributes.Decorator{T,TAttribute}.OnCreatePropertyGUI"/>
        public string OnCreate { get; set; } = string.Empty;
        
        /// <inheritdoc cref="Space3x.InspectorAttributes.Decorator{T,TAttribute}.OnAttachedAndReady"/>
        public string OnAttached { get; set; } = string.Empty;
        
        /// <inheritdoc cref="Space3x.InspectorAttributes.Decorator{T,TAttribute}.OnUpdate"/>
        public string OnUpdate { get; set; } = string.Empty;
        
        public bool UpdateOnAnyValueChange { get; set; } = false;

        public VirtualDecoratorAttribute() : this(false) { }
    
        public VirtualDecoratorAttribute(bool applyToCollection)
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: applyToCollection)
#endif
        { }
    }
}