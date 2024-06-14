using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InlineAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whether to show as enabled, not the property field itself, but it's inlined content UI.
        /// </summary>
        public bool ContentEnabled { get; } = true;
    }
}
