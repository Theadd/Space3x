using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    [Flags]
    public enum VTypeFlags
    {
        None = 0,
        HideInInspector = 1,
        ShowInInspector = 2,
        Serializable = 4,
    }
    
    public class VTypeMember
    {
        public string Name;
        public FieldInfo RuntimeField;
        public PropertyInfo RuntimeProperty;
        public MethodInfo PropertyGetter;
        public MethodInfo PropertySetter;
        public Type FieldType;
        public List<CustomAttributeData> CustomAttributes;
        public List<PropertyAttribute> PropertyAttributes;
        public VTypeFlags Flags;
        
        private List<Type> m_DecoratorDrawers;
        private Type m_PropertyDrawer;

        public List<Type> DecoratorDrawers => m_DecoratorDrawers ?? (Rebuild() ? m_DecoratorDrawers : null);
        public Type PropertyDrawer => m_DecoratorDrawers == null ? (Rebuild() ? m_PropertyDrawer : null) : m_PropertyDrawer;

        public bool IsHidden => (Flags | VTypeFlags.HideInInspector) == Flags ||
                                !((Flags | VTypeFlags.Serializable) == Flags ||
                                  (Flags | VTypeFlags.ShowInInspector) == Flags);
        public bool IsSerializable => (Flags | VTypeFlags.Serializable) == Flags;

        private bool Rebuild()
        {
            m_DecoratorDrawers = new List<Type>(PropertyAttributes.Count);
            for (var i = 0; i < PropertyAttributes.Count; i++)
            {
                var drawer = CachedDrawers.GetCustomDrawer(PropertyAttributes[i].GetType());
                if (drawer == null)
                {
                    Debug.LogWarning($"CachedDrawers.GetCustomDrawer({PropertyAttributes[i].GetType().Name}) is null.");
                    m_DecoratorDrawers.Add(null);
                    continue;
                }
                if (typeof(DecoratorDrawer).IsAssignableFrom(drawer) && (
                        !(RuntimeField != null)
                        || !IsArrayOrList(FieldType)
                        /* || propertyType.IsArrayOrList() */))
                {
                    m_DecoratorDrawers.Add(drawer);
                }
                else if (typeof(PropertyDrawer).IsAssignableFrom(drawer))
                {
                    m_DecoratorDrawers.Add(null);
                    if (IsArrayOrList(FieldType) && !PropertyAttributes[i].applyToCollection)   // TODO: Not really accurate.
                        continue;
                    m_PropertyDrawer = drawer;
                }
            }

            m_PropertyDrawer ??= CachedDrawers.GetCustomDrawer(FieldType);

            return true;
        }
        
        private static bool IsArrayOrList(Type listType) => 
            listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof (List<>);
    }

    public class AnnotatedRuntimeType
    {
        public List<string> Keys;
        public List<VTypeMember> Values;
        public List<CustomAttributeData> CustomAttributes;

        private static Dictionary<Type, AnnotatedRuntimeType> s_Instances;
        private static Comparer<PropertyAttribute> s_Comparer;

        public static AnnotatedRuntimeType GetInstance(Type declaringType)
        {
            if (s_Instances == null)
            {
                s_Instances = new Dictionary<Type, AnnotatedRuntimeType>();
                s_Comparer = Comparer<PropertyAttribute>.Create((p1, p2) => 
                    p1.order.CompareTo(p2.order));
            }
            if (!s_Instances.TryGetValue(declaringType, out var value))
            {
                value = new AnnotatedRuntimeType();
                value.Bind(declaringType);
                s_Instances.Add(declaringType, value);
            }

            return value;
        }
        
        private void Bind(Type declaringType)
        {
            Keys = new List<string>();
            Values = new List<VTypeMember>();
            CustomAttributes = new List<CustomAttributeData>();
            var allProperties = new Dictionary<string, PropertyInfo>();
            var allSpecialMethods = new Dictionary<string, MethodInfo>();
            var allMembers = declaringType.GetMembers(
                BindingFlags.Instance | BindingFlags.Static | 
                BindingFlags.Public | BindingFlags.NonPublic);

            for (var i = 0; i < allMembers.Length; i++)
            {
                var memberInfo = allMembers[i];
                var customAttributes = memberInfo.CustomAttributes?
                    .Where(data => typeof(PropertyAttribute).IsAssignableFrom(data.AttributeType))
                    .ToList();

                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Method:
                        if (memberInfo is MethodInfo methodInfo)
                        {
                            if (methodInfo.IsSpecialName)
                                allSpecialMethods.Add(methodInfo.Name, methodInfo);
                        }
                        if (customAttributes.Count > 0)
                            Debug.LogWarning("// TODO: A valid PropertyAttribute is assigned to an unhandled MemberInfo.");
                        continue;
                    
                    case MemberTypes.Field:
                        if (memberInfo is FieldInfo fieldInfo)
                        {
                            if (fieldInfo.Name.EndsWith("k__BackingField"))
                            {
                                if (allProperties.TryGetValue(fieldInfo.Name, out var propInfo))
                                {
                                    if (Keys.Contains(propInfo.Name))
                                    {
                                        Debug.LogError("Unexpected duplicated property found: " + propInfo.Name);
                                        continue;
                                    }
                                    Values.Add(new VTypeMember()
                                    {
                                        FieldType = fieldInfo.FieldType,
                                        Name = propInfo.Name,
                                        RuntimeProperty = propInfo,
                                        RuntimeField = fieldInfo,
                                        PropertyGetter =
                                            allSpecialMethods.GetValueOrDefault("get_" + propInfo.Name, null),
                                        PropertySetter =
                                            allSpecialMethods.GetValueOrDefault("set_" + propInfo.Name, null),
                                        CustomAttributes = customAttributes,
                                        PropertyAttributes = GetSortedCustomPropertyAttributes(fieldInfo),
                                        Flags = (HasHideInInspectorAttribute(fieldInfo) ? VTypeFlags.HideInInspector : VTypeFlags.None)
                                                | (HasShowInInspectorAttribute(fieldInfo) ? VTypeFlags.ShowInInspector : VTypeFlags.None)
                                                | (IsSerializableField(fieldInfo) ? VTypeFlags.Serializable : VTypeFlags.None)
                                    });
                                    Keys.Add(propInfo.Name);
                                }
                            }
                            else
                            {
                                if (Keys.Contains(fieldInfo.Name))
                                {
                                    Debug.LogError("Unexpected duplicated field found: " + fieldInfo.Name);
                                    continue;
                                }
                                Values.Add(new VTypeMember()
                                {
                                    FieldType = fieldInfo.FieldType,
                                    Name = fieldInfo.Name,
                                    CustomAttributes = customAttributes,
                                    PropertyAttributes = GetSortedCustomPropertyAttributes(fieldInfo),
                                    Flags = (HasHideInInspectorAttribute(fieldInfo) ? VTypeFlags.HideInInspector : VTypeFlags.None)
                                            | (HasShowInInspectorAttribute(fieldInfo) ? VTypeFlags.ShowInInspector : VTypeFlags.None)
                                            | (IsSerializableField(fieldInfo) ? VTypeFlags.Serializable : VTypeFlags.None)
                                });
                                Keys.Add(fieldInfo.Name);
                            }
                        }
                        break;
                    
                    case MemberTypes.Property:
                        if (memberInfo is PropertyInfo propertyInfo)
                            allProperties.Add($"<{propertyInfo.Name}>k__BackingField", propertyInfo);
                        if (customAttributes.Count > 0)
                            Debug.LogWarning("// TODO: A valid PropertyAttribute is assigned to an unhandled MemberInfo.");
                        continue;
                    
                    default:
                        Debug.LogWarning($"<b>[NO HANDLER IMPLEMENTED FOR THIS MEMBER TYPE, YET]</b> {memberInfo.MemberType.ToString()}, {memberInfo}");
                        if (customAttributes.Count > 0)
                            Debug.LogWarning("// TODO: A valid PropertyAttribute is assigned to an unhandled MemberInfo.");
                        continue;
                }
                if (customAttributes.Count > 0)
                    CustomAttributes = CustomAttributes.Concat(customAttributes).ToList();
            }
        }
        
        private static bool IsSerializableField(FieldInfo fieldInfo) => 
            fieldInfo.IsPublic || (!fieldInfo.IsNotSerialized && fieldInfo.IsDefined(typeof(SerializeField), false));

        private static bool HasHideInInspectorAttribute(FieldInfo fieldInfo) => 
            fieldInfo.IsDefined(typeof(HideInInspector), false);

        private static bool HasShowInInspectorAttribute(FieldInfo fieldInfo) =>
            fieldInfo.IsDefined(typeof(ShowInInspectorAttribute), false);

        // private static List<PropertyAttribute> GetSortedCustomPropertyAttributes(MemberInfo field)
        // {
        //     var list = new List<PropertyAttribute>();
        //     if (field != null)
        //     {
        //         // var customAttributes = memberInfo.CustomAttributes?
        //         //     .Where(data => typeof(PropertyAttribute).IsAssignableFrom(data.AttributeType))
        //         //     .ToList();
        //         // var customAttributes = field
        //         //     .GetCustomAttributes<PropertyAttribute>(true);
        //         var customAttributes = field
        //             .GetCustomAttributesData()
        //             .Where(data => typeof(PropertyAttribute).IsAssignableFrom(data.AttributeType));
        //         foreach (var propertyAttribute in customAttributes)
        //             list.AddSorted<CustomAttributeData>(propertyAttribute, (IComparer<CustomAttributeData>)s_Comparer);
        //     }
        //     return list;
        // }

        private static List<PropertyAttribute> GetSortedCustomPropertyAttributes(MemberInfo field)
        {
            var list = new List<PropertyAttribute>();
            if (field == null) return list;
            foreach (var propertyAttribute in field.GetCustomAttributes<PropertyAttribute>(true))
                list.AddSorted<PropertyAttribute>(propertyAttribute, (IComparer<PropertyAttribute>)s_Comparer);
            return list;
        }
    }
}
