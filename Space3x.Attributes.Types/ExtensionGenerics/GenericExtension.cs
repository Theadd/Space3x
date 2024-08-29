using System;
using System.Collections.Generic;
using Space3x.Attributes.Types.DeveloperNotes;

namespace Space3x.Attributes.Types
{
    [ViolatesYAGNI]
    public abstract class GenericExtension
    {
        private static readonly Dictionary<Type, GenericExtension> Extensions = new Dictionary<Type, GenericExtension>();

        public static Extension<TType> GetExtension<TType>(Type type, Type handler)
        {
            if (!Extensions.ContainsKey(type)) 
                Extensions.Add(type, Activator.CreateInstance(handler) as GenericExtension);

            return Extensions[type] as Extension<TType>;
        }
    }
}
