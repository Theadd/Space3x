using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.FieldFactories;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.Properties.Types.Editor;
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
        // public PropertyAttributeController Controller;

        protected override bool UpdateOnAnyValueChange => true;

        private bool m_IsReady;
        
        private List<BindablePropertyField> m_BindableFields = new List<BindablePropertyField>();
        
        private FieldFactoryExtender m_FieldFactory;

        private void Extend()
        {
            // if (Field.ClassListContains(UssConstants.UssAttributesExtended)) 
            //     DebugLog.Warning($"<color=#FF0000FF><b>Field: {Field.name} already has {UssConstants.UssAttributesExtended} " +
            //                      $"class list item! ThisHash: {this.GetHashCode()}</b></color>");
            var parentElement = Container.hierarchy.parent;
            if (parentElement.ClassListContains(UssConstants.UssFactoryPopulated))
            {
                // DebugLog.Info("Populated already by factory!");
                return;
            }

            m_FieldFactory ??= new FieldFactoryExtender(Property.GetController());
            m_FieldFactory.PropertyFieldOrigin = Field as PropertyField;
            m_FieldFactory.Rebuild(parentElement);
            // Field.WithClasses(UssConstants.UssAttributesExtended);
        }

        public override void OnAttachedAndReady(VisualElement element)
        {
            DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
            // Controller = PropertyAttributeController.GetInstance(Property);
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
            var instanceId = Property.GetController().InstanceID;
            
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
                // Controller = Property.GetController();
                // if (Controller == null)
                // {
                //     DebugLog.Error($"Controller is null at AllowExtendedAttributesDecorator.OnUpdate().  (ThisHash: {this.GetHashCode()})");
                //     return;
                // }
                Extend();
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
                if (Property is IPropertyWithSerializedObject propertyWithSerializedObject && propertyWithSerializedObject.Controller != null)
                    PropertyAttributeController.RemoveFromCache((PropertyAttributeController)propertyWithSerializedObject.Controller);

            base.OnReset(disposing);
        }
    }
}
