using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.InspectorAttributes.Editor;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.Properties.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    public abstract class PropertyControllerBase
    {
#if UNITY_EDITOR
        private UnityEditor.Editor m_Editor;
#endif
        
        protected int ControllerID { get; private set; }
        
        public int InstanceID { get; private set; }

        public UnityEngine.Object TargetObject { get; private set; }
        
        public Type TargetType { get; private set; }
        
        public object DeclaringObject { get; protected set; }

        public Type DeclaringType => DeclaringObject?.GetType();
        
        public string ParentPath { get; private set; }
        
        public bool IsSerialized { get; private set; }
        
        public bool IsRuntimeUI { get; set; }

        public int CachedDeclaringObjectHashCode { get; private set; }

        public object SerializedObject { get; protected set; }

#if UNITY_EDITOR
        public UnityEditor.Editor Editor => m_Editor ??=
            UnityEditor.ActiveEditorTracker.sharedTracker.activeEditors.FirstOrDefault(
                e => e.target.GetInstanceID() == InstanceID);
#endif

        public bool IsEditingMultipleObjects { get; private set; }

#if UNITY_EDITOR
        protected PropertyControllerBase(UnityEditor.SerializedProperty property, int controllerId)
        {
            ControllerID = controllerId;
            var serializedObject = property.serializedObject;
            if (serializedObject != null)
            {
                SerializedObject = serializedObject;
                ParentPath = property.GetParentPath();
                // IsSerialized = true;
                IsSerialized = !Application.isPlaying;
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
#endif

        protected PropertyControllerBase(IPropertyNode parentPropertyTreeRoot, int controllerId)
        {
            ControllerID = controllerId;
            var parentController = parentPropertyTreeRoot.GetController();
            IsRuntimeUI = parentController.IsRuntimeUI;
            SerializedObject = parentController.SerializedObject;
            IsSerialized = !Application.isPlaying && parentPropertyTreeRoot.HasSerializedProperty();
            ParentPath = parentPropertyTreeRoot.PropertyPath;
            IsEditingMultipleObjects = parentController.IsEditingMultipleObjects;
            TargetObject = parentController.TargetObject;
            InstanceID = TargetObject.GetInstanceID();
            TargetType = TargetObject.GetType();
            if (parentPropertyTreeRoot.IsRuntimeUI())
                DeclaringObject = parentPropertyTreeRoot.GetValueUnsafe();
            else
                DeclaringObject = parentPropertyTreeRoot.GetUnderlyingValue();  // .GetValue();
            CachedDeclaringObjectHashCode = DeclaringObject?.GetHashCode() ?? 0;
        }

        protected PropertyControllerBase(UnityEngine.Object target, int controllerId)
        {
            IsRuntimeUI = true;
            ControllerID = controllerId;
            SerializedObject = null;
            IsSerialized = false;
            ParentPath = "";
            IsEditingMultipleObjects = false;
            TargetObject = target;
            InstanceID = TargetObject.GetInstanceID();
            TargetType = TargetObject.GetType();
            DeclaringObject = target;
            CachedDeclaringObjectHashCode = DeclaringObject?.GetHashCode() ?? 0;
        }

        private static bool AllObjectTypesAreTheSame(IReadOnlyList<UnityEngine.Object> targetObjects)
        {
            if (targetObjects == null || targetObjects.Count == 0)
                throw new Exception("<color=#FF0000FF><b>UNEXPECTED RETURN VALUE FOR AllObjectTypesAreTheSame IN PropertyControllerBase.</b></color>");
                // return false;

            var type = targetObjects[0].GetType();
            for (var i = 1; i < targetObjects.Count; i++)
                if (targetObjects[i].GetType() != type)
                    throw new Exception("<color=#FF0000FF><b>UNEXPECTED RETURN VALUE FOR AllObjectTypesAreTheSame IN PropertyControllerBase.</b></color>");
                    // return false;

            return true;
        }
    }
}
