using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AllowExtendedAttributes), useForChildren: false)]
    public class AllowExtendedAttributesDecorator : Decorator<AutoDecorator, AllowExtendedAttributes>
    {
        public PropertyAttributeController Controller;

        protected override bool UpdateOnAnyValueChange => true;

        private bool m_IsReady;

        private void PopulateNonSerializedProperties()
        {
            if (Container == null)
                DebugLog.Error($"Container is null, Property is null? {Property == null} {this.GetHashCode()}");
            var parentElement = Container.hierarchy.parent;
            var allFields = new Dictionary<string, VisualElement>();
            var allChildren = parentElement.hierarchy.Children().ToList();
            for (var i = 0; i < allChildren.Count; i++)
            // foreach (var child in parentElement.hierarchy.Children())
            {
                var child = allChildren.ElementAt(i);
                if (child is PropertyField childField)
                {
                    var childProp = childField.GetSerializedProperty();
                    var shouldRebind = childProp == null;
                    if (!shouldRebind)
                    {
                        try
                        {
                            shouldRebind = !childField.name.EndsWith(childProp.name);
                        }
                        catch (ObjectDisposedException _)
                        {
                            shouldRebind = true;
                        }
                    }
                    if (shouldRebind)
                    {
                        childField.Unbind();
                        childField.Bind(Property.GetSerializedObject());
                        childProp = childField.GetSerializedProperty();
                        DebugLog.Info($"<color=#FFFF00FF>{childField.name} REBOUND, success: {childProp != null}</color>");
                    }
                    if (childProp != null)
                    {
                        try
                        {
                            allFields.Add(childProp.name, childField);
                        }
                        catch (Exception ex)
                        {
                            DebugLog.Error(ex.ToString() + "\n\n" + ex.StackTrace);
                            DebugLog.Warning(string.Join("\n", allFields.Keys));
                        }
                    }
                }
            }
            VisualElement previousField = null;
            for (var i = 0; i < Controller.Properties.Values.Count; i++)
            {
                var prop = Controller.Properties.Values[i];
                if (prop is SerializedPropertyNode serializedNode)
                {
                    if (allFields.TryGetValue(serializedNode.Name, out VisualElement targetField))
                    {
                        previousField = targetField;
                        // serializedNode.Field = targetField;
                    }
                    else
                        Debug.LogWarning($"No PropertyField found for {serializedNode.Name}.");
                }
                else if (prop is NonSerializedPropertyNode nonSerializedNode)
                {
                    var bindableField = new BindablePropertyField();
                    // nonSerializedNode.Field = bindableField;
                    bindableField.BindProperty(nonSerializedNode, applyCustomDrawers: true);
                    previousField.AddAfter(bindableField);
                    bindableField.AttachDecoratorDrawers();
                    previousField = bindableField;
                }
            }
        }
        
        public override void OnAttachedAndReady(VisualElement element)
        {
            Controller = PropertyAttributeController.GetInstance(Property);
            #if SPACE3X_DEBUG
            element.Add(new Button(() => OnClick()) { text = "DEBUG ME!" });
            element.SetVisible(true);
            #endif
        }
        
        public int GetPanelVisualTreeHashCode() =>
            Container.panel?.visualTree is VisualElement { childCount: >= 2 } vPanel
                ? vPanel[1].GetHashCode()
                : 0;

        private void OnClick()
        {
            LogActiveEditors();
            // if (Container.panel?.visualTree is VisualElement { childCount: >= 2 } vPanel)
            // {
            //     var vRoot = vPanel[1];
            //     DebugLog.Info($"<b>vRoot: {vRoot.GetHashCode()} ({vRoot.name})</b>");
            // }
            //     
            // var vTree = this.Container.panel.visualTree;
            // var hierarchyChilds = vTree.hierarchy.childCount;
            // var contContainer = vTree.contentContainer;
            // var contContainerChilds = contContainer.childCount;
            // DebugLog.Info($"Hierarchy: {hierarchyChilds}, ContentContainer: {contContainerChilds}, contContainerName: {contContainer.name}");
            // DebugLog.Info($"Panel: {this.Container.panel.GetHashCode()}, VisualTree: {this.Container.panel.visualTree.GetHashCode()} ({this.Container.panel.visualTree.name})");
        }

        private void LogActiveEditors()
        {
            var editors = ActiveEditorTracker.sharedTracker.activeEditors;
            var instanceId = Controller.InstanceID;
            var xProp = Controller.GetProperty(Property.Name);
            
            for (var index = 0; index < editors.Length; index++)
            {
                var editor = editors[index];
                var numTargets = editor.targets.Length;
                var targetInstanceId = editor.target.GetInstanceID();
                Debug.LogWarning($"#{index} (nºTargets: {numTargets}) - {editor.target.name} - InstanceID: {instanceId} - TargetInstanceID: {targetInstanceId} - " +
                                 $"{editor.serializedObject.GetType().Name} - {editor.GetType().Name} - {editor.serializedObject.GetHashCode()}");
            }
        }

        public override void OnUpdate()
        {
            if (!m_IsReady)
            {
                m_IsReady = true;
                PopulateNonSerializedProperties();
            }
        }

        public override void OnReset(bool disposing = false)
        {
            if (disposing)
                if (Controller != null)
                    PropertyAttributeController.RemoveFromCache(Controller);

            Controller = null;
            base.OnReset(disposing);
        }
    }
}
