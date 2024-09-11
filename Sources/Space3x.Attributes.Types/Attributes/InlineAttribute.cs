using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Inlines the inspector UI of the annotated property's referenced value as if it was part of this one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InlineAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whether to show as enabled, not the property field itself, but its inlined content UI.
        /// </summary>
        public bool ContentEnabled { get; } = true;
    }
}
