using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Space3x.InspectorAttributes.Editor
{
    public abstract class EditorObjectProvider
    {
        private UnityEditor.Editor m_Editor;
        
        protected int ControllerID { get; private set; }
        
        public int InstanceID { get; private set; }

        public Object TargetObject { get; private set; }    // TODO: rename
        
        public Type TargetType { get; private set; }    // TODO: rename
        
        public object DeclaringObject { get; protected set; }

        public Type DeclaringType => DeclaringObject?.GetType();
        
        public string ParentPath { get; private set; }
        
        public bool IsSerialized { get; private set; }
        
        public SerializedObject SerializedObject { get; protected set; }

        public UnityEditor.Editor Editor => m_Editor ??=
            ActiveEditorTracker.sharedTracker.activeEditors.FirstOrDefault(
                e => e.target.GetInstanceID() == InstanceID);

        public bool IsEditingMultipleObjects { get; private set; }

        protected EditorObjectProvider(SerializedProperty property, int controllerId)
        {
            ControllerID = controllerId;
            var serializedObject = property.serializedObject;
            if (serializedObject != null)
            {
                SerializedObject = serializedObject;
                ParentPath = property.GetParentPath();
                IsSerialized = true;
                IsEditingMultipleObjects = serializedObject.isEditingMultipleObjects;
                // TODO: get rid of AllObjectTypesAreTheSame, it's always true by design
                if (!IsEditingMultipleObjects || (IsEditingMultipleObjects && AllObjectTypesAreTheSame(serializedObject.targetObjects)))
                {
                    TargetObject = serializedObject.targetObject;
                    InstanceID = TargetObject.GetInstanceID();
                    TargetType = TargetObject.GetType();
                    DeclaringObject = property.GetDeclaringObject();
                }
            }
        }

        protected EditorObjectProvider(IPropertyNode parentPropertyTreeRoot, int controllerId)
        {
            ControllerID = controllerId;
            var parentController = parentPropertyTreeRoot.GetController();
            SerializedObject = parentController.SerializedObject;
            IsSerialized = parentPropertyTreeRoot.HasSerializedProperty();
            ParentPath = parentPropertyTreeRoot.PropertyPath;
            IsEditingMultipleObjects = parentController.IsEditingMultipleObjects;
            TargetObject = parentController.TargetObject;
            InstanceID = TargetObject.GetInstanceID();
            TargetType = TargetObject.GetType();
            DeclaringObject = parentPropertyTreeRoot.GetUnderlyingValue();
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
