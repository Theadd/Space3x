using System;
using Space3x.InspectorAttributes.Editor.Drawers.NonSerialized;
using UnityEditor;
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
        public object Value { get; }
        public Type ValueType { get; }
        public VisualElement Field { get; set; }
    }
}
