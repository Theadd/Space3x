using System;
using System.Diagnostics;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public static class VisualElementExtensionsMock
    {
        [Flags]
        public enum VerbosityFlags
        {
            None = 0,
            Hash = 1,
            Dev = 2,
            Info = 4,
            All = Hash | Dev | Info
        }
        
#if SPACE3X_DEBUG
        public static VerbosityFlags Verbosity { get; set; } = VerbosityFlags.All;
#else
        public static VerbosityFlags Verbosity { get; set; } = VerbosityFlags.None;
#endif
        
        private static bool HasFlag(VerbosityFlags flags) => (flags & Verbosity) != 0;

        public static VisualElement WithDevTools(this VisualElement self, object target)
        {
            AddDevNotes(self, target);
            return self;
        }
        
        [Conditional("SPACE3X_DEBUG")]
        private static void AddDevNotes(VisualElement element, object target)
        {
            if (Verbosity == VerbosityFlags.None) return;

#if !UNITY_EDITOR
            var info = "";
#else
            var info = !HasFlag(VerbosityFlags.Info) ? "" : target switch
            {
                PropertyDrawer propertyDrawer => "ui3x-info-" + propertyDrawer.attribute.GetType().Name.ToLower(CultureInfo.InvariantCulture),
                DecoratorDrawer decoratorDrawer => "ui3x-info-" + decoratorDrawer.attribute.GetType().Name.ToLower(CultureInfo.InvariantCulture),
                _ => ""
            };
#endif

            element.WithClasses(
                HasFlag(VerbosityFlags.Hash) ? "ui3x-hash-" + target.GetHashCode() : "", 
                HasFlag(VerbosityFlags.Dev) ? "ui3x-dev-" + target.GetType().Name.ToLower(CultureInfo.InvariantCulture) : "",
                info);
        }
    }
}
