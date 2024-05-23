using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Utilities
{
    public static class VisualStyle
    {
//        public static void ApplyStyles(GroupType type, VisualElement element, bool groupHasLabel = false)
//        {
//            element.style.flexDirection = type switch
//            {
//                GroupType.Row => FlexDirection.Row,
//                GroupType.Column => FlexDirection.Column,
//                _ => element.style.flexDirection
//            };
//            element.style.flexGrow = 1;
//            element.style.flexShrink = 1;
//            
//            element.Children().Where(el =>
//            {
//                return el switch
//                {
//                    PropertyGroup or PropertyGroupField => false,
//                    PropertyField => true,
//                    Label => NoFlexGrow(el) is null,
//                    BindableElement => IsPropertyField(el) ? true : IsBaseField(el) && !ManuallyWalkOnBaseField(el, type, groupHasLabel),
//                    VisualElement => !IsUseless(el),
//                    _ => false
//                };
//            }).ForEach(el => ApplyStyles(type, el, groupHasLabel));
//        }
//        
//        private static bool IsPropertyField(this VisualElement element) => element.ClassListContains("unity-property-field");
//        
//        private static bool IsBaseField(this VisualElement element) => element.ClassListContains("unity-base-field");
//
//        private static bool IsUseless(this VisualElement element)
//        {
//            if (!element.ClassListContains("unity-decorator-drawers-container")) return false;
//            element.style.display = DisplayStyle.None;
//            return true;
//        }
//        
//        private static bool ManuallyWalkOnBaseField(this VisualElement element, GroupType type, bool groupHasLabel = false)
//        {
//            var rowReverse = groupHasLabel && type == GroupType.Row;
//            var noGrowField = element is Toggle;
//            if (type == GroupType.Row)
//                if (rowReverse)
//                    RowReverseGrowShrink(element);
//                else
//                    RowGrowShrink(element);
//            else
//                ColumnGrowShrink(element);
//
//            if (element.childCount > 0)
//            {
//                element.Children().Skip(1).ForEach(rowReverse && noGrowField ? NoFlexGrow : RowGrowShrink);
//                element.Children().Take(1).ForEach(rowReverse ? ColumnGrowShrink : NoFlexGrow);
//            }
//
//            return true;
//        }
//
//        public static VisualElement NoFlexGrow(this VisualElement element)
//        {
//            element.style.flexGrow = 0;
//            return element;
//        }
        
        public static VisualElement RowGrowShrink(this VisualElement element)
        {
            element.style.flexGrow = 1;
            element.style.flexShrink = 1;
            element.style.flexDirection = FlexDirection.Row;
            return element;
        }
        
//        public static VisualElement RowReverseGrowShrink(this VisualElement element)
//        {
//            element.style.flexGrow = 1;
//            element.style.flexShrink = 1;
//            element.style.flexDirection = FlexDirection.RowReverse;
//            return element;
//        }
        
        public static VisualElement ColumnGrowShrink(this VisualElement element)
        {
            element.style.flexGrow = 1;
            element.style.flexShrink = 1;
            element.style.flexDirection = FlexDirection.Column;
            return element;
        }
    }
}
