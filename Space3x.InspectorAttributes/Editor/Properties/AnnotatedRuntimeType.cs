using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor
{
    public class AnnotatedRuntimeType
    {
        public List<string> Keys = new List<string>();
        public List<VTypeMember> Values = new List<VTypeMember>();
        
        public static readonly AnnotatedRuntimeType Empty = new AnnotatedRuntimeType();

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
            if (declaringType == null) return Empty;
            if (!s_Instances.TryGetValue(declaringType, out var value))
            {
                value = new AnnotatedRuntimeType();
                value.Bind(declaringType);
                s_Instances.Add(declaringType, value);
            }

            return value;
        }
        
        public VTypeMember GetValue(string key)
        {
            var i = Keys.IndexOf(key);
            return i >= 0 ? Values[i] : null;
        }
        
        private void Bind(Type declaringType)
        {
            var allProperties = new Dictionary<string, PropertyInfo>();
            var allMembers = declaringType.GetMembers(
                BindingFlags.Instance | BindingFlags.Static | 
                BindingFlags.Public | BindingFlags.NonPublic);
            var invokableKeys = new List<string>();
            var invokableValues = new List<VTypeMember>();

            for (var i = 0; i < allMembers.Length; i++)
            {
                VTypeMember item = null;
                var memberInfo = allMembers[i];
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Method:
                        if (memberInfo is MethodInfo methodInfo)
                        {
                            if (methodInfo.GetCustomAttributes<PropertyAttribute>(true).Any())
                            {
                                if (invokableKeys.Contains(methodInfo.Name))
                                {
                                    Debug.LogError("Unexpected duplicated method found: " + methodInfo.Name);
                                    continue;
                                }
                                item = new VTypeMember()
                                {
                                    FieldType = methodInfo.ReturnType,
                                    Name = methodInfo.Name,
                                    PropertyAttributes = GetSortedCustomPropertyAttributes(methodInfo),
                                    RuntimeMethod = methodInfo,
                                };
                                item.Flags = ComputeInvokableNodeFlags(item);
                                invokableValues.Add(item);
                                invokableKeys.Add(methodInfo.Name);
                            }
                        }
                        continue;
                    
                    case MemberTypes.Field:
                        if (memberInfo is FieldInfo fieldInfo)
                        {
                            if (fieldInfo.Name.EndsWith("k__BackingField"))
                            {
                                if (allProperties.TryGetValue(fieldInfo.Name, out var propInfo))
                                {
                                    if (Keys.Contains(fieldInfo.Name))
                                    {
                                        Debug.LogError("Unexpected duplicated property found: " + fieldInfo.Name);
                                        continue;
                                    }

                                    item = new VTypeMember()
                                    {
                                        FieldType = fieldInfo.FieldType,
                                        Name = fieldInfo.Name,
                                        RuntimeField = fieldInfo,
                                        PropertyAttributes = GetSortedCustomPropertyAttributes(propInfo)
                                            .Concat(GetSortedCustomPropertyAttributes(fieldInfo))
                                            .ToList(),
                                    };
                                    item.Flags = ComputeFlags(item);
                                    Values.Add(item);
                                    Keys.Add(fieldInfo.Name);
                                }
                            }
                            else
                            {
                                if (Keys.Contains(fieldInfo.Name))
                                {
                                    Debug.LogError("Unexpected duplicated field found: " + fieldInfo.Name);
                                    continue;
                                }
                                item = new VTypeMember()
                                {
                                    FieldType = fieldInfo.FieldType,
                                    Name = fieldInfo.Name,
                                    PropertyAttributes = GetSortedCustomPropertyAttributes(fieldInfo),
                                    RuntimeField = fieldInfo,
                                };
                                item.Flags = ComputeFlags(item);
                                Values.Add(item);
                                Keys.Add(fieldInfo.Name);
                            }
                        }
                        break;
                    
                    case MemberTypes.Property:
                        if (memberInfo is PropertyInfo propertyInfo)
                            allProperties.Add($"<{propertyInfo.Name}>k__BackingField", propertyInfo);
                        continue;
                    
                    default:
                        continue;
                }
            }

            if (invokableKeys.Count > 0)
            {
                Keys = Keys.Concat(invokableKeys).ToList();
                Values = Values.Concat(invokableValues).ToList();
            }
        }

        private static VTypeFlags ComputeInvokableNodeFlags(VTypeMember vType) => VTypeFlags.None;

        private static VTypeFlags ComputeFlags(VTypeMember vType) =>
            (IsSerializableField(vType.RuntimeField) 
                ? HasHideInInspectorAttribute(vType.RuntimeField) 
                    ? VTypeFlags.Serializable | VTypeFlags.HideInInspector 
                    : VTypeFlags.Serializable 
                : HasHideInInspectorAttribute(vType.RuntimeField) 
                    ? VTypeFlags.HideInInspector 
                    : VTypeFlags.None) 
            | (vType.PropertyAttributes.Any(attr => attr is ShowInInspectorAttribute) 
                ? VTypeFlags.ShowInInspector 
                : VTypeFlags.None) 
            | (vType.PropertyAttributes.Any(attr => attr is NonReorderableAttribute) 
                ? VTypeFlags.NonReorderable 
                : VTypeFlags.None) 
            | (vType.FieldType.IsArray
                ? VTypeFlags.Array 
                : VTypeFlags.None)
            | (typeof(System.Collections.IList).IsAssignableFrom(vType.FieldType)
                ? VTypeFlags.List 
                : VTypeFlags.None);

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
