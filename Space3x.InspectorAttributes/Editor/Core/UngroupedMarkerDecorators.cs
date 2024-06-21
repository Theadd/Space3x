using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [InitializeOnLoad]
    public static class UngroupedMarkerDecorators
    {
        private static Dictionary<int, MarkerDecoratorsCache> s_Instances;

        private static HashSet<int> s_AutoDisableGroups;

        private static int s_ActiveSelectedObjectHash = 0;

        static UngroupedMarkerDecorators() => RegisterCallbacks(true);

        public static void SetAutoDisableGroupingWhenCreatingCachesInGroup(SerializedObject serializedObject, IPanel panel, bool autoDisable)
        {
            DebugLog.Info($"  -> SetAutoDisableGroupingWhenCreatingCachesInGroup(SerializedObject serializedObject, IPanel panel, autoDisable: {autoDisable})");
            SetAutoDisableGroupingWhenCreatingCachesInGroup(
                serializedObject.GetHashCode() * 397 ^ GetPanelContentHash(panel), 
                autoDisable);
        }
        
        private static void SetAutoDisableGroupingWhenCreatingCachesInGroup(int groupId, bool autoDisable)
        {
            SetupActiveSelection();
            if (autoDisable)
                s_AutoDisableGroups.Add(groupId);
            else
                s_AutoDisableGroups.Remove(groupId);
        }
        
        private static MarkerDecoratorsCache GetInstance(int instanceId, int groupId = 0)
        {
            SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new MarkerDecoratorsCache();
                DebugLog.Info($"<b>  -> MarkerDecoratorsCache {instanceId} on {groupId} <color=#FF0000FF>created</color>!</b> CACHE_ID: {value.GetHashCode()}");
                if (groupId != 0 && s_AutoDisableGroups.Contains(groupId))
                {
                    DebugLog.Info($"<b>    -> WITH DISABLED AUTO GROUPING!</b>");
                    value.DisableAutoGroupingOnActiveSelection(disable: true);
                }
                s_Instances.Add(instanceId, value);
            }

            return value;
        }
        
        // public MarkerDecoratorsCache DecoratorsCache => 
        //     CachedDecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(
        //         Field?.GetParentPropertyField()?.GetSerializedProperty()?.GetHashCode() ?? Property.GetSerializedObject().GetHashCode(),
        //         Property.GetSerializedObject().GetHashCode());
        
        public static MarkerDecoratorsCache GetInstance(IDrawer drawer)
        {
            if (drawer.Property is IPropertyWithSerializedObject property)
            {
                var panelId = GetPanelContentHash(drawer.GetPanel());
                var instanceId = property.SerializedObject.targetObject.GetInstanceID() * 397 
                                 ^ drawer.Property.ParentPath.GetHashCode();
                                 // * 397 ^ drawer.Property.PropertyPath.GetHashCode();
                DebugLog.Info($"  -> <color=#e15e50ff>(MDC) GetInstance(IDrawer {drawer.GetType().Name}) ..." +
                              $" [{drawer.Property.PropertyPath}]/[{drawer.Property.ParentPath}]/[{drawer.Property.Name}] " +
                              $"{instanceId} :: {panelId}</color>");
                return GetInstance(
                    instanceId * 397 ^ panelId, 
                    property.SerializedObject.GetHashCode() * 397 ^ panelId);
            }
            DebugLog.Error("<b>UNEXPECTED!</b> IDrawer.Property is not an IPropertyWithSerializedObject");
            return null;
        }
        
        public static MarkerDecoratorsCache GetInstance(PropertyField propertyField, SerializedProperty fallbackProperty = null, bool propertyContainsChildrenProperties = false)
        {
            var propertyNode = propertyField.GetPropertyNode() 
                               ?? (fallbackProperty == null 
                                   ? null 
                                   : PropertyAttributeController.GetInstance(fallbackProperty)?.GetProperty(fallbackProperty.name));
            if (propertyNode is IPropertyWithSerializedObject property)
            {
                var panelId = GetPanelContentHash(propertyField.panel);
                var instanceId = property.SerializedObject.targetObject.GetInstanceID() * 397 
                                 ^ (propertyContainsChildrenProperties ? propertyNode.PropertyPath : propertyNode.ParentPath).GetHashCode();
                DebugLog.Info($"  -> <color=#FF7F00FF>MarkerDecoratorsCache GetInstance(" + 
                              $"PropertyField: [{propertyNode.PropertyPath}]/[{propertyNode.ParentPath}]/[{propertyNode.Name}], " + 
                              $"SerializedProperty fallbackProperty = " + 
                              $"{(fallbackProperty == null ? "null" : "`" + fallbackProperty.propertyPath + "`")})</color>");
                return GetInstance(instanceId * 397 ^ panelId);
            }
            DebugLog.Error("<b><color=#FF0000FF>UNEXPECTED!</color></b> PropertyField.Property is not an IPropertyWithSerializedObject");
            return null;
            // var instanceIdX = propertyField.GetSerializedProperty()?.GetHashCode() ?? fallbackProperty?.GetHashCode() ?? 0;
            // return instanceIdX == 0 ? null : GetInstance(instanceIdX * 397 ^ GetPanelContentHash(propertyField.panel));
        }

        // public static MarkerDecoratorsCache GetInstance(SerializedProperty property, IPanel panel)
        // {
        //     // public static IProperty GetPropertyNode(this VisualElement element)
        //     return null;
        // }
        
        public static MarkerDecoratorsCache GetInstance(SerializedObject serializedObject, IPanel panel)
        {
            DebugLog.Error($"<b><color=#FF0000FF>THIS METHOD IS NOT PROPERLY IMPLEMENTED, PROBABLY BUGGED, PLEASE FIX</color></b>");
            DebugLog.Info("  -> MarkerDecoratorsCache GetInstance(SerializedObject serializedObject, IPanel panel) #PATH: ");
            var id = serializedObject.GetHashCode() * 397 ^ GetPanelContentHash(panel);
            return GetInstance(id, id);
        }
        
        // public static MarkerDecoratorsCache GetInstance(PropertyField propertyField, SerializedProperty fallbackProperty = null)
        // {
        //     var prop = propertyField.GetSerializedProperty();
        //     var pNode = propertyField.GetPropertyNode();
        //     
        //     DebugLog.Info($"  -> <color=#FF7F00FF>MarkerDecoratorsCache GetInstance(" +
        //                   $"PropertyField [propPath: `{prop.propertyPath}`, propName: `{prop.name}`, pNode.PropertyPath:" +
        //                   $" [{pNode.PropertyPath}]/[{pNode.ParentPath}]/[{pNode.Name}], " +
        //                   $"SerializedProperty fallbackProperty = " +
        //                   $"{(fallbackProperty == null ? "null" : "`" + fallbackProperty.propertyPath + "`")})</color>");
        //     var instanceId = propertyField.GetSerializedProperty()?.GetHashCode() ?? fallbackProperty?.GetHashCode() ?? 0;
        //     return instanceId == 0 ? null : GetInstance(instanceId * 397 ^ GetPanelContentHash(propertyField.panel));
        // }
        
        private static int GetPanelContentHash(IPanel panel) =>
            panel?.visualTree is VisualElement { childCount: >= 2 } vPanel
                ? vPanel[1].GetHashCode()
                : 0;

        // TODO: Get hash of multiple selected objects.
        private static int GetActiveSelectionHash() =>
            Selection.activeObject != null ? Selection.activeObject.GetHashCode() : 0;

        private static void SetupActiveSelection()
        {
            var hash = GetActiveSelectionHash();
            if (s_ActiveSelectedObjectHash == hash)
                return;

            s_ActiveSelectedObjectHash = hash;
            ClearCache();
        }

        private static void RegisterCallbacks(bool register)
        {
            DebugLog.Warning($"  -> <color=#FF7F00FF>UngroupedMarkerDecorators.RegisterCallbacks({register})</color> (AKA: RESETTING CACHES)");
            s_Instances = new Dictionary<int, MarkerDecoratorsCache>();
            s_AutoDisableGroups = new HashSet<int>();
            Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged() => SetupActiveSelection();

        private static void ClearCache()
        {
            DebugLog.Warning($"  -> <color=#FF7F00FF>UngroupedMarkerDecorators.ClearCache() s_ActiveSelectedObjectHash = {s_ActiveSelectedObjectHash}</color>");
            s_Instances.Clear();
            s_AutoDisableGroups.Clear();
        }
    }
}
