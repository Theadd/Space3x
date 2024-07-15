using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
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
        
        private List<BindablePropertyField> m_BindableFields = new List<BindablePropertyField>();

        private SerializedProperty RebindPropertyFieldIfNecessary(PropertyField propertyField)
        {
            var childProp = propertyField.GetSerializedProperty();
            var shouldRebind = childProp == null;
            if (!shouldRebind)
            {
                try
                {
                    shouldRebind = !propertyField.name.EndsWith(childProp.name);
                }
                catch (Exception)   // catch (ObjectDisposedException _)
                {
                    shouldRebind = true;
                }
            }

            if (!shouldRebind)
            {
                if (propertyField.hierarchy.childCount > 0)
                {
                    if (propertyField.hierarchy[0] is VisualElement decoratorsContainer && decoratorsContainer.ClassListContains("unity-decorator-drawers-container")) {
                        for (var i = decoratorsContainer.hierarchy.childCount - 1; i >= 0; i--)
                        {
                            if (decoratorsContainer.hierarchy[i] is GhostDecorator ghostDecorator)
                            {
                                if (!ghostDecorator.TargetDecorator.HasValidContainer())
                                {
                                    shouldRebind = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                    shouldRebind = true;
            }
            if (shouldRebind)
            {
                propertyField.Unbind();
                propertyField.Bind(Property.GetSerializedObject());
                childProp = propertyField.GetSerializedProperty();
                DebugLog.Info($"<color=#FFFF00FF>{propertyField.name} REBOUND, success: {childProp != null}</color>");
            }
            // "unity-decorator-drawers-container"
            return childProp;
        }
        
        private void PopulateNonSerializedProperties()
        {
            // var parentPath = Property?.ParentPath ?? "";
            DebugLog.Info($"<color=#00FF00FF>IN !! AllowExtendedAttributesDecorator.PopulateNonSerializedProperties: {Property?.ParentPath}; m_BindableFields.Count: {m_BindableFields.Count}</color>");
            var parentElement = Container.hierarchy.parent;
            var allFields = new Dictionary<string, VisualElement>();
            var allChildren = parentElement.hierarchy.Children().ToList();
            for (var i = 0; i < allChildren.Count; i++)
            // foreach (var child in parentElement.hierarchy.Children())
            {
                var child = allChildren.ElementAt(i);
                if (child is PropertyField childField)
                {
                    var childProp = RebindPropertyFieldIfNecessary(childField);
                    if (childProp != null)
                    {
                        try
                        {
                            allFields.Add(childProp.name, childField);
                            // DebugLog.Info($"    <color=#00FF00FF>{childProp.name} ADDED ON {parentPath}</color>");
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
                // DebugLog.Info($"  <color=#FFFF00FF>REQUESTING {prop.Name} ON: {parentPath}</color> ({prop.PropertyPath})");
                if (prop is SerializedPropertyNodeBase serializedNode)
                {
                    if (allFields.TryGetValue(serializedNode.Name, out VisualElement targetField))
                    {
                        if (serializedNode.HasChildren() && targetField is PropertyField propertyField)
                        {
                            propertyField.TrackPropertyValue(serializedNode, OnPropertyValueChanged);
                        }
                        previousField = targetField;
                        // DebugLog.Info($"    <color=#66FF66FF>{serializedNode.Name} SYNCED as SERIALIZED ON: {parentPath}</color> ({serializedNode.PropertyPath})");
                        // serializedNode.Field = targetField;
                    }
                    else
                        Debug.LogWarning($"No PropertyField found for {serializedNode.Name}.");
                }
                else if (prop is NonSerializedPropertyNodeBase nonSerializedNode)
                {
                    var bindableField = new BindablePropertyField();
                    // nonSerializedNode.Field = bindableField;
                    bindableField.BindProperty(nonSerializedNode, applyCustomDrawers: true);
                    previousField.AddAfter(bindableField);
                    bindableField.AttachDecoratorDrawers();
                    previousField = bindableField;
                    m_BindableFields.Add(bindableField);
                    if (nonSerializedNode.HasChildren())
                        bindableField.TrackPropertyValue(nonSerializedNode, OnPropertyValueChanged);
                    // DebugLog.Info($"    <color=#66FF66FF>{nonSerializedNode.Name} SYNCED as <b>NON</b>-SERIALIZED ON: {parentPath}</color> ({nonSerializedNode.PropertyPath})");
                }
            }
        }

        private void OnPropertyValueChanged(IProperty property)
        {
            DebugLog.Info($"<color=#00FF00FF>Tracked change on {property.GetType().Name}: {property.PropertyPath}</color>");
            PropertyAttributeController.OnPropertyValueChanged(property);
        }
        
        public override void OnAttachedAndReady(VisualElement element)
        {
            DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
            Controller = PropertyAttributeController.GetInstance(Property);
            #if SPACE3X_DEBUG
            element.Add(new Button(() => OnClick()) { text = "DEBUG ME!" });
            element.Add(new Button(OnClickAddDevTooltips) { text = "Dev Tooltips" });
            element.SetVisible(true);
            #endif
        }

        private void OnClickAddDevTooltips()
        {
            foreach (var element in Container.hierarchy.parent.GetChildrenFields())
            {
                element.tooltip = element.AsString();
                DebugLog.Info(element.tooltip);
            }
        }

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
                Controller ??= PropertyAttributeController.GetInstance(Property);
                // DebugLog.Info("[ROOT] BEFORE PopulateNonSerializedProperties() ON " + Property.PropertyPath + ": Container.AsHierarchyString(depth: 10)");
                // DebugLog.Info("[ROOT] " + Container.hierarchy.parent.AsHierarchyString(depth: 10));
                PopulateNonSerializedProperties();
                DebugLog.Warning($"[ROOT] # ONE !!!!!!!!!!!!!!!!!! {Property.PropertyPath}");
                // EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
                // {
                //     DebugLog.Warning($"[ROOT] # DELAYED !!!!!!!!!!!!!!!!!! {Property.PropertyPath}");
                //     
                //     DebugLog.Info("[ROOT] BEFORE RebuildAll() ON " + Property.PropertyPath + ": Container.AsHierarchyString(depth: 10)");
                //     DebugLog.Info("[ROOT] " + Container.hierarchy.parent.AsHierarchyString(depth: 10));
                    DecoratorsCache.RebuildAll();
                    DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                    // DebugLog.Info("[ROOT] BEFORE HandlePendingDecorators() ON " + Property.PropertyPath + ": Container.AsHierarchyString(depth: 10)");
                    // DebugLog.Info("[ROOT] " + Container.hierarchy.parent.AsHierarchyString(depth: 10));
                    DecoratorsCache.HandlePendingDecorators();
                //     DebugLog.Info("[ROOT] AFTER Everything! ON " + Property.PropertyPath + ": Container.AsHierarchyString(depth: 10)");
                //     DebugLog.Info("[ROOT] " + Container.hierarchy.parent.AsHierarchyString(depth: 10));
                // });
                DebugLog.Warning($"[ROOT] # DELAYED END !!!!!!!!!!!!!!!!!! {Property.PropertyPath}");
            }
        }
        
        private void RemoveAllBindableFields()
        {
            for (var index = m_BindableFields.Count - 1; index >= 0; index--)
            {
                var bindableField = m_BindableFields[index];
                bindableField.ProperlyRemoveFromHierarchy();
            }

            m_BindableFields.Clear();
        }

        public override void OnReset(bool disposing = false)
        {
            DebugLog.Error($"<b> ========== ON RESET ========= </b> disposing: {disposing}, ParentPath: {Property?.ParentPath}");
            
            if (!disposing)
            {
                m_IsReady = false;
                RemoveAllBindableFields();
            }
            
            if (disposing)
                if (Controller != null)
                    PropertyAttributeController.RemoveFromCache(Controller);

            Controller = null;
            base.OnReset(disposing);
        }
    }
}
