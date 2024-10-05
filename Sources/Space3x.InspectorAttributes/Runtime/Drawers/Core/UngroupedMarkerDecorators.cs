using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
    public static class UngroupedMarkerDecorators
    {
        private static Dictionary<int, MarkerDecoratorsCache> s_Instances;

        // private static HashSet<int> s_AutoDisableGroups;

        private static int s_ActiveSelectedObjectHash = 0;
        
        // TODO: Remove
        internal static MarkerDecoratorsCache[] GetAllInstances() => s_Instances.Values.ToArray();
        
        private static MarkerDecoratorsCache GetInstance(int instanceId, int groupId = 0)
        {
            SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new MarkerDecoratorsCache();
                // if (groupId != 0 && s_AutoDisableGroups.Contains(groupId))
                //     value.DisableAutoGroupingOnActiveSelection(disable: true);
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
#if UNITY_EDITOR
                    ((object)drawer.Property.GetSerializedObject() ?? drawer.Property.GetTargetObject()).GetHashCode() * 397 ^ panelId);
#else
                   ((object)drawer.Property.GetTargetObject()).GetHashCode() * 397 ^ panelId);
#endif
            }
            return null;
        }
        
        private static int GetPanelContentHash(IPanel panel) =>
            panel?.visualTree is VisualElement { childCount: >= 2 } vPanel
                ? vPanel[1].GetHashCode()
                : 0;

#if UNITY_EDITOR
        // TODO: Get hash of multiple selected objects.
        private static int GetActiveSelectionHash() =>
            UnityEditor.Selection.activeObject != null ? UnityEditor.Selection.activeObject.GetHashCode() : 0;

        private static void RegisterCallbacks(bool register)
        {
            if (s_Instances != null)
                ClearCache();
            else
            {
                s_Instances = new Dictionary<int, MarkerDecoratorsCache>();
                // s_AutoDisableGroups = new HashSet<int>();
            }
            if (Application.isPlaying) return;
            UnityEditor.Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                UnityEditor.Selection.selectionChanged += OnSelectionChanged;
        }
#else
        private static int GetActiveSelectionHash() => 0;

        private static void RegisterCallbacks(bool register)
        {
            if (s_Instances != null)
                ClearCache();
            else
            {
                s_Instances = new Dictionary<int, MarkerDecoratorsCache>();
                // s_AutoDisableGroups = new HashSet<int>();
            }
        }
#endif
        
        private static void SetupActiveSelection()
        {
            if (Application.isPlaying) return;
            var hash = GetActiveSelectionHash();
            if (s_ActiveSelectedObjectHash == hash)
                return;

            s_ActiveSelectedObjectHash = hash;
            ClearCache();
        }
        
        private static void OnSelectionChanged() => SetupActiveSelection();

        internal static void ClearCache()
        {
            DebugLog.Notice(nameof(UngroupedMarkerDecorators) + ".ClearCache()!");
            try
            {
                var instances = s_Instances.Values.ToArray();
                for (var i = 0; i < instances.Length; i++)
                {
                    instances[i].Dispose();
                }
            } catch (Exception ex) { Debug.LogException(ex); }
            s_Instances.Clear();
            // s_AutoDisableGroups.Clear();
        }
        
        internal static void ReloadAll()
        {
            s_ActiveSelectedObjectHash = 0;
            RegisterCallbacks(true);
        }
    }
}
