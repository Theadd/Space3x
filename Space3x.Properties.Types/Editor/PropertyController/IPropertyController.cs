using UnityEditor;

namespace Space3x.Properties.Types.Editor
{
    public interface IPropertyController
    {
        public string ParentPath { get; }
        
        object DeclaringObject { get; }
        
        public bool IsSerialized { get; }

        IUnreliableEventHandler EventHandler { get; }
        
        public SerializedObject SerializedObject { get; }

        public IPropertyNode GetProperty(string propertyName);

        public IPropertyNode GetProperty(string propertyName, int arrayIndex);
    }
}
