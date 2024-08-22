using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    public interface IPropertyController
    {
        public string ParentPath { get; }
        
        public bool IsSerialized { get; }
        
        public SerializedObject SerializedObject { get; }

        public IPropertyNode GetProperty(string propertyName);

        public IPropertyNode GetProperty(string propertyName, int arrayIndex);
    }
}
