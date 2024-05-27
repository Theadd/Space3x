using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    public static class GroupMarkerExtensions
    {
        public static PropertyGroupField UngroupAll(this PropertyGroupField group)
        {
            var parent = group.hierarchy.parent;
            var indexInParent = parent.hierarchy.IndexOf(group);
            var parentGroupTypeClass = "";
            var exceptionTarget = "";
            var iMax = group.contentContainer.hierarchy.childCount - 1;
            if (parent is PropertyGroup pg && pg.hierarchy.parent is PropertyGroupField pgf)
                parentGroupTypeClass = $"ui3x-group--{pgf.Type.ToString().ToLower()}";
            
            for (var i = iMax; i >= 0; i--)
            {
                exceptionTarget = $"#i = {i}/{iMax}; ";
                try
                {
                    var element = group.contentContainer.hierarchy.ElementAt(i);
                    exceptionTarget += element.AsString(); /* TODO: remove */
                    if (element is PropertyField /*or BlockDecorator or PropertyGroupField*/)
                    {
                        element.WithClasses(false, "ui3x-group--none", "ui3x-group--column", "ui3x-group--row", "ui3x-group-item--last");
                        if (parentGroupTypeClass != "")
                            element.WithClasses(true, parentGroupTypeClass);
                    }
                    parent.hierarchy.Insert(indexInParent, element);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString() + "\n\n" + exceptionTarget + "\n");
                }
            }

            return group;
        }

        public static void PopulateGroupMarker(this GroupMarker self)
        {
            var beginDecorator = self.MarkerDecorator.GetGroupBeginMarkerDecorator();
            var endDecorator = self.MarkerDecorator.GetGroupEndMarkerDecorator();
            Debug.Log($"  ~> [POPULATING] {beginDecorator.DebugId} => {endDecorator.DebugId}");
            if (beginDecorator.Marker == null)
                beginDecorator.RebuildGroupMarkerIfRequired();
            if (endDecorator.Marker == null)
                endDecorator.RebuildGroupMarkerIfRequired();
            var beginMarker = beginDecorator.Marker;
            var endMarker = endDecorator.Marker;
            var group = GetOrCreatePropertyGroupFieldForMarker(beginMarker);
            Debug.Log($"    --> NULLs? group: {group == null}, beginMarker: {beginMarker == null}/{beginDecorator == null}, endMarker: {endMarker == null}/{endDecorator == null}");
            group.WithClasses(beginMarker.MarkerDecorator.GetType().Name + "-" + beginMarker.MarkerDecorator.GetHashCode(),
                endMarker.MarkerDecorator.GetType().Name + "-" + endMarker.MarkerDecorator.GetHashCode());
            group.LogThis("<color=#797348FF>BEGIN: CLOSE GROUP</color>");
            var parent = endMarker.parent;
            var beginIndex = parent.IndexOf(beginMarker);
            var endIndex = parent.IndexOf(endMarker);
            group.IsUsed = true;
            // Debug.Log($"$$$$$$$$$$ SHOULD BE TRUE: {((PropertyGroupField) self.MarkerDecorator.GetGroupBeginMarkerDecorator().GroupContainer).IsUsed}");
            var rawNodes = parent.Children()
                .Skip(beginIndex)
                .Take(endIndex - beginIndex + 1).ToList();
            group.AddAllToGroup(rawNodes);
            group.LogThis("<color=#797348FF>END: CLOSE GROUP</color>");
        }
      
