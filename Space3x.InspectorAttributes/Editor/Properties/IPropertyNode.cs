using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public interface IProperty
    {
        public string Name { get; }
        
        public string ParentPath { get; }
        
        /* Computed properties */
        public string PropertyPath { get; }
    }

    public interface IPropertyWithSerializedObject
    {
        public SerializedObject SerializedObject { get; }
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

    public interface ISerializedPropertyNode : IPropertyNode, IPropertyWithSerializedObject
    {
        public VisualElement Field { get; }
    }
    
    public interface INonSerializedPropertyNode : IPropertyNode, IPropertyWithSerializedObject
    {
        public VisualElement Field { get; }
    }

    // TODO
    public interface INodeArray : IProperty
    {
        public IEnumerable<IPropertyNode> Children();

        public int ChildCount { get; }
    }
}
