using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Extensions;
using Space3x.Properties.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    public class AnnotatedRuntimeType
    {
        public List<string> Keys = new List<string>();
        public List<VTypeMember> Values = new List<VTypeMember>();
        
        public static readonly AnnotatedRuntimeType Empty = new AnnotatedRuntimeType();

        private static Dictionary<Type, AnnotatedRuntimeType> s_Instances;
        private static Comparer<PropertyAttribute> s_Comparer;

        public VTypeMember GetMissingValue(string memberName, object declaringObject)
        {
            var keyIndex = Keys.IndexOf(memberName);
            if (keyIndex == -1) keyIndex = Keys.IndexOf($"<{memberName}>k__BackingField");
            if (keyIndex >= 0) return Values[keyIndex];
            var memberInfo = declaringObject?.GetType()
                .GetMember(memberName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault();
            if (memberInfo == null) return null;
            VTypeMember item = null;
            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    item = new VTypeMember()
                    {
                        FieldType = methodInfo.ReturnType,
                        Name = methodInfo.Name,
                        PropertyAttributes = new List<PropertyAttribute>(),
                        RuntimeMethod = methodInfo,
                        Flags = VTypeFlags.IncludeInInspector
                    };
                    break;
                default:
                    // Other member types shouldn't be missing.
                    return null;
            }

            Values.Add(item);
            Keys.Add(memberInfo.Name);
            return item;
        }

        public static AnnotatedRuntimeType GetInstance(Type declaringType, bool asUnreliable = false)
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

            return asUnreliable ? CreateUnreliableInstanceFrom(value, declaringType) : value;
        }

        private static AnnotatedRuntimeType CreateUnreliableInstanceFrom(AnnotatedRuntimeType other, Type declaringType)
        {
            var value = new AnnotatedRuntimeType();
            value.Keys = other.Keys.Select(k => k).ToList();
            value.Values = other.Values.Select(VTypeMember.CreateUnreliableCopy).ToList();
            Debug.LogWarning($"<b>[PAC!] CreateUnreliableInstanceFrom({declaringType?.Name})</b>");
            return value;
        }

        public VTypeMember GetValue(string key)
        {
            var i = Keys.IndexOf(key);
            return i >= 0 ? Values[i] : null;
        }
        
        private static MemberInfo[] GetAllMembers(Type type)
        {
            var baseTypes = new List<Type>();
            Type rootType = type.IsValueType ? typeof(ValueType) : typeof(object);

            while (true)
            {
                if (type == null || type.IsInterface || type == rootType)
                    break;
                baseTypes.Add(type);
                type = type.BaseType;
            }
            
            return baseTypes
                .AsEnumerable()
                .Reverse()
                .SelectMany(t => t.GetMembers(
                    BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly))
                .ToArray();
        }
        
        private void Bind(Type declaringType)
        {
            var allProperties = new Dictionary<string, PropertyInfo>();
            var allMembers = GetAllMembers(declaringType);
            var invokableKeys = new List<string>();
            var invokableValues = new List<VTypeMember>();
            var keyIndex = -1;

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
                                keyIndex = invokableKeys.IndexOf(methodInfo.Name);
                                if (keyIndex >= 0)
                                {
                                    invokableValues.RemoveAt(keyIndex);
                                    invokableKeys.RemoveAt(keyIndex);
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
                                    keyIndex = Keys.IndexOf(fieldInfo.Name);
                                    if (keyIndex >= 0)
                                    {
                                        Values.RemoveAt(keyIndex);
                                        Keys.RemoveAt(keyIndex);
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
                                keyIndex = Keys.IndexOf(fieldInfo.Name);
                                if (keyIndex >= 0)
                                {
                                    Values.RemoveAt(keyIndex);
                                    Keys.RemoveAt(keyIndex);
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

        private static VTypeFlags ComputeInvokableNodeFlags(VTypeMember vType) => 
            vType.PropertyAttributes.Any(attr => attr is ShowInInspectorAttribute) 
                ? VTypeFlags.ShowInInspector | VTypeFlags.IncludeInInspector
                : VTypeFlags.IncludeInInspector;

        private static VTypeFlags ComputeFlags(VTypeMember vType) =>
            (IsSerializableField(vType.RuntimeField)
                ? HasHideInInspectorAttribute(vType.RuntimeField)
                    ? VTypeFlags.Serializable | VTypeFlags.HideInInspector
                    : VTypeFlags.Serializable | VTypeFlags.IncludeInInspector
                : HasHideInInspectorAttribute(vType.RuntimeField)
                    ? VTypeFlags.HideInInspector
                    : vType.PropertyAttributes.Any()
                        ? VTypeFlags.IncludeInInspector
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
                : VTypeFlags.None)
            | (vType.RuntimeField.IsInitOnly
                ? VTypeFlags.ReadOnly
                : VTypeFlags.None);

        private static bool IsSerializableField(FieldInfo fieldInfo) =>
            !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral && (
                fieldInfo.IsPublic || (
                    !fieldInfo.IsNotSerialized && (
                        fieldInfo.IsDefined(typeof(SerializeField), false) 
                        || fieldInfo.IsDefined(typeof(SerializeReference), false))));

        private static bool HasHideInInspectorAttribute(FieldInfo fieldInfo) => 
            fieldInfo.IsDefined(typeof(HideInInspector), false);

        private static bool HasShowInInspectorAttribute(FieldInfo fieldInfo) =>
            fieldInfo.IsDefined(typeof(ShowInInspectorAttribute), false);

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
