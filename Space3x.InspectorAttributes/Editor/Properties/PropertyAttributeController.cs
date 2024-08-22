using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor
{
    [InitializeOnLoad]
    public class PropertyAttributeController : EditorObjectProvider, IPropertyController
    {
        private static Dictionary<int, PropertyAttributeController> s_Instances;
        
        private static int s_ActiveSelectedObjectHash = 0;
        
        public AnnotatedRuntimeType AnnotatedType { get; private set; }
        
        public RuntimeTypeProperties Properties { get; private set; }

        private PropertyAttributeController(SerializedProperty property, int controllerId) : base(property, controllerId) { }
        
        private PropertyAttributeController(IPropertyNode parentPropertyTreeRoot, int controllerId) : base(parentPropertyTreeRoot, controllerId) { }
        
        // TODO: Remove
        internal static PropertyAttributeController[] GetAllInstances() => s_Instances.Values.ToArray();
        // TODO: Remove
        internal static int[] GetAllInstanceKeys() => s_Instances.Keys.ToArray();

        public static PropertyAttributeController GetInstance(SerializedProperty prop)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            var instanceId = prop.GetParentObjectHash();
            if (instanceId == 0)
                return null;

            SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new PropertyAttributeController(prop, instanceId);
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(value.DeclaringType);
                value.Properties = new RuntimeTypeProperties(value);
                s_Instances.Add(instanceId, value);
            }
            else
            {
                // When a component is added or removed in a GameObject, previous SerializedObject instances on
                // that GameObject are no longer valid. We need to re-assign them.
                value.SerializedObject = prop.serializedObject;
            }
            
            return value;
        }
        
        public static PropertyAttributeController GetInstance(IPropertyNode prop)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            if (prop is IPropertyWithSerializedObject { Controller: PropertyAttributeController controller })
                return controller;

            var instanceId = prop.GetParentObjectHash();
            if (instanceId == 0)
                return null;

            SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                throw new ArgumentException(
                    $"{nameof(PropertyAttributeController)} instance {instanceId} not found for property with path: {prop.PropertyPath}.");
            }

            return value;
        }
        
        internal static PropertyAttributeController GetInstance(int instanceId)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            SetupActiveSelection();
            if (instanceId == 0) return null;
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                DebugLog.Error(new ArgumentException(
                    $"{nameof(PropertyAttributeController)} instance {instanceId} not found.").ToString());
                return null;
            }

            return value;
        }

        public bool TryGetInstance(string parentPath, out PropertyAttributeController controller)
        {
            SetupActiveSelection();
            var instanceId = string.IsNullOrEmpty(parentPath) 
                ? InstanceID * 397 
                : InstanceID * 397 ^ parentPath.GetHashCode();

            return s_Instances.TryGetValue(instanceId, out controller);
        }
        
        public static PropertyAttributeController GetOrCreateInstance(IPropertyNode parentPropertyTreeRoot, Type expectedType = null, bool forceCreate = false)
        {
            SetupActiveSelection();
            var parentController = parentPropertyTreeRoot.GetController();
            var instanceId = parentController.InstanceID * 397 ^ parentPropertyTreeRoot.PropertyPath.GetHashCode();
            PropertyAttributeController value = null;

            if (s_Instances.TryGetValue(instanceId, out value) && (forceCreate || (expectedType != null && value.DeclaringType != expectedType)))
            {
                s_Instances.Remove(instanceId);
                value = null;
            }
            
            if (value == null)
            {
                value = new PropertyAttributeController(parentPropertyTreeRoot, instanceId);
                if (expectedType != null && value.DeclaringType != null && value.DeclaringType != expectedType)
                    throw new ArgumentException($"Expected type {expectedType.Name} but found {value.DeclaringType?.Name}");
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(expectedType ?? value.DeclaringType);
                value.Properties = new RuntimeTypeProperties(value);
                s_Instances.Add(instanceId, value);
            }

            return value;
        }

        public static void OnPropertyValueChanged(IPropertyNode prop)
        {
            DebugLog.Info($"<color=#00FF00FF>OnPropertyValueChanged: Tracked change on {prop.GetType().Name}: {prop.PropertyPath}</color>");
            if (!prop.HasChildren())
            {
                DebugLog.Error($"<b>IN OnPropertyValueChanged WITH NO CHILDREN</b>");
                return;
            }

            var controller = prop.GetController() ??
                             GetInstance(prop.GetTargetObjectInstanceID() * 397 ^ prop.PropertyPath.GetHashCode());
            controller?.Rebuild(prop);
            // GetInstance(prop.GetTargetObjectInstanceID() * 397 ^ prop.PropertyPath.GetHashCode())?.Rebuild(prop);
        }

        private void Rebuild(IPropertyNode declaringProperty)
        {
            if (TryGetInstance(declaringProperty.PropertyPath, out var controller))
            {
                object declaringObject = declaringProperty.GetUnderlyingValue();
                var areEqual = ReferenceEquals(declaringObject, controller.DeclaringObject);
                DebugLog.Info($"<color=#00FF00FF>{(areEqual ? "<b><u>NO</u></b> " : "")}Rebuild: {areEqual} " +
                              $"{controller.DeclaringObject?.GetType().Name} {declaringObject?.GetType().Name} " +
                              $"{declaringProperty.PropertyPath}</color>");
                if (areEqual) return;
                controller.DeclaringObject = declaringObject;
                controller.AnnotatedType = AnnotatedRuntimeType.GetInstance(controller.DeclaringType);
                controller.Properties = new RuntimeTypeProperties(controller);
            }
            // object declaringObject = declaringProperty.GetUnderlyingValue();
            // var areEqual = ReferenceEquals(declaringObject, DeclaringObject);
            // DebugLog.Info($"<color=#00FF00FF>{(areEqual ? "<b><u>NO</u></b> " : "")}Rebuild: {areEqual} {DeclaringObject?.GetType().Name} {declaringObject?.GetType().Name} {declaringProperty.PropertyPath}</color>");
            // if (areEqual) return;
            // DebugLog.Error($"<b>// TODO: FIX REBUILD WHEN ASSIGNING A NEW OBJECT INSTANCE TO A ROOT PROPERTY</b>");
            // Code below is reassigning the annotated type and properties of the controller to the type and value
            // of the underlying value of the 'declaringProperty', which in many cases it isn't the one the controller
            // is currently associated with, but an inner property of it.
            // ---
            // DeclaringObject = declaringObject;
            // AnnotatedType = AnnotatedRuntimeType.GetInstance(DeclaringType);
            // Properties = new RuntimeTypeProperties(this);
        }

        public static void RemoveFromCache(PropertyAttributeController controller) => 
            s_Instances.Remove(controller.ControllerID);

        public IPropertyNode GetProperty(string propertyName) => 
            Properties.GetValue(propertyName); // Properties.GetValue(propertyName.GetHashCode() ^ ParentPath.GetHashCode());
        
        public IPropertyNode GetProperty(string propertyName, int arrayIndex)
        {
            var indexer = Properties.GetValue(propertyName);
            if (indexer is IBindablePropertyNode bindable &&
                bindable.TryGetPropertyAtIndex(arrayIndex, out var property))
                return property;

            return null;
            // IPropertyNode prop = null;
            // Type elementType = indexer.GetUnderlyingElementType();
            // var isNodeTree = elementType != null && (elementType.IsClass || elementType.IsInterface) && elementType != typeof(string);
            // if (indexer is ISerializedPropertyNode serializedIndexer)
            // {
            //     if (isNodeTree)
            //         prop = new SerializedPropertyNodeIndexTree()
            //         {
            //             Indexer = serializedIndexer,
            //             Index = arrayIndex
            //         };
            //     else
            //         prop = new SerializedPropertyNodeIndex()
            //         {
            //             Indexer = serializedIndexer,
            //             Index = arrayIndex
            //         };
            // }
            // else if (indexer is INonSerializedPropertyNode nonSerializedIndexer)
            // {
            //     if (isNodeTree)
            //         prop = new NonSerializedPropertyNodeIndexTree()
            //         {
            //             Indexer = nonSerializedIndexer,
            //             Index = arrayIndex
            //         };
            //     else
            //         prop = new NonSerializedPropertyNodeIndex()
            //         {
            //             Indexer = nonSerializedIndexer,
            //             Index = arrayIndex
            //         };
            // }
            // else
            //     throw new ArgumentException("Unexpected value.");
            //
            // return prop;
        }

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
            s_Instances.Clear();
        }
    }
}
