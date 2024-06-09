using System;
using System.Collections.Generic;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    [InitializeOnLoad]
    public class PropertyAttributeController : EditorObjectProvider
    {
        private static Dictionary<int, PropertyAttributeController> s_Instances;
        
        private static int s_ActiveSelectedObjectHash = 0;
        
        public AnnotatedRuntimeType AnnotatedType { get; private set; }
        
        public RuntimeTypeProperties Properties { get; private set; }

        // private PropertyAttributeController(IDrawer drawer) : base(drawer) { }
        
        private PropertyAttributeController(SerializedProperty property) : base(property) { }

        public static PropertyAttributeController GetInstance(SerializedProperty prop)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            var instanceId = prop.GetParentObjectHash();
            if (instanceId == 0)
                return null;

            SetupActiveSelection();
            Debug.Log($"[PAC]  * Requesting {prop.GetParentPath()} @{prop.GetParentObjectHash()} from PropertyAttributeController's cache using the <u>{prop.name}</u> SerializedProperty...");
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                Debug.Log($"[PAC] <b>!! Creating NEW Controller FOR:</b> {prop.GetParentPath()} @{prop.GetParentObjectHash()}");
                value = new PropertyAttributeController(prop);
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(value.DeclaringType);
                value.Properties = new RuntimeTypeProperties(value);
                s_Instances.Add(instanceId, value);
            }
            // Debug.LogWarning($"  <b>[PATH_ZERO]: {prop.propertyPath} --{prop.name} ({instanceId}):</b> {value.ParentPath}");
            
            return value;
        }
        
        public static PropertyAttributeController GetInstance(IProperty prop)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            var instanceId = prop.GetParentObjectHash();
            if (instanceId == 0)
                return null;

            SetupActiveSelection();
            Debug.Log($"[PAC]  * Requesting {prop.ParentPath} @{prop.GetParentObjectHash()} from PropertyAttributeController's cache using the <u>{prop.Name}</u> IProperty...");
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                throw new ArgumentException(
                    $"{nameof(PropertyAttributeController)} instance {instanceId} not found on {prop.PropertyPath}.");
            }

            return value;
        }

        public static void RemoveFromCache(PropertyAttributeController controller) => 
            s_Instances.Remove(controller.InstanceID);

        public IProperty GetProperty(string propertyName) => 
            Properties.GetValue(propertyName.GetHashCode() ^ ParentPath.GetHashCode());
        
        static PropertyAttributeController() => RegisterCallbacks(true);
        
        private static int GetActiveSelectionHash() =>
            Selection.activeObject != null ? Selection.activeObject.GetHashCode() : 0;

        private static void SetupActiveSelection()
        {
            var hash = GetActiveSelectionHash();
            if (s_ActiveSelectedObjectHash == hash)
                return;

            s_ActiveSelectedObjectHash = hash;
            ClearCache();
        }

        private static void RegisterCallbacks(bool register)
        {
            s_Instances = new Dictionary<int, PropertyAttributeController>();
            Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged() => SetupActiveSelection();

        private static void ClearCache()
        {
            Debug.LogWarning($"[PAC] <color=#FFFF00FF><b>Clearing PropertyAttributeController cache...</b></color>");
            s_Instances.Clear();
        }
    }
}
