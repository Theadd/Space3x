using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    public static class IDrawerExtensions
    {
        public static SerializedObject GetSerializedObject(this IDrawer drawer) => 
            drawer.Property?.serializedObject;

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
    }
}
