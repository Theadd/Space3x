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
    public class PropertyAttributeController : EditorObjectProvider
    {
        private static Dictionary<int, PropertyAttributeController> s_Instances;
        
        private static int s_ActiveSelectedObjectHash = 0;
        
        public AnnotatedRuntimeType AnnotatedType { get; private set; }
        
        public RuntimeTypeProperties Properties { get; private set; }

        private PropertyAttributeController(SerializedProperty property) : base(property) { }

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
                value = new PropertyAttributeController(prop);
                value.AnnotatedType = AnnotatedRuntimeType.GetInstance(value.DeclaringType);
                value.Properties = new RuntimeTypeProperties(value);
                s_Instances.Add(instanceId, value);
            }
            
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

        public static void OnPropertyValueChanged(IProperty prop)
        {
            if (!prop.HasChildren())
            {
                DebugLog.Error($"<b>IN OnPropertyValueChanged WITH NO CHILDREN</b>");
                return;
            }
            GetInstance(prop.GetTargetObjectInstanceID() * 397 ^ prop.PropertyPath.GetHashCode())?.Rebuild(prop);
        }

        private void Rebuild(IProperty declaringProperty)
        {
            object declaringObject = declaringProperty.GetUnderlyingValue();
            var areEqual = ReferenceEquals(declaringObject, DeclaringObject);
            DebugLog.Info($"<color=#00FF00FF>{(areEqual ? "<b><u>NO</u></b> " : "")}Rebuild: {areEqual} {DeclaringObject?.GetType().Name} {declaringObject?.GetType().Name} {declaringProperty.PropertyPath}</color>");
            if (areEqual) return;
            DeclaringObject = declaringObject;
            AnnotatedType = AnnotatedRuntimeType.GetInstance(DeclaringType);
            Properties = new RuntimeTypeProperties(this);
        }

        public static void RemoveFromCache(PropertyAttributeController controller) => 
            s_Instances.Remove(controller.InstanceID);

        public IProperty GetProperty(string propertyName) => 
            Properties.GetValue(propertyName); // Properties.GetValue(propertyName.GetHashCode() ^ ParentPath.GetHashCode());
        
        public IProperty GetProperty(string propertyName, int arrayIndex)
        {
            IProperty indexer = Properties.GetValue(propertyName);
            IProperty prop = null;
            Type elementType = indexer.GetUnderlyingElementType();
            var isNodeTree = elementType != null && (elementType.IsClass || elementType.IsInterface) && elementType != typeof(string);
            if (indexer is ISerializedPropertyNode serializedIndexer)
            {
                if (isNodeTree)
                    prop = new SerializedPropertyNodeIndexTree()
                    {
                        Indexer = serializedIndexer,
                        Index = arrayIndex
                    };
                else
                    prop = new SerializedPropertyNodeIndex()
                    {
                        Indexer = serializedIndexer,
                        Index = arrayIndex
                    };
            }
            else if (indexer is INonSerializedPropertyNode nonSerializedIndexer)
            {
                if (isNodeTree)
                    prop = new NonSerializedPropertyNodeIndexTree()
                    {
                        Indexer = nonSerializedIndexer,
                        Index = arrayIndex
                    };
                else
                    prop = new NonSerializedPropertyNodeIndex()
                    {
                        Indexer = nonSerializedIndexer,
                        Index = arrayIndex
                    };
            }
            else
                throw new ArgumentException("Unexpected value.");

            return prop;
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
