using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    public static class IDrawerExtensions
    {
        public static SerializedObject GetSerializedObject(this IDrawer drawer) => 
            drawer.Property?.GetSerializedObject();

        public static UnityEditor.Editor[] GetActiveEditors(this IDrawer drawer) => 
            ActiveEditorTracker.sharedTracker.activeEditors;
        
        public static bool IsInspectorInDebugMode(this IDrawer drawer) =>
            ActiveEditorTracker.sharedTracker.inspectorMode != InspectorMode.Normal;
        
        public static void ForceRebuild(this IDrawer drawer) =>
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        
        public static void RebuildIfNecessary(this IDrawer drawer) =>
            ActiveEditorTracker.sharedTracker.RebuildIfNecessary();

        public static void LogActiveEditors(this IDrawer drawer)
        {
            var editors = drawer.GetActiveEditors();
            for (var index = 0; index < editors.Length; index++)
            {
                var editor = editors[index];
                Debug.LogWarning($"#{index} - {editor.target.name} - {editor.serializedObject.targetObject.name} - " +
                          $"{editor.serializedObject.GetType().Name} - {editor.GetType().Name} - {editor.serializedObject.GetHashCode()}");
            }
        }

        public static IPanel GetPanel(this IDrawer drawer) => drawer is IDecorator decorator
            ? decorator.GhostContainer.panel
            : drawer.Container.panel;

        // public static string GetParentPath(this IDrawer drawer)
        // {
        //     var prop = drawer.Property;
        //     if (prop == null || prop.propertyPath == prop.name)
        //         return "";
        //     if (prop.propertyPath.EndsWith("." + prop.name))
        //         return prop.propertyPath[..^(prop.name.Length + 1)];
        //     else
        //     {
        //         Debug.LogError($"Case not implemented in IDrawer.GetParentPath() for: {prop.propertyPath}");
        //         return prop.propertyPath;
        //     }
        // }
        //
        // public static int GetParentObjectHash(this IDrawer drawer)
        // {
        //     var parentPath = drawer.GetParentPath();
        //     if (string.IsNullOrEmpty(parentPath))
        //     {
        //         return drawer.Property.serializedObject.targetObject.GetInstanceID();
        //     }
        //     else
        //     {
        //         return drawer.Property.serializedObject.targetObject.GetInstanceID() ^ parentPath.GetHashCode();
        //     }
        // }
    }
}
