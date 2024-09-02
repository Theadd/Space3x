using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Space3x.InspectorAttributes
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class PropertyAttributeController : PropertyControllerBase, IPropertyController
    {
        private static Dictionary<int, PropertyAttributeController> s_Instances;
        
        private static int s_ActiveSelectedObjectHash = 0;
        
        public AnnotatedRuntimeType AnnotatedType { get; private set; }
        
        public RuntimeTypeProperties Properties { get; private set; }
        
        public IUnreliableEventHandler EventHandler { get; set; }

#if UNITY_EDITOR
        private PropertyAttributeController(SerializedProperty property, int controllerId) : base(property, controllerId) { }
#endif
        private PropertyAttributeController(IPropertyNode parentPropertyTreeRoot, int controllerId) : base(parentPropertyTreeRoot, controllerId) { }
        
        /// <summary>
        /// This constructor is for Runtime UI only.
        /// </summary>
        private PropertyAttributeController(UnityEngine.Object target, int controllerId) : base(target, controllerId) { }
        
        // TODO: Remove
        public static PropertyAttributeController[] GetAllInstances() => s_Instances.Values.ToArray();
        // TODO: Remove
        public static int[] GetAllInstanceKeys() => s_Instances.Keys.ToArray();

#if UNITY_EDITOR
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
#endif
        
        /// <summary>
        /// GetInstance overload for Runtime UI on topmost level object. No safe checks implemented since Runtime UI
        /// could also be populated when not in play mode if ExecuteAlways or ExecuteInEditMode are used.
        /// </summary>
        public static PropertyAttributeController GetInstance(UnityEngine.Object target)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            var instanceId = target.GetInstanceID();
            if (instanceId == 0)
                return null;

            // SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new PropertyAttributeController(target, instanceId)
                {
                    IsRuntimeUI = true
                };
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(value.DeclaringType, asUnreliable: true);
                value.Properties = new RuntimeTypeProperties(value);
                value.EventHandler = UnreliableEventHandler.Create((BindablePropertyNode)value.GetProperty(string.Empty), true);
                s_Instances.Add(instanceId, value);
            }

            return value;
        }
        
        public static PropertyAttributeController GetInstance(IPropertyNode prop)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            if (prop is IControlledProperty { Controller: PropertyAttributeController controller })
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
                // DebugLog.Error(new ArgumentException(
                //     $"{nameof(PropertyAttributeController)} instance {instanceId} not found.").ToString());
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
            // TODO: typeof(UnityEngine.Object).IsAssignableFrom(
            var instanceId = parentController.InstanceID * 397 ^ parentPropertyTreeRoot.PropertyPath.GetHashCode();
            PropertyAttributeController value = null;
            
            object underlyingValue = parentPropertyTreeRoot.IsRuntimeUI() 
                ? parentPropertyTreeRoot.GetValueUnsafe()
                : parentPropertyTreeRoot.GetValue();
            
            // TODO: There is a double validation on expectedType and CachedDeclaringObjectHashCode. Redundant check.
            if (s_Instances.TryGetValue(instanceId, out value) && (forceCreate || (expectedType != null && value.DeclaringType != expectedType)))
            {
                if (value.CachedDeclaringObjectHashCode != (underlyingValue?.GetHashCode() ?? 0))
                {
                    Debug.LogWarning($"[PAC!] GetOrCreateInstance('{parentPropertyTreeRoot.PropertyPath}', {expectedType?.Name}, <b>{forceCreate}</b>) " +
                                     $"<b><color=#FF0000FF>REMOVING EXISTING INSTANCE</color> {instanceId}</b>");
                    s_Instances.Remove(instanceId);
                    value = null;
                }
                else
                {
                    Debug.LogWarning($"[PAC!] GetOrCreateInstance('{parentPropertyTreeRoot.PropertyPath}', {expectedType?.Name}, <b>{forceCreate}</b>) " +
                                     $"<b><color=#FF005AFF>NOT REMOVING EXISTING INSTANCE, SAME HASHES!</color> {instanceId}</b>");
                }
            }
            
            if (value == null)
            {
                Debug.LogWarning($"[PAC!] GetOrCreateInstance('{parentPropertyTreeRoot.PropertyPath}', {expectedType?.Name}, <b>{forceCreate}</b>) " +
                                 $"<b><color=#5AFF5AFF>CREATING NEW INSTANCE</color> {instanceId}</b>");
                value = new PropertyAttributeController(parentPropertyTreeRoot, instanceId);
                if (expectedType != null && value.DeclaringType != null && value.DeclaringType != expectedType)
                    throw new ArgumentException($"Expected type {expectedType.Name} but found {value.DeclaringType?.Name}");
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(expectedType ?? value.DeclaringType, asUnreliable: true);
                value.EventHandler = UnreliableEventHandler.Create((BindablePropertyNode)parentPropertyTreeRoot);
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
            Properties.GetValue(propertyName);
        
        public IPropertyNode GetProperty(string propertyName, int arrayIndex)
        {
            var indexer = Properties.GetValue(propertyName);
            if (indexer is IBindablePropertyNode bindable &&
                bindable.TryGetPropertyAtIndex(arrayIndex, out var property))
                return property;

            return null;
        }

        /// <summary>
        /// Returns the next sibling of the specified <see cref="IPropertyNode"/> by its name. 
        /// </summary>
        public IPropertyNode GetNextProperty(string propertyName) => 
            Properties.GetNextValue(propertyName);
        
        public IPropertyNode GetNextVisibleProperty(string propertyName)
        {
            IPropertyNode next = Properties.GetNextValue(propertyName);
            while (next != null && next.IsHidden())
                next = Properties.GetNextValue(next.Name);
            
            return next;
        }

        public ReadOnlyCollection<IPropertyNode> GetAllProperties() => 
            Properties.Values.AsReadOnly();

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInitialize()
        {
            Log(s_Instances?.Count ?? 0, "s_Instances.Count");
        }
        
        static PropertyAttributeController() => RegisterCallbacks(true);
        
#if UNITY_EDITOR
        private static int GetActiveSelectionHash() =>
            UnityEditor.Selection.activeObject != null ? UnityEditor.Selection.activeObject.GetHashCode() : 0;

        private static void RegisterCallbacks(bool register)
        {
            Log(s_Instances?.Count ?? 0, "s_Instances.Count");
            s_Instances = new Dictionary<int, PropertyAttributeController>();
            UnityEditor.Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                UnityEditor.Selection.selectionChanged += OnSelectionChanged;
        }
#else
        private static int GetActiveSelectionHash() => 0;

        private static void RegisterCallbacks(bool register)
        {
            s_Instances = new Dictionary<int, PropertyAttributeController>();
        }
#endif
        
        private static void SetupActiveSelection()
        {
            if (Application.isPlaying) return;
            var hash = GetActiveSelectionHash();
            if (s_ActiveSelectedObjectHash == hash)
                return;

            s_ActiveSelectedObjectHash = hash;
            ClearCache();
        }

        private static void OnSelectionChanged()
        {
            Log();
            SetupActiveSelection();
        }

        internal static void ClearCache()
        {
            Log();
            s_Instances.Clear();
        }

        private static int s_LogCounter = 0;
        
        private static T Log<T>(T value, string message = "", [CallerMemberName] string memberName = "")
        {
            Debug.Log($"<color=#00FF33FF>  {s_LogCounter++} > @PAC.<b>{memberName}</b>: {message}</color> :: <b>{value}</b>");
            return value;
        }
        
        private static void Log(string message = "", [CallerMemberName] string memberName = "") => 
            Debug.Log($"<color=#00FF33FF>  {s_LogCounter++} > @PAC.<b>{memberName}</b>: {message}</color>");
    }
}
