using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class TagPickerAttribute : PropertyAttribute
    {
        public TagPickerAttribute() : base(applyToCollection: false) { }
    }
}
