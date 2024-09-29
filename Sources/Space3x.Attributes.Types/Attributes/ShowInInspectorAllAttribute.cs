using System;

namespace Space3x.Attributes.Types
{
    [Flags]
    public enum ShowAll
    {
        Default = 0,
        Fields = 1,
        Properties = 2,
        Methods = 4,
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class ShowInInspectorAllAttribute : Attribute
    {
        public ShowAll ShowAll { get; private set; }
        
        public ShowInInspectorAllAttribute(ShowAll showAll) => ShowAll = showAll;
    }
}
