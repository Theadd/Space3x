using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
    [InitializeOnLoad]
    public static class UngroupedMarkerDecorators
    {
        private static Dictionary<int, MarkerDecoratorsCache> s_Instances;

        private static HashSet<int> s_AutoDisableGroups;

        private static int s_ActiveSelectedObjectHash = 0;

        static UngroupedMarkerDecorators() => RegisterCallbacks(true);

        // public static void SetAutoDisableGroupingWhenCreatingCachesInGroup(SerializedObject serializedObject, IPanel panel, bool autoDisable) =>
        //     SetAutoDisableGroupingWhenCreatingCachesInGroup(
        //         serializedObject.GetHashCode() * 397 ^ GetPanelContentHash(panel), 
        //         autoDisable);
        //
        // private static void SetAutoDisableGroupingWhenCreatingCachesInGroup(int groupId, bool autoDisable)
        // {
        //     SetupActiveSelection();
        //     if (autoDisable)
        //         s_AutoDisableGroups.Add(groupId);
        //     else
        //         s_AutoDisableGroups.Remove(groupId);
        // }
        
        private static MarkerDecoratorsCache GetInstance(int instanceId, int groupId = 0)
        {
            SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new MarkerDecoratorsCache();
                if (groupId != 0 && s_AutoDisableGroups.Contains(groupId))
                    value.DisableAutoGroupingOnActiveSelection(disable: true);
                s_Instances.Add(instanceId, value);
            }

            return value;
        }
        
        public static MarkerDecoratorsCache GetInstance(IDrawer drawer)
        {
            if (drawer.Property is IControlledProperty property)
            {
                var panelId = GetPanelContentHash(drawer.GetPanel());
                var instanceId = drawer.Property.GetTargetObject().GetInstanceID() * 397 
                                 ^ drawer.Property.ParentPath.GetHashCode();
                return GetInstance(
                    instanceId * 397 ^ panelId, 
                    ((object)drawer.Property.GetSerializedObject() ?? drawer.Property.GetTargetObject()).GetHashCode() * 397 ^ panelId);
            }
            DebugLog.Error("<b>UNEXPECTED!</b> IDrawer.Property is not an IControlledProperty");
            return null;
        }
        
        public static MarkerDecoratorsCache GetInstance(PropertyField propertyField, SerializedProperty fallbackProperty = null, bool propertyContainsChildrenProperties = false)
        {
            var propertyNode = propertyField.GetPropertyNode() 
                               ?? (fallbackProperty == null 
                                   ? null 
                                   : PropertyAttributeController.GetInstance(fallbackProperty)?.GetProperty(fallbackProperty.name));
            if (propertyNode is IControlledProperty property)
            {
                var panelId = GetPanelContentHash(propertyField.panel);
                var instanceId = propertyNode.GetTargetObject().GetInstanceID() * 397 
                                 ^ (propertyContainsChildrenProperties ? propertyNode.PropertyPath : propertyNode.ParentPath).GetHashCode();
                return GetInstance(instanceId * 397 ^ panelId);
            }
            DebugLog.Error("<b><color=#FF0000FF>UNEXPECTED!</color></b> related IPropertyNode is not an IControlledProperty");
            return null;
        }
        
        // public static MarkerDecoratorsCache GetInstance(SerializedObject serializedObject, IPanel panel)
        // {
        //     DebugLog.Error($"<b><color=#FF0000FF>THIS METHOD IS NOT PROPERLY IMPLEMENTED, PROBABLY BUGGED, PLEASE FIX</color></b>");
        //     DebugLog.Info("  -> MarkerDecoratorsCache GetInstance(SerializedObject serializedObject, IPanel panel) #PATH: ");
        //     var id = serializedObject.GetHashCode() * 397 ^ GetPanelContentHash(panel);
        //     return GetInstance(id, id);
        // }
        
        private static int GetPanelContentHash(IPanel panel) =>
            panel?.visualTree is VisualElement { childCount: >= 2 } vPanel
                ? vPanel[1].LogThis("UngroupedMarkerDecorators.GetPanelContentHash()").GetHashCode()
                : LogAndReturn($"[UMD!] GetPanelContentHash({((VisualElement)panel?.visualTree)?.AsString()})", 0);
        
        // TODO: remove
        private static int LogAndReturn(string msg, int returnValue)
        {
            Debug.Log($"<color=#FF7F00FF>[UMD!] LogAndReturn (<b>{returnValue}</b>): {msg}</color>");
            return returnValue;
        }

        // TODO: Get hash of multiple selected objects.
        private static int GetActiveSelectionHash() =>
            Selection.activeObject != null ? Selection.activeObject.GetHashCode() : 0;

        private static void SetupActiveSelection()
        {
            var hash = GetActiveSelectionHash();
            if (s_ActiveSelectedObjectHash == hash)
                return;

            Debug.Log($"<color=#FF7F00FF>[UMD!] UngroupedMarkedDecorators.<b>ClearCache()</b> -> {s_ActiveSelectedObjectHash} != {hash}");
            s_ActiveSelectedObjectHash = hash;
            ClearCache();
        }

        private static void RegisterCallbacks(bool register)
        {
            s_Instances = new Dictionary<int, MarkerDecoratorsCache>();
            s_AutoDisableGroups = new HashSet<int>();
            Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged() => SetupActiveSelection();

        private static void ClearCache()
        {
            s_Instances.Clear();
            s_AutoDisableGroups.Clear();
        }
    }
}
