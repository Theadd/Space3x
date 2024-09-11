using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Allows a non-serialized property or field to be shown in the inspector. When used on a method, it displays the
    /// return value of the method in a read-only TextField along a Button to call the method.
    /// </summary>
    /// <remarks>Requires the <see cref="AllowExtendedAttributes"/> to be properly annotated.</remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ShowInInspectorAttribute : PropertyAttribute { }
}
