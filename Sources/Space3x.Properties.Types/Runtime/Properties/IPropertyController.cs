namespace Space3x.Properties.Types
{
    public interface IPropertyController
    {
        public string ParentPath { get; }
        
        UnityEngine.Object TargetObject { get; }
        
        object DeclaringObject { get; }
        
        public bool IsSerialized { get; }

        IUnreliableEventHandler EventHandler { get; }
        
        public object SerializedObject { get; }

        public IPropertyNode GetProperty(string propertyName);

        public IPropertyNode GetProperty(string propertyName, int arrayIndex);

        /// <summary>
        /// Whether this controller is managing properties within a runtime UI Document or not.
        /// </summary>
        public bool IsRuntimeUI { get; }
    }
}
