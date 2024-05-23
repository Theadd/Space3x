using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Allows to select a type from a list of types deriving from a base type
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DerivedTypeSearcherAttribute : TypeSearcherAttribute //, IDerivedType, IConditional
    {
        public Type BaseType { get; internal set; }
        public List<Type> GenericParameterTypes { get; internal set; }
//        public bool Value { get; set; } = true;
//        public string OnCondition { get; set; } = string.Empty;
        
        public DerivedTypeSearcherAttribute(Type baseType)
        {
            BaseType = baseType;
            GenericParameterTypes = Type.EmptyTypes.ToList();
        }
        
        public DerivedTypeSearcherAttribute(bool applyToCollection, Type baseType) : base(applyToCollection: applyToCollection)
        {
            BaseType = baseType;
            GenericParameterTypes = Type.EmptyTypes.ToList();
        }
        
        public DerivedTypeSearcherAttribute(Type baseType, params Type[] genericParameterTypes)
        {
            BaseType = baseType;
            GenericParameterTypes = genericParameterTypes.ToList();
        }
        
        public DerivedTypeSearcherAttribute(bool applyToCollection, Type baseType, params Type[] genericParameterTypes) : base(applyToCollection: applyToCollection)
        {
            BaseType = baseType;
            GenericParameterTypes = genericParameterTypes.ToList();
        }
    }
}
