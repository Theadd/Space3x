using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    public class AnnotatedRuntimeType
    {
        public AnnotatedRuntimeType(Type targetType)
        {
            
        }
        
        private void Bind(Type declaringType)
        {
            // declaringType.DeclaringType.GetRuntimeMethods()

            var allMembers = declaringType.GetMembers(
                BindingFlags.Instance | BindingFlags.Static | 
                BindingFlags.Public | BindingFlags.NonPublic);

            for (var i = 0; i < allMembers.Length; i++)
            {
                MemberInfo memberInfo = allMembers[i];
                var customAttributes = memberInfo.CustomAttributes?
                    .Where(data => typeof(PropertyAttribute).IsAssignableFrom(data.AttributeType))
                    .ToList();
                
                // foreach (var customAttributeData in customAttributes)
                // {
                //     customAttributeData.AttributeType.
                // }

                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Method:

                        break;
                    
                    case MemberTypes.Field:

                        break;
                    
                    case MemberTypes.Property:

                        break;
                    
                    default:
                        Debug.LogWarning($"<b>[NO HANDLER IMPLEMENTED FOR THIS MEMBER TYPE, YET]</b> {memberInfo.MemberType.ToString()}, {memberInfo}");
                        break;
                }

            }

            // public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
            //     return (IEnumerable<MethodInfo>) type.GetMethods(BindingFlags.Instance |
            //          BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // public static IEnumerable<FieldInfo> GetRuntimeFields(this Type type)
            //     return (IEnumerable<FieldInfo>) type.GetFields(BindingFlags.Instance |
            //          BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}