using System;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.Attributes.Types.DeveloperNotes;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public static class CallbackUtility
    {
        // public static Action RegisterCallback<TEventType>(
        //     VisualElement target,
        //     EventCallback<TEventType> callback,
        //     TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
        //     where TEventType : EventBase<TEventType>, new()
        // {
        //     if (target == null) throw new ArgumentNullException(nameof(target));
        //     // if (target is IOffscreenEventHandler handler)
        //     // {
        //     //     if (typeof(TEventType) == typeof(GeometryChangedEvent))
        //     //     {
        //     //         void RemovableCallback() => callback((TEventType)null);
        //     //         handler.onElementAdded += RemovableCallback;
        //     //         return () => handler.onElementAdded -= RemovableCallback;
        //     //     }
        //     // }
        //     // Debug.LogError("FATAL!");
        //     target.RegisterCallback<TEventType>(callback, useTrickleDown);
        //     return () => target.UnregisterCallback<TEventType>(callback, useTrickleDown);
        // }
        //
        // public static Action RegisterCallbackX<TEventType>(
        //     VisualElement target,
        //     EventCallback<TEventType> callback,
        //     TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
        //     where TEventType : EventBase<TEventType>, new()
        // {
        //     if (target == null) throw new ArgumentNullException(nameof(target));
        //     if (target is IOffscreenEventHandler handler)
        //     {
        //         if (typeof(TEventType) == typeof(GeometryChangedEvent))
        //         {
        //             void RemovableCallback() => callback((TEventType)null);
        //             handler.onElementAdded += RemovableCallback;
        //             return () => handler.onElementAdded -= RemovableCallback;
        //         }
        //     }
        //     Debug.LogError("FATAL!");
        //     return () => { Debug.LogError("UNREGISTERING FATAL!"); };
        // }

        [Obsolete]
        public static void RegisterCallbackOnce<TEventType>(
            VisualElement target,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target is IOffscreenEventHandler handler)
            {
                if (typeof(TEventType) == typeof(GeometryChangedEvent))
                {
                    Debug.Log($"[VD!] RegisterCallbackOnce<GeometryChangedEvent>! {target.GetHashCode()}");
                    handler.delayCall += () => callback((TEventType)null);
                    return;
                }
            }
            Debug.LogError($"[VD!] <b>NOT</b> RegisterCallbackOnce<GeometryChangedEvent>! {target.GetHashCode()}");
            target.RegisterCallbackOnce<TEventType>(callback, useTrickleDown);
        }
        
        [Obsolete]
        public static Action RegisterOnPropertyAddedCallback(this IDrawer self, Action callback)
        {
            
            Debug.Log($"[VD!] RegisterOnPropertyAddedCallback! {self.GetHashCode()}");
            // if (target == null) throw new ArgumentNullException(nameof(target));
            if ((self is IDecorator decorator ? decorator.GhostContainer : self.Container)
                ?.GetCommonClosestParentEventHandler() is IOffscreenEventHandler handler)
            {
                void RemovableCallback()
                {
                    Debug.Log($"[VD!] RemovableCallback -> callback(); {self.GetHashCode()}");
                    callback();
                }

                handler.onPropertyAdded += RemovableCallback;
                return () => handler.onPropertyAdded -= RemovableCallback;
            }
            Debug.LogError("[VD!] FATAL! this drawer does not descend from any VisualElement implementing " + nameof(IOffscreenEventHandler) + ".");
            return () => { Debug.LogError("[VD!] UNREGISTERING FATAL!"); };
        }
        
        [Obsolete]
        public static Action RegisterOnFullyRenderedCallback(this IDrawer self, Action callback)
        {
            
            Debug.Log($"[VD!] RegisteOnFullyRenderedCallback! {self.GetHashCode()}");
            // if (target == null) throw new ArgumentNullException(nameof(target));
            if ((self is IDecorator decorator ? decorator.GhostContainer : self.Container)
                ?.GetCommonClosestParentEventHandler() is IOffscreenEventHandler handler)
            {
                void RemovableCallback()
                {
                    Debug.Log($"[VD!] RemovableCallback -> callback(); {self.GetHashCode()} onFullyRendered");
                    callback();
                }

                handler.onFullyRendered += RemovableCallback;
                return () => handler.onFullyRendered -= RemovableCallback;
            }
            Debug.LogError("[VD!] FATAL! this drawer does not descend from any VisualElement implementing " + nameof(IOffscreenEventHandler) + ".");
            return () => { Debug.LogError("[VD!] UNREGISTERING FATAL!"); };
        }
        
        [Experimental(Text = "Temporary naming.")]
        public static void RegisterOnAttachToPanelEventOnce(this VisualElement self, Action callback)
        {
            if (self.ThrowIfNull().GetEventHandler() is OffscreenEventHandler handler)
                handler.onAttachToPanelEventOnce += callback;
            else
                self
                    // .LogThis("RegisterOnAttachToPanelEventOnce FALLBACK")
                    .RegisterCallbackOnce<AttachToPanelEvent>(_ => callback());
        }
        
        [Experimental(Text = "Temporary naming.")]
        public static void RegisterOnGeometryChangedEventOnce(this VisualElement self, Action<GeometryChangedEvent> callback, bool fallbackToParent = false)
        {
            if (self.ThrowIfNull().GetEventHandler() is OffscreenEventHandler handler)
            {
                handler.onGeometryChangedEventOnce -= callback;
                handler.onGeometryChangedEventOnce += callback;
            }
            else
                (fallbackToParent ? self.parent ?? self : self)
                    .RegisterCallbackOnce<GeometryChangedEvent>(new EventCallback<GeometryChangedEvent>(callback));
        }
        
        public static void UnregisterOnGeometryChangedEventOnce(this VisualElement self, Action<GeometryChangedEvent> callback)
        {
            if (self == null) return;
            self.parent?.UnregisterCallback<GeometryChangedEvent>(new EventCallback<GeometryChangedEvent>(callback));
            self.UnregisterCallback<GeometryChangedEvent>(new EventCallback<GeometryChangedEvent>(callback));
            if (self.GetEventHandler() is OffscreenEventHandler handler) 
                handler.onGeometryChangedEventOnce -= callback;
        }
        
        // private void OnAttachToPanel(AttachToPanelEvent evt)
        // {
        //     this.RegisterCallback<KeyDownEvent>(new EventCallback<KeyDownEvent>(this.OnKeyDownEvent));
        //     this.m_LayoutDisplay.RegisterCallback<PointerUpEvent>(new EventCallback<PointerUpEvent>(this.OnPointerUpEvent));
        //     this.m_LayoutDisplay.RegisterCallback<WheelEvent>(new EventCallback<WheelEvent>(this.OnWheelEvent));
        // }
        //
        // private void OnDetachFromPanel(DetachFromPanelEvent evt)
        // {
        //     this.UnregisterCallback<KeyDownEvent>(new EventCallback<KeyDownEvent>(this.OnKeyDownEvent));
        //     this.m_LayoutDisplay.UnregisterCallback<PointerUpEvent>(new EventCallback<PointerUpEvent>(this.OnPointerUpEvent));
        //     this.m_LayoutDisplay.UnregisterCallback<WheelEvent>(new EventCallback<WheelEvent>(this.OnWheelEvent));
        // }
        
        public static TElement ThrowIfNull<TElement>(this TElement element, [CallerMemberName] string callerName = "") => 
            element ?? throw new ArgumentNullException(callerName);
    }
}
