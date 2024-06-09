using System;
using System.Collections.Generic;
using System.Reflection;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Extensions
{
    public static class VisualElementExtensions
    {
        public static T GetClosestParentOfType<T>(this VisualElement element) where T : VisualElement
        {
            VisualElement parent = element;
            while (parent != null)
            {
                if (parent is T)
                {
                    return parent as T;
                }
                parent = parent.parent;
            }
            return null;
        }
        
        public static T GetClosestParentOfType<T, TLimit>(this VisualElement element) where T : VisualElement
        {
            VisualElement parent = element;
            while (parent != null)
            {
                if (parent is T)
                    return parent as T;
                if (parent is TLimit)
                    return null;
                parent = parent.parent;
            }
            return null;
        }
        
        public static VisualElement GetClosestParentOfAnyType<T, T2, TLimit>(this VisualElement element) 
            where T : VisualElement
            where T2 : VisualElement
        {
            VisualElement parent = element;
            while (parent != null)
            {
                if (parent is T)
                    return parent as T;
                if (parent is T2)
                    return parent as T2;
                if (parent is TLimit)
                    return null;
                parent = parent.parent;
            }
            return null;
        }

        public static VisualElement GetPreviousSibling(this VisualElement element)
        {
            if (element.hierarchy.parent == null)
                return null;

            var index = element.hierarchy.parent.hierarchy.IndexOf(element);
            return index <= 0 ? null : element.hierarchy.parent.hierarchy.ElementAt(index - 1);
        }
        
        public static VisualElement GetPreviousSibling<TExclude>(this VisualElement element)
        {
            var index = element.parent.IndexOf(element);
            var sibling = index <= 0 ? null : element.parent.ElementAt(index - 1);
            return sibling is TExclude ? sibling.GetPreviousSibling<TExclude>() : sibling;
        }
        
        public static VisualElement GetPreviousSibling<TExclude, TExcept>(this VisualElement element)
        {
            var index = element.parent.IndexOf(element);
            var sibling = index <= 0 ? null : element.parent.ElementAt(index - 1);
            return sibling is TExclude and not TExcept ? sibling.GetPreviousSibling<TExclude, TExcept>() : sibling;
        }
        
        public static VisualElement GetNextSibling(this VisualElement element)
        {
            if (element.hierarchy.parent == null)
                return null;
            
            var index = element.hierarchy.parent.hierarchy.IndexOf(element);
            return index >= element.hierarchy.parent.hierarchy.childCount - 1 ? null : element.hierarchy.parent.hierarchy.ElementAt(index + 1);
        }
        
        public static VisualElement GetNextSibling<TExclude>(this VisualElement element)
        {
            if (element.hierarchy.parent == null)
                return null;
            
            var index = element.hierarchy.parent.hierarchy.IndexOf(element);
            var sibling = index >= element.hierarchy.parent.hierarchy.childCount - 1 ? null : element.hierarchy.parent.hierarchy.ElementAt(index + 1);
            return sibling is TExclude ? sibling.GetNextSibling<TExclude>() : sibling;
        }
        
        public static VisualElement GetNextSibling<TExclude, TExcept>(this VisualElement element)
        {
            if (element.hierarchy.parent == null)
                return null;

            var index = element.hierarchy.parent.hierarchy.IndexOf(element);
            var sibling = index >= element.hierarchy.parent.hierarchy.childCount - 1 ? null : element.hierarchy.parent.hierarchy.ElementAt(index + 1);
            return sibling is TExclude and not TExcept ? sibling.GetNextSibling<TExclude, TExcept>() : sibling;
        }
        
        public static T GetNextSiblingOfType<T>(this VisualElement element) where T : VisualElement
        {
            var parent = element.hierarchy.parent;
            if (parent == null)
                return null;
            
            var index = parent.hierarchy.IndexOf(element);
            
            for (var i = index + 1; i < parent.hierarchy.childCount; i++)
            {
                if (parent.hierarchy.ElementAt(i) is T match)
                    return match;
            }
            return null;
        }
        
        /// <summary>
        /// Add a sibling before this element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="sibling"></param>
        /// <returns></returns>
        public static VisualElement AddBefore(this VisualElement element, VisualElement sibling)
        {
            var index = Mathf.Max(element.hierarchy.parent.hierarchy.IndexOf(element), 0);
            element.hierarchy.parent.hierarchy.Insert(index, sibling);
            return element;
        }
        
        /// <summary>
        /// Add a sibling after this element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="sibling"></param>
        /// <returns></returns>
        public static VisualElement AddAfter(this VisualElement element, VisualElement sibling)
        {
            var index = Mathf.Min(
                Mathf.Max(
                    element.hierarchy.parent.hierarchy.IndexOf(element) + 1, 
                    0),
                element.hierarchy.parent.hierarchy.childCount);
            element.hierarchy.parent.hierarchy.Insert(index, sibling);
            return element;
        }
        
        public static VisualElement SetVisible(this VisualElement self, bool value)
        {
            self.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            return self;
        }

        [Obsolete("Use VisualElement.RemoveFromHierarchy instead.")]
        public static VisualElement RemoveFromHierarchy(this VisualElement self)
        {
            self.hierarchy.parent?.hierarchy.Remove(self);
            return self;
        }
        
        public static IEnumerable<T> GetChildren<T>(this VisualElement self) where T : VisualElement
        {
            foreach (var child in self.Children())
            {
                if (child is T t)
                    yield return t;
                else
                    foreach (var c in GetChildren<T>(child))
                        yield return c;
            }
            yield break;
        }
        
        // TODO: remove
        public static string CreateRichTextReport(this VisualElement element, string titlePrefix = "")
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
    }
}
