using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Displays an UnityEngine.Object picker.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ObjectPickerAttribute : PropertyAttribute
    {
        public Type ObjectType { get; set; } = typeof(UnityEngine.Object);

        public ObjectPickerAttribute(Type type) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: false)
#endif
        {
            ObjectType = type;
        }
        
        public ObjectPickerAttribute() 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: false)
#endif
        { }
    }
}
