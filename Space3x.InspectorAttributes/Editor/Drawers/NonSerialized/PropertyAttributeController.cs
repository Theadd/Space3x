using System;
using System.Collections.Generic;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    public class PropertyAttributeController : EditorObjectProvider
    {
        private static Dictionary<int, PropertyAttributeController> s_Instances;
        
        public AnnotatedRuntimeType AnnotatedType { get; private set; }
        
        public RuntimeTypeProperties Properties { get; private set; }

        private PropertyAttributeController(IDrawer drawer) : base(drawer) { }

        public static PropertyAttributeController GetInstance(IDrawer drawer)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            // var instanceId = drawer.Property?.serializedObject.targetObject.GetInstanceID() ?? 0;
            var instanceId = drawer.GetParentObjectHash();
            if (instanceId == 0)
                return null;

            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new PropertyAttributeController(drawer);
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(value.DeclaringType);
                value.Properties = new RuntimeTypeProperties(value);
                s_Instances.Add(instanceId, value);
            }

            Debug.LogWarning($"  <b>[PATH_ZERO]: {drawer.Property.propertyPath} --{drawer.Property.name} ({instanceId}):</b> {value.ParentPath}");
            
            return value;
        }

        public static void RemoveFromCache(PropertyAttributeController controller) => 
            s_Instances.Remove(controller.InstanceID);
    }
}
