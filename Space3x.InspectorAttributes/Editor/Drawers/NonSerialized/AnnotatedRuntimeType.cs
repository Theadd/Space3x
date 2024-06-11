using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
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
        
        public VTypeMember GetValue(string key)
        {
            var i = Keys.IndexOf(key);
            return i >= 0 ? Values[i] : null;
        }
        
        private void Bind(Type declaringType)
        {
            Debug.LogWarning($"<b>AnnotatedRuntimeType.Bind({declaringType.FullName})</b>");
            Keys = new List<string>();
            Values = new List<VTypeMember>();
            CustomAttributes = new List<CustomAttributeData>();
            var allProperties = new Dictionary<string, PropertyInfo>();
            // var allSpecialMethods = new Dictionary<string, MethodInfo>();
            var attributeNames = "";
            VTypeMember item = null;
            var allMembers = declaringType.GetMembers(
                BindingFlags.Instance | BindingFlags.Static | 
                BindingFlags.Public | BindingFlags.NonPublic);

            for (var i = 0; i < allMembers.Length; i++)
            {
                var memberInfo = allMembers[i];
                // var customAttributes = memberInfo.CustomAttributes?
                //     .Where(data => typeof(PropertyAttribute).IsAssignableFrom(data.AttributeType))
                //     .ToList();

                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Method:
                        if (memberInfo is MethodInfo methodInfo)
                        {
                            // if (methodInfo.IsSpecialName)
                            //     allSpecialMethods.Add(methodInfo.Name, methodInfo);
                            attributeNames = string.Join(", ", memberInfo
                                .GetCustomAttributes<PropertyAttribute>(true)
                                .Select(attr => attr.GetType().Name)
                                .ToList());
                            if (!string.IsNullOrEmpty(attributeNames))
                                Debug.LogWarning($"<color=#FF0000FF><b>// TODO: Valid PropertyAttribute are assigned to an unhandled MethodInfo ({memberInfo.Name}):</b> {attributeNames}.</color>");
                        }
                        // if (customAttributes.Count > 0)
                        //     Debug.LogWarning("// TODO: A valid PropertyAttribute is assigned to an unhandled MemberInfo.");
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
                                        // RuntimeProperty = propInfo,
                                        RuntimeField = fieldInfo,
                                        // PropertyGetter =
                                        //     allSpecialMethods.GetValueOrDefault("get_" + propInfo.Name, null),
                                        // PropertySetter =
                                        //     allSpecialMethods.GetValueOrDefault("set_" + propInfo.Name, null),
                                        // CustomAttributes = customAttributes,
                                        PropertyAttributes = GetSortedCustomPropertyAttributes(propInfo)
                                            .Concat(GetSortedCustomPropertyAttributes(fieldInfo))
                                            .ToList(),
                                        // Flags = (HasHideInInspectorAttribute(fieldInfo)
                                        //             ? VTypeFlags.HideInInspector
                                        //             : VTypeFlags.None)
                                        //         | (HasShowInInspectorAttribute(fieldInfo)
                                        //             ? VTypeFlags.ShowInInspector
                                        //             : VTypeFlags.None)
                                        //         | (IsSerializableField(fieldInfo)
                                        //             ? VTypeFlags.Serializable
                                        //             : VTypeFlags.None)
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
                                    // CustomAttributes = customAttributes,
                                    PropertyAttributes = GetSortedCustomPropertyAttributes(fieldInfo),
                                    RuntimeField = fieldInfo,
                                    // Flags = (HasHideInInspectorAttribute(fieldInfo) ? VTypeFlags.HideInInspector : VTypeFlags.None)
                                    //         | (HasShowInInspectorAttribute(fieldInfo) ? VTypeFlags.ShowInInspector : VTypeFlags.None)
                                    //         | (IsSerializableField(fieldInfo) ? VTypeFlags.Serializable : VTypeFlags.None)
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
                        // allAttributesOnProperties.Add($"<{memberInfo.Name}>k__BackingField", GetSortedCustomPropertyAttributes(memberInfo));
                        // attributeNames = string.Join(", ", memberInfo
                        //     .GetCustomAttributes<PropertyAttribute>(true)
                        //     .Select(attr => attr.GetType().Name)
                        //     .ToList());
                        // if (!string.IsNullOrEmpty(attributeNames))
                        //     Debug.LogWarning($"<color=#FF0000FF><b>// TODO: Valid PropertyAttribute are assigned to an unhandled PropertyInfo ({memberInfo.Name}):</b> {attributeNames}.</color>");
                        continue;
                    
                    default:
                        // Debug.LogWarning($"<b>[NO HANDLER IMPLEMENTED FOR THIS MEMBER TYPE, YET]</b> {memberInfo.MemberType.ToString()}, {memberInfo}");
                        // if (customAttributes.Count > 0)
                        //     Debug.LogWarning("<b>// TODO: A valid PropertyAttribute is assigned to an unhandled MemberInfo.</b>");
                        continue;
                }
                // if (customAttributes.Count > 0)
                //     CustomAttributes = CustomAttributes.Concat(customAttributes).ToList();
            }
        }

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
