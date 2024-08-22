using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    [ExcludeFromDocs]
    public static class PropertyExtensions
    {
        /// <summary>
        /// Determines whether the property is also a container for other properties. For example, an object or struct.
        /// </summary>
        public static bool HasChildren(this IPropertyNode self) => self is INodeTree;
        
        /// <summary>
        /// Determines whether this is the top property of the property tree.
        /// </summary>
        public static bool IsRootNode(this IPropertyNode self) => self.Name == string.Empty && self is INodeTree;

        /// <summary>
        /// Unity structures array paths like "fieldName.Array.data[i]".
        /// Fix that quirk and directly go to index, i.e. "fieldName[i]".
        /// </summary>
        private static string FixedPropertyPath(this IPropertyNode self) => 
            self.PropertyPath.Replace(".Array.data[", "[");
        
        private static string FixedPropertyPath(string propertyPath) => 
            propertyPath.Replace(".Array.data[", "[");
        
        private static string UnfixedPropertyPath(string propertyPath) => 
            propertyPath.Replace("[", ".Array.data[");
        
        private static string[] PropertyPathParts(this IPropertyNode self) => 
            self.FixedPropertyPath().Split('.');
        
        // /// <summary>
        // /// Determines whether this property is an element of an array or IList. If so, provides the parent path and
        // /// the property index within the array as out parameters.
        // /// </summary>
        // /// <seealso cref="IPropertyNodeIndex"/>
        // /// <param name="self">This IPropertyNode.</param>
        // /// <param name="parentPath">The parent path as out parameter.</param>
        // /// <param name="index">The property index within the array as out parameter.</param>
        // public static bool IsPropertyIndexer(this IPropertyNode self, out string parentPath, out int index) => 
        //     IsPropertyIndexer(self.PropertyPath, out parentPath, out index);

        public static bool IsPropertyIndexer(string propertyPath, out string parentPath, out int index)
        {
            var propertyPart = FixedPropertyPath(propertyPath);
            if (propertyPart.Length > 0 && propertyPart[^1] == ']')
            {
                var iStart = propertyPart.LastIndexOf("[", propertyPart.Length - 1, 6, StringComparison.Ordinal);
                if (iStart >= 0)
                {
                    var sIndex = propertyPart.Substring(iStart + 1, propertyPart.Length - (iStart + 2));
                    index = int.Parse(sIndex);
                    parentPath = UnfixedPropertyPath(propertyPart[..iStart]);
                    return true;
                }
            }
            parentPath = propertyPath;
            index = -1;
            return false;
        }

        /// <summary>
        /// Determines whether this is an invokable property.
        /// </summary>
        public static bool IsInvokable(this IPropertyNode self) => self is IInvokablePropertyNode;

        public static bool IsValid(this IPropertyNode self)
        {
            if (self is not IPropertyWithSerializedObject property) 
                return true;
            
            try
            {
                return property.SerializedObject.targetObject != null;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets the <see cref="SerializedObject"/> related to this property, if any.
        /// </summary>
        public static SerializedObject GetSerializedObject(this IPropertyNode self) => 
            self is IPropertyWithSerializedObject property ? property.SerializedObject : null;

        public static bool HasSerializedProperty(this IPropertyNode self) => 
            self is ISerializedPropertyNode property && (property.Controller?.IsSerialized ?? true);

        public static SerializedProperty GetSerializedProperty(this IPropertyNode self)
        {
            if (self is not ISerializedPropertyNode property)
                return null;

            try
            {
                return property.SerializedObject.FindProperty(property.PropertyPath);
            }
            catch (NullReferenceException)
            {
                // Can happen when switching from Debug mode to Normal mode in the inspector after editing some properties.
                DebugLog.Warning("NullReferenceException when trying to get property: " + property.PropertyPath + Environment.NewLine
                                 + "Can happen when switching from Debug mode to Normal mode in the inspector after editing some properties.");
            }

            return null;
        }

        public static int GetParentObjectHash(this IPropertyNode property)
        {
            var parentPath = property.ParentPath;
            var instanceId = property.GetTargetObjectInstanceID();
            if (instanceId == 0) return 0;
            if (string.IsNullOrEmpty(parentPath))
                return instanceId * 397;
            else
                return instanceId * 397 ^ parentPath.GetHashCode();
        }
        
        public static IPropertyNode GetPropertyNode(this VisualElement element)
        {
            if (element is PropertyField propertyField)
            {
                var prop = propertyField.GetSerializedProperty();
                return prop == null ? null : PropertyAttributeController.GetInstance(prop)?.GetProperty(prop.name);
            }
            if (element is BindablePropertyField bindablePropertyField)
                return bindablePropertyField.Property;
            if (element.dataSource is IBindableDataSource bindableDataSource)
                return bindableDataSource.GetPropertyNode();

            throw new ArgumentException(
                $"Type {element.GetType().Name} is not valid in {nameof(GetPropertyNode)}.", nameof(element));
        }
        
        /// <summary>
        /// Determines whether this property is an array.
        /// </summary>
        public static bool IsArray(this IPropertyNode self) => 
            // EDIT: (self is IPropertyFlags property && property.IsArray) || (self.HasSerializedProperty() && self.GetSerializedProperty().isArray);
            self is IPropertyFlags property && property.IsArray;
        
        /// <summary>
        /// Determines whether this property derives from <see cref="System.Collections.IList">IList</see>.
        /// </summary>
        public static bool IsList(this IPropertyNode self) => 
            self is IPropertyFlags property && property.IsList;
        
        /// <summary>
        /// Determines whether this property is an array or IList.
        /// </summary>
        public static bool IsArrayOrList(this IPropertyNode self) => self.IsArray() || self.IsList();
        
        /// <summary>
        /// Determines whether this property is an element of an array or IList.
        /// </summary>
        public static bool IsArrayOrListElement(this IPropertyNode property) =>
            property is IPropertyNodeIndex;
        
        internal static bool IncludeInInspector(this IPropertyNode self) => 
            self is IPropertyFlags property && property.IncludeInInspector;
        
        internal static bool ShowInInspector(this IPropertyNode self) => 
            self is IPropertyFlags property && property.ShowInInspector;
        
        /// <summary>
        /// On an Array or IList property, determines whether it is non-reorderable.
        /// </summary>
        public static bool IsNonReorderable(this IPropertyNode self) => 
            self is IPropertyFlags property && property.IsNonReorderable;
        
        public static bool IsExpanded(this IPropertyNode self) => 
            self.GetSerializedProperty()?.isExpanded ?? false;

        public static void SetExpanded(this IPropertyNode self, bool expanded)
        {
            if (self.HasSerializedProperty())
                self.GetSerializedProperty().isExpanded = expanded;
        }

        public static string DisplayName(this IPropertyNode self)
        {
            if (self.HasSerializedProperty())
                return self.GetSerializedProperty().displayName;
            else
                return ObjectNames.NicifyVariableName(self.Name);
        }
        
        /// <summary>
        /// Tries to create an invokable of type <see cref="Invokable{TIn, TOut}"/> for the given property.
        /// </summary>
        /// <typeparam name="TIn">The type of the parameter passed in to the callable member when invoking a method.</typeparam>
        /// <typeparam name="TOut">The type of the value returned from the callable member.</typeparam>
        /// <param name="self">The <see cref="IPropertyNode"/> to create the invokable for.</param>
        /// <param name="memberName">The name of the member to create the invokable for.</param>
        /// <param name="invokableMember">The created invokable, or null if creation failed.</param>
        /// <returns>True if the invokable was successfully created, false otherwise.</returns>
        public static bool TryCreateInvokable<TIn, TOut>(
            this IPropertyNode self,
            string memberName, 
            out Invokable<TIn, TOut> invokableMember)
        {
            // if (self.HasSerializedProperty())
            //     invokableMember = ReflectionUtility.CreateInvokable<TIn, TOut>(memberName, self.GetSerializedProperty());
            // else
                invokableMember = ReflectionUtility.CreateInvokable<TIn, TOut>(memberName, self);
            
            return invokableMember != null;
        }
        
        public static object GetDeclaringObject(this IPropertyNode property) => 
            property.GetController()?.DeclaringObject;
        
        public static Type GetUnderlyingType(this IPropertyNode property) =>
            property.GetVTypeMember()?.FieldType;
        
        /// <summary>
        /// Drop-in replacement for <see cref="SerializedProperty.GetArrayElementAtIndex"/>.
        /// </summary>
        public static IPropertyNode GetArrayElementAtIndex(this IPropertyNode property, int propertyIndex)
        {
            if (property is not IBindablePropertyNode bindableProperty)
                return null;

            return bindableProperty.TryGetPropertyAtIndex(propertyIndex, out var propertyNode) ? propertyNode : null;
        }
        
        // public static object GetUnderlyingValue(this IPropertyNode property)
        // {
        //     if (property.IsRootNode())
        //     {
        //         Unity.Properties.IProperty ppp;
        //     }
        //     if (property.HasSerializedProperty())
        //     {
        //         DebugLog.Info($"IN GetUnderlyingValue: {property.Name}; IsValid: {property.IsValid()}; IsArray: {property.IsArray()}");
        //         return property.GetSerializedProperty().boxedValue;
        //     }
        //     
        //     object parentValue = null;
        //     if (property is IPropertyNodeIndex propertyNodeIndex)
        //     {
        //         parentValue = propertyNodeIndex.Indexer.GetUnderlyingValue();
        //         if (parentValue != null)
        //             return ((IList)GetFieldValue(parentValue, property.Name))[propertyNodeIndex.Index];
        //     }
        //     if (property.TryGetParentProperty(out var parentProperty))
        //     {
        //         parentValue = parentProperty.GetUnderlyingValue();
        //         if (parentValue != null)
        //             return GetFieldValue(parentValue, property.Name);
        //     }
        //     DebugLog.Error($"RETURNING NULL FROM GetUnderlyingValue() FOR \"{property.PropertyPath}\".");
        //     return null;
        // }
        
        /// <summary>
        /// Tries to get the parent property of the provided property.
        /// </summary>
        /// <seealso cref="GetParentProperty"/>
        /// <param name="parentProperty">The parent property as an out parameter, or null if not found.</param>
        /// <returns>True if the parent property was found.</returns>
        public static bool TryGetParentProperty(this IPropertyNode property, out IPropertyNode parentProperty)
        {
            parentProperty = property.GetParentProperty();
            return parentProperty != null;
        }

        /// <summary>
        /// Gets the parent property of the provided property.
        /// </summary>
        /// <seealso cref="TryGetParentProperty"/>
        public static IPropertyNode GetParentProperty(this IPropertyNode property) =>
            property is IPropertyNodeIndex propertyNodeIndex
                ? propertyNodeIndex.Indexer
                : property.FindProperty(property.ParentPath);

        /// <summary>
        /// Finds a property using the provided property path and returns it, or null if not found.
        /// Should work the same way as <see cref="SerializedObject.FindProperty"/> does, including non-serialized
        /// properties.
        /// </summary>
        /// <seealso cref="FindPropertyRelative"/>
        /// <param name="propertyPath">Target property path.</param>
        /// <returns>The property, or null if not found.</returns>
        public static IPropertyNode FindProperty(this IPropertyNode property, string propertyPath)
        {
            property.PropertyBreakdownOnPath(
                propertyPath, 
                out var instanceId, 
                out var parentPath, 
                out var propertyIndexerPath, 
                out var propertyIndex, 
                out var propertyName);

            var controller = PropertyAttributeController.GetInstance(instanceId);
            return controller != null
                ? propertyIndex >= 0
                    ? controller.GetProperty(propertyName, propertyIndex)
                    : controller.GetProperty(propertyName)
                : null;
        }
        
        /// <summary>
        /// Finds a property relative to the provided property and returns it, or null if not found.
        /// Should work the same way as <see cref="SerializedProperty.FindPropertyRelative"/> does, including
        /// non-serialized properties.
        /// </summary>
        /// <seealso cref="FindProperty"/>
        /// <param name="relativePath">Property path relative to this property.</param>
        /// <returns>The relative property, or null if not found.</returns>
        public static IPropertyNode FindPropertyRelative(this IPropertyNode property, string relativePath)
        {
            return property.FindProperty(property.PropertyPath.Length == 0
                ? relativePath
                : property.PropertyPath + (relativePath.StartsWith('.')
                    ? relativePath
                    : "." + relativePath));
        }

        public static string GetParentPath(string propertyPath)
        {
            if (propertyPath.Length > 0 && propertyPath[^1] == ']')
            {
                var path = FixedPropertyPath(propertyPath);
                var iStart = path.LastIndexOf("[", path.Length - 1, 6, StringComparison.Ordinal);
                // The null keyword below is used instead of an empty string in order to produce an error on
                // the caller side, meaning that the provided propertyPath string has an invalid format.
                return iStart >= 0 ? UnfixedPropertyPath(path[..iStart]) : null;
            }

            var lastDot = propertyPath.LastIndexOf('.');
            return lastDot < 0 ? "" : propertyPath[..lastDot];
        }

        private static void PropertyBreakdownOnPath(
            this IPropertyNode prop, 
            string propertyPath, 
            out int instanceId, 
            out string parentPath, 
            out string propertyIndexerPath, 
            out int propertyIndex,
            out string propertyName)
        {
            parentPath = GetParentPath(
                IsPropertyIndexer(propertyPath, out propertyIndexerPath, out propertyIndex) 
                    ? propertyIndexerPath 
                    : propertyPath);
            propertyName = propertyIndex >= 0 
                ? propertyIndexerPath[(parentPath.Length > 0 ? parentPath.Length + 1 : 0)..] 
                : propertyPath[(parentPath.Length > 0 ? parentPath.Length + 1 : 0)..];
            if (string.IsNullOrEmpty(parentPath))
                instanceId = prop.GetTargetObjectInstanceID() * 397;
            else
                instanceId = prop.GetTargetObjectInstanceID() * 397 ^ parentPath.GetHashCode();
        }

        internal static int GetTargetObjectInstanceID(this IPropertyNode property)
        {
            try
            {
                return property.GetSerializedObject()?.targetObject?.GetInstanceID() ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static object GetFieldValue(object declaringObject, string fieldName) =>
            declaringObject
                .GetType()
                .GetField(
                    fieldName, 
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                ?.GetValue(declaringObject);

        public static FieldInfo GetUnderlyingField(this IPropertyNode property) =>
            property.GetVTypeMember()?.RuntimeField;

        public static Type GetUnderlyingElementType(this IPropertyNode property) =>
            property.GetVTypeMember()?.FieldType.GetArrayOrListElementType();

        private static Type GetArrayOrListElementType(this Type listType) =>
            listType.IsArray 
                ? listType.GetElementType()
                : listType.IsGenericType 
                    ? listType.GetGenericArguments().FirstOrDefault()
                    : null;

        private static VTypeMember GetVTypeMember(this IPropertyNode property) =>
            property.GetController()?.AnnotatedType.GetValue(property.Name);
        
        internal static PropertyAttributeController GetController(this IPropertyNode property) => 
            property is IPropertyWithSerializedObject { Controller: PropertyAttributeController controller } 
                ? controller
                : PropertyAttributeController.GetInstance(property);
    }
}
