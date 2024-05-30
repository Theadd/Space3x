using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
//    [InitializeOnLoad]
    public class MarkerDecoratorsCache
    {
        private List<IGroupMarkerDecorator> m_CachedInstances;
        private List<IGroupMarkerDecorator> m_PendingInstances;

        private int m_ActiveSelectedObjectHash = 0;
        
        private bool m_DisableAutoGrouping = false;

        private int m_AutoGroupingDisabledForHash = 0;
        
        public MarkerDecoratorsCache()
        {
            m_CachedInstances = new List<IGroupMarkerDecorator>();
            m_PendingInstances = new List<IGroupMarkerDecorator>();
            m_ActiveSelectedObjectHash = GetActiveSelectionHash();
        }

        public void DisableAutoGroupingOnActiveSelection(bool disable = true)
        {
            SetupActiveSelection();
            m_AutoGroupingDisabledForHash = m_ActiveSelectedObjectHash;
            m_DisableAutoGrouping = disable;
        }
        
        public bool IsAutoGroupingDisabled() => m_DisableAutoGrouping && m_AutoGroupingDisabledForHash == GetActiveSelectionHash();
        
        public void Add(IGroupMarkerDecorator decorator)
        {
            SetupActiveSelection();
            if (!m_CachedInstances.Contains(decorator))
            {
                var o = decorator.GetSerializedObject();
                Debug.Log($"<b>  ![ADD] {decorator.Property.propertyPath} ({((DecoratorDrawer) decorator).attribute.GetType().Name}) " +
                          $"@ {o.GetHashCode()} :: {o.targetObject.name} {o.targetObject.GetHashCode()}</b>");
                m_CachedInstances.Add(decorator);
            }
        }

        public void Remove(IGroupMarkerDecorator decorator)
        {
            if (m_CachedInstances.Contains(decorator))
                m_CachedInstances.Remove(decorator);
            if (m_PendingInstances.Contains(decorator))
                m_PendingInstances.Remove(decorator);
        }

        public void MarkPending(IGroupMarkerDecorator decorator)
        {
            if (m_CachedInstances.Contains(decorator) && !m_PendingInstances.Contains(decorator))
                m_PendingInstances.Add(decorator);
        }

        public int Count() => m_CachedInstances.Count;
        
        public bool HasOnlyPending()
        {
            if (m_PendingInstances.Count > 0 && m_PendingInstances.Count == m_CachedInstances.Count)
            {
                foreach (var groupMarkerDecorator in m_PendingInstances)
                {
                    if (!m_CachedInstances.Contains(groupMarkerDecorator))
                        return false;
                }
                return true;
            }

            return false;
        }

        public bool TryGet(Func<IGroupMarkerDecorator, bool> predicate)
        {
            var isValid = false;
            var index = 0;

            for (; index < m_CachedInstances.Count; index++)
            {
                var cachedInstance = m_CachedInstances[index];
                if (predicate(cachedInstance))
                {
                    isValid = true;
                    break;
                }
            }
            
            if (isValid)
            {
                if (m_PendingInstances.Contains(m_CachedInstances.ElementAt(index)))
                    m_PendingInstances.Remove(m_CachedInstances.ElementAt(index));
                m_CachedInstances.RemoveAt(index);
            }

            return isValid;
        }

        public bool TryRebuildAll()
        {
            foreach (var groupMarkerDecorator in m_CachedInstances)
            {
                groupMarkerDecorator.RebuildGroupMarkerIfRequired();
                if (groupMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                    groupMarkerDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
            }

            Debug.Log("  ________ AFTER TryRebuildAll() _________  ");
            PrintCachedInstances();

            return true;
        }
        
        public bool TryRebuildAndLinkAll()
        {
            var cachedCount = m_CachedInstances.Count;
            SetupActiveSelection();
            Debug.Log($"_______________________ <b>@MarkerDecoratorsCache.TryRebuildAndLinkAll</b> ({cachedCount}/{m_CachedInstances.Count}) _______________________");
            
            var allInstances = m_CachedInstances.GetRange(0, m_CachedInstances.Count);
            foreach (var groupMarkerDecorator in allInstances)
            {
                Debug.Log($"<color=#f2ff47ff><b>#> In Cache: {groupMarkerDecorator.DebugId}</b></color> (AutoGrouping: {(IsAutoGroupingDisabled() ? "OFF" : "ON")})");
                if (!m_CachedInstances.Contains(groupMarkerDecorator))
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

            Debug.Log($"________F I N A L______ <b>@MarkerDecoratorsCache.TryRebuildAndLinkAll</b> ({m_CachedInstances.Count}) _______________________");
            
            return true;
        }

        public void HandlePendingDecorators()
        {
            if (!IsAutoGroupingDisabled() && HasOnlyPending())
            {
                IGroupMarkerDecorator pendingDecorator = null;
                if (TryGet(decorator =>
                    {
                        pendingDecorator = decorator;
                        return true;
                    }))
                {
                    Debug.Log("Calling pending decorator: " + pendingDecorator.DebugId);
                    pendingDecorator.OnUpdate();
                }
            }
        }
        
        private int GetActiveSelectionHash()
        {
            var o = Selection.activeObject;
            return o != null ? o.GetHashCode() : 0;
        }

        private void SetupActiveSelection()
        {
            var hash = GetActiveSelectionHash();
            if (m_ActiveSelectedObjectHash == hash)
                return;

            Debug.LogError("<color=#ff0000ff><b>... // TODO: ...</b></color>");
            m_ActiveSelectedObjectHash = hash;
            ClearCache();
        }

//        private void RegisterCallbacks(bool register)
//        {
//            Debug.Log("<color=#000000FF><b>@MarkerDecoratorsCache.RegisterCallbacks</b></color>");
//            s_CachedInstances = new List<IGroupMarkerDecorator>();
//            s_PendingInstances = new List<IGroupMarkerDecorator>();
//            Selection.selectionChanged -= OnSelectionChanged;
//            if (register)
//                Selection.selectionChanged += OnSelectionChanged;
//        }

//        private void OnSelectionChanged() => SetupActiveSelection();

        public void ClearCache()
        {
            Debug.Log("<color=#000000FF><b>.. @MarkerDecoratorsCache.ClearCache</b></color>");
//            try
//            {
//                foreach (var cachedInstance in s_CachedInstances)
//                    cachedInstance.Dispose();
//            }
//            finally
//            {
                m_CachedInstances.Clear();
                m_PendingInstances.Clear();
//            }
        }

        public void PrintCachedInstances()
        {
            Debug.Log("_____ PrintCachedInstances _____");
            foreach (var groupMarkerDecorator in m_CachedInstances)
            {
                var isPending = m_PendingInstances.Contains(groupMarkerDecorator);
                Debug.Log($"<color=#f2ff47ff><b>#> In Cache: {groupMarkerDecorator.DebugId}</b>{(isPending ? " (PENDING)" : "")}</color>");
            }
        }
    }
}
