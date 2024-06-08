using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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
        
        private static string[] PropertyPathParts(this IProperty self) => 
            self.FixedPropertyPath().Split('.');
        
        public static bool IsPropertyIndexer(this IProperty self, out string fieldName, out int index)
        {
            var propertyPart = FixedPropertyPath(self);
            if (propertyPart[^1] == ']')
            {
                var iStart = propertyPart.LastIndexOf("[", propertyPart.Length - 1, 6, StringComparison.Ordinal);
                if (iStart >= 0)
                {
                    var sIndex = propertyPart.Substring(iStart, propertyPart.Length - (iStart + 1));
                    Debug.Log("sIndex: " + sIndex);
                    index = int.Parse(sIndex);
                    fieldName = propertyPart[..iStart];
                    return true;
                }
            }
            fieldName = propertyPart;
            index = -1;
            return false;
        }
        
        // private static Type GetUnderlyingElementType(this IProperty self)  // SerializedProperty property
        // {
        //     SerializedPropertyUtility.
        //     var type = property.GetUnderlyingType();
        //     if (property.isArray) type = type.GetElementType() ?? type.GetGenericArguments().FirstOrDefault();
        //     return type;
        // }
    }
}
