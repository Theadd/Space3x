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


#if UNITY_EDITOR
        [HideInCallstack]
        public static string AsString(this VisualElement self) => 
            NJoin(" ", new[] 
            {
                StyleTag.Highlight.Wrap(StyleTag.Bold, self.GetType().Name),
                Prefix(StyleTag.Primary.Start + "#", self.name, StyleTag.Primary.End),
                StyleTag.Light.Wrap(Prefix(".", NJoin(" .", self.GetClasses())))
            });
        
        [HideInCallstack]
        public static VisualElement LogThis(this VisualElement self, string message = null, int indentLevel = 1)
        {
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
        
        [HideInCallstack]
        public static string AsFullHierarchyString(this VisualElement self, int depth = 999, int indentLevel = 0) =>
            string.Join("", Enumerable.Repeat("<color=#FFFFFF33>|</color> ", indentLevel))
            + self.AsString()
            + (depth > 0 && self.hierarchy.childCount > 0
                ? "\n" + string.Join('\n', self.hierarchy
                    .Children()
                    .Select(e => e.AsHierarchyString(depth - 1, indentLevel + 1)))
                : string.Empty);

        [HideInCallstack]
        private static string Prefix(string prefix, string value, string postfix = null) => 
            string.IsNullOrEmpty(value) ? string.Empty : $"{prefix}{value}{postfix ?? string.Empty}";
        
        [HideInCallstack]
        private static string NJoin(string separator, IEnumerable<string> values) => 
            string.Join(separator, values.Where(s => !string.IsNullOrEmpty(s)));
        
#else
        public static VisualElement LogThis(this VisualElement self, string message = null, int indentLevel = 1) => self;        
#endif
    }
}
