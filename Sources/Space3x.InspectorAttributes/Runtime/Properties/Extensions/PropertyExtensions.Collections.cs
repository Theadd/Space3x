using JetBrains.Annotations;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public static partial class PropertyExtensions
    {
        /// <summary>
        /// Determines whether this property is an array.
        /// </summary>
        public static bool IsArray(this IPropertyNode self) => 
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
        public static bool IsArrayOrListElement(this IPropertyNode self) =>
            self is IPropertyNodeIndex;
        
        /// <summary>
        /// Delete the element at the specified index in the array.
        /// </summary>
        [UsedImplicitly]
        public static void DeleteArrayElementAtIndex(this IPropertyNode self, int index)
        {
#if UNITY_EDITOR
            if (self.HasSerializedProperty())
                self.GetSerializedProperty().DeleteArrayElementAtIndex(index);
            else
#endif
                ArrayPropertyUtility.DeleteArrayElementAtIndex(self, index);
        }
        
        /// <summary>
        /// Insert an new element at the specified index in the array.
        /// </summary>
        [UsedImplicitly]
        public static void InsertArrayElementAtIndex(this IPropertyNode self, int index)
        {
#if UNITY_EDITOR
            if (self.HasSerializedProperty())
                self.GetSerializedProperty().InsertArrayElementAtIndex(index);
            else
#endif
                ArrayPropertyUtility.InsertArrayElementAtIndex(self, index);
        }
        
        /// <summary>
        /// Move an array element from <paramref name="srcIndex"/> to <paramref name="dstIndex"/>.
        /// </summary>
        [UsedImplicitly]
        public static bool MoveArrayElement(this IPropertyNode self, int srcIndex, int dstIndex)
        {
#if UNITY_EDITOR
            if (self.HasSerializedProperty())
                return self.GetSerializedProperty().MoveArrayElement(srcIndex, dstIndex);
#endif
            return ArrayPropertyUtility.MoveArrayElement(self, srcIndex, dstIndex);
        }
    }
}
