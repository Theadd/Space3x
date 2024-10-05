using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public abstract partial class Decorator<T, TAttribute>
    {
        private void OnDetachGhostFromPanel(DetachFromPanelEvent evt)
        {
            DebugInternal($"[D!] OnDetachGhostFromPanel() => <b>DetachItself:</b>{m_DetachingItself}; <b>State:</b> {m_State}");
            if (m_DetachingItself) return;
            if (Property?.HasSerializedProperty() ?? false)
            {
                if (!Property.IsValid() || !Property.Equals(GetClosestParentField(GhostContainer)?.GetPropertyNode()))
                    HandleDetachFromPanelEventOnInvalidState(CreateContext());
                /* Here, it's probably being detached by a GroupMarkerDecorator when reorganizing the hierarchy (1),
                   or by user implementation on top of our own (2). */
            }
            else
            {
                /* A Decorator on a Non-Serialized property being detached might be caused by offscreen renderers (3).
                   In addition to (1) and (2). */
                if (!(GhostContainer.GetTreeContainer()?.Equals(Field?.GetTreeContainer()) ?? false))
                    HandleDetachFromPanelEventOnInvalidState(CreateContext());
            }

            // if (!(GhostContainer?.IsPhysicallyAndLogicallyDetachedFromPanel() ?? true)) return;

            // var hasPanel = GhostContainer.hierarchy.parent?.hierarchy.parent != null;
            // var originPanelHash = GetPanelContentHash(evt.originPanel);
            // // DebugLog.Info($"<color=#FFFF00FF>OnDetachGhostFromPanel: <b>[{(hasPanel ? "IGNORE, HAS PANEL" : "REMOVE, NO PANEL")}]</b> " +
            // //               $"{this.GetType().Name}, Removed: {m_Removed}, Added: {m_Added}, ThisHash: {this.GetHashCode()}</color>");
            // var NOT_isPhysicallyAndLogicallyDetached =
            //     (!(GhostContainer?.IsPhysicallyAndLogicallyDetachedFromPanel() ?? true));
            // DebugInternal(
            //     $"    [D!] OnDetachGhostFromPanel() :: HasPanel: {hasPanel}, OriginalPanelHash: {originPanelHash}, <b>NOT</b>_isPhysicallyAndLogicallyDetached: {NOT_isPhysicallyAndLogicallyDetached}");
            // if (NOT_isPhysicallyAndLogicallyDetached)
            // {
            //     Debug.Log("STOP");
            //     return;
            // }
            //
            // if (hasPanel)
            //     return; // TODO
            // m_Removed = true;
            // if (originPanelHash != 0)
            // {
            //     // GhostContainer?.LogThis($"REMOVING GHOST... HAS PANEL: {hasPanel}, THash: {this.GetHashCode()} => " +
            //     //                         $"{this.GetType().Name}, OriginPanel: {originPanelHash}, DestPanel: " +
            //     //                         $"{(evt.destinationPanel == null ? -1 : GetPanelContentHash(evt.destinationPanel))}");
            //     ProperlyRemoveFromHierarchy();
            // }
            // else
            // {
            //     GhostContainer?.LogThis(
            //         $"INVALIDATING GHOST... HAS PANEL: {hasPanel}, THash: {this.GetHashCode()} => {this.GetType().Name}, " +
            //         $"OriginPanel: {originPanelHash}, DestPanel: {(evt.destinationPanel == null ? -1 : GetPanelContentHash(evt.destinationPanel))}");
            // }
        }

        public void ProperlyRemoveFromHierarchy()
        {
            DebugInternal(
                $"        [D!] ProperlyRemoveFromHierarchy() :: <b>FIXME: DOUBLE DisposeGhostDecorator() CALL! // m_DetachingItself:</b> {m_DetachingItself}");
            m_Removed = true;
            m_TotallyRemoved = true;
            // GhostContainer?.UnregisterCallback<AttachToPanelEvent>(OnAttachGhostToPanel);
//             GhostContainer?.UnregisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);
// #if UNITY_EDITOR
//             if (GhostContainer != null)
//                 UnityEditor.UIElements.BindingExtensions.Unbind(GhostContainer);
// #endif
            DisposeGhostDecorator();
            // EDIT: 
            // OnReset(disposing: false);
            Dispose(disposing: false);
            //
            // GhostContainer = null;
        }

        private static int GetPanelContentHash(IPanel panel) =>
            panel?.visualTree is VisualElement { childCount: >= 2 } vPanel
                ? vPanel[1].GetHashCode()
                : 0;

        private void OnAttachGhostToPanel()
        {
            LogInternal("[D!] OnAttachGhostToPanel()! <b>FROM STATE:</b> " + m_State);
            Assert.IsTrue(m_State == State.AwaitingGhost);
            var ctx = CreateContext();
            Assert.IsTrue(ctx.Field != null && ctx.Property != null);
            Field = ctx.Field;
            Property = ctx.Property;
            m_State = State.ContextReady;
            if (GhostContainer?.panel is IPanel editorPanel && editorPanel.contextType == ContextType.Editor)
                ((IDrawer)this).AddDefaultStyles(editorPanel.visualTree);
            Container = new T();
            Container.WithDevTools(this);
            OnCreatePropertyGUI(Container);
            
            if (UpdateOnAnyValueChange)
                GhostContainer.TrackSerializedObjectValue(Property, OnUpdate);
            DecoratorsCache?.AddDisposable(this);
            
            EnsureContainerIsProperlyAttached(() =>
            {
                OnAttachContainerToPanel();
                Container.RegisterOnGeometryChangedEventOnce(OnGeometryChanged, fallbackToParent: true);
#if UNITY_EDITOR
                if (!Property.IsRuntimeUI())
                    UnityEditor.EditorApplication.delayCall += () => OnGeometryChanged(null);
#endif
            });
        }
        
//         private void OnAttachGhostToPanel()
//         {
//             // Debug.LogException(new Exception("<color=#0000FFFF><b>HERREEEEEEE!!! OnAttachGhostToPanel() SEE STACK TRACE"));
//             // if (ev.destinationPanel == null) return;
//             if (GhostContainer?.IsPhysicallyAndLogicallyDetachedFromPanel() ?? true) return;
//             if (m_TotallyRemoved)
//             {
//                 LogInternal(
//                     $"<b>IN OnAttachGhostToPanel (1) FOR A TOTALLY REMOVED {this.GetType().Name} ThisHash: {this.GetHashCode()}</b>");
//                 return;
//             }
//
//             if (m_Added)
//             {
//                 // GhostContainer.LogThis($"  <color=#FFFF00FF>OnAttachGhostToPanel: <b>[SKIP ADD!!]</b> {this.GetType().Name}, " +
//                 //                        $"THash: {this.GetHashCode()}, Rem: {m_Removed}, Add: {m_Added}, " +
//                 //                        $"PanelHash: {GetPanelContentHash(ev.destinationPanel)}</color>");
//                 BindToClosestParentPropertyFieldOf(GhostContainer);
//                 return;
//             }
//
//             // GhostContainer.LogThis($"  <color=#FFFF00FF>OnAttachGhostToPanel: <b>[ADD]</b> {this.GetType().Name}, " +
//             //                        $"THash: {this.GetHashCode()}, Rem: {m_Removed}, Add: {m_Added}, " +
//             //                        $"PanelHash: {GetPanelContentHash(ev.destinationPanel)}</color>");
//             BindToClosestParentPropertyFieldOf(GhostContainer);
//             m_Added = true;
//             if (!m_Removed)
//             {
//                 if (Field != null)
//                 {
//                     if (Container == null)
//                     {
//                         if (GhostContainer?.panel is IPanel editorPanel &&
//                             editorPanel.contextType == ContextType.Editor)
//                             ((IDrawer)this).AddDefaultStyles(editorPanel.visualTree);
//                         Container = new T();
//                         Container.WithDevTools(this);
//                         OnCreatePropertyGUI(Container);
//                     }
//
//                     EnsureContainerIsProperlyAttached(() =>
//                     {
//                         if (m_TotallyRemoved)
//                         {
//                             LogInternal(
//                                 $"<b>IN OnAttachGhostToPanel (3) FOR A TOTALLY REMOVED {this.GetType().Name} ThisHash: {this.GetHashCode()}</b>");
//                             return;
//                         }
//
//                         OnAttachContainerToPanel();
//
//                         Container.RegisterOnGeometryChangedEventOnce(OnGeometryChanged, fallbackToParent: true);
// // #if UNITY_EDITOR
// //                         // if (!Property.IsRuntimeUI())
// //                         //     UnityEditor.EditorApplication.delayCall += OnGeometryChanged;
// // #endif
//                     });
//                 }
//                 else
//                     DebugLog.Warning(
//                         $"<color=#FF0000CC>Could not find parent PropertyField for {attribute.GetType().Name}</color>");
//             }
//         }
    }
}
