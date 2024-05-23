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
        private static List<IGroupMarkerDecorator> s_CachedInstances;
        private static List<IGroupMarkerDecorator> s_PendingInstances;

        private static int s_ActiveSelectedObjectHash = 0;
        
        private static bool s_DisableAutoGrouping = false;

        private static int s_AutoGroupingDisabledForHash = 0;
        
        static UngroupedMarkerDecorators() => RegisterCallbacks(true);

        public static void DisableAutoGroupingOnActiveSelection(bool disable = true)
        {
            SetupActiveSelection();
            s_AutoGroupingDisabledForHash = s_ActiveSelectedObjectHash;
            s_DisableAutoGrouping = disable;
        }
        
        public static bool IsAutoGroupingDisabled() => s_DisableAutoGrouping && s_AutoGroupingDisabledForHash == GetActiveSelectionHash();
        
        public static void Add(IGroupMarkerDecorator decorator)
        {
            SetupActiveSelection();
            if (!s_CachedInstances.Contains(decorator))
            {
                var o = decorator.GetSerializedObject();
                Debug.Log($"<b>  ![ADD] {decorator.Property.propertyPath} ({((DecoratorDrawer) decorator).attribute.GetType().Name}) " +
                          $"@ {o.GetHashCode()} :: {o.targetObject.name} {o.targetObject.GetHashCode()}</b>");
                s_CachedInstances.Add(decorator);
            }
        }

        public static void Remove(IGroupMarkerDecorator decorator)
        {
            if (s_CachedInstances.Contains(decorator))
                s_CachedInstances.Remove(decorator);
            if (s_PendingInstances.Contains(decorator))
                s_PendingInstances.Remove(decorator);
        }

        public static void MarkPending(IGroupMarkerDecorator decorator)
        {
            if (!s_PendingInstances.Contains(decorator))
                s_PendingInstances.Add(decorator);
        }

        public static int Count() => s_CachedInstances.Count;
        
        public static bool HasOnlyPending()
        {
            if (s_PendingInstances.Count > 0 && s_PendingInstances.Count == s_CachedInstances.Count)
            {
                foreach (var groupMarkerDecorator in s_PendingInstances)
                {
                    if (!s_CachedInstances.Contains(groupMarkerDecorator))
                        return false;
                }
                return true;
            }

            return false;
        }

        public static bool TryGet(Func<IGroupMarkerDecorator, bool> predicate)
        {
            var isValid = false;
            var index = 0;

            for (; index < s_CachedInstances.Count; index++)
            {
                var cachedInstance = s_CachedInstances[index];
                if (predicate(cachedInstance))
                {
                    isValid = true;
                    break;
                }
            }
            
            if (isValid)
            {
                if (s_PendingInstances.Contains(s_CachedInstances.ElementAt(index)))
                    s_PendingInstances.Remove(s_CachedInstances.ElementAt(index));
                s_CachedInstances.RemoveAt(index);
            }

            return isValid;
        }

        public static bool TryRebuildAll()
        {
            foreach (var groupMarkerDecorator in s_CachedInstances)
            {
                groupMarkerDecorator.RebuildGroupMarkerIfRequired();
                if (groupMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                    groupMarkerDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
            }

            return true;
        }
        
        public static bool TryRebuildAndLinkAll()
        {
            var cachedCount = s_CachedInstances.Count;
            SetupActiveSelection();
            Debug.Log($"_______________________ <b>@UngroupedMarkerDecorators.TryRebuildAndLinkAll</b> ({cachedCount}/{s_CachedInstances.Count}) _______________________");
            
            var allInstances = s_CachedInstances.GetRange(0, s_CachedInstances.Count);
            foreach (var groupMarkerDecorator in allInstances)
            {
                Debug.Log($"<color=#f2ff47ff><b>#> In Cache: {groupMarkerDecorator.DebugId}</b></color> (AutoGrouping: {(IsAutoGroupingDisabled() ? "OFF" : "ON")})");
                if (!s_CachedInstances.Contains(groupMarkerDecorator))
                {
                    Debug.Log($"<color=#f2ff47ff>#>     > Skipped: {groupMarkerDecorator.DebugId}</color>");
                    continue;
                }
                groupMarkerDecorator.RebuildGroupMarkerIfRequired();
                if (IsAutoGroupingDisabled())
                {
                    if (groupMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                    {
                        groupMarkerDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
                    }
                }
                else
                {
                    if (groupMarkerDecorator.TryLinkToMatchingGroupMarkerDecorator())
                    {
                        if (groupMarkerDecorator.GetGroupBeginMarkerDecorator() is IGroupMarkerDecorator beginDecorator)
                        {
                            beginDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
                        }

                        Remove(decorator: groupMarkerDecorator);
                        Remove(decorator: groupMarkerDecorator.LinkedMarkerDecorator);
                    }
                }
            }

            Debug.Log($"________F I N A L______ <b>@UngroupedMarkerDecorators.TryRebuildAndLinkAll</b> ({s_CachedInstances.Count}) _______________________");
            
            return true;
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
            s_CachedInstances = new List<IGroupMarkerDecorator>();
            s_PendingInstances = new List<IGroupMarkerDecorator>();
            Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged() => SetupActiveSelection();

        public static void ClearCache()
        {
            Debug.Log("<color=#000000FF><b>@UngroupedMarkerDecorators.ClearCache</b></color>");
            try
            {
                foreach (var cachedInstance in s_CachedInstances)
                    cachedInstance.Dispose();
            }
            finally
            {
                s_CachedInstances.Clear();
                s_PendingInstances.Clear();
            }
        }

        public static void PrintCachedInstances()
        {
            Debug.Log("_____ PrintCachedInstances _____");
            foreach (var groupMarkerDecorator in s_CachedInstances)
            {
                var isPending = s_PendingInstances.Contains(groupMarkerDecorator);
                Debug.Log($"<color=#f2ff47ff><b>#> In Cache: {groupMarkerDecorator.DebugId}</b>{(isPending ? " (PENDING)" : "")}</color>");
            }
        }
    }
}
