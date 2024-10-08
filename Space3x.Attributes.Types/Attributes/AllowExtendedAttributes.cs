﻿using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// This attribute must be declared as the top-most attribute on the first member in every class containing
    /// any <see cref="ShowInInspectorAttribute">[ShowInInspector]</see> attribute in order to take them into account,
    /// otherwise, those non-serialized members with the <see cref="ShowInInspectorAttribute">[ShowInInspector]</see>
    /// attribute will be ignored. Where that "first member" must be a serialized one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AllowExtendedAttributes : PropertyAttribute { }
}
