using System;
using Space3x.InspectorAttributes.Editor.Drawers.NonSerialized;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class NonSerializedPropertyNode : INonSerializedPropertyNode
    {
        public event Action<IProperty> ValueChanged;
        public void NotifyValueChanged() => ValueChanged?.Invoke(this); 
        public VTypeFlags Flags { get; set; }
        public string Name { get; set; }
        public SerializedObject SerializedObject { get; set; }
        public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public string ParentPath { get; set; }
        public object Value { get; }
        public Type ValueType { get; }
        public VisualElement Field { get; set; }
    }
    
    public class NonSerializedPropertyNodeIndex : INonSerializedPropertyNodeIndex
    {
        public IBindablePropertyNode Indexer { get; set; }
        public int Index { get; set; }
        public event Action<IProperty> ValueChanged
        {
            add => ((INonSerializedPropertyNode)Indexer).ValueChanged += value;
            remove => ((INonSerializedPropertyNode)Indexer).ValueChanged -= value;
        }
        public void NotifyValueChanged() => ((INonSerializedPropertyNode)Indexer).NotifyValueChanged();
        public VTypeFlags Flags => Indexer.Flags;
        public string Name => "Array.data[" + Index + "]";
        public SerializedObject SerializedObject => Indexer.SerializedObject;
        public string PropertyPath => ParentPath + "." + Name;
        public string ParentPath => Indexer.PropertyPath;
        // TODO: everything below
        public object Value { get; }
        public Type ValueType { get; }
        public VisualElement Field { get; set; }
    }
}
