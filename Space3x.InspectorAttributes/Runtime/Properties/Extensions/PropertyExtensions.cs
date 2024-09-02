using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.InspectorAttributes.Extensions;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
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
        /// Determines whether this property is read-only, such as readonly fields or properties with no setter.
        /// </summary>
        public static bool IsReadOnly(this IPropertyNode self) => self is IPropertyFlags property && property.IsReadOnly;

        /// <inheritdoc cref="VTypeFlags.Unreliable"/>
        internal static bool IsUnreliable(this IPropertyNode self) =>
            (self is IPropertyFlags property && property.IsUnreliable); 
            // || (self.IsArrayOrListElement() && !self.GetParentProperty().HasSerializedProperty());

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

        /// <summary>
        /// On a serialized property, checks if its reference to the <see cref="UnityEditor.SerializedObject"/> instance
        /// is still valid. On non-serialized properties it should return true unless you're accessing internal objects
        /// that implement the IPropertyNode interface but are not actual properties.
        /// </summary>
        public static bool IsValid(this IPropertyNode self)
        {
#if UNITY_EDITOR
            if (self is not IControlledProperty property || !self.HasSerializedProperty())
                return true;
            
            try
            {
                return (property.SerializedObject as UnityEditor.SerializedObject)?.targetObject != null;
            }
            catch (NullReferenceException)
            {
                return false;
            }
#else
            return self is IControlledProperty;
#endif
        }
        
        /// <summary>
        /// Gets the <see cref="SerializedObject"/> related to this property, if any.
        /// </summary>
        /// <remarks>
        /// Since it returns a <see cref="UnityEditor.SerializedObject"/> it is not available in player builds, use
        /// it only on editor scripts or surround it with the <c>#if UNITY_EDITOR</c> compiler directive.
        /// </remarks>
#if UNITY_EDITOR
        public static UnityEditor.SerializedObject GetSerializedObject(this IPropertyNode self) => 
            self is IControlledProperty property ? property.SerializedObject as UnityEditor.SerializedObject : null;
#endif

        public static bool HasSerializedProperty(this IPropertyNode self) => 
            self is ISerializedPropertyNode property && (property.Controller?.IsSerialized ?? true);

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
        
#if UNITY_EDITOR
        public static IPropertyNode GetPropertyNode(this VisualElement element)
        {
            if (element is UnityEditor.UIElements.PropertyField propertyField)
            {
                var prop = propertyField.GetSerializedProperty();
                return prop == null ? null : PropertyAttributeController.GetInstance(prop)?.GetProperty(prop.name);
            }
            if (element is BindablePropertyField bindablePropertyField)
                return bindablePropertyField.Property;
            if (element.dataSource is IBindableDataSource bindableDataSource)
                return bindableDataSource.GetPropertyNode();

            return null;
        }
#else
        public static IPropertyNode GetPropertyNode(this VisualElement element)
        {
            if (element is BindablePropertyField bindablePropertyField)
                return bindablePropertyField.Property;
            if (element.dataSource is IBindableDataSource bindableDataSource)
                return bindableDataSource.GetPropertyNode();

            return null;
        }
#endif
        
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
        
        public static bool IsHidden(this IPropertyNode self) => 
            self is IPropertyFlags property && property.IsHidden;
        
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

#if UNITY_EDITOR
        public static string DisplayName(this IPropertyNode self) =>
            self.HasSerializedProperty()
                ? self.GetSerializedProperty().displayName
                : self is IPropertyNodeIndex nodeIndex
                    ? "Element " + nodeIndex.Index
                    : UnityEditor.ObjectNames.NicifyVariableName(self.Name);
#else
        public static string DisplayName(this IPropertyNode self) =>
            self is IPropertyNodeIndex nodeIndex
                    ? "Element " + nodeIndex.Index
                    : self.Name;
