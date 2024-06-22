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
        // public object Value { get; }
        //
        // public Type ValueType { get; }
    }
    
    public interface IBindablePropertyNode : IPropertyNode, IPropertyWithSerializedObject
    {
        // public VisualElement Field { get; set; }
    }

    public interface ISerializedPropertyNode : IBindablePropertyNode { }

    public interface INonSerializedPropertyNode : IBindablePropertyNode
    {
        public event Action<IProperty> ValueChanged;
        public void NotifyValueChanged();
    }

    public interface IPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }

        public int Index { get; set; }
    }
    
    public interface ISerializedPropertyNodeIndex : ISerializedPropertyNode, IPropertyNodeIndex { }
    
    public interface INonSerializedPropertyNodeIndex : INonSerializedPropertyNode, IPropertyNodeIndex { }
    
    // TODO
    public interface INodeArray : IProperty
    {
        public IEnumerable<IPropertyNode> Children();

        public int ChildCount { get; }
    }
}
