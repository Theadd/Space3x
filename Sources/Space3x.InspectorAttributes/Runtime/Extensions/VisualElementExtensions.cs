using System.Collections.Generic;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
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
        
        public static VisualElement GetCommonClosestParentEventHandler(this VisualElement element)
        {
            VisualElement parent = element;
            while (parent != null)
            {
                if (parent is IOffscreenEventHandler) return parent;
#if UNITY_EDITOR
                if (parent is UnityEditor.UIElements.InspectorElement) return parent;
#endif
                if (parent is TemplateContainer) return parent;
                
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
        
        public static VisualElement GetNextSiblingOfType<T, T2>(this VisualElement element)
            where T : VisualElement
            where T2 : VisualElement
        {
            var parent = element.hierarchy.parent;
            if (parent == null)
                return null;
            
            var index = parent.hierarchy.IndexOf(element);
            
            for (var i = index + 1; i < parent.hierarchy.childCount; i++)
            {
                VisualElement match = parent.hierarchy.ElementAt(i) switch
                {
                    T m1 => m1,
                    T2 m2 => m2,
                    _ => null
                };
                if (match != null)
                    return match;
            }
            return null;
        }
        
        /// <summary>
        /// Add a sibling before this element
        /// </summary>
        public static VisualElement AddBefore(this VisualElement element, VisualElement sibling)
        {
            var index = Mathf.Max(element.hierarchy.parent.hierarchy.IndexOf(element), 0);
            element.hierarchy.parent.hierarchy.Insert(index, sibling);
            return element;
        }
        
        /// <summary>
        /// Add a sibling after this element
        /// </summary>
        public static VisualElement AddAfter(this VisualElement element, VisualElement sibling)
        {
            var index = Mathf.Min(Mathf.Max(element.hierarchy.parent.hierarchy.IndexOf(element) + 1, 0),
                element.hierarchy.parent.hierarchy.childCount);
            element.hierarchy.parent.hierarchy.Insert(index, sibling);
            return element;
        }
        
        public static VisualElement AddTo(this VisualElement self, VisualElement parent)
        {
            if (parent is ITreeAdd treeAdd)
                treeAdd.Add(self);
            else
                parent.Add(self);

            return self;
        }
        
        public static VisualElement SetVisible(this VisualElement self, bool value)
        {
            self.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
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
        
        /// <summary>
        /// Enumerates all children fields (deriving from <see cref="BaseField{TValueType}"/>-like types) in the
        /// correct order regardless of their nesting level.
        /// </summary>
        public static IEnumerable<BindableElement> GetChildrenFields(this VisualElement self, bool includeNestedFields = false)
        {
            foreach (var child in self.hierarchy.Children())
            {
                if (child is not ILayoutElement && child is IMixedValueSupport and BindableElement t)
                {
                    yield return t;
                    if (!includeNestedFields) continue;
                    foreach (var c in GetChildrenFields(child, true))
                        yield return c;
                }
                else
                {
                    var bindableElements = GetChildrenFields(child, includeNestedFields);
                    foreach (var c in bindableElements)
                        yield return c;
                }
            }
            yield break;
        }
        
        
    }
}
