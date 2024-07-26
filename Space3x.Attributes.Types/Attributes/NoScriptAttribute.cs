using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Hides the disabled script field in the inspector. Can be placed on any property or field, although it is
    /// recommended to be placed on the first one for better readability.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class NoScriptAttribute : PropertyAttribute { }
}
