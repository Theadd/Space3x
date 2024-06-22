using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace Space3x.InspectorAttributes.Utilities
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>, named after the fantastic Type extension
    /// methods from Unity's VisualScripting package team.
    /// <see cref="Unity.VisualScripting.TypeUtility"/>
    /// </summary>
    public static class TypeUtilityExtensions
    {
        /// <summary>
        /// Returns a simplified version of the full type name for the given type.
        /// <seealso cref="TypeName.Parse"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FullTypeName(this Type type) => TypeName.SimplifyFast(type.AssemblyQualifiedName);
        
        private static Assembly GetAssemblyByName(string name) => AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(asm => asm.GetName().Name == name);
        
        /// <summary>
        /// Returns a <see cref="Type"/> from a type name which is of one of those formats:
        ///     1. "AssemblyName Namespace.TypeName"
        ///     2. "Namespace.TypeName, AssemblyName"
        /// WHERE: "AssemblyName" is in its short form, no Version, Culture or PublicKeyToken.
        /// </summary>
        /// <param name="typeName">String representation of a System.Type</param>
        /// <returns>A System.Type matching provided typeName</returns>
        public static Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            var (assemblyName, fullName) = GetTypeNameParts(typeName);
            var assembly = GetAssemblyByName(assemblyName);
            return assembly.GetType(fullName);
        }
        
        public static (string assemblyName, string fullName) GetTypeNameParts(string typeName)
        {
            var splitter = typeName.LastIndexOf(' ');
            return typeName[splitter - 1] == ',' 
                ? (typeName[(splitter + 1)..], typeName[..(splitter - 1)])
                : (typeName[..splitter], typeName[(splitter + 1)..]);
        }

        public static IReadOnlyList<Type> GetAllTypes(this Type baseType) => GetAllTypes(baseType, Type.EmptyTypes);
        
        public static IReadOnlyList<Type> GetAllTypes(this Type baseType, IReadOnlyCollection<Type> genericParameterTypes) => 
            !baseType.IsGenericTypeDefinition 
                ? GetDerivedTypes(baseType) 
                : genericParameterTypes.Count == 0 
                    ? GetDerivedTypesOfGeneric(baseType) 
                    : GetDerivedTypesOfGenericWithParameters(baseType, genericParameterTypes);

        private static IReadOnlyList<Type> GetDerivedTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(t => t != baseType && baseType.IsAssignableFrom(t)).ToList();
        }
        
        private static IReadOnlyList<Type> GetDerivedTypesOfGeneric(Type genericType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType))
                .ToList();
        }
        
        private static IReadOnlyList<Type> GetDerivedTypesOfGenericWithParameters(Type genericType, IReadOnlyCollection<Type> genericParameterTypes)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(domainAssembly => domainAssembly.GetTypes())
                    .Where(t => t.GetInterfaces().Any(i => 
                        i.IsGenericType && i.GetGenericTypeDefinition() == genericType &&
                        i.GetGenericArguments().Count() == genericParameterTypes.Count &&
                        i.GetGenericArguments().Zip(genericParameterTypes, 
                                (f, s) => s.IsAssignableFrom(f))
                            .All(z => z)))
                    .ToList();
        }
    }
}
