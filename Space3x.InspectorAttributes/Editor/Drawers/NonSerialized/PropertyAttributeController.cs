using System;
using System.Collections.Generic;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    public interface IWorkingABC
    {
        object DeclaringObject { get; }
        Type DeclaringType { get; }
        object DeclaredMemberInf { get; }
        // object DeclaringObject { get; }
        // object DeclaringObject { get; }
    }
    
    public class PropertyAttributeController : EditorObjectProvider
    {
        private static Dictionary<int, PropertyAttributeController> s_Instances;
        
        public AnnotatedRuntimeType AnnotatedType { get; private set; }

        private PropertyAttributeController(IDrawer drawer) : base(drawer) { }

        public static PropertyAttributeController GetInstance(IDrawer drawer)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            var instanceId = drawer.Property?.serializedObject.targetObject.GetInstanceID() ?? 0;
            if (instanceId == 0)
                return null;

            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new PropertyAttributeController(drawer);
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(value.DeclaringType);
                s_Instances.Add(instanceId, value);
            }

            return value;
        }

        public static void RemoveFromCache(PropertyAttributeController controller) => 
            s_Instances.Remove(controller.InstanceID);
    }
}
