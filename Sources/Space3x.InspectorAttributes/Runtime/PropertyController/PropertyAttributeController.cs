using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    public class PropertyAttributeController : PropertyControllerBase, IPropertyController
    {
        /// <summary>
        /// Using separate controllers for Runtime UI results in providing the proper PropertyDrawer or DecoratorDrawer
        /// to use, specially when the same decorated object instance is being rendered in a runtime panel and in an
        /// editor panel at the same time or since last domain reload. With the downside that they'd be controlled
        /// separately, in other words, without synchronizing their values.
        /// </summary>
        public static bool UseSeparateControllersForRuntimeUI = true;
        
        private static Dictionary<int, PropertyAttributeController> s_Instances;
        
        private static int s_ActiveSelectedObjectHash = 0;
        
        public AnnotatedRuntimeType AnnotatedType { get; private set; }
        
        public RuntimeTypeProperties Properties { get; private set; }
        
        public IUnreliableEventHandler EventHandler { get; set; }

#if UNITY_EDITOR
        private PropertyAttributeController(UnityEditor.SerializedProperty property, int controllerId) : base(property, controllerId) { }
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
        
        public static PropertyAttributeController GetInstance(UnityEditor.SerializedProperty prop)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            var instanceId = GetParentObjectHash(prop);
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
        
        private static int GetParentObjectHash(UnityEditor.SerializedProperty prop)
        {
            var parentPath = prop.GetParentPath();
            if (string.IsNullOrEmpty(parentPath))
                return prop.serializedObject.targetObject.GetInstanceID() * 397;
            else
                return prop.serializedObject.targetObject.GetInstanceID() * 397 ^ parentPath.GetHashCode();
        }
#endif
        
        /// <summary>
        /// GetInstance overload for Runtime UI on topmost level object. No safe checks implemented since Runtime UI
        /// could also be populated when not in play mode if ExecuteAlways or ExecuteInEditMode are used.
        /// </summary>
        public static PropertyAttributeController GetInstance(UnityEngine.Object target, IPropertyNode propertyNode = null)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            var isRuntimeUI = propertyNode?.IsRuntimeUI() ?? true;
            var instanceId = target.GetInstanceID() * 397 * (isRuntimeUI && UseSeparateControllersForRuntimeUI ? 673 : 1);
            if (instanceId == 0)
                return null;

            // SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new PropertyAttributeController(target, instanceId)
                {
                    IsRuntimeUI = isRuntimeUI
                };
                // Only those controllers (when not in play mode / runtime) for an UnityEngine.Object property which its
                // IPropertyNode is already known and not flagged as unreliable, are not flagged as unreliable.
                // In other words, only those UnityEngine.Object in a serialized declaring object are not unreliable. 
                bool isUnreliable = Application.isPlaying || (propertyNode?.IsUnreliable() ?? true);
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(value.DeclaringType, asUnreliable: isUnreliable);
                value.Properties = new RuntimeTypeProperties(value);
                if (isUnreliable)
                    value.EventHandler = UnreliableEventHandler.Create((BindablePropertyNode)value.GetProperty(string.Empty), true);
                s_Instances.Add(instanceId, value);
            }

            return value;
        }
        
        public static PropertyAttributeController GetInstance(int instanceId)
        {
            if (s_Instances == null)
                s_Instances = new Dictionary<int, PropertyAttributeController>();

            SetupActiveSelection();
            if (instanceId == 0) return null;
            if (!s_Instances.TryGetValue(instanceId, out var value))
                return null;

            return value;
        }
        
        public bool TryGetInstance(string parentPath, out PropertyAttributeController controller)
        {
            SetupActiveSelection();
            var instanceId = (string.IsNullOrEmpty(parentPath) 
                ? InstanceID * 397 
                : InstanceID * 397 ^ parentPath.GetHashCode()) 
                             * (this.IsRuntimeUI && UseSeparateControllersForRuntimeUI ? 673 : 1);

            return s_Instances.TryGetValue(instanceId, out controller);
        }
        
        public static PropertyAttributeController GetOrCreateInstance(IPropertyNode parentPropertyTreeRoot, Type expectedType = null, bool forceCreate = false)
        {
            Log();
            if (typeof(UnityEngine.Object).IsAssignableFrom(parentPropertyTreeRoot.GetUnderlyingType()))
                return GetInstance((UnityEngine.Object)parentPropertyTreeRoot.GetValue(), parentPropertyTreeRoot);
            
            SetupActiveSelection();
            var parentController = parentPropertyTreeRoot.GetController();
            var instanceId =
                (((PropertyAttributeController)parentController).InstanceID * 397 ^
                 parentPropertyTreeRoot.PropertyPath.GetHashCode()) *
                (parentController.IsRuntimeUI && UseSeparateControllersForRuntimeUI ? 673 : 1);
            PropertyAttributeController value = null;
            
            object underlyingValue = parentController.IsRuntimeUI // parentPropertyTreeRoot.IsRuntimeUI() 
                ? parentPropertyTreeRoot.GetValueUnsafe()
                : parentPropertyTreeRoot.GetValue();
            
            // TODO: There is a double validation on expectedType and CachedDeclaringObjectHashCode. Redundant check.
            if (s_Instances.TryGetValue(instanceId, out value) && (forceCreate || (expectedType != null && value.DeclaringType != expectedType)))
            {
                if (value.CachedDeclaringObjectHashCode != (underlyingValue?.GetHashCode() ?? 0))
                {
                    s_Instances.Remove(instanceId);
                    value = null;
                }
            }
            
            if (value == null)
            {
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
        
        public static PropertyAttributeController GetOrCreateOverloadedInstance(IPropertyNode parentPropertyTreeRoot, Type originalType, Type overloadedType, Func<object> overloadedDeclaringObjectFactory)
        {
            var parentController = parentPropertyTreeRoot.GetController();
            var instanceId = ((((PropertyAttributeController)parentController).InstanceID
                                     * 397 ^ parentPropertyTreeRoot.PropertyPath.GetHashCode())
                                 * 397 ^ overloadedType.GetHashCode()) *
                             (parentController.IsRuntimeUI && UseSeparateControllersForRuntimeUI ? 673 : 1);
            PropertyAttributeController value = null;

            if (s_Instances.TryGetValue(instanceId, out value))
                if (value.DeclaringType != overloadedType)
                    throw new InvalidOperationException();
            
            if (value == null)
            {
                value = new PropertyAttributeController(parentPropertyTreeRoot, instanceId);
                value.DeclaringObject = overloadedDeclaringObjectFactory.Invoke();
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(overloadedType, asUnreliable: true);
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
#if SPACE3X_DEBUG
            var controller = prop.GetController();
            if (controller == null) 
                throw new NullReferenceException($"{prop.GetType().Name} with no valid controller at path: {prop.PropertyPath}");
#else
            // Obsolete version, the null-coalescing operator shouldn't be required anymore,
            // kept for backwards-compatibility, should be removed soon.
            var controller = prop.GetController() ??
                             GetInstance((prop.GetTargetObject()?.GetInstanceID() ?? 0) * 397 ^ prop.PropertyPath.GetHashCode());
#endif
            ((PropertyAttributeController)controller)?.Rebuild(prop);
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
                controller.AnnotatedType = AnnotatedRuntimeType.GetInstance(controller.DeclaringType, asUnreliable: declaringProperty.IsUnreliable());
                controller.Properties = new RuntimeTypeProperties(controller);
            }
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

        internal static void ReloadAll() => RegisterCallbacks(true);

#if UNITY_EDITOR
        private static int GetActiveSelectionHash() =>
            UnityEditor.Selection.activeObject != null ? UnityEditor.Selection.activeObject.GetHashCode() : 0;

        private static void RegisterCallbacks(bool register)
        {
            Log(s_Instances?.Count ?? 0, "s_Instances.Count");
            s_Instances = new Dictionary<int, PropertyAttributeController>();
            if (Application.isPlaying) return;
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
        
        private static T Log<T>(T value, string message, [CallerMemberName] string memberName = "")
        {
            Debug.LogWarning($"<color=#00FF33FF>  {s_LogCounter++} > @PAC.<b>{memberName}</b>: {message}</color> :: <b>{value}</b>");
            return value;
        }
        
        private static void Log(string message = "", [CallerMemberName] string memberName = "")
        {
            // Debug.LogWarning($"<color=#00FF33FF>  {s_LogCounter++} > @PAC.<b>{memberName}</b>: {message}</color>");
        }
    }
}