//        public static bool CloseGroupMarker(this GroupMarker endMarker, GroupMarker beginMarker = null)
//        {
//            var parent = endMarker.parent;
//            beginMarker ??= endMarker.GetMatchingGroupMarker();
//            if (beginMarker == null)
//            {
//                Debug.LogError("<color=#FF0000CC><b>Couldn't find matching group marker<b></color>");
//                endMarker.Origin.LogThis("<color=#FF0000FF>NO MATCHING BEGIN MARKER</color>");
//                return false;
//            }
//            var endIndex = parent.IndexOf(endMarker);
//            var beginIndex = parent.IndexOf(beginMarker);
//            
//            if (endIndex < 0 || beginIndex < 0)
//                throw new Exception($"Invalid group marker indexes: {endIndex}, {beginIndex}");
//
//            var rawNodes = parent.Children()
//                .Skip(beginIndex)
//                .Take(endIndex - beginIndex + 1).ToList();
//
//            var group = GetOrCreatePropertyGroupFieldForMarker(beginMarker);
//            
//            endMarker.LinkTo(beginMarker);
//            
//            group.WithClasses(beginMarker.MarkerDecorator.GetType().Name + "-" + beginMarker.MarkerDecorator.GetHashCode(),
//                endMarker.MarkerDecorator.GetType().Name + "-" + endMarker.MarkerDecorator.GetHashCode());
//            
//            
//            
//            group.LogThis("<color=#797348FF>BEGIN: CLOSE GROUP</color>");
//            parent.Insert(beginIndex, group);
////            rawNodes.ForEach(group.AddToGroup);
//            group.AddAllToGroup(rawNodes);
//            
//            group.LogThis("<color=#797348FF>END: CLOSE GROUP</color>");
////            Debug.Log("AFTER GROUP CLOSED: " + beginMarker.MarkerDecorator.ValidateAllString());
//            return true;
//        }

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
                Type = beginMarker.Type,
                style =
                {
                    flexDirection = FlexDirection.Row
                },
                contentContainer =
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                        flexShrink = 1
                    }
                }
            };
            group.WithClasses($"ui3x-group-type--{beginMarker.Type.ToString().ToLower()}");
            beginMarker.MarkerDecorator.GroupContainer = group;
            if (beginMarker.Type == GroupType.Column)
                group.contentContainer.ColumnGrowShrink();
            else
                group.contentContainer.RowGrowShrink();
            
            return group;
        }

        private static string CreateHierarchyReportAndRetrieveReportTitleFor(VisualElement element, string titlePrefix = "")
        {
            var parentInspector = element.GetClosestParentOfType<InspectorElement>();
            var title = string.IsNullOrEmpty(titlePrefix) ? element.AsString() : titlePrefix + " " + element.AsString();
            if (parentInspector == null)
            {
                Debug.LogError($"<color=#FF0000CC><b>No matching parent InspectorElement found for {element.name}</b></color>\n{title}");
                return title;
            }
            var allText = parentInspector.AsHierarchyString();
            var count = RichTextViewer.Count();
            title = $"{count}: {title}";
            RichTextViewer.AddText(title, allText);
            return title;
        }

        private static void AddAllToGroup(this PropertyGroupField group, List<VisualElement> allElements)
        {
            var msg = "";
            var elements = allElements.GetRange(0, allElements.Count);
            elements.Reverse();
            var iMax = elements.Count - 1;
            for (var index = 0; index <= iMax; index++)
            {
                var element = elements[iMax - index];
                try
                {
                    msg = element.AsString();
//                    group.Add(element);
                    group.contentContainer.hierarchy.Add(element);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString() + "\n" + msg + "\n");
                    Debug.Log(CreateHierarchyReportAndRetrieveReportTitleFor(element, e.Message));
                    element.LogThis($"<color=#FF0000FF><b>ERROR ADDING TO GROUP ({((group == null) ? "NULL" : "NOT NULL")})</b></color>");
                }

                if (element is PropertyField /*or BlockDecorator or PropertyGroupField*/)
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
                else
                    Debug.LogWarning(element.GetType().Name);
            }

            elements
                .FirstOrDefault(e => e is PropertyField)
                ?.WithClasses(true, "ui3x-group-item--last");
        }
        
//        private static void AddToGroup(this PropertyGroupField group, VisualElement element)
//        {
//            group.Add(element);
//            if (element is PropertyField /*or BlockDecorator or PropertyGroupField*/)
//            {
//                switch (group.Type)
//                {
//                    case GroupType.Row:
//                        element.WithClasses(false, "ui3x-group--none", "ui3x-group--column")
//                            .WithClasses(true, "ui3x-group--row");
//                        break;
//                    case GroupType.Column:
//                        element.WithClasses(false, "ui3x-group--none", "ui3x-group--row")
//                            .WithClasses(true, "ui3x-group--column");
//                        break;
//                    case GroupType.None:
//                        element.WithClasses(false, "ui3x-group--row", "ui3x-group--column")
//                            .WithClasses(true, "ui3x-group--none");
//                        break;
//                }
//            }
//            else
//                Debug.LogWarning(element.GetType().Name);
////            else
////                VisualStyle.ApplyStyles(group.Type, element, !string.IsNullOrEmpty(group.GroupName));
//        }

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
                {
                    Debug.Log("    --------------------------------------------- TODO: REMOVE THIS LOG ENTRY -----------------------------------------------");
                    return endMarker.GetMatchingGroupMarker(maxRecursiveCalls: maxRecursiveCalls - 1);
                }

            return beginMarker as GroupMarker;
        }

        /// <summary>
        /// Rebuilds the group marker and it's related decorator container, if required.
        /// </summary>
        /// <param name="groupMarker"></param>
        /// <returns>Whether the group marker and/or it's related decorator container was rebuilt or not.</returns>
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
