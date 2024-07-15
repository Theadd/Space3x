using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public static class PropertyExtensions
    {
        /// <summary>
        /// Determines whether the property is also a container for other properties. For example, an object or struct.
        /// </summary>
        public static bool HasChildren(this IProperty self) => self is INodeTree;

        /// <summary>
        /// Unity structures array paths like "fieldName.Array.data[i]".
        /// Fix that quirk and directly go to index, i.e. "fieldName[i]".
        /// </summary>
        private static string FixedPropertyPath(this IProperty self) => 
            self.PropertyPath.Replace(".Array.data[", "[");
        
        private static string FixedPropertyPath(string propertyPath) => 
            propertyPath.Replace(".Array.data[", "[");
        
        private static string UnfixedPropertyPath(string propertyPath) => 
            propertyPath.Replace("[", ".Array.data[");
        
        private static string[] PropertyPathParts(this IProperty self) => 
            self.FixedPropertyPath().Split('.');
        
        /// <summary>
        /// Determines whether this property is an element of an array or IList. If so, provides the parent path and
        /// the property index within the array as out parameters.
        /// </summary>
        /// <seealso cref="IPropertyNodeIndex"/>
        /// <param name="self">This IProperty.</param>
        /// <param name="parentPath">The parent path as out parameter.</param>
        /// <param name="index">The property index within the array as out parameter.</param>
        [UsedImplicitly]
        public static bool IsPropertyIndexer(this IProperty self, out string parentPath, out int index) => 
            IsPropertyIndexer(self.PropertyPath, out parentPath, out index);

        public static bool IsPropertyIndexer(string propertyPath, out string parentPath, out int index)
        {
            var propertyPart = FixedPropertyPath(propertyPath);
            if (propertyPart[^1] == ']')
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
        /// Gets the <see cref="SerializedObject"/> related to this property, if any.
        /// </summary>
        public static SerializedObject GetSerializedObject(this IProperty self)
        {
            if (self is IPropertyWithSerializedObject property)
                return property.SerializedObject;

            return null;
        }
        
        public static bool HasSerializedProperty(this IProperty self) => 
            self is ISerializedPropertyNode;

        public static SerializedProperty GetSerializedProperty(this IProperty self) =>
            self is ISerializedPropertyNode property
                ? property.SerializedObject.FindProperty(property.PropertyPath)
                : null;

        public static int GetParentObjectHash(this IProperty property)
        {
            var parentPath = property.ParentPath;
            if (string.IsNullOrEmpty(parentPath))
                return property.GetSerializedObject().targetObject.GetInstanceID() * 397;
            else
                return property.GetSerializedObject().targetObject.GetInstanceID() * 397 ^ parentPath.GetHashCode();
        }
        
        public static IProperty GetPropertyNode(this VisualElement element)
        {
            if (element is PropertyField propertyField)
            {
                var prop = propertyField.GetSerializedProperty();
                return prop == null ? null : PropertyAttributeController.GetInstance(prop)?.GetProperty(prop.name);
            }
            if (element is BindablePropertyField bindablePropertyField)
                return bindablePropertyField.Property;

            throw new ArgumentException(
                $"Type {element.GetType().Name} is not valid in {nameof(GetPropertyNode)}.", nameof(element));
        }
        
        public static bool IsArray(this IProperty self) => 
            (self is INodeArray) || (self.HasSerializedProperty() && self.GetSerializedProperty().isArray);

        public static bool IsNonReorderable(this IProperty self) => 
            self is IPropertyFlags property && property.IsNonReorderable;
        
        public static bool IsExpanded(this IProperty self) => 
            self.GetSerializedProperty()?.isExpanded ?? false;

        public static void SetExpanded(this IProperty self, bool expanded)
        {
            if (self.HasSerializedProperty())
                self.GetSerializedProperty().isExpanded = expanded;
        }

        public static string DisplayName(this IProperty self)
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
        /// <param name="self">The <see cref="IProperty"/> to create the invokable for.</param>
        /// <param name="memberName">The name of the member to create the invokable for.</param>
        /// <param name="invokableMember">The created invokable, or null if creation failed.</param>
        /// <returns>True if the invokable was successfully created, false otherwise.</returns>
        public static bool TryCreateInvokable<TIn, TOut>(
            this IProperty self,
            string memberName, 
            out Invokable<TIn, TOut> invokableMember)
        {
            if (self.HasSerializedProperty())
                invokableMember = ReflectionUtility.CreateInvokable<TIn, TOut>(memberName, self.GetSerializedProperty());
            else
                invokableMember = ReflectionUtility.CreateInvokable<TIn, TOut>(memberName, self);
            
            return invokableMember != null;
        }
        
        public static object GetDeclaringObject(this IProperty property) => 
            PropertyAttributeController.GetInstance(property)?.DeclaringObject;
        
        public static Type GetUnderlyingType(this IProperty property) =>
            property.GetVTypeMember()?.FieldType;
        
        public static object GetUnderlyingValue(this IProperty property)
        {
            if (property.HasSerializedProperty())
                return property.GetSerializedProperty().boxedValue;

            object parentValue = null;
            if (property is IPropertyNodeIndex propertyNodeIndex)
            {
                parentValue = propertyNodeIndex.Indexer.GetUnderlyingValue();
                if (parentValue != null)
                    return ((IList)GetFieldValue(parentValue, property.Name))[propertyNodeIndex.Index];
            }
            if (property.TryGetParentProperty(out var parentProperty))
            {
                parentValue = parentProperty.GetUnderlyingValue();
                if (parentValue != null)
                    return GetFieldValue(parentValue, property.Name);
            }

            return null;
        }
        
        /// <summary>
        /// Tries to get the parent property of the provided property.
        /// </summary>
        /// <seealso cref="GetParentProperty"/>
        /// <param name="parentProperty">The parent property as an out parameter, or null if not found.</param>
        /// <returns>True if the parent property was found.</returns>
        public static bool TryGetParentProperty(this IProperty property, out IProperty parentProperty)
        {
            parentProperty = property.GetParentProperty();
            return parentProperty != null;
        }

        /// <summary>
        /// Gets the parent property of the provided property.
        /// </summary>
        /// <seealso cref="TryGetParentProperty"/>
        public static IProperty GetParentProperty(this IProperty property) =>
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
        public static IProperty FindProperty(this IProperty property, string propertyPath)
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
        public static IProperty FindPropertyRelative(this IProperty property, string relativePath) =>
            property.FindProperty(property.PropertyPath.Length == 0 
                ? relativePath 
                : property.PropertyPath + (relativePath.StartsWith('.') 
                    ? relativePath 
                    : "." + relativePath));

        public static string GetParentPath(string propertyPath)
        {
            if (propertyPath[^1] == ']')
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
            this IProperty prop, 
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
                : propertyPath[(parentPath.Length > 0 ? parentPath.Length + 1 : 0)..] ;
            if (string.IsNullOrEmpty(parentPath))
                instanceId = prop.GetTargetObjectInstanceID() * 397;
            else
                instanceId = prop.GetTargetObjectInstanceID() * 397 ^ parentPath.GetHashCode();
        }

        public static int GetTargetObjectInstanceID(this IProperty property) =>
            property.GetSerializedObject().targetObject.GetInstanceID();

        private static object GetFieldValue(object declaringObject, string fieldName) =>
            declaringObject
                .GetType()
                .GetField(
                    fieldName, 
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                ?.GetValue(declaringObject);

        public static FieldInfo GetUnderlyingField(this IProperty property) =>
            property.GetVTypeMember()?.RuntimeField;

        public static Type GetUnderlyingElementType(this IProperty property) =>
            property.GetVTypeMember()?.FieldType.GetArrayOrListElementType();

        private static Type GetArrayOrListElementType(this Type listType) =>
            listType.IsArray 
                ? listType.GetElementType()
                : listType.IsGenericType 
                    ? listType.GetGenericArguments().FirstOrDefault()
                    : null;

        private static VTypeMember GetVTypeMember(this IProperty property) =>
            PropertyAttributeController.GetInstance(property)?.AnnotatedType.GetValue(property.Name);
        
        // public static void SetUnderlyingValue(this SerializedProperty property, object value)
        // {
        //     EnsureReflectable(property);
        //
        //     // Serialize so we don't overwrite other modifications with our deserialization later
        //     property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        //
        //     object parent = property.serializedObject.targetObject;
        //     var parts = PropertyPathParts(property);
        //
        //     for (var i = 0; i < parts.Length - 1; i++)
        //     {
        //         var part = parts[i];
        //
        //         if (parent == null)
        //         {
        //             throw new NullReferenceException($"Parent of '{SerializedObjectLabel(property.serializedObject)}.{string.Join(".", parts, 0, i + 1)}' is null.");
        //         }
        //
        //         parent = GetPropertyPartValue(part, parent);
        //     }
        //
        //     string fieldName;
        //     int index;
        //     IsPropertyIndexer(parts[parts.Length - 1], out fieldName, out index);
        //
        //     var field = GetSerializedFieldInfo(parent.GetType(), fieldName);
        //
        //     field.SetValue(parent, value);
        //
        //     // Deserialize the object for continued operations after this call
        //     property.serializedObject.Update();
        // }
        
        #region PROPERTY BINDING
        public static void BindProperty<TValue>(this BaseField<TValue> field, IProperty property)
        {
            if (property.HasSerializedProperty() && property.GetSerializedProperty() is SerializedProperty serializedProperty) 
                field.BindProperty(serializedProperty);
            else
            {
                field.dataSource = new BindableDataSource<TValue>(property);
                field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
                    bindingMode = BindingMode.TwoWay
                });
            }
        }
        
        public static void BindProperty<TValue>(this BindableElement element, IProperty property, BindingId bindingId)
        {
            if (property.HasSerializedProperty() && property.GetSerializedProperty() is SerializedProperty serializedProperty) 
                element.BindProperty(serializedProperty);
            else
            {
                element.dataSource = new BindableDataSource<TValue>(property);
                element.SetBinding(bindingId, new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
                    bindingMode = BindingMode.TwoWay
                });
            }
        }

        public static void Unbind<TValue>(this BaseField<TValue> field)
        {
            if (field.HasBinding(nameof(BaseField<TValue>.value)))
                field.ClearBinding(nameof(BaseField<TValue>.value));
            BindingExtensions.Unbind((VisualElement)field);
        }
        
        public static void TrackPropertyValue(this VisualElement element, IProperty property, Action<IProperty> callback = null)
        {
            if (property.HasSerializedProperty())
                element.TrackPropertyValue(property.GetSerializedProperty(), callback == null ? null : _ => callback(property));
            else
            {
                if (callback != null && property is INonSerializedPropertyNode bindableProperty)
                {
                    bindableProperty.ValueChanged -= callback;
                    bindableProperty.ValueChanged += callback;
                }
            }
        }

        public static void TrackSerializedObjectValue(this VisualElement element, IProperty property, Action callback = null)
        {
            element.TrackSerializedObjectValue(property.GetSerializedObject(), callback == null ? null : _ => callback.Invoke());
        }
        
        #endregion
    }
}
