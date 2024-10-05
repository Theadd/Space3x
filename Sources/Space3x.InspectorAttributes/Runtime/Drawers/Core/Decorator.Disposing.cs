using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public abstract partial class Decorator<T, TAttribute>
    {
        public void Dispose()
        {
            LogInternal($"[D!] Dispose() => IDisposable.Dispose();");
            Dispose(disposing: true);
        }

        protected void Dispose(bool disposing)
        {
            LogInternal($"[D!] Dispose(disposing: {disposing}) => {(m_State == State.Disposed ? "<b>ALREADY DISPOSED, EARLY RETURN!</b>" : "Not previously disposed with `disposing: true`.")}");
            if (m_State == State.Disposed)
                return;

            // Dirty hack to prevent unexpected calls to dispose between OnAttachedAndReady and the first OnUpdate call.
            // This behaviour has been observed when adding new components to a GameObject with existing annotated components.
            if (disposing && !m_Removed && m_Ready && !m_TotallyRemoved && !m_OnUpdateCalled && m_Added)
            {
                LogInternal("<b><u>[D!] Dispose(true) => EARLY RETURN :: Dirty hack</u></b> to prevent unexpected calls to dispose between OnAttachedAndReady and the first OnUpdate call.");
                // TODO: VERIFY THAT WITH THE NEW IMPLEMENTATION, THIS IS NOT NEEDED ANYMORE.
                Assert.IsTrue(false);
                return;
            }

            if (disposing)
                m_State = State.Disposed;

            m_DetachingItself = true;
            OnReset(disposing: true);
            m_DetachingItself = false;
        }

        /// <summary>
        /// When overriding this method on a derived class, make sure to call base.OnReset(disposing) on it, especially 
        /// when disposing is true.
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void OnReset(bool disposing = false)
        {
            LogInternal($"[D!] OnReset({disposing})");
            var aux = m_DetachingItself;
            m_DetachingItself = true;
            if (Container != null)
            {
#if UNITY_EDITOR
                UnityEditor.UIElements.BindingExtensions.Unbind(Container);
#endif
                Container.ClearBindings();
                Container.RemoveFromHierarchy();
            }

            Field = null;
            Container = null;
            m_Ready = false;
            if (disposing)
            {
                Property = null;
                DisposeGhostDecorator();
            }

            m_DetachingItself = aux;
        }

        private void DisposeGhostDecorator()
        {
            LogInternal(
                $"[D!] DisposeGhostDecorator() :: <b>m_DetachingItself:</b> {m_DetachingItself} :: {GhostContainer?.AsString()}");
            if (GhostContainer == null) return;
            GhostContainer.TargetDecorator = null;
            // TODO: GhostContainer.UnregisterOnAttachToPanelEventOnce(OnAttachGhostToPanel);
            GhostContainer.UnregisterCallback<DetachFromPanelEvent>(OnDetachGhostFromPanel);
#if UNITY_EDITOR
            UnityEditor.UIElements.BindingExtensions.Unbind(GhostContainer);
#endif
            GhostContainer.ClearBindings();
            GhostContainer.RemoveFromHierarchy();
            GhostContainer = null;
        }

        private void ResetToInitialState()
        {
            LogInternal($"[D!] ResetToInitialState()");
            Dispose(false);
            m_Added = false;
            m_OnUpdateCalled = false;
            m_Ready = false;
            m_Removed = false;
            m_TotallyRemoved = false;
            CachedDecoratorsCache = null;
        }

        private void LogInternal(string msg, [CallerMemberName] string callerName = "")
        {
            DebugLog.Error(
                $"<color=#000000FF>{GetType().Name} #{GetHashCode()}</color> {msg} <color=#FFFFFFEA>{callerName}</color>");
        }
        
        private void LogInternalError(string msg, [CallerMemberName] string callerName = "")
        {
            DebugLog.Error(
                $"{GetType().Name} #{GetHashCode()} <b>[ERROR]</b> <color=#000000FF>{msg}</color> <color=#FFFFFFEA>{callerName}</color>");
        }

        private void DebugInternal(string msg, [CallerMemberName] string callerName = "")
        {
            DebugLog.Notice(
                $"<color=#3EF19EFF>{GetType().Name} #{GetHashCode()}</color> {msg} <color=#FFFFFFFF><b>{callerName}</b></color>");
        }
    }
}
