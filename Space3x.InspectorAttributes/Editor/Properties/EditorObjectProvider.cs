using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class EditorObjectProvider
    {
        private UnityEditor.Editor m_Editor;
        
        public int InstanceID { get; private set; }

        public Object TargetObject { get; private set; }    // TODO: rename
        
        public Type TargetType { get; private set; }    // TODO: rename
        
        public object DeclaringObject { get; private set; }

        public Type DeclaringType => DeclaringObject?.GetType();
        
        public string ParentPath { get; private set; }
        
        public SerializedObject SerializedObject { get; private set; }

        public UnityEditor.Editor Editor => m_Editor ??=
            ActiveEditorTracker.sharedTracker.activeEditors.FirstOrDefault(
                e => e.target.GetInstanceID() == InstanceID);

        public bool IsValid { get; private set; } = false;  // TODO: use

        public bool IsEditingMultipleObjects { get; private set; }

        protected EditorObjectProvider(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            if (serializedObject != null)
            {
                SerializedObject = serializedObject;
                ParentPath = property.GetParentPath();
                IsEditingMultipleObjects = serializedObject.isEditingMultipleObjects;
                if (!IsEditingMultipleObjects || (IsEditingMultipleObjects && AllObjectTypesAreTheSame(serializedObject.targetObjects)))
                {
                    TargetObject = serializedObject.targetObject;
                    InstanceID = TargetObject.GetInstanceID();
                    TargetType = TargetObject.GetType();
                    DeclaringObject = property.GetDeclaringObject(); // TODO: IProperty.DeclaringObject
                    IsValid = true;
                }
            }
        }
        
        private static bool AllObjectTypesAreTheSame(IReadOnlyList<Object> targetObjects)
        {
            if (targetObjects == null || targetObjects.Count == 0)
                return false;

            var type = targetObjects[0].GetType();
            for (var i = 1; i < targetObjects.Count; i++)
                if (targetObjects[i].GetType() != type)
                    return false;

            return true;
        }
    }
}