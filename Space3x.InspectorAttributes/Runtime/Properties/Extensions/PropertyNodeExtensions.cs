using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public static class PropertyNodeExtensions
    {
        private struct Node
        {
            public string Name;
            public string ParentPath;
            public string PropertyPath;
            public int PropertyIndex;
            
            public Node(string name, string parentPath, string propertyPath, int propertyIndex = -1)
            {
                Name = name;
                ParentPath = parentPath;
                PropertyPath = propertyPath;
                PropertyIndex = propertyIndex;
            }

            public static Node Create(string propertyPath)
            {
                var parentPath = PropertyExtensions.GetParentPath(
                    PropertyExtensions.IsPropertyIndexer(propertyPath, out var propertyIndexerPath, out var propertyIndex) 
                        ? propertyIndexerPath 
                        : propertyPath);
                var propertyName = propertyIndex >= 0 
                    ? propertyIndexerPath[(parentPath.Length > 0 ? parentPath.Length + 1 : 0)..] 
                    : propertyPath[(parentPath.Length > 0 ? parentPath.Length + 1 : 0)..];
                
                return new Node(propertyName, parentPath, propertyPath, propertyIndex);
            }

            public override string ToString() => $"'<b>{Name}</b>' @ '{ParentPath}' ({PropertyPath}){(PropertyIndex >= 0 ? $" #{PropertyIndex}" : "")}";
        }
        
        public static IEnumerable<IPropertyNode> GetAllParentProperties(this IPropertyNode self, bool skipNodeIndexes = true)
        {
            var controller = self.GetController();
            Stack<Node> nodeStack = new Stack<Node>();
            Stack<IPropertyNode> propertyStack = new Stack<IPropertyNode>();
            
            foreach (var node in GetAllParentNodes(self.PropertyPath))
            {
                nodeStack.Push(node);
                if (((PropertyAttributeController)controller).TryGetInstance(node.ParentPath, out var parentController))
                {
                    controller = parentController;
                    while (nodeStack.Count > 0)
                    {
                        var stackedNode = nodeStack.Pop();
                        var property = parentController.GetProperty(stackedNode.Name);
                        propertyStack.Push(property);
                        if (!skipNodeIndexes && stackedNode.PropertyIndex >= 0)
                        {
                            if (property is not IBindablePropertyNode bindablePropertyNode)
                                throw new ArgumentException($"Unexpected property type for a property indexer.");

                            if (bindablePropertyNode.TryGetPropertyAtIndex(stackedNode.PropertyIndex,
                                    out var indexedProperty))
                            {
                                propertyStack.Push(indexedProperty);
                            }
                        }
                        
                        parentController = (PropertyAttributeController)property.GetController();
                    }
                    while (propertyStack.Count > 0)
                    {
                        yield return propertyStack.Pop();
                    }
                }
            }
            
            yield break;
        }

        public static object GetUnderlyingValue(this IPropertyNode property)
        {
            object value = null;
            var allParents = property.GetAllParentProperties(skipNodeIndexes: false);
            var enumerator = allParents.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                // value = property.GetTargetObject();
                // if (value != null && !string.IsNullOrEmpty(property.Name))
                //     value = GetFieldValueOrThrow(value, property.Name);
                value = property.GetValueUnsafe();
            }
            else
                value = GetUnderlyingValueRecursively(enumerator, true);
            enumerator.Dispose();
            return value;
        }
        
        /// <summary>
        /// Returns the supposed value of this property using reflection on the declaring object in which the
        /// property was created from. Unsafe, please use <see cref="GetValue"/> instead.
        /// </summary>
        /// <remarks>
        /// It should only be used while building the UI layout for the first time since it
        /// relies on a reference to the initial declaring object containing that property and if that object or any
        /// other parent of it gets overwritten (on non-serialized & unreliable-flagged properties), properties
        /// referencing that object will be invalid along the propagation/reconstruction cycle.
        /// </remarks>
        public static object GetValueUnsafe(this IPropertyNode propertyNode)
        {
            if (propertyNode is IPropertyNodeIndex collectionElementProperty)
                return (GetFieldValueOrThrow(collectionElementProperty.Indexer.GetDeclaringObject(),
                    collectionElementProperty.Indexer.Name) as IList)?[collectionElementProperty.Index];

            return string.IsNullOrEmpty(propertyNode.Name)
                ? propertyNode.GetDeclaringObject()
                : GetFieldValueOrThrow(propertyNode.GetDeclaringObject(), propertyNode.Name);
        }

        public static void SetUnderlyingValue(this IPropertyNode property, object value)
        {
            if (property.HasSerializedProperty() && property is ISerializedPropertyNode serializedPropertyNode &&
                serializedPropertyNode.IsValid())
                serializedPropertyNode.GetSerializedProperty().boxedValue = value;
            else
            {
                var allParents = property.GetAllParentProperties(skipNodeIndexes: false);
                var firstParent = allParents.Skip(1).FirstOrDefault();
                object parentValue = firstParent?.GetUnderlyingValue();
                if (parentValue != null)
                {
                    if (property is IPropertyNodeIndex propertyNodeIndex)
                        ((IList)parentValue)[propertyNodeIndex.Index] = value;
                    else
                        SetFieldValue(parentValue, property.Name, value);
                }
                else
                    SetFieldValue(property.GetDeclaringObject(), property.Name, value);
            }
        }
        
        private static object GetUnderlyingValueRecursively(IEnumerator<IPropertyNode> enumerator, bool skipMoveNext = false)
        {
            var previousNode = enumerator.Current;  // it's in an invalid state when skipMoveNext is true but ignored since it's not used in such case.
            if (!skipMoveNext && !enumerator.MoveNext())
                return (object)previousNode?.GetDeclaringObject();  // .GetTargetObject();
            IPropertyNode currentNode = enumerator.Current;
            if (currentNode.HasSerializedProperty() && currentNode is ISerializedPropertyNode serializedPropertyNode &&
                serializedPropertyNode.IsValid() && !currentNode.IsArrayOrList())   // an array cannot be read with boxedValue.
                return serializedPropertyNode.GetSerializedProperty().boxedValue;
            if (currentNode is IControlledProperty controlledNode && controlledNode.Controller.IsRuntimeUI &&
                (controlledNode.Controller.EventHandler?.IsTopLevelRuntimeEventHandler ?? true))
                return string.IsNullOrEmpty(currentNode.Name) 
                    ? controlledNode.Controller.DeclaringObject 
                    : GetFieldValueOrThrow(controlledNode.Controller.DeclaringObject, currentNode.Name);
            
            object parentValue = GetUnderlyingValueRecursively(enumerator);
            if (parentValue != null)
            {
                if (currentNode is IPropertyNodeIndex propertyNodeIndex)
                    return ((IList)parentValue)[propertyNodeIndex.Index];
                return GetFieldValue(parentValue, currentNode!.Name);
            }

            return null;
        }

        private static IEnumerable<Node> GetAllParentNodes(string propertyPath)
        {
            var parentPath = propertyPath;
            while (!string.IsNullOrEmpty(parentPath))
            {
                var node = Node.Create(parentPath);
                parentPath = node.ParentPath;
                yield return node;
            }
            yield break;
        }
        
        private static object GetFieldValue(object declaringObject, string fieldName) =>
            declaringObject
                .GetType()
                .GetField(
                    fieldName, 
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                ?.GetValue(declaringObject);
        
        internal static object GetFieldValueOrThrow(object declaringObject, string fieldName)
        {
            var fieldInfo = declaringObject
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return fieldInfo != null
                ? fieldInfo.GetValue(declaringObject) 
                : throw new System.InvalidOperationException($"{fieldName} is not a valid member name in {declaringObject?.GetType()}.");
        }

        private static void SetFieldValue(object declaringObject, string fieldName, object value) =>
            declaringObject
                .GetType()
                .GetField(
                    fieldName, 
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                ?.SetValue(declaringObject, value);

        public static bool TryGetPropertyAtIndex(this IBindablePropertyNode indexer, int propertyIndex,
            out IPropertyNode property)
        {
            Type elementType = indexer.GetUnderlyingElementType();
            var isNodeTree = elementType != null && (elementType.IsClass || elementType.IsInterface) && elementType != typeof(string);
            if (indexer.HasSerializedProperty() && indexer is ISerializedPropertyNode serializedIndexer)
            {
                if (isNodeTree)
                    property = new SerializedPropertyNodeIndexTree()
                    {
                        Indexer = serializedIndexer,
                        Index = propertyIndex
                    };
                else
                    property = new SerializedPropertyNodeIndex()
                    {
                        Indexer = serializedIndexer,
                        Index = propertyIndex
                    };
            }
            else if (indexer is INonSerializedPropertyNode nonSerializedIndexer)
            {
                if (isNodeTree)
                    property = new NonSerializedPropertyNodeIndexTree()
                    {
                        Indexer = nonSerializedIndexer,
                        Index = propertyIndex
                    };
                else
                    property = new NonSerializedPropertyNodeIndex()
                    {
                        Indexer = nonSerializedIndexer,
                        Index = propertyIndex
                    };
            }
            else
            {
                Debug.LogException(
                    new ArgumentException($"Unexpected value for the property indexer at {nameof(TryGetPropertyAtIndex)}" +
                                          $", having: {indexer?.GetType().Name}"));
                property = null;
                return false;
            }
            return true;
        }

        private static bool TrySetValue(this IPropertyNode property, object value)
        {
            if (property is IBindablePropertyNode { DataSource: IBindableDataSource bindableDataSource })
            {
                bindableDataSource.BoxedValue = value;
                return true;
            }
            return false;
        }
        
        private static bool TryGetValue(this IPropertyNode property, out object value)
        {
            if (property is IBindablePropertyNode { DataSource: IBindableDataSource bindableDataSource })
            {
                value = bindableDataSource.BoxedValue;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Gets the value associated with this property.
        /// </summary>
        public static object GetValue(this IPropertyNode property)
        {
            if (!property.TryGetValue(out var value))
                value = property.GetUnderlyingValue();
            return value;
        }

        /// <summary>
        /// Sets a new value for this property, automatically handling undo/redo support on serialized properties
        /// and value changed event propagation for both serialized and non-serialized properties. 
        /// </summary>
        public static void SetValue(this IPropertyNode property, object value)
        {
#if UNITY_EDITOR
            if (property.HasSerializedProperty())
            {
                // Get ready to save modified value on serialized object
                if (property.GetSerializedObject().hasModifiedProperties)
                    property.GetSerializedObject().ApplyModifiedPropertiesWithoutUndo();
                property.GetSerializedObject().Update();
            }
#endif
            
            // Modify property's value
            if (!property.TrySetValue(value))
                property.SetUnderlyingValue(value);
            
#if UNITY_EDITOR
            if (property.HasSerializedProperty())
            {
                // Save modified value on serialized object
                if (property.GetSerializedObject().hasModifiedProperties)
                    property.GetSerializedObject().ApplyModifiedProperties();
                property.GetSerializedObject().Update();
            }
#endif
        }
    }
}
