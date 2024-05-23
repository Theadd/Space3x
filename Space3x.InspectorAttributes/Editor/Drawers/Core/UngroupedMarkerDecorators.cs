using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [InitializeOnLoad]
    public static class UngroupedMarkerDecorators
    {
        private static List<IGroupMarkerDecorator> s_CachedInstances;

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
        }

        public static int Count() => s_CachedInstances.Count;

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
                s_CachedInstances.RemoveAt(index);

            return isValid;
        }

        public static bool TryRebuildAll()
        {
            foreach (var groupMarkerDecorator in s_CachedInstances)
            {
                if (groupMarkerDecorator.RebuildGroupMarkerIfRequired() && groupMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                {
                    groupMarkerDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
                }
            }

            return true;
        }
        
        public static bool TryRebuildAndLinkAll()
        {
            var cachedCount = s_CachedInstances.Count;
            SetupActiveSelection();
            Debug.Log($"_______________________ <b>@UngroupedMarkerDecorators.TryRebuildAndLinkAll</b> ({cachedCount}/{s_CachedInstances.Count}) _______________________");
            
            var allInstances = s_CachedInstances.GetRange(0, s_CachedInstances.Count);
            Debug.Log("  :: FOREACH #-1");
            foreach (var groupMarkerDecorator in allInstances)
            {
                Debug.Log("  :: FOREACH #0");
                if (!s_CachedInstances.Contains(groupMarkerDecorator))
                    continue;
                Debug.Log("  :: FOREACH #1");
                groupMarkerDecorator.RebuildGroupMarkerIfRequired();
                Debug.Log("  :: FOREACH #2 --> " + groupMarkerDecorator.Container.AsString());
                // MOD
                if (IsAutoGroupingDisabled())
                {
                    if (groupMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                    {
                        Debug.Log("  :: FOREACH #X.1");
                        groupMarkerDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
                    }
                }
                else
                {
                    if (groupMarkerDecorator.TryLinkToMatchingGroupMarkerDecorator())
                    {
                        Debug.Log("  :: FOREACH #2A");
                        if (groupMarkerDecorator.GetGroupBeginMarkerDecorator() is IGroupMarkerDecorator beginDecorator)
                        {
                            Debug.Log("  :: FOREACH #2AB");
                            beginDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
                        }
                        Debug.Log("  :: FOREACH #2B");
                        Remove(decorator: groupMarkerDecorator);
                        Remove(decorator: groupMarkerDecorator.LinkedMarkerDecorator);
                        Debug.Log("  :: FOREACH #2C");
                    }
                }

                Debug.Log("  :: FOREACH #3");
            }
            Debug.Log("  :: FOREACH #4");

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
            }
        }
    }
}
