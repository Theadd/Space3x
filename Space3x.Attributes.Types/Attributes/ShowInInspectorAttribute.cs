using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Allows a non-serialized property or field to be shown in the inspector.
    /// </summary>
    /// <remarks>Requires the <see cref="AllowExtendedAttributes"/> to be properly annotated.</remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ShowInInspectorAttribute : PropertyAttribute { }
}
