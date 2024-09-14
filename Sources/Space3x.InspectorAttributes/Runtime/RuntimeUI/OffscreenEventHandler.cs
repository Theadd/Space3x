using System;
using System.Linq;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public abstract class OffscreenEventHandler : BindableElement, IOffscreenEventHandler
    {
        /// <inheritdoc/>
        public event Action onPropertyAdded;
        
        /// <inheritdoc/>
        public event Action onParentChanged;

        /// <inheritdoc/>
        public event Action onAttachToPanelEventOnce;

        /// <inheritdoc/>
        public event Action onGeometryChangedEventOnce;

        /// <inheritdoc/>
        public event Action onElementAdded;

        /// <inheritdoc/>
        public event Action delayCall
        {
            add => OffscreenEventHandler.globalDelayCall += value;
            remove => OffscreenEventHandler.globalDelayCall -= value;
        }

        /// <inheritdoc/>
        public event Action onFullyRendered
        {
            add => OffscreenEventHandler.globalOnFullyRendered += value;
            remove => OffscreenEventHandler.globalOnFullyRendered -= value;
        }
        
        private static event Action globalDelayCall;
        
        private static event Action globalOnFullyRendered;

        public void RaiseOnPropertyAddedEvent()
        {
            onPropertyAdded?.Invoke();
            RaiseAndResetDelayedCalls();
        }

        protected void RaiseOnParentChangedEvent()
        {
            onParentChanged?.Invoke();
            RaiseAndResetDelayedCalls();
        }

        protected void RaiseOnElementAddedEvent()
        {
            onElementAdded?.Invoke();
            RaiseAndResetDelayedCalls();
        }

        protected static void RaiseAndResetDelayedCalls()
        {
            if (globalDelayCall != null)
            {
                var callbacks = globalDelayCall.GetInvocationList().ToList();
                globalDelayCall = null;
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback.DynamicInvoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        public static void RaiseAndResetOnFullyRendered()
        {
            if (globalOnFullyRendered != null)
            {
                var callbacks = globalOnFullyRendered.GetInvocationList().ToList();
                globalOnFullyRendered = null;
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback.DynamicInvoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        
        public void RaiseAndResetOnAttachToPanelEventOnce()
        {
            if (onAttachToPanelEventOnce != null)
            {
                var callbacks = onAttachToPanelEventOnce.GetInvocationList().ToList();
                onAttachToPanelEventOnce = null;
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback.DynamicInvoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        
        public void RaiseAndResetOnGeometryChangedEventOnce()
        {
            if (onGeometryChangedEventOnce != null)
            {
                var callbacks = onGeometryChangedEventOnce.GetInvocationList().ToList();
                onGeometryChangedEventOnce = null;
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback.DynamicInvoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
    }
}
