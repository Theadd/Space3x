using System;
using System.Linq;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
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

        public UnityEditor.Editor Editor => m_Editor ??=
            ActiveEditorTracker.sharedTracker.activeEditors.FirstOrDefault(
                e => e.target.GetInstanceID() == InstanceID);

        public bool IsValid { get; private set; } = false;

        public bool IsEditingMultipleObjects { get; private set; }

        protected EditorObjectProvider(IDrawer drawer)
        {
            var serializedObject = drawer.Property?.serializedObject;
            if (serializedObject != null)
            {
                Debug.Log($"  [PATH]: {drawer.Property.propertyPath}");
                ParentPath = drawer.GetParentPath();
                IsEditingMultipleObjects = serializedObject.isEditingMultipleObjects;
                if (!IsEditingMultipleObjects || (IsEditingMultipleObjects &&
                                                  PropertyHandlingExtensions.AllObjectTypesAreTheSame(
                                                      serializedObject.targetObjects)))
                {
                    TargetObject = serializedObject.targetObject;
                    InstanceID = TargetObject.GetInstanceID();
                    TargetType = TargetObject.GetType();
                    DeclaringObject = drawer.Property.GetDeclaringObject(); // TODO: IProperty.DeclaringObject
                    var targetDeclaringType = TargetType.DeclaringType;
                    Type fieldDeclaringType = null;
                    
                    if (drawer is PropertyDrawer propertyDrawer)
                    {
                        fieldDeclaringType = propertyDrawer.fieldInfo.DeclaringType;
                        
                    }
                    
                    Debug.Log($"<color=#000000ff><b>// ANALYZING :: InstanceID = {InstanceID}; " +
                              $"TargetType = {TargetType.Name}; TargetDeclaringType = {targetDeclaringType?.Name}; " +
                              $"FieldDeclaringType = {fieldDeclaringType?.Name}; </b></color>");

                    IsValid = true;
                }
            }
        }
    }
}