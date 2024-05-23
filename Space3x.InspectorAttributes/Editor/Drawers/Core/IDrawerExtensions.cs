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
                Debug.Log($"#{index} - {editor.target.name} - {editor.serializedObject.targetObject.name} - " +
                          $"{editor.name} - {editor.GetType().Name} - {editor.serializedObject.ToString()}");
            }
        }
        
//        internal void SetDrawMode(SpriteDrawMode drawMode)
//        {
//            if (drawMode == (SpriteDrawMode) this.m_DrawMode.intValue)
//                return;
//            foreach (SpriteRenderer targetObject in this.serializedObject.targetObjects)
//            {
//                Transform transform = targetObject.transform;
//                Undo.RecordObjects(new UnityEngine.Object[2]
//                {
//                    (UnityEngine.Object) targetObject,
//                    (UnityEngine.Object) transform
//                }, SpriteRendererEditor.Styles.drawModeChange.text);
//                targetObject.drawMode = drawMode;
//                foreach (Editor activeEditor in ActiveEditorTracker.sharedTracker.activeEditors)
//                {
//                    if (activeEditor.target == (UnityEngine.Object) transform)
//                        activeEditor.serializedObject.SetIsDifferentCacheDirty();
//                }
//            }
//            this.serializedObject.SetIsDifferentCacheDirty();
//        }
    }
}
