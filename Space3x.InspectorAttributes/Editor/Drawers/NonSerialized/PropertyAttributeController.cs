using System;
using System.Reflection;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    public class PropertyAttributeController
    {
        public Type DeclaringType { get; private set; }

        private void Init(FieldInfo fieldInfo)
        {
            DeclaringType = fieldInfo.DeclaringType;
        }
    }
}
