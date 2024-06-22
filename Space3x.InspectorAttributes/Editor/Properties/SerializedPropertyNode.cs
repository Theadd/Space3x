using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class SerializedPropertyNode : ISerializedPropertyNode
    {
        public VTypeFlags Flags { get; set; }
        public string Name { get; set; }
        public SerializedObject SerializedObject { get; set; }
        public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public string ParentPath { get; set; }
        // public object Value { get; }
        // public Type ValueType { get; }
        // public VisualElement Field { get; set; }
        public int Hash => this.GetHashCode();
    }
    
    public class SerializedPropertyNodeIndex : ISerializedPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }
        public int Index { get; set; }
        public VTypeFlags Flags => Indexer.Flags;
        public string Name => "Array.data[" + Index + "]";
        public SerializedObject SerializedObject => Indexer.SerializedObject;
        public string PropertyPath => ParentPath + "." + Name;

        public string ParentPath => Indexer.PropertyPath;
        // TODO: everything below
        // public object Value { get; }
        // public Type ValueType { get; }
        // public VisualElement Field { get; set; }
    }
}
