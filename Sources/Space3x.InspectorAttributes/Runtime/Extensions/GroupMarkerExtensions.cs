using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
    public static class GroupMarkerExtensions
    {
        public static PropertyGroupField UngroupAll(this PropertyGroupField group)
        {
            var parent = group.hierarchy.parent;
            if (parent == null)
                return group;
            var indexInParent = parent.hierarchy.IndexOf(group);
            var parentGroupTypeClass = "";
            var iMax = group.contentContainer.hierarchy.childCount - 1;
            if (parent is PropertyGroup pg && pg.hierarchy.parent is PropertyGroupField pgf)
                parentGroupTypeClass = $"ui3x-group--{pgf.Type.ToString().ToLower()}";
            
            for (var i = iMax; i >= 0; i--)
            {
                try
                {
                    var element = group.contentContainer.hierarchy.ElementAt(i);
#if UNITY_EDITOR
                    if (element is UnityEditor.UIElements.PropertyField or BindablePropertyField)
#else
                    if (element is BindablePropertyField)
#endif
                    {
                        element.WithClasses(false, "ui3x-group--none", "ui3x-group--column", "ui3x-group--row", "ui3x-group-item--last");
                        if (parentGroupTypeClass != "")
                            element.WithClasses(true, parentGroupTypeClass);
                    }
                    parent.hierarchy.Insert(indexInParent, element);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }

            return group;
        }

        public static void PopulateGroupMarker(this GroupMarker self)
        {
            var beginDecorator = self.MarkerDecorator.GetGroupBeginMarkerDecorator();
            var endDecorator = self.MarkerDecorator.GetGroupEndMarkerDecorator();
            if (beginDecorator.Marker == null)
                beginDecorator.RebuildGroupMarkerIfRequired();
            if (endDecorator.Marker == null)
                endDecorator.RebuildGroupMarkerIfRequired();
            var beginMarker = beginDecorator.Marker;
            var endMarker = endDecorator.Marker;
            var group = GetOrCreatePropertyGroupFieldForMarker(beginMarker);
            var parent = endMarker.parent;
            var beginIndex = parent.IndexOf(beginMarker);
            var endIndex = parent.IndexOf(endMarker);
            if (group.Type != GroupType.Row)
                group.WithClasses(true, "unity-inspector-element", "unity-inspector-main-container");
            group.IsUsed = true;
            var rawNodes = parent.Children()
                .Skip(beginIndex)
                .Take(endIndex - beginIndex + 1).ToList();
            group.AddAllToGroup(rawNodes);
            // if (group.Type == GroupType.Row)
            //     group.MakeFieldsOnRowGroupsNotAligned();
        }

        // private static void MakeFieldsOnRowGroupsNotAligned(this PropertyGroupField group)
        // {
        //     foreach (var field in group.GetChildrenFields(includeNestedFields: true))
        //     {
        //         // if (field.parent.ClassListContains("ui3x-group--row") ||
        //         //     (field.parent?.parent?.parent?.ClassListContains("ui3x-group--row") ?? false))
        //         if (field.ClassListContains(UssConstants.UssAligned))
        //         {
        //             field.EnableInClassList(UssConstants.UssAligned, false);
        //             if (field.hierarchy.childCount > 0 && field.hierarchy[0] is Label label)
        //             {
        //                 label.style.minWidth = new StyleLength(StyleKeyword.Null);
        //                 label.style.width = new StyleLength(StyleKeyword.Null);
        //             }
        //         }
        //     }
        // }

        public static PropertyGroupField GetOrCreatePropertyGroupFieldForMarker(this GroupMarker beginMarker)
        {
            if (beginMarker.MarkerDecorator.GroupContainer is not PropertyGroupField group)
                group = CreatePropertyGroupFieldForMarker(beginMarker);

            if (!IsValidGroupForMarker(group, beginMarker))
            {
                if (beginMarker.IsUsed && group.hierarchy.parent != null)
                    throw new Exception($"Unable to find valid group for marker: {beginMarker}");
                
                beginMarker.AddBefore(group);
            }

            return group;
        }

        private static bool IsValidGroupForMarker(PropertyGroupField group, GroupMarker beginMarker)
        {
            if (!group.IsUsed)
                return beginMarker.GetPreviousSibling() == group;
            
            return beginMarker.hierarchy.parent == group.contentContainer;
        }
        
        private static PropertyGroupField CreatePropertyGroupFieldForMarker(this GroupMarker beginMarker)
        {
            var group = new PropertyGroupField(beginMarker.GroupName)
            {
                Text = beginMarker.GroupName,
                GroupName = beginMarker.GroupName,
                Type = beginMarker.Type
            };
#if UNITY_EDITOR
            if (beginMarker.MarkerDecorator is UnityEditor.DecoratorDrawer { attribute: GroupMarkerAttribute { ProportionalSize: false } })
#else
            if (beginMarker.MarkerDecorator is Space3x.Properties.Types.DecoratorDrawerAdapter { attribute: GroupMarkerAttribute { ProportionalSize: false } })
#endif
                group.WithClasses(UssConstants.UssNonProportional);
            group.WithClasses($"ui3x-group-type--{beginMarker.Type.ToString().ToLower()}");
            beginMarker.MarkerDecorator.GroupContainer = group;
            
            return group;
        }

        private static void AddAllToGroup(this PropertyGroupField group, List<VisualElement> allElements)
        {
            var elements = allElements.GetRange(0, allElements.Count);
            elements.Reverse();
            var iMax = elements.Count - 1;
            for (var index = 0; index <= iMax; index++)
            {
                var element = elements[iMax - index];
                try
                {
                    group.contentContainer.hierarchy.Add(element);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }

#if UNITY_EDITOR
                if (element is UnityEditor.UIElements.PropertyField or BindablePropertyField)
#else
                    if (element is BindablePropertyField)
#endif
                {
                    switch (group.Type)
                    {
                        case GroupType.Row:
                            element.WithClasses(false, "ui3x-group--none", "ui3x-group--column", "ui3x-group-item--last")
                                .WithClasses(true, "ui3x-group--row");
                            break;
                        case GroupType.Column:
                            element.WithClasses(false, "ui3x-group--none", "ui3x-group--row", "ui3x-group-item--last")
                                .WithClasses(true, "ui3x-group--column");
                            break;
                        case GroupType.None:
                            element.WithClasses(false, "ui3x-group--row", "ui3x-group--column", "ui3x-group-item--last")
                                .WithClasses(true, "ui3x-group--none");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            elements
#if UNITY_EDITOR
                .FirstOrDefault(e => e is UnityEditor.UIElements.PropertyField or BindablePropertyField)
#else
                .FirstOrDefault(e => e is BindablePropertyField)
#endif
                ?.WithClasses(true, "ui3x-group-item--last");
        }
        
        public static GroupMarker GetMatchingGroupMarker(this GroupMarker endMarker, int maxRecursiveCalls = 5)
        {
            if (maxRecursiveCalls < 0)
            {
                Debug.LogWarning($"<color=#FF0000CC><b>Recursive call limit reached ({maxRecursiveCalls})</b></color>");
                return null;
            }
            var parent = endMarker.hierarchy.parent;
            var endIndex = parent.hierarchy.IndexOf(endMarker);
            var allNodes = parent.hierarchy.Children()
                .Take(endIndex + 1);

            var beginMarker = allNodes.LastOrDefault(
                node => node is GroupMarker marker
                        && (marker.Type == endMarker.Type && marker.IsOpen && !marker.IsUsed));

            // If matching group marker was not properly attached (positioned), rebuild it and recursively call this method again.
            if (beginMarker is GroupMarker other)
                if (other.RebuildGroupMarkerIfRequired())
                    return endMarker.GetMatchingGroupMarker(maxRecursiveCalls: maxRecursiveCalls - 1);

            return beginMarker as GroupMarker;
        }

        /// <summary>
        /// Rebuilds the group marker and its related decorator container, if required.
        /// </summary>
        /// <param name="groupMarker"></param>
        /// <returns>Whether the group marker and/or its related decorator container was rebuilt or not.</returns>
        public static bool RebuildGroupMarkerIfRequired(this GroupMarker groupMarker)
        {
            var isValid = false;
            if (groupMarker.MarkerDecorator is IGroupMarkerDecorator decorator)
            {
                isValid = decorator.EnsureContainerIsProperlyAttached();
                if (!isValid)
                    decorator.RebuildGroupMarker();
                else
                    if (!decorator.HasValidMarker())
                    {
                        isValid = false;
                        decorator.RebuildGroupMarker();
                    }
            }
            
            return !isValid;
        }
    }
}
