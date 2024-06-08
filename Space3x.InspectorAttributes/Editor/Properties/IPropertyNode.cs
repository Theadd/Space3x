using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

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
    
    public interface IPropertyNode : IProperty, IPropertyFlags
    {
        public object Value { get; }
        
        public Type ValueType { get; }
        
        
    }

    public interface ISerializedPropertyNode : IPropertyNode
    {
        public VisualElement Field { get; }
    }
    
    public interface INonSerializedPropertyNode : IPropertyNode
    {
        public VisualElement Field { get; }
    }

    public interface INodeArray : IProperty
    {
        public IEnumerable<IPropertyNode> Children();

        public int ChildCount { get; }
    }
}