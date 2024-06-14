using System;
using System.Collections.Generic;
using System.Linq;
//using System.Collections;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor
{
    public static class QuickType
    {
        internal static readonly Dictionary<Type, string> Primitives = new Dictionary<Type, string>
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(string), "string" },
            { typeof(char), "char" },
            { typeof(bool), "bool" },
            { typeof(void), "void" },
            { typeof(object), "object" },
        };
        
        public static string GetDisplayName(this Type type)
        {
            if (type == null) return "";
            var name = "";
            if (type.IsGenericType && type.Name.IndexOf('`') < 0)
                name = type.Name;
            else
                try
                {
                    name = type.GetTypeNameWithGenericParameters();
                }
                catch (Exception)
                {
                    name = type.Name;
                }

            return "<voffset=0em><line-height=0px><alpha=#FF>" + name + " <br><voffset=0em><align=\"right\"><alpha=#7F>" + type.Assembly.GetName().Name + "<alpha=#FF>";
        }
        
        public static string SimpleName(this Type type)
        {
            if (type == null) return "";
            
            if (type.IsGenericType && type.Name.IndexOf('`') < 0)
                // Debug.Log($"  [SimpleName] Inconsistent type: {type}\n{type.Assembly.Location}");
                return type.Name;
            return (type.IsGenericType) ? type.Name[..type.Name.IndexOf('`')] : type.Name;
        }
        
        private static string GetTypeNameWithGenericParameters(this Type type)
        {
            if (type.IsPrimitive || type == typeof(decimal) || type == typeof(string) || type == typeof(void) || type == typeof(object))
                if (Primitives.TryGetValue(type, out var sharpName))
                    return sharpName;
            if (type.IsGenericParameter)
                return type.Name;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type).GetTypeNameWithGenericParameters() + "?";

            var name = type.Name;

            if (type.IsGenericType && name.Contains('`'))
                name = name[..name.IndexOf('`')];

            var genericArguments = (IEnumerable<Type>) type.GetGenericArguments();

            if (type.IsNested)
            {
                name = type.DeclaringType.GetTypeNameWithGenericParameters() + "." + name;
                if (type.DeclaringType.IsGenericType)
                    genericArguments = genericArguments.Skip(type.DeclaringType.GetGenericArguments().Length).ToList();
            }

            if (genericArguments.Any())
            {
                name += "<";
                name += string.Join(", ", genericArguments.Select(t => t.GetTypeNameWithGenericParameters()).ToArray());
                name += ">";
            }

            return name;
        }
    }
}
