using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class TagPickerAttribute : PropertyAttribute
    {
        public TagPickerAttribute() 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: false)
#endif
        { }
    }
}
