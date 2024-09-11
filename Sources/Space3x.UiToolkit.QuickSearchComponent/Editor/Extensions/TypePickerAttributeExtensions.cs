using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.UiToolkit.QuickSearchComponent.Editor.Drawers;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Extensions
{
    public static class TypePickerAttributeExtensions
    {
        public static IEnumerable<Type> GetAllTypes(this ITypePickerAttribute self, IDrawer drawer, bool includeAbstractTypes = true) =>
            ((ITypeSearchHandler)self.Handler).CachedTypes ??= self.RemoveDuplicates
                ? includeAbstractTypes
                    ? self.RebuildTypes(drawer).Distinct()
                    : self.RebuildTypes(drawer).Distinct().Where(t => !t.IsAbstract && !t.IsInterface)
                : includeAbstractTypes
                    ? self.RebuildTypes(drawer)
                    : self.RebuildTypes(drawer).Where(t => !t.IsAbstract && !t.IsInterface);

        private static IEnumerable<Type> RebuildTypes(this ITypePickerAttribute self, IDrawer drawer)
        {
            ReadOnlyCollection<Type> rawTypes = null;
            if (!string.IsNullOrEmpty(self.PropertyName) && drawer.Property.TryCreateInvokable<object, Type[]>(self.PropertyName, out var invokable, drawer: drawer))
                rawTypes = (invokable.Parameters == null ? invokable.Invoke() : invokable.InvokeWith(invokable.Parameters))?.ToList().AsReadOnly();

            if (rawTypes == null)
                rawTypes = (self.Types ?? Type.EmptyTypes).ToList().AsReadOnly();

            if (!rawTypes.Any()) return rawTypes;
            var hasGenerics = (self.GenericParameterTypes?.Length ?? 0) > 0;

            return self.DerivedTypes switch
            {
                true when hasGenerics => rawTypes.SelectMany(baseType => baseType
                    .GetAllTypes(self.GenericParameterTypes)
                    .Where(t => (t.MemberType & MemberTypes.NestedType) != MemberTypes.NestedType || IsAllowedTypeName(t.Name))),
                true => rawTypes.SelectMany(baseType => baseType
                    .GetAllTypes()
                    .Where(t => (t.MemberType & MemberTypes.NestedType) != MemberTypes.NestedType || IsAllowedTypeName(t.Name))),
                _ => rawTypes
            };
        }

        public static void ReloadCache(this ITypePickerAttribute self) => 
            ((ITypeSearchHandler)self.Handler).CachedTypes = null;
        
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
