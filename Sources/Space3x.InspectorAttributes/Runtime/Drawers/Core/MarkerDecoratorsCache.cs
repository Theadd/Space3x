using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using UnityEngine.Internal;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
    public class MarkerDecoratorsCache : IDisposable
    {
        private List<IGroupMarkerDecorator> m_CachedInstances;
        private List<IGroupMarkerDecorator> m_PendingInstances;
        private List<IGroupMarkerDecorator> m_FailedInstances;

        private HashSet<IDrawer> m_Drawers;

        private int m_ActiveSelectedObjectHash = 0;
        
        private bool m_DisableAutoGrouping = false;

        private int m_AutoGroupingDisabledForHash = 0;
        
        public MarkerDecoratorsCache()
        {
            m_CachedInstances = new List<IGroupMarkerDecorator>();
            m_PendingInstances = new List<IGroupMarkerDecorator>();
            m_FailedInstances = new List<IGroupMarkerDecorator>();
            m_Drawers = new HashSet<IDrawer>();
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
                m_CachedInstances.Add(decorator);
        }

        public void Remove(IGroupMarkerDecorator decorator)
        {
            if (m_CachedInstances.Contains(decorator))
                m_CachedInstances.Remove(decorator);
            if (m_PendingInstances.Contains(decorator))
                m_PendingInstances.Remove(decorator);
            if (m_FailedInstances.Contains(decorator))
                m_FailedInstances.Remove(decorator);
        }

        public void MarkPending(IGroupMarkerDecorator decorator)
        {
            var isCached = m_CachedInstances.Contains(decorator);
            var isPending = m_PendingInstances.Contains(decorator);
            if (isCached && !isPending)
                m_PendingInstances.Add(decorator);
            else if (!isCached && !m_FailedInstances.Contains(decorator))
                m_FailedInstances.Add(decorator);
        }

        private bool HasOnlyPending()
        {
            if (m_PendingInstances.Count > 0 && m_PendingInstances.Count == m_CachedInstances.Count)
            {
                foreach (var groupMarkerDecorator in m_PendingInstances)
                    if (!m_CachedInstances.Contains(groupMarkerDecorator))
                        return false;
                return true;
            }

            return false;
        }

        private bool TryGet(Func<IGroupMarkerDecorator, bool> predicate)
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

        public void RebuildAll()
        {
            foreach (var groupMarkerDecorator in m_CachedInstances)
            {
                groupMarkerDecorator.RebuildGroupMarkerIfRequired();
                if (groupMarkerDecorator.GetGroupMarkerAttribute().IsOpen)
                    groupMarkerDecorator.Marker.GetOrCreatePropertyGroupFieldForMarker();
            }
        }
        
        public void HandlePendingDecorators()
        {
            // TODO: remove
            PrintCachedInstances(false);
            
            // if (!IsAutoGroupingDisabled())
            // {
                if (HasOnlyPending())
                {
                    IGroupMarkerDecorator pendingDecorator = null;
                    if (TryGet(decorator =>
                        {
                            pendingDecorator = decorator;
                            return true;
                        }))
                    {
                        pendingDecorator.OnUpdate();
                    }
                }
                else if (m_CachedInstances.Count == 0 && m_FailedInstances.Count > 0)
                {
                    for (var i = m_FailedInstances.Count - 1; i >= 0; i--)
                    {
                        var failedInstance = m_FailedInstances[i];
                        failedInstance.OnReset(disposing: false);
                    }

                    m_FailedInstances.Clear();
                }
            // }
        }
        
#if UNITY_EDITOR

        // TODO: Get hash of multiple selected objects.
        private int GetActiveSelectionHash() =>
            UnityEditor.Selection.activeObject != null ? UnityEditor.Selection.activeObject.GetHashCode() : 0;
#else
        private int GetActiveSelectionHash() => 0;
#endif

        private void SetupActiveSelection()
        {
            var hash = GetActiveSelectionHash();
            if (m_ActiveSelectedObjectHash == hash)
                return;

            PrintCachedInstances();
            m_ActiveSelectedObjectHash = hash;
            ClearCache();
        }

        public void ClearCache()
        {
            DebugLog.Notice(nameof(MarkerDecoratorsCache) + ".ClearCache()!");
            m_CachedInstances.Clear();
            m_PendingInstances.Clear();
            m_FailedInstances.Clear();
            m_Drawers.Clear();
        }

        // TODO: remove
        public void PrintCachedInstances(bool fullReport = true)
        {
            // var str = $"_____ PrintCachedInstances _____ PENDING: {m_PendingInstances.Count} " +
            //           $"CACHED: {m_CachedInstances.Count} FAILED: {m_FailedInstances.Count} CACHE_ID: {this.GetHashCode()}";
            // // if (!fullReport) return;
            // foreach (var groupMarkerDecorator in m_CachedInstances)
            // {
            //     var isPending = m_PendingInstances.Contains(groupMarkerDecorator);
            //     var isFailed = m_FailedInstances.Contains(groupMarkerDecorator);
            //     str += $"\n<color=#f2ff47ff><b>#> In Cache: {groupMarkerDecorator.DebugId}</b>{(isPending ? " (PENDING)" : "")} {(isFailed ? " (<b>FAILED</b>)" : "")}</color>";
            // }
            // 
            // DebugLog.Info(str + "\n\n");
        }

        internal void AddDisposable(IDrawer drawer)
        {
#if UNITY_EDITOR
            if (drawer is AllowExtendedAttributesDecorator || m_Drawers.Any())
            {
                m_Drawers.Add(drawer);
            }
#endif
        }

        public void Dispose()
        {
            DebugLog.Notice(nameof(MarkerDecoratorsCache) + ".Dispose()!");
            var drawers = m_Drawers.ToArray();
            for (var i = 0; i < drawers.Length; i++)
            {
                if (drawers[i] is IDisposable disposable and not AllowExtendedAttributesDecorator)
                    disposable.Dispose();
            }
            ClearCache();
        }
    }
}
