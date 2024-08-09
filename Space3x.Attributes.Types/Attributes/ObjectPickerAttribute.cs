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

        public ObjectPickerAttribute(Type type) : base(applyToCollection: false)
        {
            ObjectType = type;
        }
        
        public ObjectPickerAttribute() : base(applyToCollection: false) { }
    }
}
