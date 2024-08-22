using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
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
                if (propertyField == Field)
                    return childProp;
                
                try
                {
                    // EDIT: shouldRebind = !propertyField.name.EndsWith(childProp.name);
                    shouldRebind = !(propertyField.bindingPath.EndsWith(childProp.name) || propertyField.name.EndsWith(childProp.name));
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
                // DebugLog.Info($"<color=#FFFF00FF>{propertyField.name} REBOUND, success: {childProp != null}</color>");
            }
            // "unity-decorator-drawers-container"
            return childProp;
        }
        
        private void PopulateNonSerializedProperties()
        {
            // var parentPath = Property?.ParentPath ?? "";
            // DebugLog.Info($"<color=#00FF00FF>IN !! AllowExtendedAttributesDecorator.PopulateNonSerializedProperties: {Property?.ParentPath}; m_BindableFields.Count: {m_BindableFields.Count}</color>");
            if (Field.ClassListContains(UssConstants.UssAttributesExtended))
            {
                DebugLog.Warning($"<color=#FF0000FF><b>Field: {Field.name} already has {UssConstants.UssAttributesExtended} class list item! ThisHash: {this.GetHashCode()}</b></color>");
                // EDIT: return;
            }
            var parentElement = Container.hierarchy.parent;
            if (parentElement.ClassListContains(UssConstants.UssFactoryPopulated))
            {
                DebugLog.Info("Populated already by factory!");
                return;
            }
            var allFields = new Dictionary<string, VisualElement>();
            var allChildren = parentElement.hierarchy.Children().ToList();
            for (var i = 0; i < allChildren.Count; i++)
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
                if (string.IsNullOrEmpty(prop.Name)) continue;
                // DebugLog.Info($"  <color=#FFFF00FF>REQUESTING {prop.Name} ON: {parentPath}</color> ({prop.PropertyPath})");
                if (prop is SerializedPropertyNodeBase serializedNode)
                {
                    if (allFields.TryGetValue(serializedNode.Name, out VisualElement targetField))
                    {
                        if (serializedNode.HasChildren() && !serializedNode.IsArrayOrList() && targetField is PropertyField propertyField)
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
                else if (prop is NonSerializedPropertyNodeBase nonSerializedNode && nonSerializedNode.IncludeInInspector())
                {
                    var bindableField = new BindablePropertyField();
                    // nonSerializedNode.Field = bindableField;
                    bindableField.WithClasses(nonSerializedNode.ShowInInspector(), UssConstants.UssShowInInspector);
                    bindableField.BindProperty(nonSerializedNode, applyCustomDrawers: true);
                    previousField.AddAfter(bindableField);
                    bindableField.AttachDecoratorDrawers();
                    previousField = bindableField;
                    m_BindableFields.Add(bindableField);
                    if (nonSerializedNode.HasChildren() && !nonSerializedNode.IsArrayOrList())
                        bindableField.TrackPropertyValue(nonSerializedNode, OnPropertyValueChanged);
                    // DebugLog.Info($"    <color=#66FF66FF>{nonSerializedNode.Name} SYNCED as <b>NON</b>-SERIALIZED ON: {parentPath}</color> ({nonSerializedNode.PropertyPath})");
                }
                else if (prop is InvokablePropertyNodeBase invokableNode && invokableNode.IncludeInInspector())
                {
                    var invokableField = new BindablePropertyField();
                    invokableField.WithClasses(invokableNode.ShowInInspector(), UssConstants.UssShowInInspector);
                    invokableField.BindProperty(invokableNode, applyCustomDrawers: true);
                    previousField.AddAfter(invokableField);
                    invokableField.AttachDecoratorDrawers();
                    previousField = invokableField;
                    m_BindableFields.Add(invokableField);
                    // if (invokableNode.HasChildren())
                    //     invokableField.TrackPropertyValue(invokableNode, OnPropertyValueChanged);
                    DebugLog.Info($"    <color=#66FF66FF>{invokableNode.Name} SYNCED as <b>INVOKABLE PROPERTY NODE</b> ON: {invokableNode.ParentPath}</color> ({invokableNode.PropertyPath})");
                }
            }
            Field.WithClasses(UssConstants.UssAttributesExtended);
        }

        private void OnPropertyValueChanged(IPropertyNode property)
        {
            PropertyAttributeController.OnPropertyValueChanged(property);
        }
        
        public override void OnAttachedAndReady(VisualElement element)
        {
            DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
            Controller = PropertyAttributeController.GetInstance(Property);
#if SPACE3X_DEBUG
            element.Add(new Button(() => OnClick()) { text = "1", tooltip = "DEBUG ME!", style = { fontSize = 8 } });
            element.Add(new Button(OnClickAddDevTooltips) { text = "2", tooltip = "Dev Tooltips", style = { fontSize = 8 } });
            element.SetVisible(true);
            element.style.flexWrap = Wrap.Wrap;
            element.style.left = -16;
            element.style.position = Position.Absolute;
            element.style.maxWidth = 4;
            element.style.flexDirection = FlexDirection.Row;
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
            DebuggingUtility.ShowAllControllers();
            var str = $"<b><u>ALL PARENT PROPERTIES:</u> {Property.PropertyPath}</b>\n";
            foreach (var parentProperty in Property.GetAllParentProperties(false))
            {
                str += $"\t'<b>{parentProperty.Name}</b>' @ '{parentProperty.ParentPath}' ({parentProperty.PropertyPath})\n";
            }
            DebugLog.Info(str);

            var underlyingValue = Property.GetUnderlyingValue();
            Debug.Log(underlyingValue);
            var parent = Property.GetParentProperty();
            if (parent != null)
            {
                var uValueParent = parent.GetUnderlyingValue();
                Debug.Log($"<u>{uValueParent}</u>: {uValueParent} ({uValueParent.GetType().Name})");
                if (parent is IPropertyNodeIndex propertyNodeIndex)
                {
                    Debug.Log($"  Index: {propertyNodeIndex.Index}");
                }
            }

        }
        
        private void PreviousVersionOfOnClick()
        {
            var allControllers = PropertyAttributeController.GetAllInstances();
            foreach (var controller in allControllers)
            {
                Debug.Log(controller.InstanceID + " - " + controller.ParentPath);
            }

            var allSheets = Resources.FindObjectsOfTypeAll<StyleSheet>();
            
            foreach (var sheet in allSheets)
            {
                Debug.Log("  - StyleSheet: " + sheet.GetHashCode() + " - " + sheet.name);
            }
            
            var allAssemblies = Resources.FindObjectsOfTypeAll<AssemblyDefinitionAsset>();
            
            foreach (var assembly in allAssemblies)
            {
                Debug.Log("  - AssemblyDefinitionAsset: " + assembly.GetHashCode() + " - " + assembly.name);
            }
            
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
                if (Controller == null)
                {
                    DebugLog.Error($"Controller is null at AllowExtendedAttributesDecorator.OnUpdate().  (ThisHash: {this.GetHashCode()})");
                    return;
                }
                PopulateNonSerializedProperties();
                DecoratorsCache.RebuildAll();
                DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                DecoratorsCache.HandlePendingDecorators();
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