#endif
        
        public static string Tooltip(this IPropertyNode property) =>
            property.GetVTypeMember()?.Tooltip ?? string.Empty;

        /// <summary>
        /// Tries to create an invokable of type <see cref="Invokable{TIn, TOut}"/> for the given property.
        /// </summary>
        /// <typeparam name="TIn">The type of the parameter passed in to the callable member when invoking a method.</typeparam>
        /// <typeparam name="TOut">The type of the value returned from the callable member.</typeparam>
        /// <param name="self">The <see cref="IPropertyNode"/> to create the invokable for.</param>
        /// <param name="memberName">The name of the member to create the invokable for.</param>
        /// <param name="invokableMember">The created invokable, or null if creation failed.</param>
        /// <param name="relativePropertyName">
        /// Optionally, if a property name relative to <paramref name="self"/> is specified and the memberName is a
        /// method with a <see cref="IPropertyNode"/> type parameter, it will invoke the method with it.
        /// When not specified (by default) and <paramref name="self"/> isn't equal to the <paramref name="memberName"/>
        /// property, it will use the <paramref name="memberName"/> property instead. 
        /// </param>
        /// <param name="drawer">The decorator or property drawer where this call originates.</param>
        /// <returns>True if the invokable was successfully created, false otherwise.</returns>
        public static bool TryCreateInvokable<TIn, TOut>(
            this IPropertyNode self,
            string memberName, 
            out Invokable<TIn, TOut> invokableMember,
            string relativePropertyName = null,
            IDrawer drawer = null)
        {
            List<object> parameters = new List<object>();
            // If memberName property is a method accepting IPropertyNode as parameter, create that invokable with the relative property as parameter. (outdated)
            if (((PropertyAttributeController)self.GetController()).AnnotatedType.GetValue(memberName) is VTypeMember vType && vType?.RuntimeMethod != null)
            {
                if (vType.RuntimeMethod.GetParameters().Any(p => p.ParameterType == typeof(IPropertyNode)))
                {
                    Queue<IPropertyNode> availableNodes = new Queue<IPropertyNode>();
                    if (!string.IsNullOrEmpty(relativePropertyName))
                        availableNodes.Enqueue(self.GetController().GetProperty(relativePropertyName));
                    availableNodes.Enqueue(self);
                    foreach (var (item, index) in vType.RuntimeMethod.GetParameters().WithIndex())
                    {
                        if (item.ParameterType == typeof(IPropertyNode))
                            parameters.Insert(index, availableNodes.Count > 0 ? availableNodes.Dequeue() : null);
                        else if (item.ParameterType == typeof(IDrawer))
                            parameters.Insert(index, drawer);
                        else
                            parameters.Insert(index, null);
                    }
                }
            }

            var mInfo = ReflectionUtility.GetValidMemberInfo(memberName, self, out object targetObject);
            invokableMember = mInfo != null
                ? new Invokable<TIn, TOut>()
                {
                    CallableMember = mInfo,
                    TargetObject = targetObject,
                    Parameters = parameters.Count > 0 ? parameters.ToArray() : null
                }
                : null;
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
        /// Should work the same way as <see cref="UnityEditor.SerializedObject.FindProperty"/> does, including non-serialized
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
            try
            {
                var relativeProperty = property.FindProperty(property.PropertyPath.Length == 0
                    ? relativePath
                    : property.PropertyPath + (relativePath.StartsWith('.')
                        ? relativePath
                        : "." + relativePath));
                if (relativeProperty != null)
                    return relativeProperty;
            }
            catch (ArgumentException)
            {
                // Ignore
            }
            
            if (((PropertyAttributeController)property.GetController()).TryGetInstance(property.PropertyPath, out var controller))
            {
                return controller.GetProperty(relativePath);
            }
            else
            {
                controller = PropertyAttributeController.GetOrCreateInstance(property);
                if (controller != null)
                {
                    return controller.GetProperty(relativePath);
                }
            }

            return null;
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
                return property.GetTargetObject()?.GetInstanceID() ?? 0;
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
            ((PropertyAttributeController)property.GetController())?.AnnotatedType.GetValue(property.Name);
        
#if UNITY_EDITOR
        /// <summary>
        /// Gets access to the controller instance shared by all properties on the same object. Many other extension
        /// methods on properties make use of it since property instances are barely empty shells.
        /// </summary>
        /// <remarks>
        /// It provides access to internal objects which are not included in the public API documentation, so they are
        /// subject to changes.
        /// </remarks>
        public static IPropertyController GetController(this IPropertyNode property) =>
            property is IControlledProperty { Controller: PropertyAttributeController controller }
                ? controller
#if SPACE3X_DEBUG && RUNTIME_UITOOLKIT_DRAWERS
                : throw new Exception($"{nameof(IPropertyNode)}.{nameof(GetController)} follows an unexpected " +
                                      $"path that would differ from the one in runtime builds.");
#else
                : PropertyAttributeController.GetInstance(property);
#endif
#endif
    }
}
