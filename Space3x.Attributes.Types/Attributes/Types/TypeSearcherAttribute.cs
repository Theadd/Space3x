using System;
using System.Collections.Generic;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Allows to select a type from a list of types
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TypeSearcherAttribute : BaseSearchableTypeAttribute
    {
        internal List<Type> RawTypes;

        protected TypeSearcherAttribute() : this(false) { }

        protected TypeSearcherAttribute(bool applyToCollection) : base(applyToCollection: applyToCollection)
            => RawTypes = null;

        public TypeSearcherAttribute(params Type[] allTypes) 
            => RawTypes = new List<Type>(allTypes);
        
        public TypeSearcherAttribute(bool applyToCollection, params Type[] allTypes) : base(applyToCollection: applyToCollection) 
            => RawTypes = new List<Type>(allTypes);
    }
}
