using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Allows to select a type from a list of types
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TypeSearcherAttribute : BaseSearchableTypeAttribute //, ITrackChanges, IValues
    {
        internal List<Type> RawTypes;
        
//        public string Values { get; set; } = string.Empty;
//        
//        /// <summary>
//        /// The name (as in nameof()) of the property that will be used to track changes in order to force a refresh.
//        /// </summary>
//        public string TrackChangesOn { get; set; } = string.Empty;

        protected TypeSearcherAttribute() : this(false) { }

        protected TypeSearcherAttribute(bool applyToCollection) : base(applyToCollection: applyToCollection)
            => RawTypes = null;     // Type.EmptyTypes.ToList();

        public TypeSearcherAttribute(params Type[] allTypes) 
            => RawTypes = new List<Type>(allTypes);
        
        public TypeSearcherAttribute(bool applyToCollection, params Type[] allTypes) : base(applyToCollection: applyToCollection) 
            => RawTypes = new List<Type>(allTypes);
    }
}
