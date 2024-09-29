using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Space3x.Attributes.Types;

namespace Space3x.UiToolkit.Types
{
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Chainable version of VisualElement.Add method.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="element">child VisualElement</param>
        /// <returns>self</returns>
        public static VisualElement AlsoAdd(this VisualElement self, VisualElement element)
        {
            self.Add(element);
            return self;
        }
        
        /// <summary>
        /// Chainable version of VisualElement.EnableInClassList method.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="classes">Class names as params array.</param>
        /// <returns>self</returns>
        public static VisualElement WithClasses(this VisualElement self, params string[] classes)
        {
            foreach (var @class in classes)
                if (!string.IsNullOrEmpty(@class))
                    self.EnableInClassList(@class, true);

            return self;
        }
        
        /// <summary>
        /// Chainable version of VisualElement.EnableInClassList method.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="asEnabled">Whether to enable or disable.</param>
        /// <param name="classes">Class names as params array.</param>
        /// <returns>self</returns>
        public static VisualElement WithClasses(this VisualElement self, bool asEnabled, params string[] classes)
        {
            foreach (var @class in classes) 
                if (!string.IsNullOrEmpty(@class))
                    self.EnableInClassList(@class, asEnabled);

            return self;
        }
        
        /// <summary>
        /// Chainable version of setting VisualElement.name.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <returns>self</returns>
        public static VisualElement WithName(this VisualElement self, string name)
        {
            self.name = name;
            return self;
        }
        
        /// <summary>
        /// Chainable version of VisualElement.SetEnabled method.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="isEnabled">Whether to enable or disable.</param>
        /// <returns>self</returns>
        public static VisualElement WithEnabled(this VisualElement self, bool isEnabled)
        {
            self.SetEnabled(isEnabled);
            return self;
        }
        
        public static VisualElement WithVerticalMargin(this VisualElement self, float value)
        {
            self.style.marginTop = value;
            self.style.marginBottom = value;
            return self;
        }

        public static bool IsFocused(this VisualElement self) =>
            self.panel != null 
            && (self.panel.focusController.focusedElement == self 
                || (self.panel.focusController.focusedElement == self.parent && self.parent?.delegatesFocus == true));

        public static bool ShowPlaceholderText(this TextElement self) =>
            !string.IsNullOrEmpty(((ITextEdition)self).placeholder)
            && !(((ITextEdition)self).hidePlaceholderOnFocus && self.IsFocused())
            && string.IsNullOrEmpty(self.text);
        
        /// <summary>
        /// Provides access to the internal VisualElement's children List as an IList{VisualElement} for quick deconstructing.
        /// <example>
        /// var (icon, label, (textField, _)) = FoldoutHeader.AsChildren();
        /// </example>
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IList<VisualElement> AsChildren(this VisualElement self) => (IList<VisualElement>) self.hierarchy.Children();
        
        public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest)
            where T : VisualElement
        {
            first = list.Count > 0 ? list[0] : default(T); // or throw
            rest = list.Skip(1).ToList();
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out IList<T> rest)
            where T : VisualElement
        {
            first = list.Count > 0 ? list[0] : default(T); // or throw
            second = list.Count > 1 ? list[1] : default(T); // or throw
            rest = list.Skip(2).ToList();
        }


