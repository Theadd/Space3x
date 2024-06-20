using System;
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
        public static bool HasChildren(this IProperty self) => self is INodeTree;

        /// <summary>
        /// Unity structures array paths like "fieldName.Array.data[i]".
        /// Fix that quirk and directly go to index, i.e. "fieldName[i]".
        /// </summary>
        private static string FixedPropertyPath(this IProperty self) => 
            self.PropertyPath.Replace(".Array.data[", "[");
        
        private static string FixedPropertyPath(string propertyPath) => 
            propertyPath.Replace(".Array.data[", "[");
        
        private static string[] PropertyPathParts(this IProperty self) => 
            self.FixedPropertyPath().Split('.');
        
        public static bool IsPropertyIndexer(this IProperty self, out string fieldName, out int index)
        {
            return IsPropertyIndexer(self.PropertyPath, out fieldName, out index);
        }
        
        public static bool IsPropertyIndexer(string propertyPath, out string fieldName, out int index)
        {
            var propertyPart = FixedPropertyPath(propertyPath);
            if (propertyPart[^1] == ']')
            {
                var iStart = propertyPart.LastIndexOf("[", propertyPart.Length - 1, 6, StringComparison.Ordinal);
                if (iStart >= 0)
                {
                    var sIndex = propertyPart.Substring(iStart + 1, propertyPart.Length - (iStart + 2));
                    index = int.Parse(sIndex);
                    fieldName = propertyPart[..iStart];
                    return true;
                }
            }
            fieldName = propertyPart;
            index = -1;
            return false;
        }

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
                return property.GetSerializedObject().targetObject.GetInstanceID();
            else
                return property.GetSerializedObject().targetObject.GetInstanceID() ^ parentPath.GetHashCode();
        }
        
        public static IProperty GetPropertyNode(this VisualElement element)
        {
            if (element is PropertyField propertyField)
            {
                var prop = propertyField.GetSerializedProperty();
                return PropertyAttributeController.GetInstance(prop)?.GetProperty(prop.name);
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
    }
}
