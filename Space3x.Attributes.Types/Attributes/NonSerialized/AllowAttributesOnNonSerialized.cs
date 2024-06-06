using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Property 
                    | AttributeTargets.Field 
                    | AttributeTargets.Constructor 
                    | AttributeTargets.Method
                    | AttributeTargets.Class, 
        AllowMultiple = false, Inherited = false)]
    public class AllowAttributesOnNonSerializedAttribute : PropertyAttribute { }
}
