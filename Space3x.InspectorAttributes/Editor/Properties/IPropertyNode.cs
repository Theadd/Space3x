using System;
using System.Collections.Generic;

namespace Space3x.InspectorAttributes.Editor
{
    public interface IProperty
    {
        public string Name { get; }
        
        public string PropertyPath { get; }
        
        public string ParentPath { get; }
    }

    public interface INodeTree : IProperty
    {
        public IEnumerable<IPropertyNode> Children();
    }
    
    public interface IPropertyNode : IProperty
    {
        public object Value { get; }
        
        public Type ValueType { get; }
        
        
    }

    public interface ISerializedPropertyNode : IPropertyNode
    {
        
    }
    
    public interface INonSerializedPropertyNode : IPropertyNode
    {
        
    }

    public interface INodeArray : IProperty
    {
        public IEnumerable<IPropertyNode> Children();

        public int ChildCount { get; }
    }
}