using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.InspectorAttributes.Utilities;
using UnityEngine;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Extensions
{
    public static class TypeSearcherAttributeExtensions
    {
        public static List<Type> GetAllTypes(this TypeSearcherAttribute self) =>
            self.CachedTypes ??= new List<Type>(self.RawTypes ?? Type.EmptyTypes.ToList());

        public static void ReloadCache(this TypeSearcherAttribute self)
        {
            self.CachedTypes?.Clear();
            self.CachedTypes = null;
        }
    }
    
    public static class BaseSearchableTypeAttributeExtensions
    {
        public static List<Type> GetAllTypes(this BaseSearchableTypeAttribute self) =>
            self.CachedTypes ??= Type.EmptyTypes.ToList();

        public static void ReloadCache(this BaseSearchableTypeAttribute self) { }
    }

    public static class DerivedTypeSearcherAttributeExtensions
    {
        public static List<Type> GetAllTypes(this DerivedTypeSearcherAttribute self, IPropertyNode property)
        {
            // self.ReloadCache();
            return self.CachedTypes ??= self.GetDerivedTypes(property);
        }
        
        public static void ReloadCache(this DerivedTypeSearcherAttribute self)
        {
            DebugLog.Info("<b><color=#0000FFFF>Reloading derived types cache.</color></b>");
            self.CachedTypes?.Clear();
            self.CachedTypes = null;
            self.RawTypes?.Clear();
            self.RawTypes = null;
        }

        private static List<Type> GetDerivedTypes(this DerivedTypeSearcherAttribute self, IPropertyNode property)
        {
            var allDerivedTypes = self.RawTypes ??= self.GetAllDerivedTypes();
            if (self is ISealedExtension<ICondition> and ICondition selfWithConditional)
            {
                if (!string.IsNullOrEmpty(selfWithConditional.Condition))
                {
                    if (property.TryCreateInvokable<Type, bool>(selfWithConditional.Condition, out var predicate))
                        return allDerivedTypes
                            .Where(t => predicate.Invoke(t))
                            .ToList();

                    Debug.LogError($"Could not find method {selfWithConditional.Condition} on {property.GetDeclaringObject()}");
                }
            }
                
            return allDerivedTypes;
        }
        
        private static List<Type> GetAllDerivedTypes(this DerivedTypeSearcherAttribute self) =>
            self.BaseType
                .GetAllTypes(self.GenericParameterTypes)
                .Where(t => (t.MemberType & MemberTypes.NestedType) != MemberTypes.NestedType || IsAllowedTypeName(t.Name))
                .ToList();

        private static bool IsAllowedTypeName(string name)
        {
            var iMax = name.Length - 1;
            for (var i = 0; i < iMax; i++)
            {
                if (name[i] != '>') continue;
                switch (name[i + 1])
                {
                    case 'd':
                    case 'g':
                    case 'j':
                    case 'e':
                        if (iMax >= i + 2 && name[i + 2] == '_' && name[i + 3] == '_')
                            return false;
                        break;
                    case 'c':
                    case 'f':
                        if (i > 0 && name[i - 1] == '<')
                            return false;
                        break;
                }
            }

            return true;
        }
    }
}
