using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public static class VisualElementDebugExtensions
    {
        public static string GetParentsAsString(this VisualElement self)
        {
            var items = new List<string>();
            var element = self;
            while (element != null)
            {
                items.Insert(0, element.GetElementAsString());
                element = element.parent;
            }

            return string.Join(" > ", items);
        }

        private static string GetElementAsString(this VisualElement self) => self?.GetType().Name;
    }
}
