using System;
using System.Reflection;
using Space3x.InspectorAttributes.Types;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor
{
    public struct NamedSymbol
    {
        public string Name { get; }
        public string AssemblyName { get; }
        public TypeAttributes Attributes { get; }
        public string DisplayName { get; }
        public Type Value { get; }
        
        public NamedSymbol(NamedType namedType) : this((Type)namedType) { }
        
        public NamedSymbol(Type type)
        {
            Value = type;
            Name = type.Name;
            AssemblyName = type.Assembly.GetName().Name;
            Attributes = type.Attributes;
            DisplayName = type.GetDisplayName();
        }
    }
}
