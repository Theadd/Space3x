using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [InitializeOnLoad]
    public static class UngroupedMarkerDecorators
    {
//        private static List<IGroupMarkerDecorator> s_CachedInstances;
//        private static List<IGroupMarkerDecorator> s_PendingInstances;

        private static Dictionary<int, MarkerDecoratorsCache> s_Instances;

        private static HashSet<int> s_AutoDisableGroups;

        private static int s_ActiveSelectedObjectHash = 0;
        
//        private static bool s_DisableAutoGrouping = false;
//
//        private static int s_AutoGroupingDisabledForHash = 0;
        
        static UngroupedMarkerDecorators() => RegisterCallbacks(true);

        public static void SetAutoDisableGroupingWhenCreatingCachesInGroup(int groupId, bool autoDisable)
        {
            SetupActiveSelection();
            if (autoDisable)
                s_AutoDisableGroups.Add(groupId);
            else
                s_AutoDisableGroups.Remove(groupId);
        }
        
        public static MarkerDecoratorsCache GetInstance(int instanceId, int groupId = 0)
        {
            SetupActiveSelection();
            if (!s_Instances.TryGetValue(instanceId, out var value))
            {
                value = new MarkerDecoratorsCache();
                if (groupId != 0 && s_AutoDisableGroups.Contains(groupId))
                    value.DisableAutoGroupingOnActiveSelection(disable: true);
                s_Instances.Add(instanceId, value);
                Debug.Log($"<color=#1ba1e2ff><b>MarkerDecoratorsCache.CreateInstance({instanceId});</b> Count = {s_Instances.Count}</color>");
            }

            return value;
        }

        private static int GetActiveSelectionHash()
        {
            var o = Selection.activeObject;
            return o != null ? o.GetHashCode() : 0;
        }

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
            Debug.Log("<color=#000000FF><b>@UngroupedMarkerDecorators.RegisterCallbacks</b></color>");
            s_Instances = new Dictionary<int, MarkerDecoratorsCache>();
            s_AutoDisableGroups = new HashSet<int>();
            Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged() => SetupActiveSelection();

        private static void ClearCache()
        {
            Debug.Log("<color=#000000FF><b>@UngroupedMarkerDecorators.ClearCache</b></color>");
            s_Instances.Clear();
            s_AutoDisableGroups.Clear();
        }
    }
}
