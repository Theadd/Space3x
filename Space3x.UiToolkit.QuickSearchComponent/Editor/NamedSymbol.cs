using System;
using System.Reflection;
using JetBrains.Annotations;
using Space3x.InspectorAttributes.Types;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor
{
    public struct NamedSymbol
    {
        public string Name { get; }
        // public string BaseTypeName { get; }
        public string AssemblyName { get; }
        public TypeAttributes Attributes { get; }
        // public string Namespace { get; }
        // [CanBeNull] private string m_DisplayName;
        // public string DisplayName => m_DisplayName ??= Value.GetDisplayName();
        public string DisplayName { get; }
        public Type Value { get; }
        
        public NamedSymbol(NamedType namedType) : this((Type)namedType) { }
        
        public NamedSymbol(Type type)
        {
            Value = type;
            Name = type.Name;
            // BaseTypeName = type.BaseType?.Name ?? string.Empty;
            AssemblyName = type.Assembly.GetName().Name;
            Attributes = type.Attributes;
            // m_DisplayName = null;
            // Namespace = type.Namespace;
            DisplayName = type.GetDisplayName();
        }
    }
}
