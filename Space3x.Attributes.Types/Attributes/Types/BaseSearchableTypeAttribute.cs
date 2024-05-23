using System;
using System.Collections.Generic;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public abstract class BaseSearchableTypeAttribute : PropertyAttribute
    {
        internal List<Type> CachedTypes { get; set; }

        protected BaseSearchableTypeAttribute() : this(false) { }

        protected BaseSearchableTypeAttribute(bool applyToCollection) : base(applyToCollection: applyToCollection) { }
    }
}