#if UNITY_EDITOR || SPACE3X_DEBUG
        [HideInCallstack]
        public static string AsString(this VisualElement self) => 
            self == null ? "null" : NJoin(" ", new[] 
            {
                StyleTag.Highlight.Wrap(StyleTag.Bold, self.GetType().Name),
                Prefix(StyleTag.Primary.Start + "#", self.name, StyleTag.Primary.End),
                StyleTag.Light.Wrap(Prefix(".", NJoin(" .", self.GetClasses())))
            });
        
        public static string AsStringForDebugger(this VisualElement self) =>
            StripTags(self.AsString());
        
        [HideInCallstack]
        public static VisualElement LogThis(this VisualElement self, string message = null, int indentLevel = 1)
        {
            if (self == null) return null;
            var content = string.Join("", Enumerable.Repeat("  ", indentLevel)) + "\u21e8 " 
                          + Prefix("", Prefix("<b>", message, "</b>"), " ‣ ") 
                          + self.AsString() + "\n" + self.AsSimplifiedParentHierarchyString();
            DebugLog.Info(content);
            
            return self;
        }

        [HideInCallstack]
        public static string AsSimplifiedParentHierarchyString(this VisualElement self, int depth = 5)
        {
            var parent = self.hierarchy.parent;
            if (parent == null) 
                return "No parent element found.";
            
            List<string> ancestors = new List<string>();
            
            for (int i = 0; i < depth && parent != null; ++i)
            {
                ancestors.Insert(0, parent.AsString());
                parent = parent.hierarchy.parent;
            }

            ancestors = ancestors
                .Select((s, i) => string.Join("", Enumerable.Repeat("  ", i)) + s)
                .ToList();

            var indent = string.Join("", Enumerable.Repeat("  ", ancestors.Count));
            parent = self.hierarchy.parent;
            var index = parent.hierarchy.IndexOf(self);
            
            ancestors.Add(indent + StyleTag.Grey.Wrap($"... {index} elements before" + ((index > 0) ? ", last one is: " + parent.hierarchy.ElementAt(index - 1).AsString() : ".")));
            ancestors.Add(indent + self.AsString());
            ancestors.Add(indent + StyleTag.Grey.Wrap($"... {parent.hierarchy.childCount - index - 1} elements after" + ((index < parent.hierarchy.childCount - 1) ? ", first one is: " + parent.hierarchy.ElementAt(index + 1).AsString() : ".")));

            return string.Join('\n', ancestors);
        }
        
        public static string AsSimplifiedParentHierarchyStringForDebugger(this VisualElement self, int depth = 5) =>
            StripTags(self.AsSimplifiedParentHierarchyString(depth));

        private static bool ShouldIterateOnChildren(VisualElement element)
        {
            if (element.hierarchy.childCount <= 0)
                return false;
            return element switch
            {
                ScrollView => false,
                _ => !element.ClassListContains("unity-base-field") || element.ClassListContains("ui3x-property-group-field")
            };
        }
        
        [HideInCallstack]
        public static string AsHierarchyString(this VisualElement self, int depth = 999, int indentLevel = 0) =>
            string.Join("", Enumerable.Repeat("<color=#FFFFFF33>|</color> ", indentLevel))
            + self.AsString()
            + (depth > 0 && ShouldIterateOnChildren(self)
                ? "\n" + string.Join('\n', self.hierarchy
                    .Children()
                    .Select(e => e.AsHierarchyString(depth - 1, indentLevel + 1)))
                : string.Empty);
        
        public static string AsHierarchyStringForDebugger(this VisualElement self, int depth = 999, int indentLevel = 0) =>
            StripTags(self.AsHierarchyString(depth, indentLevel));
        
        [HideInCallstack]
        public static string AsFullHierarchyString(this VisualElement self, int depth = 999, int indentLevel = 0) =>
            string.Join("", Enumerable.Repeat("<color=#FFFFFF33>|</color> ", indentLevel))
            + self.AsString()
            + (depth > 0 && self.hierarchy.childCount > 0
                ? "\n" + string.Join('\n', self.hierarchy
                    .Children()
                    .Select(e => e.AsFullHierarchyString(depth - 1, indentLevel + 1)))
                : string.Empty);

        public static string AsFullHierarchyStringForDebugger(this VisualElement self, int depth = 999, int indentLevel = 0) =>
            StripTags(self.AsFullHierarchyString(depth, indentLevel));
        
        public static string StringifyThis(this VisualElement self, int depth = 999, int indentLevel = 0)
        {
            if (self == null) return "";
            return StripTags(self.AsFullHierarchyString(depth, indentLevel))
                .Replace(".unity-base-field", ".UBF")
                .Replace(".unity-base", ".UB")
                .Replace(".unity", ".U");
        }

        private static string StripTags(string richStr)
        {
            var sb = new System.Text.StringBuilder(richStr.Length);
            try
            {
                var tag = false;
                for (var index = 0; index < richStr.Length; index++)
                {
                    var c = richStr[index];
                    if (tag)
                        tag = c == '>' ? false : tag;
                    else 
                        if (c == '<')
                            tag = true;
                        else
                            sb.Append(c);
                }
                // return sb.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                // return "";
            }
            return sb.ToString();
        }

        [HideInCallstack]
        private static string Prefix(string prefix, string value, string postfix = null) => 
            string.IsNullOrEmpty(value) ? string.Empty : $"{prefix}{value}{postfix ?? string.Empty}";
        
        [HideInCallstack]
        private static string NJoin(string separator, IEnumerable<string> values) => 
            string.Join(separator, values.Where(s => !string.IsNullOrEmpty(s)));
        
#else
        public static VisualElement LogThis(this VisualElement self, string message = null, int indentLevel = 1) => self;        
        public static string AsString(this VisualElement self) => string.Empty;
#endif
    }
}
